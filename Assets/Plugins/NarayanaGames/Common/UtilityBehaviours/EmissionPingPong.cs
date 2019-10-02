using UnityEngine;

namespace NarayanaGames.Common.UtilityBehaviours {
    public class EmissionPingPong : MonoBehaviour {

        public Material emissiveMaterial;

        public AnimationCurve pingPongCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public float minValue = 0F;
        public float maxValue = 1F;
        public float offset = 0F;
        public float frequency = 1F;

        private Color originalColor;

        public void Awake() {
            if (!emissiveMaterial.HasProperty("_EmissionColor")) {
                this.enabled = false;
                return;
            }
            originalColor = emissiveMaterial.GetColor("_EmissionColor");
        }

        public void OnDisable() {
            if (emissiveMaterial.HasProperty("_EmissionColor")) {
                emissiveMaterial.SetColor("_EmissionColor", originalColor);
            }
        }

        public void Update() {
            float pingPong = minValue + pingPongCurve.Evaluate(Mathf.PingPong(Time.time * frequency + offset, 1F)) * (maxValue - minValue);
            emissiveMaterial.SetColor("_EmissionColor", originalColor * pingPong);
        }
    }
}