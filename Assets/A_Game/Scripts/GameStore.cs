using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStore : MonoBehaviour
{
    [SerializeField] private int _maxBoxTierNumber = 3;

    [SerializeField] private List<string> _packNames = new List<string>() { "Bronze Pack", "Silver Pack", "Gold Pack" };  
    [SerializeField] private List<string> _packDescriptions = new List<string>() { "No guarantees.", "Guarantees a Silver Unit.", "Guarantees a Gold Unit." };

    [SerializeField] private List<Image> _packImages;
    [SerializeField] private List<string> _packPrices = new List<string>() { "5.00", "10.00", "15.00" };

    [SerializeField] private LootBoxBuyConfirmPopUp _lootBoxBuyConfirmPopUp;
    [SerializeField] private OpenLootBoxPopUp _openLootBoxPopUp;
    [SerializeField] private BuyResultPopUp _buyResultPopUp;

    [SerializeField] private Transform _lootBoxGridContent;

    private int _selectedPackTier;

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

        refreshLootBoxesGrid();
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

            int boxTier = boxTierIndex + 1;
            grantBuy(boxTier);
            _buyResultPopUp.Open("Thanks for your Purchase!");
        }
        else
            _buyResultPopUp.Open("Purchase Failed. Please try again later.");
    }

    public void OnPackBuyButtonDown(int tier)
    {
        _selectedPackTier = tier;
        int packIndex = tier - 1;

        _lootBoxBuyConfirmPopUp.SetPackName(_packNames[packIndex]);
        _lootBoxBuyConfirmPopUp.SetPackDescription(_packDescriptions[packIndex]);
        _lootBoxBuyConfirmPopUp.SetStars(tier);
        _lootBoxBuyConfirmPopUp.SetPackSprite(_packImages[packIndex].sprite);
        _lootBoxBuyConfirmPopUp.SetPrice(_packPrices[packIndex]);
        _lootBoxBuyConfirmPopUp.gameObject.SetActive(true);
    }

    public void OnLootBoxOpenPopUpDismissButtonDown()
    {
        _openLootBoxPopUp.Close();
    }

    public void OnBuyResultPopUpDismissButtonDown()
    {
        _buyResultPopUp.Close();
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

    private void onLootBoxBuyConfirmButtonDown()
    {
        int packIndex = _selectedPackTier - 1;
        IAPManager.BuyConsumable(packIndex);
        _lootBoxBuyConfirmPopUp.Close();
        //grantBuy(_selectedPackTier); //TODO: Remove hack
    }

    private void onLootBoxBuyCancelButtonDown()
    {
        _lootBoxBuyConfirmPopUp.Close();
    }

    private void grantBuy(int lootBoxTier)
    {
        GameInventory.Instance.ChangeLootBoxAmount(1, lootBoxTier, true, true);
        refreshLootBoxesGrid();
    }

    private void onLootBoxOpenButtonDown(int lootBoxTier)
    {
        Dictionary<string, int> unitsWithTier = new Dictionary<string, int>();
        GameInventory gameInventory = GameInventory.Instance;

        List<int> lootBoxUnitNumbers = gameInventory.OpenLootBox(lootBoxTier);
        foreach (int unitNumber in lootBoxUnitNumbers)
            unitsWithTier.Add(unitNumber.ToString(), gameInventory.GetUnitTier(unitNumber));

        refreshLootBoxesGrid();

        _openLootBoxPopUp.Feed(unitsWithTier);
        _openLootBoxPopUp.Open();
    }
}
