/*! \cond PRIVATE */
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace DarkTonic.MasterAudio {
    public static class SpatializerHelper {
        public static bool SpatializerOptionExists {
            get {
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
                return false;
#else 
				return true;
#endif
            }
        }

        public static void TurnOnSpatializerIfEnabled(AudioSource source) {
            if (!SpatializerOptionExists) {
                return; // no spatializer option!
            }

            // hopefully, there's a way later to detect if the option is turned on, in AudioManager!

            if (MasterAudio.SafeInstance == null) {
                return;
            }

            if (!MasterAudio.Instance.useSpatializer) {
                return;
            }

			#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 
				// no spatializer!
			#else
				source.spatialize = true;
			#endif
        }
    }
}
/*! \endcond */