using Entitas;
using UnityEngine;

namespace Entitas.VisualDebugging.Unity {
    /// For context, please see README.md
    public sealed class DebugSystemFlowObserver : MonoBehaviour {

        public GameObject systemPrefab;
        public GameObject entityPrefab;

        public Transform[] systemPositions;

        DebugSystemsBehaviour _systemsBehaviour;

        SystemEntityWillBeExecuted _snapEntityObserver;

        void Start() {
            _snapEntityObserver = snapEntityObserverToSystem;
            _systemsBehaviour = FindObjectOfType<DebugSystemsBehaviour>();
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(_systemsBehaviour.gameObject);

            ObservableSystem.OnEntityWillBeExecuted -= _snapEntityObserver;
            ObservableSystem.OnEntityWillBeExecuted += _snapEntityObserver;
        }

        void OnDestroy() {
            ObservableSystem.OnEntityWillBeExecuted -= _snapEntityObserver;
        }

        void snapEntityObserverToSystem(IEntity entity, ISystem system) {
            Debug.Log("snapEntityObserverToSystem: TODO entity " + entity + " system " + system);
        }
    }
}
