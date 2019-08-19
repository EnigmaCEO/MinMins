/*
 * Game Of Whales SDK
 *
 * https://www.gameofwhales.com/
 * 
 * Copyright © 2018 GameOfWhales. All rights reserved.
 *
 * Licence: https://github.com/Game-of-whales/GOW-SDK-UNITY/blob/master/LICENSE
 *
 */

#define GAME_OF_WHALES
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using UnityEngine;
using JsonUtils = GameOfWhalesJson.MiniJSON;
#if UNITY_PURCHASING && UNITY_5_6_OR_NEWER
using UnityEngine.Purchasing;
#endif
#if UNITY_EDITOR
using GameOfWhalesType = GameOfWhalesEditor;
// ReSharper disable MemberCanBePrivate.Global

#elif UNITY_ANDROID
using GameOfWhalesType = GameOfWhalesAndroid;
#elif UNITY_IOS
using GameOfWhalesType = GameOfWhalesIOS;
#else
using GameOfWhalesType = GameOfWhales;
#endif

//UNITY_IOS

public class GameOfWhales : MonoBehaviour
{
    public const string VERSION = "2.0.27";

    private int maxErrorCount = 10;
    private int errorCount = 0;

    public const string PROVIDER_FCM = "fcm";
    public const string PROVIDER_GCM = "gcm";
    public const string PROVIDER_APN = "apn";

    public const string VERIFY_STATE_LEGAL = "legal";//a purchase is legal
    public const string VERIFY_STATE_ILLEGAL = "illegal";//a purchase is a cheater's
    public const string VERIFY_STATE_BYPASS = "bypass";//the system doesn't verify a purchase
    public const string VERIFY_STATE_DOUBLE = "double";//the system has had purchase with the same 
    public const string VERIFY_STATE_UNDEFINED = "undefined";//the system couldn't verify a purchase.
    
    public const string STORE_SAMSUNG = "SamsungApps";
    public const string STORE_GOOGLEPLAY = "GooglePlay";
    public const string STORE_AMAZON = "AmazonStore";
    public const string STORE_APPLEAPPSTORE = "AppleAppStore";
    public const string STORE_OTHER = "Other";

    public delegate void OnSpecialOfferAppearedHandler(SpecialOffer offer);
    public delegate void OnSpecialOfferDisappearedHandler(SpecialOffer offer);
    public delegate void OnFutureSpecialOfferAppearedHandler(SpecialOffer offer);
    public delegate void OnPurchaseVerifiedHandler(string transactionID, string state);
    public delegate void OnPushDeliveredHandler(SpecialOffer offer, string camp, string title, string message);
    public delegate void OnAdLoadedHandler();
    public delegate void OnAdLoadFailedHandler();
    public delegate void OnAdClosedHandler();
    public delegate void OnInitializedHandler();
    
    public static event OnPurchaseVerifiedHandler OnPurchaseVerified = delegate { };
    public static event OnPushDeliveredHandler OnPushDelivered = delegate { };

    public static event OnSpecialOfferAppearedHandler OnSpecialOfferAppeared = delegate { };
    public static event OnSpecialOfferDisappearedHandler OnSpecialOfferedDisappeared = delegate { };
    public static event OnFutureSpecialOfferAppearedHandler OnFutureSpecialOfferAppeared = delegate { };

    public static event OnAdLoadedHandler OnAdLoaded = delegate { };
    public static event OnAdLoadFailedHandler OnAdLoadFailed = delegate { };
    public static event OnAdClosedHandler OnAdClosed = delegate { };
    
    public static event OnInitializedHandler  OnInitialized = delegate { };

    private readonly Dictionary<string, SpecialOffer> activeOffers = new Dictionary<string, SpecialOffer>();
    private readonly Dictionary<string, SpecialOffer> futureOffers = new Dictionary<string, SpecialOffer>();
    protected static string store = "";
    protected static bool nonPersonal = false;
    protected static readonly Dictionary<string, object> _emptyProperties = new Dictionary<string, object>();

    protected GameOfWhales()
    {
    }

    protected static bool CheckInstance(string from)
    {
        if (_instance == null)
        {
            Debug.LogError("GameOfWhales instance is null from: " + from);
            return false;
        }

        return true;
    }

    protected static void Initialize()
    {
        if (CheckInstance("Initialize"))
        {
            _instance._Initialize();
        }
    }
    
    protected virtual void _Initialize()
    {
    }

    private void ErrMessage(string funcName)
    {
        //Debug.Log ("[GameOfWhales] " + funcName + " is not supported on this platform");
    }
    
    

    public static string ExtractPayload(string androidReceipt)
    {
        if (androidReceipt == null)
        {
            return null;
        }
        
        var data = JsonUtils.Deserialize(androidReceipt) as Dictionary<string, object>;

        if (data.ContainsKey("Payload"))
        {
            return data["Payload"].ToString();
        }

        return androidReceipt;
    }
    
    public static void Purchase(string product, string currency, double doublePrice, string transactionID, string receipt, bool verify)
    {
        if (CheckInstance("Purchase(full)"))
        {
            _instance._Purchase(product, currency, doublePrice, transactionID, receipt, verify);
        }
    }
    
    protected virtual void _Purchase(string product, string currency, double doublePrice, string transactionID, string receipt, bool verify){
        ErrMessage("Purchase");
    }
    
    public static void Purchase(string product, string currency, double doublePrice)
    {
        if (CheckInstance("Purchase(short)"))
        {
            _instance._Purchase(product, currency, doublePrice);
        }
    }
    
    protected virtual void _Purchase(string product, string currency, double doublePrice){
        ErrMessage("Purchase");
    }

    public static void InAppPurchased(string sku, double price, string currency, string transactionID, string receipt) {
        if (CheckInstance("InAppPurchased"))
        {
            _instance._InAppPurchased(sku, price, currency, transactionID, receipt);
        }
    }

    protected virtual void _InAppPurchased(string sku, double price, string currency, string transactionID, string receipt){
        ErrMessage("InAppPurchased");
    }


    public static void UpdateToken(string token, string provider)
    {
        if (CheckInstance("UpdateToken"))
        {
            _instance._UpdateToken(token, provider);
        }
    }
    
    protected virtual void _UpdateToken(string token, string provider){
        ErrMessage("UpdateToken");
    }


    public static void PushReacted(string camp)
    {
        if (CheckInstance("PushReacted"))
        {
            _instance._PushReacted(camp);
        }
    }
    
    protected virtual void _PushReacted(string camp){
        ErrMessage("PushReacted");
    }

    public static void Converting(IDictionary<string, long> resources, string place)
    {
        if (CheckInstance("Converting"))
        {
            _instance._Converting(resources, place);
        }
    }

    protected virtual void _Converting(IDictionary<string, long> resources, string place) {
        ErrMessage("Converting");
    }


    public static void Profile(IDictionary<string, object> parameters)
    {
        if (CheckInstance("Profile"))
        {
            _instance._Profile(parameters);
        }
    }

    protected virtual void _Profile(IDictionary<string, object> parameters){
        ErrMessage("Profile");
    }


    public static void Consume(string currency, long number, string sink, long amount, string place)
    {
        if (CheckInstance("Consume"))
        {
            _instance._Consume(currency, number, sink, amount, place);
        }
    }

    protected virtual void _Consume(string currency, long number, string sink, long amount, string place){
        ErrMessage("Consume");
    }

    public static void Acquire(string currency, long number, string sink, long amount, string place)
    {
        if (CheckInstance("Acquire"))
        {
            _instance._Acquire(currency, number, sink, amount, place);
        }
    }

    protected virtual void _Acquire(string currency, long amount, string source, long number, string place){
        ErrMessage("Acquire");
    }


    public static void RegisterForNotifications()
    {
        if (CheckInstance("RegisterForNotifications"))
        {
            _instance._RegisterForNotifications();
        }
    }
    
    protected virtual void _RegisterForNotifications(){
        ErrMessage("RegisterForNotifications");
    }



    public static void ReportError(string message, string stacktrace)
    {
        if (CheckInstance("RegisterForNotifications"))
        {
            _instance._ReportError(message, stacktrace);
        }
    }
    
    protected virtual void _ReportError(string message, string stacktrace){
        ErrMessage("ReportError");
    }


    public static SpecialOffer GetSpecialOffer(string productID)
    {
        if (CheckInstance("GetSpecialOffer"))
        {
            return _instance._GetSpecialOffer(productID);
        }

        return null;
    }

    protected SpecialOffer _GetSpecialOffer(string productID){
        SpecialOffer offer;
        activeOffers.TryGetValue(productID, out offer);
        return offer;
    }


    public static void SetPushNotificationsEnable(bool value)
    {
        if (CheckInstance("SetPushNotificationsEnable"))
        {
            _instance._SetPushNotificationsEnable(value);
        }
    }
    
    protected virtual void _SetPushNotificationsEnable(bool value){
        ErrMessage("SetPushNotificationsEnable");
    }


    public static System.DateTime GetServerTime()
    {
        if (CheckInstance("GetServerTime"))
        {
            return _instance._GetServerTime();
        }
        
        return System.DateTime.UtcNow;
    }
    
    protected virtual System.DateTime _GetServerTime(){
        return System.DateTime.UtcNow;
    }


    public static void LoadAd()
    {
        if (CheckInstance("LoadAd"))
        {
            _instance._LoadAd();
        }
    }
    

    protected virtual void _LoadAd(){
        ErrMessage("LoadAd");
    }


    public static bool IsAdLoaded()
    {
        if (CheckInstance("IsAdLoaded"))
        {
            return _instance._IsAdLoaded();
        }

        return false;
    }
    
    
    protected virtual bool _IsAdLoaded(){
        return false;
    }

    public static void ShowAd()
    {
        if (CheckInstance("ShowAd"))
        {
            _instance._ShowAd();
        }
    }

    protected virtual void _ShowAd(){
        ErrMessage("ShowAd");
    }

    public static Dictionary<string, object> GetProperties()
    {
        if (CheckInstance("GetProperties"))
        {
            return _instance._GetProperties();
        }

        return _emptyProperties;
    }

    protected virtual Dictionary<string, object> _GetProperties()
    {
        ErrMessage("GetProperties");
        return _emptyProperties;
    }

    public static string GetUserGroup()
    {
        if (CheckInstance("GetUserGroup"))
        {
            return _instance._GetUserGroup();
        }

        return "";
    }

    protected string _GetUserGroup()
    {
        try
        {
            var dict = GetProperties();

            if (dict.ContainsKey("group"))
            {
                return dict["group"] as string;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }

        return "";
    }

    //Private methods * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 

    private void Internal_OnInitialized()
    {
        OnInitialized();
    }

    private void Internal_OnAdLoaded(){
        OnAdLoaded();
    }

    private void Internal_OnAdLoadFailed(){
        OnAdLoadFailed();
    }

    private void Internal_OnAdClosed(){
        OnAdClosed();
    }

    private SpecialOffer CreateSpecialOffer(string json)
    {
        try
        {
                var data = JsonUtils.Deserialize(json) as Dictionary<string, object>;
                var offer = new SpecialOffer();
                offer.id = data["camp"] as string;
                offer.product = data["product"] as string;
                offer.countFactor = float.Parse(data["countFactor"].ToString());
                offer.priceFactor = float.Parse(data["priceFactor"].ToString());
                offer.finishedAt = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
                offer.finishedAt = offer.finishedAt.AddMilliseconds((long) data["finishedAt"]);
                
                offer.activatedAt = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
                offer.activatedAt = offer.activatedAt.AddMilliseconds((long) data["activatedAt"]);

                offer.redeemable = bool.Parse(data["redeemable"].ToString());
                offer.payload = data["payload"] as string;

                if (data.ContainsKey("custom")){
                    offer.customValues = data["custom"] as Dictionary<string, object>;
                }


                return offer;
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
        
        return null;
    }

    private void Internal_OnSpecialOfferAppeared(string json){
        
        Debug.Log("GameOfWhales::Internal_OnSpecialOfferAppeared: " + json);
        try
        {
            var offer = CreateSpecialOffer(json);

            if (offer == null)
            {
                Debug.LogError("GameOfWhales: Internal_OnSpecialOfferAppeared: active offer not found");
                return;
            }
            else
            {
                    activeOffers.Add(offer.product, offer);
                    OnSpecialOfferAppeared(offer);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }


    private void Internal_OnSpecialOfferDisappeared(string productID){

        try
        {
            var offer = GetSpecialOffer(productID);
            if (offer == null){
                Debug.LogError("GameOfWhales: Internal_OnSpecialOfferDisappeared: active offer not found");
                return;
            }

            activeOffers.Remove(productID);
            OnSpecialOfferedDisappeared(offer);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    private void Internal_OnFutureSpecialOfferAppeared(string json)
    {   
        try
        {
            var offer = CreateSpecialOffer(json);

            if (offer == null){
                Debug.LogError("GameOfWhales: Internal_OnFutureSpecialOfferAppeared: active offer not found");
                return;
            }

            futureOffers.Add(offer.product, offer);
            OnFutureSpecialOfferAppeared(offer);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    } 
        

    private void Internal_OnPushDelivered(string json){
        try
        {
            Debug.Log("GameOfWhales:: Internal_OnPushDelivered: " + json);

            var data = JsonUtils.Deserialize(json) as Dictionary<string, object>;
            var camp = "";
            if (data.ContainsKey("camp")){
                camp = data["camp"] as string;
            }

            SpecialOffer offer = null;
            if (data.ContainsKey("offerProduct")){
                var offerProduct = data["offerProduct"] as string;
                offer = GetSpecialOffer(offerProduct);
            }

            var title = data["title"] as string;
            var message = data["message"] as string;
            OnPushDelivered(offer, camp, title, message);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }


    private void Internal_OnPurchaseVerified(string json){
        try
        {
            var data = JsonUtils.Deserialize(json) as Dictionary<string, object>;
            var transactionID = data["transactionID"] as string;
            var verifyState = data["verifyState"] as string;
            OnPurchaseVerified.Invoke(transactionID, verifyState);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    //private void 

    public static void Init(string currentStore, bool nonPersonal)
    {
        try
        {
            GameOfWhales.store = currentStore;
            GameOfWhales.nonPersonal = nonPersonal;
            GameOfWhales instance = GameOfWhales.Instance;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }
    
    public static void Init(string currentStore){
        Init(currentStore, false);
    }

    // Singleton
    private static GameOfWhalesType _instance;

    private static object _lock = new object();

    public static GameOfWhalesType Instance{
        get{
            if (applicationIsQuitting){
                Debug.LogWarning("[Singleton] Instance '" + typeof(GameOfWhalesType) +
                                 "' already destroyed on application quit." +
                                 " Won't create again - returning null.");
                return null;
            }

            lock (_lock){
                if (_instance == null){
                    try
                    {
                        _instance = (GameOfWhalesType) FindObjectOfType(typeof(GameOfWhalesType));
    
                        if (FindObjectsOfType(typeof(GameOfWhalesType)).Length > 1){
                            Debug.LogError("[Singleton] Something went really wrong " +
                                           " - there should never be more than 1 singleton!" +
                                           " Reopening the scene might fix it.");
                            return _instance;
                        }
    
                        if (_instance == null){
                            GameObject singleton = new GameObject();
                            singleton.name = "(singleton) " + typeof(GameOfWhalesType).ToString();
                            _instance = singleton.AddComponent<GameOfWhalesType>();
                            
                            Initialize();
    
                            DontDestroyOnLoad(singleton);
    
                            Debug.Log("[Singleton] An instance of " + typeof(GameOfWhalesType) +
                                      " has been created with DontDestroyOnLoad.");
                        }
                        else{
                            Debug.Log("[Singleton] Using instance already created: " +
                                      _instance.gameObject.name);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                    }
                }

                return _instance;
            }
        }
    }


    private static bool applicationIsQuitting = false;

    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    public void OnDestroy(){
        applicationIsQuitting = true;
    }

    protected void ErrorsSubscribe(){
        Application.logMessageReceived += HandleLog;
    }

    private void HandleLog(string message, string stacktrace, LogType type){
        if (errorCount < maxErrorCount && type != LogType.Log){
            errorCount++;
            ReportError(message, stacktrace);
        }
    }

//Comment in case of compile error
#if UNITY_PURCHASING && UNITY_5_6_OR_NEWER
    public static string GetCurrentStore()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return STORE_APPLEAPPSTORE;
        }
        
        var module = StandardPurchasingModule.Instance();

        if (Application.platform == RuntimePlatform.Android && module.appStore == AppStore.GooglePlay)
        {
            return STORE_GOOGLEPLAY;
        }  

        if (Application.platform == RuntimePlatform.Android && module.appStore == AppStore.SamsungApps)
        {
            return STORE_SAMSUNG;
        }

        if (Application.platform == RuntimePlatform.Android && module.appStore == AppStore.AmazonAppStore)
        {
            //return STORE_AMAZON;
        }

        return "";
    }
#endif
}