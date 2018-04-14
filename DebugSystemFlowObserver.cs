using UnityEngine;

namespace Entitas.VisualDebugging.Unity {
    public sealed class DebugSystemFlowObserver : MonoBehaviour {

        public GameObject systemPrefab;
        public GameObject entityPrefab;

        public Transform[] systemPositions;

        private void Start()
        {
            DontDestroyOnLoad();
        }

        private void DontDestroyOnLoad()
        {
            DontDestroyOnLoad(gameObject);
            DebugSystemsBehaviour[] systems = FindObjectsOfType<DebugSystemsBehaviour>();
            foreach (var system in systems)
            {
                DontDestroyOnLoad(system.gameObject);
            }
        }
    }
}
