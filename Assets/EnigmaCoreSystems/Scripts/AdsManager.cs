using Enigma.CoreSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    static public AdsManager Instance;

 #if (UNITY_ANDROID || UNITY_IOS)
    
    // AdColony values
    static bool IsAdInitialized = false;
    static bool IsAdAvailable = false;
#endif

    public delegate void OnAdRewardGrantedCallback(string zoneId, bool success, string name, int amount);
    public OnAdRewardGrantedCallback OnAdRewardGranted;

    public delegate void OnAdFailedCallback();
    public OnAdFailedCallback OnAdFailed;

    void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start ()
    {

 #if (UNITY_ANDROID || UNITY_IOS)
        initialize();
#endif
    }

    public void ShowAd()
    {


    }


#if (UNITY_ANDROID || UNITY_IOS)
    private void OnDestroy()
    {
        
    }

    private void initialize()
    {
        // Configure the AdColony SDK
        Debug.Log("**** Configure ADC SDK **** " + Ads.appId);

        
    }

    private void onAdRewardGranted(string zoneId, bool success, string name, int amount)
    {
        if (OnAdRewardGranted != null)
            OnAdRewardGranted(zoneId, success, name, amount);
    }
#endif

    public class Ads
    {
        public string appVersion = "1.0";

#if UNITY_ANDROID
        // App ID
        static public string appId = "app42a4a7795d6642b4ae";  //"app168a5d9b97bb47739a";
                                                               // Video zones
        static public string[] zoneId = { "vze64e837cdaeb448494" }; //{ "vz28f417ceecca4ae4b2", "vzf2257354d2b64e08a8" };
                                                                    //If not android defaults to setting the zone strings for iOS

#else
        // App ID
        static public string appId = "app970a83943f644f9a90";
        // Video zones
        static public string[] zoneId = { "vzf8e4e97704c4445c87504e" };
#endif
    }
}
