
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enigma.CoreSystems;
using SimpleJSON;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

public delegate void IAPManagerPurchaseCompleteCallback();

public class IAPManager : Manageable<IAPManager>, IStoreListener 
{
    public string APPLE_IOS_KEY = "";
    public string ANDROID_GOOGLE_PLAY_KEY = "";
    public string SAMSUNG_GROUP_ID = "";
    public string FORTUMO_SEC = "";
    public string FORTUMO_SERVICE = "";
    public string[] IAP_IDS;

    public string Id;

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

#if UNITY_IOS
        string store = GameOfWhales.STORE_APPLEAPPSTORE;
#else
            string store = GameOfWhales.STORE_GOOGLEPLAY;
#endif
            // TODO: Restore when Game of Whales is enabled======
           // GameOfWhales.Init(store);  
         //   GameOfWhales.SetPushNotificationsEnable(true);
            //===================================================
        }

       /* GameOfWhales.OnPurchaseVerified += OnPurchaseVerifiedCallback;
        GameOfWhales.OnSpecialOfferAppeared += OnOfferAppearedCallback;
        GameOfWhales.OnSpecialOfferedDisappeared += OnOfferDisappearedCallback;
        GameOfWhales.OnPushDelivered += OnPushDeliveredCallback;*/
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

        // TODO: Restore when GameOfWhales can be used =========================
        //if (GameOfWhales.Instance != null)
        //{
        //    GameOfWhales.OnPurchaseVerified -= OnPurchaseVerifiedCallback;
        //    GameOfWhales.OnSpecialOfferAppeared -= OnOfferAppearedCallback;
        //    GameOfWhales.OnSpecialOfferedDisappeared -= OnOfferDisappearedCallback;
        //    GameOfWhales.OnPushDelivered -= OnPushDeliveredCallback;
        //}
        //=====================================================================


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
        IAPManager instance = IAPManager.Instance;
        string id = instance.Id;
        string sec = instance.FORTUMO_SEC;
        string service =  instance.FORTUMO_SERVICE;

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
        GameOfWhales.Consume(currency, number, sink, amount, place);
#endif
    }

    public virtual void RegisterAcquire(string currency, long amount, string source, long number, string place)
    {
#if (UNITY_ANDROID || UNITY_IOS)
        GameOfWhales.Consume(currency, amount, source, number, place);
#endif
    }

    public float GetOfferPriceFactor(string product)
    {
        float priceFactor = 1;

#if (UNITY_ANDROID || UNITY_IOS)
        SpecialOffer offer = GameOfWhales.GetSpecialOffer(product);

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
        bool validPurchase = true; // Presume valid for platforms with no R.V.

        if (!EnigmaHacks.Instance.ByPassIAPReceiptCheck)
        {
            string purchaseToken = "";
            string transactionId = "";
            string productId = "";

            // Unity IAP's validation logic is only included on these platforms.
#if (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX)
            // Prepare the validator with the secrets we prepared in the Editor
            // obfuscation window.
            CrossPlatformValidator validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            

            try
            {
                // On Google Play, result has a single product ID.
                // On Apple stores, receipts contain multiple products.
                IPurchaseReceipt[] result = validator.Validate(args.purchasedProduct.receipt);
                // For informational purposes, we list the receipt(s)
                Debug.Log("Receipt is valid. Contents:");
                foreach (IPurchaseReceipt productReceipt in result)
                {
                    Debug.Log(productReceipt.productID);
                    Debug.Log(productReceipt.purchaseDate);
                    Debug.Log(productReceipt.transactionID);

                    GooglePlayReceipt google = productReceipt as GooglePlayReceipt;
                    if (null != google)
                    {
                        // This is Google's Order ID.
                        // Note that it is null when testing in the sandbox
                        // because Google's sandbox does not provide Order IDs.
                        Debug.Log(google.transactionID);
                        Debug.Log(google.purchaseState);
                        Debug.Log(google.purchaseToken);
                    }

                    AppleInAppPurchaseReceipt apple = productReceipt as AppleInAppPurchaseReceipt;
                    if (null != apple)
                    {
                        Debug.Log(apple.originalTransactionIdentifier);
                        Debug.Log(apple.subscriptionExpirationDate);
                        Debug.Log(apple.cancellationDate);
                        Debug.Log(apple.quantity);
                    }

                    purchaseToken = google.purchaseToken;
                    transactionId = productReceipt.transactionID;
                    productId = productReceipt.productID;

                    if(!args.purchasedProduct.definition.id.Contains(Application.identifier)) validPurchase = false;
                }
            }
            catch (IAPSecurityException)
            {
                Debug.Log("Invalid receipt, not unlocking content");
                validPurchase = false;
            }
#endif

            // Unlock the appropriate content here.
#if (UNITY_ANDROID || UNITY_IOS)
            if (validPurchase)
            {

                /*GameOfWhales.InAppPurchased(
                                                    args.purchasedProduct.definition.id,
                                                    (float)args.purchasedProduct.metadata.localizedPrice,
                                                    args.purchasedProduct.metadata.isoCurrencyCode,
                                                    args.purchasedProduct.transactionID,
                                                    args.purchasedProduct.receipt
                                                    );
*/

                Debug.Log("Receipt: " + args.purchasedProduct.receipt);

                Hashtable hashtable = new Hashtable();
                hashtable.Add("receipt", args.purchasedProduct.receipt);
                hashtable.Add("transaction_id", args.purchasedProduct.transactionID);
                hashtable.Add("product_id", args.purchasedProduct.definition.id);
                NetworkManager.Transaction(NetworkManager.Transactions.PURCHASE_SUCCESSFUL, hashtable, onPurchaseSuccesfulTransaction);
            } else
            {
                Hashtable hashtable = new Hashtable();
                hashtable.Add("receipt", args.purchasedProduct.receipt);
                hashtable.Add("transaction_id", args.purchasedProduct.transactionID);
                hashtable.Add("product_id", args.purchasedProduct.definition.id);
                hashtable.Add("results", "fail");
                NetworkManager.Transaction(NetworkManager.Transactions.PURCHASE_SUCCESSFUL, hashtable, onPurchaseSuccesfulTransaction);
            }
        }

        IAPResult(args.purchasedProduct.definition.id, validPurchase);

#endif

        return PurchaseProcessingResult.Complete;
    }

    private void onPurchaseSuccesfulTransaction(JSONNode response)
    {
        if (response != null)
        {

            JSONNode response_hash = response[0];
            string status = response_hash["status"].ToString().Trim('"');

            print("onPurchaseSuccesfulTransaction -> response: "  + response.ToString() + " status: " + status);

            if (status == "SUCCESS")
            {               
                //handleLogin(response_hash);
            }
            else
            {
                //string term = "";

                //if (status == "ERR_REGISTER")
                //    term = "RegisterError";
                //else if (status == "ERR_INVALID_PASSWORD")
                //    term = "InvalidPassword";
                //else if (status == "ERR_INVALID_USERNAME")
                //    term = "InvalidUsername";
                //else
                //    term = "ServerError";

                //enableLoginRegisterUI();
                //GameLogicRef.DisplayErrorText(term);
            }
        }
        else
        {
            //enableLoginRegisterUI();
            //GameLogicRef.DisplayErrorText("ConnectionError");
            Debug.LogError("onPurchaseSuccesfulTransaction: CONNECTION ERROR"); 
        }
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
        GameOfWhales.PushReacted(campID);
    }
#endif
    #endregion

    
}
