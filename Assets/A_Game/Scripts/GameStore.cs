using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStore : MonoBehaviour
{
    [SerializeField] private int _maxBoxTierNumber = 3;

    [SerializeField] private List<string> _packNames;
    [SerializeField] private List<string> _packDescriptions;

    [SerializeField] private List<Image> _packImages;

    [SerializeField] private LootBoxBuyConfirmPopUp _lootBoxBuyConfirmPopUp;
    [SerializeField] private OpenLootBoxPopUp _openLootBoxPopUp;

    [SerializeField] private Transform _lootBoxGridContent;

    private int _selectedPackTier;

    void Start()
    {
        _lootBoxBuyConfirmPopUp.ConfirmButton.onClick.AddListener(() => onLootBoxBuyConfirmButtonDown());
        _lootBoxBuyConfirmPopUp.CancelButton.onClick.AddListener(() => onLootBoxBuyCancelButtonDown());
        _lootBoxBuyConfirmPopUp.Close();

        _openLootBoxPopUp.Close();

        refreshLootBoxesGrid();
    }

    private void OnDestroy()
    {
        _lootBoxBuyConfirmPopUp.ConfirmButton.onClick.RemoveListener(() => onLootBoxBuyConfirmButtonDown());
        _lootBoxBuyConfirmPopUp.CancelButton.onClick.RemoveListener(() => onLootBoxBuyCancelButtonDown());
    }

    public void OnPackBuyButtonDown(int tier)
    {
        _selectedPackTier = tier;
        int packIndex = tier - 1;

        _lootBoxBuyConfirmPopUp.SetPackName(_packNames[packIndex]);
        _lootBoxBuyConfirmPopUp.SetPackDescription(_packDescriptions[packIndex]);
        _lootBoxBuyConfirmPopUp.SetStars(tier);
        _lootBoxBuyConfirmPopUp.SetPackSprite(_packImages[packIndex].sprite);
        _lootBoxBuyConfirmPopUp.gameObject.SetActive(true);
    }

    public void OnLootBoxOpenPopUpDismissButtonDown()
    {
        _openLootBoxPopUp.Close();
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
                box.SetTier(tier);
                int tierCopy = tier;
                box.OpenButton.onClick.AddListener(() => onLootBoxOpenButtonDown(tierCopy));
            }
        }

        boxGridItemTemplate.SetActive(false);
    }

    private void onLootBoxBuyConfirmButtonDown()
    {
        //TODO: Complete IAP Code
        onBuySuccessful(_selectedPackTier); //TODO: Remove hack
        _lootBoxBuyConfirmPopUp.Close();
    }

    private void onLootBoxBuyCancelButtonDown()
    {
        _lootBoxBuyConfirmPopUp.Close();
    }

    private void onBuySuccessful(int lootBoxTier)
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
