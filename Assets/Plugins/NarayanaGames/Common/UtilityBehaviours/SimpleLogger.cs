using UnityEngine;
using UnityEngine.UI;

namespace NarayanaGames.Common.UtilityBehaviours {
    /// <summary>
    ///     This is a very simple logger. Just put it anywhere in
    ///     your scene with an InputField attached and use the 
    ///     Log(...) method to post your log statements.
    /// </summary>
    public class SimpleLogger : MonoBehaviour {

        private static SimpleLogger instance;

        public InputField inputFieldLog = null;

        public void Awake() {
            instance = this;
            if (inputFieldLog == null) {
                inputFieldLog = GetComponent<InputField>();
            }
            Clear();
            Log("Game Started");
        }

        public static void Log(string msg) {
            if (instance == null) {
                return;
            }
            instance.inputFieldLog.text += System.Environment.NewLine + Timestamp + msg;
        }

        public static void Log(string msg, params object[] args) {
            if (instance == null) {
                return;
            }
            instance.inputFieldLog.text += System.Environment.NewLine + Timestamp + string.Format(msg, args);
        }

        public static void Clear() {
            if (instance == null) {
                return;
            }
            instance.ClearInstance();
        }

        public void ClearInstance() {
            instance.inputFieldLog.text = "";
        }

        public static string Timestamp {
            get {
                return string.Format("[{0}] ", System.DateTime.Now.ToString("HH:mm:ss"));
            }
        }
    }
}