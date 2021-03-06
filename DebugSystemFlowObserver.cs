using Entitas;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entitas.VisualDebugging.Unity {
    /// For context, please see README.md
    public sealed class DebugSystemFlowObserver : MonoBehaviour {

        public GameObject systemPrefab;
        public GameObject entityPrefab;

        public bool drawsSystemConnector = false;
        public Color systemConnectorColor = Color.green;
        public float systemConnectorDuration = float.MaxValue;

        public bool reparentsEntityToSystem = false;

        [Header("If not enough system positions, offset and rotation lays out circle")]
        public Vector3 firstPositionOffset = new Vector3(0f, 5f, 0f);
        public float nextPositionArc = -4f;

        [Header("Entities circle around offset from center")]
        public Vector3 entityPositionOffset = new Vector3(0f, 0.05f, 0f);
        public float nextEntityArc = -14f;

        [Header("Moves toward camera. Eventually may go behind camera.")]
        public float zStepOnEntitySnapped = -0.0000001f;
        private int _numSnaps = 0;

        public Transform[] systemPositions;

// Retains and loads serialized data outside of debugging.
#if (!ENTITAS_DISABLE_VISUAL_DEBUGGING && UNITY_EDITOR)
        // Caches delegate to avoid garbage each call.
        SystemEntityWillBeExecuted _snapEntityToSystem;

        EntityEvent _onDestroyEntity;
        EntityComponentChanged _onEntityChanged;

        DebugSystemsBehaviour _systemsBehaviour;

        readonly Dictionary<string, Dictionary<int, EntityBehaviour>> _contextEntities = new Dictionary<string, Dictionary<int, EntityBehaviour>>();
        readonly Dictionary<ISystem, DebugSystemObserver> _systems = new Dictionary<ISystem, DebugSystemObserver>();

        const string nullSystemName = "No System";
        Transform nullSystemTransform;


        void Start() {
            init();
        }

        void OnDestroy() {
            ObservableSystem.OnEntityWillBeExecuted -= _snapEntityToSystem;
            _contextEntities.Clear();
            _systems.Clear();
        }

        void init() {
            _onDestroyEntity = destroyEntity;
            _onEntityChanged = updateEntityName;
            _snapEntityToSystem = snapEntityToSystem;
            _systemsBehaviour = FindObjectOfType<DebugSystemsBehaviour>();
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(_systemsBehaviour.gameObject);

            mapEntitiesToBehaviours(FindObjectsOfType<EntityBehaviour>());
            createSystems(_systemsBehaviour.systems.executeSystemInfos);
            snapEntitiesToSystem(_contextEntities, null);

            ObservableSystem.OnEntityWillBeExecuted -= _snapEntityToSystem;
            ObservableSystem.OnEntityWillBeExecuted += _snapEntityToSystem;
        }

        void mapEntitiesToBehaviours(EntityBehaviour[] behaviours) {
            foreach (var behaviour in behaviours) {
                createEntity(behaviour.entity, behaviour);
            }
        }

        void createEntity(IEntity entity, EntityBehaviour behaviour) {
            string contextName = entity.contextInfo.name;
            if (!_contextEntities.ContainsKey(contextName)) {
                _contextEntities[contextName] = new Dictionary<int, EntityBehaviour>();
            }
            _contextEntities[contextName][entity.creationIndex] = behaviour;
            var observerObject = Instantiate(entityPrefab, behaviour.transform);
            var observer = observerObject.GetComponent<DebugEntityObserver>();
            if (observer != null) {
                string entityName = describeEntity(entity);
                observer.SetName(entityName);
            }
            entity.OnDestroyEntity -= _onDestroyEntity;
            entity.OnDestroyEntity += _onDestroyEntity;
            entity.OnComponentAdded -= _onEntityChanged;
            entity.OnComponentAdded += _onEntityChanged;
            entity.OnComponentRemoved -= _onEntityChanged;
            entity.OnComponentRemoved += _onEntityChanged;
        }

        // Context observer already destroys behaviour, so no need to destroy that.
        void destroyEntity(IEntity entity) {
            string contextName = entity.contextInfo.name;
            if (!_contextEntities.ContainsKey(contextName)) {
                return;
            }
            int entityId = entity.creationIndex;
            var entities = _contextEntities[contextName];
            if (entities.ContainsKey(entityId)) {
                var behaviour = entities[entityId];
                entities.Remove(entityId);
                // Destroy(behaviour.gameObject);
            }
        }

        void updateEntityName(IEntity entity, int index, IComponent component) {
            updateEntity(entity);
        }

        void updateEntity(IEntity entity) {
            string contextName = entity.contextInfo.name;
            if (!_contextEntities.ContainsKey(contextName)) {
                return;
            }
            int entityId = entity.creationIndex;
            var entities = _contextEntities[contextName];
            if (!entities.ContainsKey(entityId)) {
                return;
            }
            var entityBehaviour = entities[entityId];
            var entityObserver = entityBehaviour.GetComponentInChildren<DebugEntityObserver>();
            if (entityObserver != null) {
                string entityName = describeEntity(entity);
                entityObserver.SetName(entityName);
            }
        }

        /// TODO: Replace slow find objects of type with a quick query,
        /// such as through modifying context observer.
        /// or creating an entity event to link to the entity behaviour.
        void createEntity(IEntity entity) {
            var behaviours = FindObjectsOfType<EntityBehaviour>();
            foreach (var behaviour in behaviours) {
                if (behaviour.entity == entity) {
                    createEntity(entity, behaviour);
                    return;
                }
            }
        }

        void createSystems(SystemInfo[] systemInfos) {
            int count = systemInfos.Length;
            int count1 = count + 1;
            var systems = new ISystem[count1];
            systems[count] = null;
            for (int index = 0; index < count; ++index) {
                var systemInfo = systemInfos[index];
                systems[index] = systemInfo.system;
            }
            createSystems(systems, 0, count1);
        }

        void addSystem(ISystem system) {
            createSystems(new ISystem[]{system}, systemPositions.Length, systemPositions.Length + 1);
        }

        void createSystems(ISystem[] systems, int start, int count) {
            if (count >= systemPositions.Length) {
                Array.Resize(ref systemPositions, count);
            }
            for (int index = start; index < count; ++index) {
                if (systemPositions[index] == null) {
                    systemPositions[index] = createSystemPosition(index);
                }
                var systemPosition = systemPositions[index];
                var system = systems[index - start];
                if (system != null && _systems.ContainsKey(system)) {
                    continue;
                }
                var observerObject = Instantiate(systemPrefab, systemPosition);
                var observer = observerObject.GetComponent<DebugSystemObserver>();
                if (observer != null) {
                    string name = system == null ? nullSystemName : system.ToString();
                    observer.name = name;
                    observer.nameText.text = name;
                    systemPosition.name = name;
                }
                if (system == null) {
                    nullSystemTransform = systemPosition;
                }
                else {
                    _systems.Add(system, observer);
                }
            }
        }

        Transform createSystemPosition(int index) {
            var systemPositionObject = new GameObject();
            var systemTransform = systemPositionObject.transform;
            systemTransform.SetParent(transform);
            systemTransform.localPosition = getSystemCircularPosition(index);
            return systemTransform;
        }

        Vector3 getSystemCircularPosition(int index) {
            return Quaternion.Euler(0f, 0f, nextPositionArc * index) * firstPositionOffset;
        }

        Vector3 getEntityCircularPosition(int index) {
            return Quaternion.Euler(0f, 0f, nextEntityArc * index) * entityPositionOffset;
        }

        void snapEntitiesToSystem(Dictionary<string, Dictionary<int, EntityBehaviour>> contextEntities, ISystem system) {
            foreach (var entities in contextEntities.Values) {
                foreach (var behaviour in entities.Values) {
                    snapEntityToSystem(behaviour.entity, system);
                }
            }
        }

        void snapEntityToSystem(IEntity entity, ISystem system) {
            string contextName = entity.contextInfo.name;
            if (!_contextEntities.ContainsKey(contextName)) {
                _contextEntities[contextName] = new Dictionary<int, EntityBehaviour>();
            }
            int entityId = entity.creationIndex;
            var entities = _contextEntities[contextName];
            if (!entities.ContainsKey(entityId)) {
                createEntity(entity);
                if (!entities.ContainsKey(entityId)) {
                    Debug.LogWarning("DebugSystemFlowObserver.snapEntityToSystem: entity observer not created for " + entity);
                    return;
                }
            }

            var entityBehaviour = entities[entityId];
            if (entityBehaviour == null) {
                return;
            }
            string entityName = describeEntity(entity);
            string systemName;
            Transform systemTransform;
            if (system == null) {
                systemTransform = nullSystemTransform;
                systemName = null;
            }
            else {
                if (!_systems.ContainsKey(system)) {
                    Debug.Log("DebugSystemFlowObserver.snapEntityToSystem: " + system.ToString() + " is not in systems. Adding that system now.");
                    addSystem(system);
                }
                systemTransform = _systems[system].transform.parent;
                var systemObserver = _systems[system];
                systemName = system.ToString();
                systemObserver.Execute(systemName);
                systemObserver.Log(entityName);
            }
            move(entityBehaviour.transform, systemTransform, system != null);
            entityBehaviour.transform.position = systemTransform.position + getEntityCircularPosition(entityId)
                + new Vector3(0f, 0f, zStepOnEntitySnapped * _numSnaps);
            _numSnaps++;
            var entityObserver = entityBehaviour.GetComponentInChildren<DebugEntityObserver>();
            if (entityObserver != null) {
                entityObserver.Execute(entityName);
                entityObserver.Log(systemName);
            }
        }

        /// Disables trail if moving from absolute origin.
        void move(Transform from, Transform to, bool isDrawingLine) {
            isDrawingLine &= from.position != Vector3.zero;
            if (isDrawingLine) {
                if (drawsSystemConnector) {
                    drawLine(from, to);
                }
            }
            else {
                var trail = from.GetComponentInChildren<TrailRenderer>();
                if (trail != null) {
                    trail.Clear();
                }
            }
            if (reparentsEntityToSystem) {
                from.SetParent(to, false);
            }
            else {
                from.position = to.position;
            }
        }

        void drawLine(Transform from, Transform to) {
            Debug.DrawLine(from.position, to.position, systemConnectorColor, systemConnectorDuration);
        }

        /// Removes reference count, which triggers false-positives for meaningful changes.
        private static string describeEntity(IEntity entity) {
            string entityName = entity.ToString();
            string[] parts = entityName.Split('(');
            return entity.contextInfo.name + '.' + parts[0] + '(' + parts[2];
        }
#endif
    }
}
