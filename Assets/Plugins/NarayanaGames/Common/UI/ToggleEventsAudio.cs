/****************************************************
 *  (c) 2016 narayana games UG (haftungsbeschränkt) *
 *  All rights reserved                             *
 ****************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace NarayanaGames.Common.UI {
    [RequireComponent(typeof(Toggle))]
    [AddComponentMenu("UI/Toggle Events Audio", 35)]
    public class ToggleEventsAudio : MonoBehaviour {

        [Header("Audio Sources")]
        [Tooltip("Audio source used for one shots. If not assigned, will try using AudioSource attached to this game object.")]
        public AudioSource oneshotAudioSource;

        [Space]
        [Header("Audio Clips for Toggles")]
        [Tooltip("Sound played when the toggle is checked.")]
        public AudioClip checkedSound;

        [Tooltip("Sound played when toggle is unchecked.")]
        public AudioClip uncheckedSound;

        private Toggle myToggle;

        public void Awake() {
            if (oneshotAudioSource == null) {
                oneshotAudioSource = GetComponent<AudioSource>();
            }
            if (oneshotAudioSource == null) {
                Debug.LogErrorFormat(this, "UIEventsAudio '{0}' needs AudioSource, please add one to object or assign at least oneshotAudioSource", this.name);
            }
            myToggle = GetComponent<Toggle>();
        }

        public void OnEnable() {
            if (myToggle != null) {
                myToggle.onValueChanged.AddListener(OnValueChanged);
            }
        }

        public void OnDisable() {
            if (myToggle != null) {
                myToggle.onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        public void OnValueChanged(bool newValue) {
            PlayOneShot(newValue ? checkedSound : uncheckedSound);
        }

        private void PlayOneShot(AudioClip sound) {
            if (sound != null && oneshotAudioSource != null) {
                oneshotAudioSource.PlayOneShot(sound);
            }
        }

    }
}
