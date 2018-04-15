using UnityEngine;

namespace Entitas.VisualDebugging.Unity {
    public sealed class DebugSystemObserver : MonoBehaviour {

        public TextMesh nameText;

        public Animator anim;
        public string executedState = "Executed";

        public void Execute() {
            if (anim != null) {
                anim.Play(executedState, -1, 0f);
            }
        }
    }
}
