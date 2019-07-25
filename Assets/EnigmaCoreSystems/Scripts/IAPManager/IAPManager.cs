
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enigma.CoreSystems;
using SimpleJSON;
using UnityEngine.Purchasing;
using TapjoyUnity;

public delegate void IAPManagerPurchaseCompleteCallback();

public class IAPManager : Manageable<IAPManager>, IStoreListener
{
    public string APPLE_IOS_KEY = "";
    public string ANDROID_GOOGLE_PLAY_KEY = "";
    public string SAMSUNG_GROUP_ID = "";
    public string FORTUMO_SEC = "";
    public string FORTUMO_SERVICE = "";
    public string[] IAP_IDS;

    static public string appVersion = "1.0";
    static public string appId = "";
    static public string[] zoneId;
    static public string appStoreKey;

    static public bool iapVendorCallbackDone = false;
    static public bool iapVendorCallbackValue = false;
    static public string iapVendorCallbackID = "";
    static public string iapVendorCallbackReceipt = "";
    static public bool iapEnigmaCallbackDone = false;
    static public bool sendToServer = false;

#if (UNITY_ANDROID || UNITY_IOS)
    private static IStoreController m_StoreController;                                                                  // Reference to the Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider;                                                         // Reference to store-specific Purchasing subsystems.

    static private StoreObjectIAP store_item;
    static private JSONNode iapEnigmaCallbackValue;

    private TJPlacement _tapJoyPlacement;
#endif

    static public StoreObjectIAP[] iapItems;
    static NetworkManager.Callback callBack = null;

    public delegate void IAPPurchased(string id, bool result);
    static public IAPPurchased IAPResult;

    public delegate void TransactionError(string messageTerm);
    public TransactionError OnTransactionError;

    public delegate void CoinsEarned(int coins);
    public CoinsEarned OnCoinsEarned;

    public delegate void SimpleCallback();
    public SimpleCallback OnHideTransactionError;

    public bool IsHacked = false;

    private string _tapJoyUserId = "";

    private const string _TAPJOY_PLAYER_PREF_KEY = "TapJoyPlayerPrefId";
    private const int _TAPJOY_ID_DIGITS_AMOUNT = 12;
    private const int _TAPJOY_ID_BLOCK_SIZE = 4;

    protected override void Awake()
    {
#if (UNITY_ANDROID || UNITY_IOS)
        base.Awake();

        string appId = Application.identifier;
        int idsCount = IAP_IDS.Length;

        for (int i = 0; i < idsCount; i++)
            IAP_IDS[i] = appId + IAP_IDS[i];

        //Tap Joy callbacks and placement responses =========================
        Tapjoy.OnConnectSuccess += handleConnectSuccess;
        TJPlacement.OnRequestSuccess += HandlePlacementRequestSuccess;
        TJPlacement.OnRequestFailure += HandlePlacementRequestFailure;
        TJPlacement.OnContentReady += HandlePlacementContentReady;
        TJPlacement.OnContentShow += HandlePlacementContentShow;
        TJPlacement.OnContentDismiss += HandlePlacementContentDismiss;
        Tapjoy.OnEarnedCurrency += handleEarnedCurrency;
        Tapjoy.OnSetUserIDSuccess += onTapJoySetUserIdSuccess;
        Tapjoy.OnSetUserIDFailure += onTapJoySetUserIdFailure;
        //====================================================================
#endif
    }

    // Use this for initialization
    protected override void Start()
    {
#if (UNITY_ANDROID || UNITY_IOS)
        // If we haven't set up the Unity Purchasing reference
        if (m_StoreController == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();

            GameOfWhales.Init(GameOfWhales.GetCurrentStore());
            GameOfWhales.Instance.SetPushNotificationsEnable(true);
        }

        GameOfWhales.Instance.OnPurchaseVerified += OnPurchaseVerifiedCallback;
        GameOfWhales.Instance.OnSpecialOfferAppeared += OnOfferAppearedCallback;
        GameOfWhales.Instance.OnSpecialOfferedDisappeared += OnOfferDisappearedCallback;
        GameOfWhales.Instance.OnPushDelivered += OnPushDeliveredCallback;
#endif

#if Kongregate
		KongAPI.instance.OnKongregatePurchaseComplete += OnPurchaseCompleteCallback;
#endif

    }

    // Update is called once per frame
    protected override void Update()
    {
#if (UNITY_ANDROID || UNITY_IOS)
        if (iapVendorCallbackDone && iapEnigmaCallbackDone)
        {
            iapVendorCallbackDone = false;
            iapEnigmaCallbackDone = false;
            sendToServer = true;
        }

        if (sendToServer)
        {
            Debug.Log("Sending to server...");
            //IAPPurchaseComplete ();
            sendToServer = false;
        }
#endif
    }

    void OnDestroy()
    {
#if (UNITY_ANDROID || UNITY_IOS)
        if (GameOfWhales.Instance != null)
        {
            GameOfWhales.Instance.OnPurchaseVerified -= OnPurchaseVerifiedCallback;
            GameOfWhales.Instance.OnSpecialOfferAppeared -= OnOfferAppearedCallback;
            GameOfWhales.Instance.OnSpecialOfferedDisappeared -= OnOfferDisappearedCallback;
            GameOfWhales.Instance.OnPushDelivered -= OnPushDeliveredCallback;
        }

        //Tap Joy callbacks and placement responses =========================
        Tapjoy.OnConnectSuccess -= handleConnectSuccess;
        TJPlacement.OnRequestSuccess -= HandlePlacementRequestSuccess;
        TJPlacement.OnRequestFailure -= HandlePlacementRequestFailure;
        TJPlacement.OnContentReady -= HandlePlacementContentReady;
        TJPlacement.OnContentShow -= HandlePlacementContentShow;
        TJPlacement.OnContentDismiss -= HandlePlacementContentDismiss;
        //TJPlacement.OnRewardRequest -= HandleOnRewardRequest;
        Tapjoy.OnEarnedCurrency -= handleEarnedCurrency;
        Tapjoy.OnSetUserIDSuccess -= onTapJoySetUserIdSuccess;
        Tapjoy.OnSetUserIDFailure -= onTapJoySetUserIdFailure;
        //====================================================================
#endif
    }

    static public void WatchPlayers(string appKey)
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        GameObject oneAudienceObject = new GameObject("OneAudienceUnity");
        OneAudienceUnity oneAudience = oneAudienceObject.AddComponent<OneAudienceUnity>();
        oneAudience.init(appKey);
#endif
    }

    static public void BuyMobile()
    {
#if (UNITY_ANDROID || UNITY_IOS)
        string id = NetworkManager.GetUserInfo("id");
        string sec = IAPManager.Instance.FORTUMO_SEC;
        string service = IAPManager.Instance.FORTUMO_SERVICE;

        string sig = md5("cuid=" + id + sec);

        Application.OpenURL("https://fortumo.com/mobile_payments/" + service + "?cuid=" + id + "&sig=" + sig);
#endif
    }


    static public void InitializePurchasing()
    {
#if (UNITY_ANDROID || UNITY_IOS)
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            return;
        }

        var module = StandardPurchasingModule.Instance();
        // The FakeStore supports: no-ui (always succeeding), basic ui (purchase pass/fail), and
        // developer ui (initialization, purchase, failure code setting). These correspond to
        // the FakeStoreUIMode Enum values passed into StandardPurchasingModule.useFakeStoreUIMode.
        //module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;

        var builder = ConfigurationBuilder.Instance(module);
        builder.Configure<IMicrosoftConfiguration>().useMockBillingSystem = false;
        builder.Configure<IGooglePlayConfiguration>().SetPublicKey(IAPManager.Instance.ANDROID_GOOGLE_PLAY_KEY);

    #if UNITY_TIZEN
        builder.Configure<ITizenStoreConfiguration>().SetGroupId("100000086671");
        IAPManager.Instance.IAP_IDS = new string[] { "000000599631", "000000599632", "000000599633", "000000599634", "000000599635", "000000599636", "000000599637", "000000599638" };
    #endif
    #if ANDROID_MOOLAH
		builder.Configure<IMoolahConfiguration>().appKey = "27";
		builder.Configure<IMoolahConfiguration>().hashKey = "1525127e80ac408f98e75ad6142e4517";
    #endif
        foreach (string id in IAPManager.Instance.IAP_IDS)
        {
            builder.AddProduct(id, ProductType.Consumable);
            builder.AddProduct(id, ProductType.NonConsumable);
            builder.AddProduct(id, ProductType.Subscription);
        }

        UnityPurchasing.Initialize(Instance, builder);
#endif
    }

#if (UNITY_ANDROID || UNITY_IOS)
    static private bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }
#endif

    static public void BuyConsumable(int index)
    {
#if (UNITY_ANDROID || UNITY_IOS)
        // Buy the consumable product using its general identifier. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
        BuyProductID(Instance.IAP_IDS[index]);
#endif
    }

    static public void BuyNonConsumable(int index)
    {
#if (UNITY_ANDROID || UNITY_IOS)
    // Buy the non-consumable product using its general identifier. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
    #if Kongregate
		    KongAPI.instance.PurchaseItem(Instance.IAP_IDS[index]);
    #else
            BuyProductID(Instance.IAP_IDS[index]);
    #endif
#endif
    }

    static public void OnPurchaseCompleteCallback(string desc, bool result)
    {
#if (UNITY_ANDROID || UNITY_IOS)
        IAPResult(desc, result);
#endif
    }

    static public void BuySubscription(int index)
    {
#if (UNITY_ANDROID || UNITY_IOS)
        // Buy the subscription product using its the general identifier. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
        BuyProductID(Instance.IAP_IDS[index]);
#endif
    }


    static void BuyProductID(string productId)
    {
#if (UNITY_ANDROID || UNITY_IOS)

        // If the stores throw an unexpected exception, use try..catch to protect my logic here.
        try
        {
            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ...
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
                    m_StoreController.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            // Otherwise ...
            else
            {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or retrying initiailization.
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }
        // Complete the unexpected exception handling ...
        catch (Exception e)
        {
            // ... by reporting any unexpected exception for later diagnosis.
            Debug.Log("BuyProductID: FAIL. Exception during purchase. " + e);
        }
#endif
    }


    // Restore purchases previously made by this customer. Some platforms automatically restore purchases. Apple currently requires explicit purchase restoration for IAP.
    static public void RestorePurchases()
    {
#if (UNITY_ANDROID || UNITY_IOS)
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ...
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) =>
            {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
#endif
    }

    //
    // --- IStoreListener
    //

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
#if (UNITY_ANDROID || UNITY_IOS)
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log("OnInitialized: PASS");

        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;
#endif
    }


    public void OnInitializeFailed(InitializationFailureReason error)
    {
#if (UNITY_ANDROID || UNITY_IOS)
        Debug.Log("Billing failed to initialize!");
        switch (error)
        {
            case InitializationFailureReason.AppNotKnown:
                Debug.LogError("Is your App correctly uploaded on the relevant publisher console?");
                break;
            case InitializationFailureReason.PurchasingUnavailable:
                // Ask the user if billing is disabled in device settings.
                Debug.Log("Billing disabled!");
                break;
            case InitializationFailureReason.NoProductsAvailable:
                // Developer configuration error; check product metadata.
                Debug.Log("No products available for purchase!");
                break;
        }
#endif
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
#if (UNITY_ANDROID || UNITY_IOS)
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing this reason with the user.
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        IAPResult(product.definition.id, false);
#endif
    }

#if (UNITY_ANDROID || UNITY_IOS)
    /// <summary>
    /// This will be called after a call to IAppleExtensions.RestoreTransactions().
    /// </summary>
    private void OnTransactionsRestored(bool success)
    {
        Debug.Log("Transactions restored.");
    }

    /// <summary>
    /// iOS Specific.
    /// This is called as part of Apple's 'Ask to buy' functionality,
    /// when a purchase is requested by a minor and referred to a parent
    /// for approval.
    ///
    /// When the purchase is approved or rejected, the normal purchase events
    /// will fire.
    /// </summary>
    /// <param name="item">Item.</param>
    private void OnDeferred(Product item)
    {
        Debug.Log("Purchase deferred: " + item.definition.id);
    }
#endif

    #region Game of Whales
    public virtual void RegisterConsume(string currency, long number, string sink, long amount, string place)
    {
#if (UNITY_ANDROID || UNITY_IOS)
        GameOfWhales.Instance.Consume(currency, number, sink, amount, place);
#endif
    }

    public virtual void RegisterAcquire(string currency, long amount, string source, long number, string place)
    {
#if (UNITY_ANDROID || UNITY_IOS)
        GameOfWhales.Instance.Consume(currency, amount, source, number, place);
#endif
    }

    public float GetOfferPriceFactor(string product)
    {
        float priceFactor = 1;

#if (UNITY_ANDROID || UNITY_IOS)
        SpecialOffer offer = GameOfWhales.Instance.GetSpecialOffer(product);

        if (offer != null)
        {
            if (offer.HasPriceFactor())
                priceFactor = offer.priceFactor;
        }

        if(IsHacked && (product == "fire"))
            priceFactor = 0.5f;
#endif
        return priceFactor;
    }

    PurchaseProcessingResult IStoreListener.ProcessPurchase(PurchaseEventArgs args)
    {
#if (UNITY_ANDROID || UNITY_IOS)
        GameOfWhales.Instance.InAppPurchased(
                                            args.purchasedProduct.definition.id,
                                            (float)args.purchasedProduct.metadata.localizedPrice,
                                            args.purchasedProduct.metadata.isoCurrencyCode,
                                            args.purchasedProduct.transactionID,
                                            args.purchasedProduct.receipt
                                            );


        Debug.Log("Receipt: " + args.purchasedProduct.receipt);
        IAPResult(args.purchasedProduct.definition.id, true);
#endif
        return PurchaseProcessingResult.Complete;
    }

#if (UNITY_ANDROID || UNITY_IOS)
    private void OnPurchaseVerifiedCallback(string transactionID, string state)
    {
        print("OnPurchaseVerifiedCallback -> transactionID: " + transactionID + " -> state: " + state);

        if (state == "VERIFY_STATE_LEGAL")
            print("Purchase was legal.");
        else if (state == "VERIFY_STATE_ILEGAL")
            print("Purchase was illegal.");
        else if (state == "VERIFY_STATE_UNDEFINED")
            print("Purchase was undefined.");
    }

    private void OnOfferAppearedCallback(SpecialOffer offer)
    {
        if (offer.HasPriceFactor())
            ShopManager.Instance.UpdateDisplayedShopItemsPriceFactor(offer.product, offer.priceFactor, true);
    }

    private void OnOfferDisappearedCallback(SpecialOffer offer)
    {
        if (offer.HasPriceFactor())
            ShopManager.Instance.UpdateDisplayedShopItemsPriceFactor(offer.product, offer.priceFactor, false);
    }

    private void OnPushDeliveredCallback(SpecialOffer offer, string campID, string title, string message)
    {
        //Show the notification to a player and then call the following method
        GameOfWhales.Instance.PushReacted(campID);
    }
#endif
    #endregion

    #region TapJoy
    public void InitPlacements()
    {
        GameObject tapJoyObjectPrefab = Resources.Load<GameObject>("Prefabs/TapjoyUnity");
        GameObject tapjoyObject = Instantiate<GameObject>(tapJoyObjectPrefab);
        tapjoyObject.name = tapJoyObjectPrefab.name;
        if (tapjoyObject != null)
            Debug.Log("Tapjoy object successfully created: " + tapjoyObject.ToString());
        else
            Debug.Log("Tapjoy object was not created.");
    }

    public void PreloadOfferWall()
    {
        StartCoroutine(handleTapJoyPreloading());
    }

    public void ShowOfferwall()
    {
        StartCoroutine(handleTapJoyShowContent());
    }

    private IEnumerator handleTapJoyPreloading()
    {
        while (true)
        {
            if (Tapjoy.IsConnected)
            {
                Debug.Log("Tapjoy is connected.");

                // Now that we are connected we can start preloading our placements
                _tapJoyPlacement = TJPlacement.CreatePlacement("Offerwall");
                _tapJoyPlacement.RequestContent();

                yield break;
            }
            else
            {
               // Debug.LogWarning("Tapjoy SDK must be connected before you can request content.");
                yield return 0;
            }
        }
    }

    private IEnumerator handleTapJoyShowContent()
    {
        while (true)
        {
            if ((_tapJoyPlacement != null) && _tapJoyPlacement.IsContentReady())
            {
                Debug.Log("Show content");
                _tapJoyPlacement.ShowContent();
                PreloadOfferWall();
                yield break;
            }
            else
            {
                // Code to handle situation where content is not ready goes here
                //Debug.Log("Content wasn't ready");
                yield return 0;
            }
        }
    }

    // Connect success
    private void handleConnectSuccess()
    {
        Debug.Log("Connect Success");

        //PlayerPrefs.DeleteKey(_TAPJOY_PLAYER_PREF_KEY); //TODO: REMOVE THIS TEST HACK
        _tapJoyUserId = PlayerPrefs.GetString(_TAPJOY_PLAYER_PREF_KEY, "");
        if (_tapJoyUserId == "")
        {
            _tapJoyUserId = createRandomTapJoyId();
            PlayerPrefs.SetString(_TAPJOY_PLAYER_PREF_KEY, _tapJoyUserId);
        }

        Tapjoy.SetUserID(_tapJoyUserId);
    }

    public void HandlePlacementRequestSuccess(TJPlacement placement)
    {
        print("HandlePlacementRequestSuccess " + placement.ToString());
    }

    public void HandlePlacementRequestFailure(TJPlacement placement, string error)
    {
        print("HandlePlacementRequestFailure " + placement.ToString() + " error: " + error.ToString());
    }

    public void HandlePlacementContentReady(TJPlacement placement)
    {
        // This gets called when content is ready to show.
        print("HandlePlacementContentReady " + placement.ToString());
    }

    public void HandlePlacementContentShow(TJPlacement placement)
    {
        print("HandlePlacementContentShow " + placement.ToString());
    }

    public void HandlePlacementContentDismiss(TJPlacement placement)
    {
        print("HandlePlacementContentDismiss " + placement.ToString());

        Hashtable hashtable = new Hashtable();
        hashtable.Add("user", _tapJoyUserId);
        NetworkManager.Transaction(NetworkManager.Transactions.COINS_EARNED, hashtable, onCoinsEarned);
    }

    //public void HandleOnRewardRequest(TJPlacement placement, TJActionRequest request, string itemId, int quantity)
    //{
    //    print("HandleOnRewardRequest itemId: " + itemId + " quantity: " + quantity);
    //}

    private void handleEarnedCurrency(string currencyName, int amount)
    {
        Debug.Log("C#: HandleEarnedCurrency: currencyName: " + currencyName + ", amount: " + amount);
    }

    private void onTapJoySetUserIdSuccess()
    {
        print("onTapJoySetUserIdSuccess");
    }

    private void onTapJoySetUserIdFailure(string errorMessage)
    {
        print("onTapJoySetUserIdFailure: " + errorMessage);
    }

    private string createRandomTapJoyId()
    {
        string id = "";

        for (int i = 1; i <= _TAPJOY_ID_DIGITS_AMOUNT; i++)
        {
            id += UnityEngine.Random.Range(0, 10);
            if ((i != _TAPJOY_ID_DIGITS_AMOUNT) && (i % _TAPJOY_ID_BLOCK_SIZE) == 0)
                id += "-";
        }

        print("TapJoyId: " + id);

        return id;
    }

    private void onCoinsEarned(JSONNode response)
    {
        if (response != null)
        {
            Debug.Log("onCoinsEarned: " + response.ToString());

            JSONNode response_hash = response[0];
            string status = response_hash["status"].ToString().Trim('"');

            if (status == "SUCCESS")
            {
                int coinsEarned = response_hash["coins"].AsInt;
                if (OnCoinsEarned != null)
                {
                    if(coinsEarned > 0)
                        OnCoinsEarned(coinsEarned);
                }

                if (OnHideTransactionError != null)
                    OnHideTransactionError();
            }
            else if(OnTransactionError != null)
                OnTransactionError("ServerError");
        }
        else if (OnTransactionError != null)
            OnTransactionError("ConnectionError");
    }
    #endregion
}
