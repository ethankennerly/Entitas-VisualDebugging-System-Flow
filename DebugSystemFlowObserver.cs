using Entitas;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entitas.VisualDebugging.Unity {
    /// For context, please see README.md
    public sealed class DebugSystemFlowObserver : MonoBehaviour {

        public GameObject systemPrefab;
        public GameObject entityPrefab;

        public Color systemConnectorColor = Color.green;
        public float systemConnectorDuration = float.MaxValue;

        public bool reparentsEntityToSystem = false;

        [Header("If not enough system positions, offset and rotation lays out circle")]
        public Vector3 firstPositionOffset = new Vector3(0f, 5f, 0f);
        public float nextPositionArc = -15f;

        public Transform[] systemPositions;

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
                string name = describeEntity(entity);
                observer.name = name;
                observer.nameText.text = name;
            }
            entity.OnDestroyEntity -= _onDestroyEntity;
            entity.OnDestroyEntity += _onDestroyEntity;
            entity.OnComponentAdded -= _onEntityChanged;
            entity.OnComponentAdded += _onEntityChanged;
            entity.OnComponentRemoved -= _onEntityChanged;
            entity.OnComponentRemoved += _onEntityChanged;
        }

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
                Destroy(behaviour.gameObject);
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
            var entityBehaviour = entities[entityId];
            var entityObserver = entityBehaviour.GetComponentInChildren<DebugEntityObserver>();
            if (entityObserver != null) {
                string name = describeEntity(entity);
                entityObserver.nameText.text = name;
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
            if (count1 >= systemPositions.Length) {
                Array.Resize(ref systemPositions, count1);
            }
            for (int index = 0; index < count1; ++index) {
                if (systemPositions[index] == null) {
                    systemPositions[index] = createSystemPosition(index);
                }
                var systemPosition = systemPositions[index];
                var system = systems[index];
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
            systemTransform.localPosition = getCircularPosition(index);
            return systemTransform;
        }

        Vector3 getCircularPosition(int index) {
            return Quaternion.Euler(0f, 0f, nextPositionArc * index) * firstPositionOffset;
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
            if (!entities.ContainsKey(entity.creationIndex)) {
                createEntity(entity);
            }
            var entityBehaviour = entities[entityId];
            Transform systemTransform;
            if (system == null) {
                systemTransform = nullSystemTransform;
            }
            else {
                if (!_systems.ContainsKey(system))
                {
                    init();
                }
                systemTransform = _systems[system].transform.parent;
                var systemObserver = _systems[system];
                systemObserver.Execute(system.ToString());
            }
            move(entityBehaviour.transform, systemTransform, system != null);
            var entityObserver = entityBehaviour.GetComponentInChildren<DebugEntityObserver>();
            if (entityObserver != null) {
                string name = describeEntity(entity);
                entityObserver.Execute(name);
            }
        }

        // Disables trail if moving from absolute origin.
        void move(Transform from, Transform to, bool isDrawingLine) {
            isDrawingLine &= from.position != Vector3.zero;
            if (isDrawingLine) {
                drawLine(from, to);
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

        private static string describeEntity(IEntity entity) {
            return entity.contextInfo.name + "." + entity.ToString();
        }
    }
}
