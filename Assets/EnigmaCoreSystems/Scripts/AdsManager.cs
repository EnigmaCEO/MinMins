using Enigma.CoreSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsListener
{
    [SerializeField] private bool _testMode = false;
    [SerializeField] private string _placementId = "video";

    static public AdsManager Instance;

 #if (UNITY_ANDROID || UNITY_IOS)
    
    // AdColony values
    static bool IsAdInitialized = false;
    static bool IsAdAvailable = false;
#endif

    public delegate void OnAdRewardGrantedCallback(string zoneId, bool success, string name, int amount);
    static public OnAdRewardGrantedCallback OnAdRewardGranted;

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
        Debug.LogWarning("AdsManager::ShowAd");
        Advertisement.Show(_placementId);
    }

    public void ShowRewardAd()
    {
        string RewardedPlacementId = "rewardedVideo";

        if (!Advertisement.IsReady(RewardedPlacementId))
        {
            Debug.Log(string.Format("Ads not ready for placement '{0}'", RewardedPlacementId));
            return;
        }

        Debug.LogWarning("AdsManager::ShowAd");
        Advertisement.Show(RewardedPlacementId);
        Advertisement.Show();
    }

    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                //
                // YOUR CODE TO REWARD THE GAMER
                // Give coins etc.
                break;
            case ShowResult.Skipped:
                Debug.Log("The ad was skipped before reaching the end.");
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                break;
        }
    }

#if (UNITY_ANDROID || UNITY_IOS)
    private void OnDestroy()
    {
        
    }

    private void initialize()
    {
        // Configure the AdColony SDK
        //Debug.Log("**** Configure ADC SDK **** " + Ads.AppId);

        string gameId = Ads.AndroidGameId;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            gameId = Ads.IosGameId;
        }

        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, _testMode);
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
        static public string AndroidGameId = "3186450";
        static public string IosGameId = "3186451";

#if UNITY_ANDROID
        // App ID
        //static public string AppId = "app42a4a7795d6642b4ae";  //"app168a5d9b97bb47739a";

        static public string[] ZoneId = { "vze64e837cdaeb448494" }; //{ "vz28f417ceecca4ae4b2", "vzf2257354d2b64e08a8" };
                                                                    //If not android defaults to setting the zone strings for iOS

#else
        // App ID
        static public string appId = "app970a83943f644f9a90";
        // Video zones
        static public string[] zoneId = { "vzf8e4e97704c4445c87504e" };
#endif
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        // Define conditional logic for each ad completion status:
        if (showResult == ShowResult.Finished)
        {
            // Reward the user for watching the ad to completion.
            if (OnAdRewardGranted != null)
                OnAdRewardGranted("rewardedVideo", true, "", 0);
        }
        else if (showResult == ShowResult.Skipped)
        {
            // Do not reward the user for skipping the ad.
        }
        else if (showResult == ShowResult.Failed)
        {
           
        }
    }

    public void OnUnityAdsReady(string placementId)
    {
        
    }

    public void OnUnityAdsDidError(string message)
    {
        // Log the error.
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        // Optional actions to take when the end-users triggers an ad.
    }
}
