using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStore : MonoBehaviour
{
    [SerializeField] private int _maxBoxTierNumber = 3;

    [SerializeField] private List<string> _confirmPopUpPackNames = new List<string>() { "STARTER BOX", "PREMIUM BOX", "MASTER BOX" };  
    [SerializeField] private List<string> _confirmPopUpPackDescriptions = new List<string>() { "No guarantees.", "Guarantees a Silver Unit.", "Guarantees a Gold Unit." };

    [SerializeField] private List<Image> _confirmPopUpPackImages;
    [SerializeField] private List<string> _confirmPopUpPackPrices = new List<string>() { "0.99", "1.99", "4.99" };

    [SerializeField] private List<int> _packTiers = new List<int> { 1, 2, 3, 3 };

    [SerializeField] private LootBoxBuyConfirmPopUp _lootBoxBuyConfirmPopUp;
    [SerializeField] private OpenLootBoxPopUp _openLootBoxPopUp;
    [SerializeField] private BuyResultPopUp _buyResultPopUp;
    [SerializeField] private GameObject _extraLootBoxPopUp;

    [SerializeField] private Transform _lootBoxGridContent;

    private int _selectedPackIndex;

    private void Awake()
    {
        IAPManager.IAPResult += handleCurrencyBuyResult;
    }

    void Start()
    {
        _lootBoxBuyConfirmPopUp.ConfirmButton.onClick.AddListener(() => onLootBoxBuyConfirmButtonDown());
        _lootBoxBuyConfirmPopUp.CancelButton.onClick.AddListener(() => onLootBoxBuyCancelButtonDown());
        _lootBoxBuyConfirmPopUp.Close();

        _openLootBoxPopUp.Close();
        _buyResultPopUp.Close();

        _extraLootBoxPopUp.SetActive(false);

        refreshLootBoxesGrid();

        handleFreeLootBoxGifts();
    }

    private void OnDestroy()
    {
        _lootBoxBuyConfirmPopUp.ConfirmButton.onClick.RemoveListener(() => onLootBoxBuyConfirmButtonDown());
        _lootBoxBuyConfirmPopUp.CancelButton.onClick.RemoveListener(() => onLootBoxBuyCancelButtonDown());

        IAPManager.IAPResult -= handleCurrencyBuyResult;
    }

    private void handleCurrencyBuyResult(string id, bool result)
    {
        //result = false; //Hack
        if (result)
        {
            IAPManager iapManager = IAPManager.Instance;
            int idsCount = iapManager.IAP_IDS.Length;
            int boxTierIndex = -1;
            for (int i = 0; i < idsCount; i++)
            {
                if (iapManager.IAP_IDS[i] == id)
                {
                    boxTierIndex = i;
                    break;
                }
            }

            int boxTier = _packTiers[boxTierIndex];
            grantBox(boxTier, 1);
            _buyResultPopUp.Open("Thanks for your Purchase!");
        }
        else
            _buyResultPopUp.Open("Purchase Failed. Please try again later.");
    }

    public void OnPackBuyButtonDown(int packIndex)
    {
        _selectedPackIndex = packIndex;

        _lootBoxBuyConfirmPopUp.SetPackName(_confirmPopUpPackNames[packIndex]);
        _lootBoxBuyConfirmPopUp.SetPackDescription(_confirmPopUpPackDescriptions[packIndex]);
        _lootBoxBuyConfirmPopUp.SetStars(_packTiers[packIndex]);
        _lootBoxBuyConfirmPopUp.SetPackSprite(_confirmPopUpPackImages[packIndex].sprite);
        _lootBoxBuyConfirmPopUp.SetPrice(_confirmPopUpPackPrices[packIndex]);
        _lootBoxBuyConfirmPopUp.gameObject.SetActive(true);
    }

    public void OnLootBoxOpenPopUpDismissButtonDown()
    {
        _openLootBoxPopUp.Close();
        handleFreeLootBoxGifts();
    }

    public void OnBuyResultPopUpDismissButtonDown()
    {
        _buyResultPopUp.Close();
    }

    public void onExtraLootBoxPopUpDismissButtonDown()
    {
        grantBox(GameInventory.Tiers.BRONZE, 2);
        _extraLootBoxPopUp.SetActive(false);
    }

    public void OnBackButtonDown()
    {
        SceneManager.LoadScene(GameConstants.Scenes.MAIN);
    }

    private void handleFreeLootBoxGifts()
    {
        GameInventory gameInventory = GameInventory.Instance;
        if (!gameInventory.HasEnoughUnitsForBattle() && !gameInventory.HasAnyLootBox())
        {
            //grantBox(GameInventory.Tiers.BRONZE, 2);
            _extraLootBoxPopUp.SetActive(true);
        }
    }

    private void refreshLootBoxesGrid()
    {
        GameObject boxGridItemTemplate = _lootBoxGridContent.GetChild(0).gameObject;
        boxGridItemTemplate.SetActive(true);

        foreach (Transform child in _lootBoxGridContent)
        {
            if (child.gameObject != boxGridItemTemplate)
                Destroy(child.gameObject);
        }

        for (int tier = 1; tier <= _maxBoxTierNumber; tier++)
        {
            int tierAmount = GameInventory.Instance.GetLootBoxTierAmount(tier);  //TODO: Check if this needs to return stats
            for (int i = 0; i < tierAmount; i++)
            {
                Transform unitTransform = Instantiate<GameObject>(boxGridItemTemplate, _lootBoxGridContent).transform;
                unitTransform.name = "BoxGridItem_Tier " + tier;
                BoxGridItem box = unitTransform.GetComponent<BoxGridItem>();
                box.SetUp(tier);
                int tierCopy = tier;
                box.OpenButton.onClick.AddListener(() => onLootBoxOpenButtonDown(tierCopy));
            }
        }

        boxGridItemTemplate.SetActive(false);
    }

    public void BuyMasterBoxOffer()
    {
        _selectedPackIndex = 3;
        onLootBoxBuyConfirmButtonDown();
        _extraLootBoxPopUp.SetActive(false);
    }

    private void onLootBoxBuyConfirmButtonDown()
    {
        //===============================================
        //IAPManager.BuyConsumable(packIndex);  
        handleCurrencyBuyResult(IAPManager.Instance.IAP_IDS[_selectedPackIndex], true);  // Buy hack to work on android
        //=========================================
        _lootBoxBuyConfirmPopUp.Close();
        //grantBuy(_selectedPackTier); //TODO: Remove hack
    }

    public void onLootBoxBuyCancelButtonDown()
    {
        _lootBoxBuyConfirmPopUp.Close();
    }

    private void grantBox(int lootBoxTier, int amount)
    {
        GameInventory.Instance.ChangeLootBoxAmount(amount, lootBoxTier, true, true);
        refreshLootBoxesGrid();
    }

    private void onLootBoxOpenButtonDown(int lootBoxTier)
    {
        Dictionary<string, int> unitsWithTier = new Dictionary<string, int>();
        GameInventory gameInventory = GameInventory.Instance;

        List<string> lootBoxUnitNumbers = gameInventory.OpenLootBox(lootBoxTier);
        foreach (string unitName in lootBoxUnitNumbers)
            unitsWithTier.Add(unitName.ToString(), gameInventory.GetUnitTier(unitName));

        refreshLootBoxesGrid();

        _openLootBoxPopUp.Feed(unitsWithTier);
        _openLootBoxPopUp.Open();
    }
}
