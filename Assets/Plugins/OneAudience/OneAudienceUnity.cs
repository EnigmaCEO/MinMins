using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class OneAudienceUnity : MonoBehaviour {
#if UNITY_ANDROID
    public void init(string appkey)
    {
        AndroidJavaClass oneAudience = new AndroidJavaClass("com.oneaudience.unity.OneAudienceUnityManager");
        oneAudience.CallStatic("init", appkey);
    }

    public void init(string appkey, bool requestPermissions)
    {
        AndroidJavaClass oneAudience = new AndroidJavaClass("com.oneaudience.unity.OneAudienceUnityManager");
        object[] parameters = new object[2];
        parameters[0] = appkey;
        parameters[1] = requestPermissions;

        oneAudience.CallStatic("init", parameters);
    }

    public void optout()
    {
        AndroidJavaClass oneAudience = new AndroidJavaClass("com.oneaudience.unity.OneAudienceUnityManager");
        oneAudience.CallStatic("optout");
    }

    public void setEmailAddress(string email)
    {
        AndroidJavaClass oneAudience = new AndroidJavaClass("com.oneaudience.unity.OneAudienceUnityManager");
        oneAudience.CallStatic("setEmailAddress", email);
    }

    public void setAge(int age)
    {
        AndroidJavaClass oneAudience = new AndroidJavaClass("com.oneaudience.unity.OneAudienceUnityManager");
        oneAudience.CallStatic("setAge", age);
    }

    public void setGender(int gender)
    {
        AndroidJavaClass oneAudience = new AndroidJavaClass("com.oneaudience.unity.OneAudienceUnityManager");
        oneAudience.CallStatic("setGender", gender);
    }
		
#elif UNITY_IPHONE
		[DllImport("__Internal")]
		private static extern void initOneAudience(string appKey);
		[DllImport("__Internal")]
		private static extern void setDeveloperEmailAddress(string email);
		[DllImport("__Internal")]
		private static extern void optoutOneAudience();

		public void init(string appkey) {
			initOneAudience (appkey);
		}

		public void setEmailAddress(string email) {
			setDeveloperEmailAddress (email);
		}

		public void optout() {
			optoutOneAudience ();
		}
#endif

}