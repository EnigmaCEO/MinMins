using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using GameConstants;

public class GameStore : EnigmaScene
{
    [SerializeField] private int _maxBoxTierNumber = 3;

    [SerializeField] private List<string> _confirmPopUpPackNames = new List<string>() { "STARTER BOX", "PREMIUM BOX", "MASTER BOX", "SPECIAL BOX", "DEMON KING BOX", "LEGEND BOX" };
    [SerializeField] private List<string> _confirmPopUpPackDescriptions = new List<string>() { "No guarantees.", "Guarantees a Silver Unit.", "Guarantees a Gold Unit.", "Special Box", "Demon King description Term", "Legend Box description Term" };

    [SerializeField] private List<Image> _confirmPopUpPackImages;

#if HUAWEI
    [SerializeField] private List<string> _confirmPopUpPackPrices = new List<string>() { "99", "199", "499", "1000", "1000" };
#else
    [SerializeField] private List<string> _confirmPopUpPackPrices = new List<string>() { "0.99", "1.99", "4.99", "10.00", "10.00" };
#endif


    [SerializeField] private LootBoxBuyConfirmPopUp _lootBoxBuyConfirmPopUp;
    [SerializeField] private OpenLootBoxPopUp _openLootBoxPopUp;
    [SerializeField] private BuyResultPopUp _buyResultPopUp;
    [SerializeField] private BuyResultPopUp _summonResultPopUp;
    [SerializeField] private GameObject _extraLootBoxPopUp;
    [SerializeField] private GameObject _enjinmftPopUp;
    [SerializeField] private GameObject _minminPopUp;

    [SerializeField] Text _crystalsAmount;

    [SerializeField] private Transform _lootBoxGridContent;
    //[SerializeField] private Image giftProgress;
    //[SerializeField] private Text giftText;

    private int _selectedBoxIndex;

    override public void Awake()
    {
        IAPManager.IAPResult += handleCurrencyBuyResult;
    }

    void Start()
    {
        SoundManager.FadeCurrentSong(1f, () => {
            int shop = Random.Range(1, 3);
            SoundManager.Stop();
            SoundManager.Play("shop" + shop, SoundManager.AudioTypes.Music, "", true);
        });

        _lootBoxBuyConfirmPopUp.ConfirmButton.onClick.AddListener(() => onLootBoxBuyConfirmButtonDown());
        _lootBoxBuyConfirmPopUp.CancelButton.onClick.AddListener(() => onLootBoxBuyCancelButtonDown());
        _lootBoxBuyConfirmPopUp.Close();

        _openLootBoxPopUp.Close();
        _buyResultPopUp.Close();
        _summonResultPopUp.Close();

        _extraLootBoxPopUp.SetActive(false);
        _enjinmftPopUp.SetActive(false);
        _minminPopUp.SetActive(false);

#if HUAWEI
        _crystalsAmount.text = GameInventory.Instance.GetCrystalsAmount().ToString();
#else
        //_crystalsAmount.transform.parent.gameObject.SetActive(false);
#endif

        refreshLootBoxesGrid();

        //if (NetworkManager.LoggedIn)
        //{
        //    NetworkManager.Transaction(NetworkManager.Transactions.GIFT_PROGRESS, new Hashtable(), onGiftProgress);

        //} 
        //else
        //{
        //    GameObject.Find("gift_panel").gameObject.SetActive(false);
        //}

        if (!GameNetwork.Instance.IsEnjinLinked)
        {
            GameObject.Find("enjin_panel").gameObject.SetActive(false);
        }

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
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.BuyResult.Enabled)
        {
            result = GameHacks.Instance.BuyResult.ValueAsBool;
        }
#endif

        if (result)
        {
            IAPManager iapManager = IAPManager.Instance;
            int idsCount = iapManager.IAP_IDS.Length;
            int itemIndex = -1;
            for (int i = 0; i < idsCount; i++)
            {
                if (iapManager.IAP_IDS[i] == id)
                {
                    itemIndex = i;
                    break;
                }
            }

            if (itemIndex <= BoxIndexes.LEGEND)
            {
                //int boxTier = _packTiers[itemIndex];
                grantBox(itemIndex, 1);
            }

            _buyResultPopUp.Open("Thanks for your Purchase!");
        }
        else
        {
            _buyResultPopUp.Open("Purchase Failed. Please try again later.");
        }
    }

    public void OnPackBuyButtonDown(int boxIndex)
    {
        GameSounds.Instance.PlayUiAdvanceSound();

        _selectedBoxIndex = boxIndex;

        _lootBoxBuyConfirmPopUp.SetPackName(_confirmPopUpPackNames[boxIndex]);
        _lootBoxBuyConfirmPopUp.SetPackDescription(_confirmPopUpPackDescriptions[boxIndex], boxIndex);
        _lootBoxBuyConfirmPopUp.SetStars(GameInventory.Instance.GetPackTier(boxIndex));
        _lootBoxBuyConfirmPopUp.SetPackSprite(_confirmPopUpPackImages[boxIndex].sprite);
        _lootBoxBuyConfirmPopUp.SetPrice(_confirmPopUpPackPrices[boxIndex]);

        _lootBoxBuyConfirmPopUp.Open();
    }

    public void OnLootBoxOpenPopUpDismissButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        _openLootBoxPopUp.Close();
        handleFreeLootBoxGifts();
    }

    public void OnBuyResultPopUpDismissButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        _buyResultPopUp.Close();
    }

    public void OnExtraLootBoxPopUpDismissButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        grantBox(BoxIndexes.STARTER, 2);
        _extraLootBoxPopUp.SetActive(false);
    }

    public void OnEnjinPopUpSummonButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();

        if (GameInventory.Instance.GetEnjinAttempts() < 1)
        {
            return;
        }

        GameInventory.Instance.AddMissingEnjinUnits();

        _enjinmftPopUp.SetActive(false);
    }

    public void OnMinMinPopUpSummonButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        GameInventory.Instance.AddMinMinEnjinUnits();

        _minminPopUp.SetActive(false);
    }

    public void OpenEnjinPopUp()
    {
        GameSounds.Instance.PlayUiAdvanceSound();

        Transform enjinContent = _enjinmftPopUp.transform.Find("EnjinGrid/Viewport/Content");
        int count = enjinContent.childCount;
        GameInventory gameInventory = GameInventory.Instance;

        foreach (Transform enjinTransform in enjinContent)
        {
            string unitName = enjinTransform.name;
            if (gameInventory.HasUnit(unitName))
            {
                Debug.Log(unitName);
                enjinTransform.gameObject.SetActive(false);
                count--;
            }
        }

        if (count == 0 || !GameNetwork.Instance.HasEnjinMft)
        {
            _enjinmftPopUp.transform.Find("SummonButton").gameObject.SetActive(false);
        }

        int attempts = GameInventory.Instance.GetEnjinAttempts();

        _enjinmftPopUp.SetActive(true);

        if (GameNetwork.Instance.HasEnjinMft)
        {
            _enjinmftPopUp.transform.Find("WindowMessage").GetComponent<Text>().text = attempts + " " + LocalizationManager.GetTermTranslation("Summons remaining");
        }
        else
        {
            _enjinmftPopUp.transform.Find("WindowMessage").GetComponent<Text>().text = LocalizationManager.GetTermTranslation("Enjin MFT required");
        }
    }

    public void OpenMinMinPopUp()
    {
        GameSounds.Instance.PlayUiAdvanceSound();

        Transform enjinContent = _minminPopUp.transform.Find("EnjinGrid/Viewport/Content");
        int count = enjinContent.childCount;

        GameInventory gameInventory = GameInventory.Instance;
        GameNetwork gameNetwork = GameNetwork.Instance;

        foreach (Transform enjinTransform in enjinContent)
        {
            string unitName = enjinTransform.name;

            if (
                    handleLegendAvailability(unitName, "100", gameNetwork.HasEnjinMaxim) ||
                    handleLegendAvailability(unitName, "101", gameNetwork.HasEnjinWitek) ||
                    handleLegendAvailability(unitName, "102", gameNetwork.HasEnjinBryana) ||
                    handleLegendAvailability(unitName, "103", gameNetwork.HasEnjinTassio) ||
                    handleLegendAvailability(unitName, "104", gameNetwork.HasEnjinSimon) ||

                    handleLegendAvailability(unitName, "122", gameNetwork.HasEnjinAlex) ||
                    handleLegendAvailability(unitName, "123", gameNetwork.HasEnjinEvan) ||
                    handleLegendAvailability(unitName, "124", gameNetwork.HasEnjinEsther) ||
                    handleLegendAvailability(unitName, "125", gameNetwork.HasEnjinBrad) ||
                    handleLegendAvailability(unitName, "126", gameNetwork.HasEnjinLizz) ||

                    handleLegendAvailability(unitName, "105", gameNetwork.HasKnightTank) ||
                    handleLegendAvailability(unitName, "106", gameNetwork.HasKnightHealer) ||
                    handleLegendAvailability(unitName, "107", gameNetwork.HasKnightScout) ||
                    handleLegendAvailability(unitName, "108", gameNetwork.HasKnightDestroyer) ||
                    handleLegendAvailability(unitName, "109", gameNetwork.HasKnightBomber) ||

                    handleLegendAvailability(unitName, "110", gameNetwork.HasDemonBomber) ||
                    handleLegendAvailability(unitName, "111", gameNetwork.HasDemonScout) ||
                    handleLegendAvailability(unitName, "112", gameNetwork.HasDemonDestroyer) ||
                    handleLegendAvailability(unitName, "113", gameNetwork.HasDemonTank) ||
                    handleLegendAvailability(unitName, "114", gameNetwork.HasDemonHealer) ||

                    handleLegendAvailability(unitName, "127", gameNetwork.HasSwissborgCyborg)
                )
            {
                enjinTransform.gameObject.SetActive(false);
                count--;
            }
        }

        Debug.Log("count: " + count);
        if (count == 0 /*|| !GameNetwork.Instance.HasEnjinMinMinsToken*/)
        {
            _minminPopUp.transform.Find("SummonButton").gameObject.SetActive(false);
        }

        _minminPopUp.SetActive(true);

        //if (GameNetwork.Instance.HasEnjinMinMinsToken)
        {
            _minminPopUp.transform.Find("WindowMessage").GetComponent<Text>().text = LocalizationManager.GetTermTranslation("Legend Unit tokens required");
        }
        //else
        //{
        //    _enjinmftPopUp.transform.Find("WindowMessage").GetComponent<Text>().text = LocalizationManager.GetTermTranslation("Min-Mins Token required");
        //}
    }

    private bool handleLegendAvailability(string gridUnitName, string tokenUnitName, bool hasToken)
    {
        return ((gridUnitName == tokenUnitName) && (!hasToken || GameInventory.Instance.HasUnit(gridUnitName)));
    }

    public void ClosePopup(GameObject obj)
    {
        GameSounds.Instance.PlayUiBackSound();
        obj.SetActive(false);
    }

    public void OnBackButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        SceneManager.LoadScene(EnigmaConstants.Scenes.MAIN);
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
            {
                Destroy(child.gameObject);
            }
        }

        for (int boxIndex = 0; boxIndex <= BoxIndexes.LEGEND; boxIndex++)
        {
            int boxIndexAmount = GameInventory.Instance.GetLootBoxIndexAmount(boxIndex);  //TODO: Check if this needs to return stats
            for (int i = 0; i < boxIndexAmount; i++)
            {
                Transform unitTransform = Instantiate<GameObject>(boxGridItemTemplate, _lootBoxGridContent).transform;
                unitTransform.name = "BoxGridItem_BoxIndex " + boxIndex;
                BoxGridItem box = unitTransform.GetComponent<BoxGridItem>();
                box.SetUp(boxIndex);
                int boxIndexCopy = boxIndex;
                box.OpenButton.onClick.AddListener(() => onLootBoxOpenButtonDown(boxIndexCopy));
            }
        }

        boxGridItemTemplate.SetActive(false);
    }

    public void BuyMasterBoxOffer()
    {
        _selectedBoxIndex = BoxIndexes.SPECIAL;
        onLootBoxBuyConfirmButtonDown();
        _extraLootBoxPopUp.SetActive(false);
    }

    private void onLootBoxBuyConfirmButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();

#if HUAWEI
        GameInventory gameInventory = GameInventory.Instance;

        int price = int.Parse(_confirmPopUpPackPrices[_selectedPackIndex]);
        int currency = gameInventory.GetCrystalsAmount();

        if (price > currency)
        {
            _buyResultPopUp.Open(GameConstants.PopUpMessages.NOT_ENOUGH_CURRENCY);
        }
        else
        {
            handleCurrencyBuyResult(IAPManager.Instance.IAP_IDS[_selectedPackIndex], true);
            gameInventory.ChangeCrystalsAmount(-price);
            _crystalsAmount.text = gameInventory.GetCrystalsAmount().ToString();
        }
#elif DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.BuyAndroid)
        {
            handleCurrencyBuyResult(IAPManager.Instance.IAP_IDS[_selectedBoxIndex], true);  // Buy hack to work on android
        }
        else
#else
        {
            IAPManager.BuyConsumable(_selectedPackIndex);
        }
#endif

            _lootBoxBuyConfirmPopUp.Close();
    }

    public void onLootBoxBuyCancelButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        _lootBoxBuyConfirmPopUp.Close();
    }

    private void grantBox(int boxIndex, int amount)
    {
        GameInventory.Instance.ChangeLootBoxAmount(amount, boxIndex, true, true);
        refreshLootBoxesGrid();
    }

    private void onLootBoxOpenButtonDown(int boxIndex)
    {
        GameSounds.Instance.PlayUiAdvanceSound();

        Dictionary<string, int> unitsWithTier = new Dictionary<string, int>();
        GameInventory gameInventory = GameInventory.Instance;

        List<string> lootBoxUnitNumbers = gameInventory.OpenLootBox(boxIndex);
        foreach (string unitName in lootBoxUnitNumbers)
        {
            unitsWithTier.Add(unitName.ToString(), gameInventory.GetUnitTier(unitName));
        }

        refreshLootBoxesGrid();

        _openLootBoxPopUp.Feed(unitsWithTier);
        _openLootBoxPopUp.Open();
    }

    //private void onGiftProgress(JSONNode response)
    //{
    //    if (response != null)
    //    {

    //        JSONNode response_hash = response[0];
    //        string status = response_hash["status"].ToString().Trim('"');

    //        print("onGiftProgress -> response: " + response.ToString() + " status: " + status);

    //        if (status == "SUCCESS")
    //        {
    //            string progress = response_hash["progress"].ToString().Trim('"');
    //            giftProgress.fillAmount = float.Parse(progress) / 1000.0f;
    //            giftText.text = "$"+ progress + " / $1000";
    //        }

    //    }
    //}
}
