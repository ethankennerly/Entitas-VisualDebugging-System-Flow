using UnityEngine;

namespace Entitas.VisualDebugging.Unity {
    public class DebugBehaviour : MonoBehaviour {

        public TextMesh nameText;

        public Animator anim;
        public string executedState = "Executed";

        public void Execute(string name) {
            if (anim != null) {
                anim.Play(executedState, -1, 0f);
            }
            if (nameText != null) {
                nameText.text = name;
            }
        }
    }
}
