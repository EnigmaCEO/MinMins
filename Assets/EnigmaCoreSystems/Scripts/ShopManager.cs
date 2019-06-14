using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public delegate void OnItemPurchaseConfirmedDelegate(ShopItem item);
    public OnItemPurchaseConfirmedDelegate OnItemPurchaseConfirmedCallback;

    public delegate void OnCurrencyPackPurchaseConfirmedDelegate(string currencyPackId);
    public OnCurrencyPackPurchaseConfirmedDelegate OnCurrencyPackPurchaseConfirmedCallback;

    static public ShopManager Instance;

    [HideInInspector] public PurchaseResultWindow MyPurchaseResultWindow;

    [SerializeField] private string _purchaseResultWindowFindPath = "Canvas/PopUps/PurchaseResultWindow";
    [SerializeField] private string _confirmItemPurchaseWindowFindPath = "Canvas/PopUps/ConfirmItemPurchaseWindow";

    [SerializeField] private string _openShopButtonFindPath = "Canvas/Layout/GameHud/PowersBackground/OpenShopButton";
    [SerializeField] private string _saleIconFindPath = "Canvas/Layout/GameHud/PowersBackground/OpenShopButton/SaleIcon";
    [SerializeField] private string _closeShopButtonFindPath = "Canvas/Layout/GameHud/PowersBackground/CloseShopButton";

    [SerializeField] private string _shopUiFindPath = "Canvas/Layout/Shop";

    [SerializeField] private string _itemsUiFindSubPath = "Items";
    [SerializeField] private string _currencyUiFindSubPath = "Currency";

    [SerializeField] private string _itemsContentTransformFindSubPath = "Items/ScrollViewVertical/Viewport/Content";
    [SerializeField] private string _currencyContentTransformFindSubPath = "Currency/ScrollViewVertical/Viewport/Content";

    [SerializeField] private string _openCurrencyButtonFindSubPath = "TitleFrame/CurrencyFrame/OpenCurrency";
    [SerializeField] private string _closeCurrencyButtonFindSubPath = "TitleFrame/CurrencyFrame/CloseCurrency";

    private ConfirmItemPurchaseWindow _confirmItemPurchaseWindow;

    private GameObject _itemsUI;
    private GameObject _currencyUI;

    private Transform _itemsContentTransform;
    private Transform _currencyContentTransform;

    private Button _openShopButton;
    private GameObject _saleIcon;
    private Button _closeShopButton;

    private Button _openCurrencyButton;
    private Button _closeCurrencyButton;

    private GameObject _shopUI;

    private GameObject _shopItemPrefab;
    private GameObject _currencyPackPrefab;

    private Dictionary<string, int> _currencyPackAmountById = new Dictionary<string, int>();


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        IAPManager.IAPResult += handleCurrencyBuyResult;
    }

    private void Update()
    {
        if ((Application.platform == RuntimePlatform.WindowsEditor) && Input.GetKeyDown(KeyCode.S))
        {
            IAPManager.Instance.IsHacked = !IAPManager.Instance.IsHacked;

            UpdateDisplayedShopItemsPriceFactor(GameEnums.Powers.Fire.ToString().ToLower());
        }
    }

    private void OnDestroy()
    {
        if (_confirmItemPurchaseWindow != null)
            _confirmItemPurchaseWindow.OnItemPurchaseCallback -= onItemPurchaseConfirmed;

        IAPManager.IAPResult -= handleCurrencyBuyResult;
    }

    public void SetUI()
    {
        _shopUI = GameObject.Find(_shopUiFindPath);

        _itemsContentTransform = findShopUI<Transform>(_itemsContentTransformFindSubPath);
        _shopItemPrefab = _itemsContentTransform.GetChild(0).gameObject;
        _shopItemPrefab.SetActive(false);

        _currencyContentTransform = findShopUI<Transform>(_currencyContentTransformFindSubPath);
        _currencyPackPrefab = _currencyContentTransform.GetChild(0).gameObject;
        _currencyPackPrefab.SetActive(false);

        _openShopButton = findUI<Button>(_openShopButtonFindPath);
        if (_openShopButton != null)
            _openShopButton.onClick.AddListener(() => { onOpenShopButtonDown(); });

        _saleIcon = findUI<Transform>(_saleIconFindPath).gameObject;
        updateSaleIconVisibility();

        _closeShopButton = findUI<Button>(_closeShopButtonFindPath);
        if (_closeShopButton != null)
            _closeShopButton.onClick.AddListener(() => { onCloseShopButtonDown(); });

        _itemsUI = findShopUI<Transform>(_itemsUiFindSubPath).gameObject;

        _currencyUI = findShopUI<Transform>(_currencyUiFindSubPath).gameObject;
        if (_currencyUI != null)
            _currencyUI.gameObject.SetActive(false);

        _openCurrencyButton = findShopUI<Button>(_openCurrencyButtonFindSubPath);
        if (_openCurrencyButton != null)
            _openCurrencyButton.onClick.AddListener(() => { onOpenCurrencyButtonDown(); });

        _closeCurrencyButton = findShopUI<Button>(_closeCurrencyButtonFindSubPath);
        if (_closeCurrencyButton != null)
        {
            _closeCurrencyButton.onClick.AddListener(() => { onCloseCurrencyButtonDown(); });
            _closeCurrencyButton.gameObject.SetActive(false);
        }

        _confirmItemPurchaseWindow = findUI<ConfirmItemPurchaseWindow>(_confirmItemPurchaseWindowFindPath);
        if (_confirmItemPurchaseWindow != null)
        {
            _confirmItemPurchaseWindow.OnItemPurchaseCallback += onItemPurchaseConfirmed;
            _confirmItemPurchaseWindow.Close();
        }

        MyPurchaseResultWindow = findUI<PurchaseResultWindow>(_purchaseResultWindowFindPath);
        if (MyPurchaseResultWindow != null)
            MyPurchaseResultWindow.Close();

        closeShop();
    }

    public void UpdateDisplayedShopItemsPriceFactor(string product)
    {
        float offerPriceFactor = IAPManager.Instance.GetOfferPriceFactor(product);
        UpdateDisplayedShopItemsPriceFactor(product, offerPriceFactor, true);
    }

    public void UpdateDisplayedShopItemsPriceFactor(string product, float offerPriceFactor, bool enable)
    {
        ShopItem firstShopItem = _itemsContentTransform.GetChild(1).GetComponent<ShopItem>();
        float priceFactor = 1;

        string itemsListProduct = firstShopItem.Power.ToString().ToLower();
        if (itemsListProduct == product)
        {
            if(enable)
                priceFactor = offerPriceFactor;
        }

        int childCount = _itemsContentTransform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            if (i > 0)  //0 is Prefab
                _itemsContentTransform.GetChild(i).GetComponent<ShopItem>().SetPriceFactor(priceFactor);
        }
        

        string baseItemDealDescriptionTerm = "";
        string baseItemValueDescriptionSuffix = "";

        if (priceFactor != 1)
        {
            baseItemDealDescriptionTerm = "BasePriceOffer";
            baseItemValueDescriptionSuffix = " *-" + Mathf.RoundToInt(priceFactor * 100) + "%*";
        }

        firstShopItem.SetDealDescriptionTerm(baseItemDealDescriptionTerm);
        firstShopItem.SetCurrencyValueDescriptionSuffix(baseItemValueDescriptionSuffix);

        updateSaleIconVisibility();
    }

    public void AddShopItem(GameEnums.Powers power, int powerUpAmount, int currencyValue, string valueDescriptionSuffix, string dealDescriptionTerm = "", string iconResourcePath = "")
    {
        ShopItem shopItem = Instantiate<GameObject>(_shopItemPrefab, _itemsContentTransform).GetComponent<ShopItem>();

        shopItem.Power = power;

        shopItem.SetPowerUpAmount(powerUpAmount);
        shopItem.SetCurrencyValueDescriptionSuffix(valueDescriptionSuffix);
        shopItem.SetCurrencyValue(currencyValue);
        shopItem.SetDealDescriptionTerm(dealDescriptionTerm);

        if(iconResourcePath != "")
            shopItem.SetIconSprite(Resources.Load<Sprite>(iconResourcePath));

        shopItem.GetComponent<Button>().onClick.AddListener(() => { onItemButtonDown(shopItem); });

        shopItem.gameObject.SetActive(true);
    }

    public void ClearShopItems()
    {
        Transform prefabTransform = _itemsContentTransform.GetChild(0);
        prefabTransform.SetParent(null);

        _itemsContentTransform.DestroyChildren();

        prefabTransform.SetParent(_itemsContentTransform);
    }

    public void AddCurrencyPack(string id, int currencyAmount, string dealPercentage, float dollarValue, string dealDescriptionTerm = "", string descriptionLocalizeTerm = "")
    {
        CurrencyPack currencyPack = Instantiate<GameObject>(_currencyPackPrefab, _currencyContentTransform).GetComponent<CurrencyPack>();

        currencyPack.SetId(id);
        currencyPack.SetCurrencyAmount(currencyAmount, dealPercentage);
        currencyPack.SetDollarValue(dollarValue);
        currencyPack.SetDealDescriptionTerm(dealDescriptionTerm);

        if(descriptionLocalizeTerm != "")
            currencyPack.SetLocalizeTerm(descriptionLocalizeTerm);

        currencyPack.GetComponent<Button>().onClick.AddListener(() => { onCurrencyPackButtonDown(currencyPack); });

        currencyPack.gameObject.SetActive(true);

        _currencyPackAmountById.Add(id, currencyPack.GetCurrencyAmountInt());
    }

    public int GetCurrencyValueFromPackId(string packId)
    {
        int currencyValue = 0;

        foreach (CurrencyPack currencyPack in _currencyContentTransform.GetComponentsInChildren<CurrencyPack>())
        {
            if (packId == currencyPack.GetId())
            {
                currencyValue = currencyPack.GetCurrencyAmountInt();
                break;
            }
        }

        return currencyValue;
    }

    private void updateSaleIconVisibility()
    {
        bool visible = false;
        foreach (GameEnums.Powers power in Enum.GetValues(typeof(GameEnums.Powers)))
        {
            if (IAPManager.Instance.GetOfferPriceFactor(power.ToString().ToLower()) != 1)
            {
                visible = true;
                break;
            }
        }

        _saleIcon.SetActive(visible);
    }

    private void openShop()
    {
        if(_openShopButton != null)
            _openShopButton.gameObject.SetActive(false);

        if(_closeCurrencyButton != null)
            _closeShopButton.gameObject.SetActive(true);

        if(_shopUI)
            _shopUI.SetActive(true);
    }

    private void closeShop()
    {
        if(_shopUI != null)
            _shopUI.SetActive(false);

        if(_closeShopButton != null)
            _closeShopButton.gameObject.SetActive(false);

        if(_openShopButton != null)
            _openShopButton.gameObject.SetActive(true);
    }

    private void onOpenShopButtonDown()
    {
        openShop();
    }

    private void onCloseShopButtonDown()
    {
        closeShop();
    }

    private void onOpenCurrencyButtonDown()
    {
        if(_openCurrencyButton != null)
            _openCurrencyButton.gameObject.SetActive(false);

        if(_closeCurrencyButton != null)
            _closeCurrencyButton.gameObject.SetActive(true);

        if(_itemsUI != null)
            _itemsUI.SetActive(false);

        if(_currencyUI != null)
            _currencyUI.SetActive(true);
    }

    private void onCloseCurrencyButtonDown()
    {
        if(_closeCurrencyButton != null)
            _closeCurrencyButton.gameObject.SetActive(false);

        if(_openCurrencyButton != null)
            _openCurrencyButton.gameObject.SetActive(true);

        if(_currencyUI != null)
            _currencyUI.SetActive(false);

        if(_itemsUI != null)
            _itemsUI.SetActive(true);
    }

    private void onItemButtonDown(ShopItem selectedItem)
    {
        if(_confirmItemPurchaseWindow != null)
            _confirmItemPurchaseWindow.Open(selectedItem);
    }

    private void onCurrencyPackButtonDown(CurrencyPack currencyPack)
    {
        IAPManager iapManager = IAPManager.Instance;
        int count = iapManager.IAP_IDS.Length;
        for (int i = 0; i < count; i++)
        {
            string id = iapManager.IAP_IDS[i];
            if (currencyPack.GetId() == id)
                IAPManager.BuyConsumable(i);
        }
    }

    private void onItemPurchaseConfirmed(ShopItem shopItem)
    {
        if (OnItemPurchaseConfirmedCallback != null)
            OnItemPurchaseConfirmedCallback(shopItem);

        IAPManager.Instance.RegisterConsume("coins", (long)shopItem.GetCurrencyValueInt(), shopItem.Power.ToString().ToLower(), (long)shopItem.GetPowerUpAmount(), "shop");
    }

    private void handleCurrencyBuyResult(string id, bool result)
    {
        if (result)
        {
            if(MyPurchaseResultWindow != null)
                MyPurchaseResultWindow.Open("ThanksPurchase");

            IAPManager.Instance.RegisterAcquire("coins", (long)_currencyPackAmountById[id], id, 1, "shop");

            if (OnCurrencyPackPurchaseConfirmedCallback != null)
                OnCurrencyPackPurchaseConfirmedCallback(id);
        }
        else if (MyPurchaseResultWindow != null)
            MyPurchaseResultWindow.Open("PurchaseFailed");
    }

    private T findShopUI<T>(string uiName) where T : Component
    {
        return findUI<T>(_shopUiFindPath, uiName);
    }

    private T findUI<T>(string findPath, string uiName) where T : Component
    {
        return findUI<T>(findPath + "/" + uiName);
    }

    private T findUI<T>(string findPath) where T : Component
    {
        return GameObject.Find(findPath).GetComponent<T>();
    }
}
