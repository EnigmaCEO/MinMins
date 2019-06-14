/*! \cond PRIVATE */
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    [AddComponentMenu("Dark Tonic/Master Audio/Ambient Sound")]
    // ReSharper disable once CheckNamespace
    [AudioScriptOrder(-20)]
    public class AmbientSound : MonoBehaviour {
        [SoundGroup] public string AmbientSoundGroup = MasterAudio.NoGroupName;
        
		[Tooltip("This option is useful if your caller ever moves, as it will make the Audio Source follow to the location of the caller every frame.")]
		public bool FollowCaller;

#if UNITY_5_6_OR_NEWER
        [Tooltip("Using this option, the Audio Source will be updated every frame to the closest position on the caller's collider, if any. This will override the Follow Caller option above and happen instead.")]
		public bool UseClosestColliderPosition;
#else
		public bool UseClosestColliderPosition = false;
#endif

        public bool UseTopCollider = true;
        public bool IncludeChildColliders = false;

        [Tooltip("This is for diagnostic purposes only. Do not change or assign this field.")]
        public Transform RuntimeFollower;

        private Transform _trans;

        // ReSharper disable once UnusedMember.Local
        void OnEnable() {
            StartTrackers();
        }

        // ReSharper disable once UnusedMember.Local
        void OnDisable() {
            if (MasterAudio.AppIsShuttingDown) {
                return; // do nothing
            }

            if (!IsValidSoundGroup) {
                return;
            }

            if (MasterAudio.SafeInstance == null) {
                return;
            }

            MasterAudio.StopSoundGroupOfTransform(Trans, AmbientSoundGroup);
            RuntimeFollower = null;
        }

        private void StartTrackers() {
            if (!IsValidSoundGroup) {
                return;
            }

            var isListenerFollowerAvailable = AmbientUtil.InitListenerFollower();
            if (!isListenerFollowerAvailable) {
                return; // don't bother creating the follower because there's no Listener to collide with.
            }

            if (!AmbientUtil.HasListenerFolowerRigidBody) {
                MasterAudio.LogWarning("Your Ambient Sound script on Game Object '" + name + "' will not function because you have turned off the Listener Follower RigidBody in Advanced Settings.");
            }

            var followerName = name + "_" + AmbientSoundGroup + "_" + Random.Range(0, 9) + "_Follower";
            RuntimeFollower = AmbientUtil.InitAudioSourceFollower(Trans, followerName, AmbientSoundGroup, FollowCaller, UseClosestColliderPosition, UseTopCollider, IncludeChildColliders);
        }

        public bool IsValidSoundGroup {
            get {
                return !MasterAudio.SoundGroupHardCodedNames.Contains(AmbientSoundGroup);
            }
        }

        public Transform Trans {
            get {
                // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
                if (_trans == null) {
                    _trans = transform;
                }

                return _trans;
            }
        }
    }
}
/*! \endcond */
