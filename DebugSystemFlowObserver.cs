using Entitas;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Entitas.VisualDebugging.Unity {
    /// For context, please see README.md
    public sealed class DebugSystemFlowObserver : MonoBehaviour {

        public GameObject systemPrefab;
        public GameObject entityPrefab;

        [Header("If not enough system positions, offset and rotation lays out circle")]
        public Vector3 firstPositionOffset = new Vector3(0f, 5f, 0f);
        public float nextPositionArc = -15f;

        public Transform[] systemPositions;

        // Caches delegate to avoid garbage each call.
        SystemEntityWillBeExecuted _snapEntityToSystem;

        DebugSystemsBehaviour _systemsBehaviour;

        readonly Dictionary<int, EntityBehaviour> _entities = new Dictionary<int, EntityBehaviour>();
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
            _snapEntityToSystem = snapEntityToSystem;
            _systemsBehaviour = FindObjectOfType<DebugSystemsBehaviour>();
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(_systemsBehaviour.gameObject);

            mapEntitiesToBehaviours(FindObjectsOfType<EntityBehaviour>());
            createSystems(_systemsBehaviour.systems.executeSystemInfos);
            snapEntitiesToSystem(_entities, null);

            ObservableSystem.OnEntityWillBeExecuted -= _snapEntityToSystem;
            ObservableSystem.OnEntityWillBeExecuted += _snapEntityToSystem;
        }

        void mapEntitiesToBehaviours(EntityBehaviour[] behaviours) {
            foreach (var behaviour in behaviours) {
                createEntity(behaviour.entity, behaviour);
            }
        }

        void createEntity(IEntity entity, EntityBehaviour behaviour) {
            _entities[entity.creationIndex] = behaviour;
            var observerObject = Instantiate(entityPrefab, behaviour.transform);
            var observer = observerObject.GetComponent<DebugEntityObserver>();
            if (observer != null) {
                observer.name = entity.ToString();
                observer.nameText.text = entity.ToString();
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
            systemPositionObject.name = "SystemPosition_" + index.ToString();
            var systemTransform = systemPositionObject.transform;
            systemTransform.SetParent(transform);
            systemTransform.localPosition = getCircularPosition(index);
            return systemTransform;
        }

        Vector3 getCircularPosition(int index) {
            return Quaternion.Euler(0f, 0f, nextPositionArc * index) * firstPositionOffset;
        }

        void snapEntitiesToSystem(Dictionary<int, EntityBehaviour> entities, ISystem system) {
            foreach (var behaviour in entities.Values) {
                snapEntityToSystem(behaviour.entity, system);
            }
        }

        void snapEntityToSystem(IEntity entity, ISystem system) {
            if (!_entities.ContainsKey(entity.creationIndex)) {
                createEntity(entity);
            }
            Transform systemTransform;
            if (system == null) {
                systemTransform = nullSystemTransform;
            }
            else {
                if (!_systems.ContainsKey(system))
                {
                    init();
                }
                systemTransform = _systems[system].transform;
            }
            _entities[entity.creationIndex].transform.position = systemTransform.position;
        }
    }
}
