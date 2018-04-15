using System.Collections.Generic;
using UnityEngine;

namespace Entitas.VisualDebugging.Unity {
    public class DebugBehaviour : MonoBehaviour {

        public TextMesh nameText;

        public Animator anim;
        public string executedState = "Executed";

        [Header("Logs first reactive systems executing an entity.")]
        public int maxLogs = 32;
        public List<string> logs = new List<string>();

        public void Execute(string name) {
            if (anim != null) {
                anim.Play(executedState, -1, 0f);
            }
            SetName(name);
        }

        public void Log(string message) {
            if (maxLogs <= 0) {
                return;
            }
            if (logs.Count >= maxLogs) {
                return;
            }
            if (string.IsNullOrEmpty(message)) {
                return;
            }
            if (logs.Count >= 1 && logs[logs.Count - 1] == message) {
                return;
            }
            logs.Add(message);
        }

        public void SetName(string name) {
            if (nameText != null) {
                if (this.name == name) {
                    return;
                }
                this.name = name;
                nameText.text = name;
                Log(name);
            }
        }
    }
}
