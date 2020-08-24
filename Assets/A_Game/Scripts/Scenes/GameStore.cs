using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

public class GameStore : EnigmaScene
{
    [SerializeField] private int _maxBoxTierNumber = 3;

    [SerializeField] private List<string> _confirmPopUpPackNames = new List<string>() { "STARTER BOX", "PREMIUM BOX", "MASTER BOX", "DEMON KING BOX", "LEGEND BOX" };
    [SerializeField] private List<string> _confirmPopUpPackDescriptions = new List<string>() { "No guarantees.", "Guarantees a Silver Unit.", "Guarantees a Gold Unit.", "Demon King description.", "Legend Box description." };

    [SerializeField] private List<Image> _confirmPopUpPackImages;
    [SerializeField] private List<string> _confirmPopUpPackPrices = new List<string>() { "0.99", "1.99", "4.99", "10.00", "10.00" };

    [SerializeField] private List<int> _packTiers = new List<int> { 1, 2, 3, 3 };

    [SerializeField] private LootBoxBuyConfirmPopUp _lootBoxBuyConfirmPopUp;
    [SerializeField] private OpenLootBoxPopUp _openLootBoxPopUp;
    [SerializeField] private BuyResultPopUp _buyResultPopUp;
    [SerializeField] private BuyResultPopUp _summonResultPopUp;
    [SerializeField] private GameObject _extraLootBoxPopUp;
    [SerializeField] private GameObject _enjinmftPopUp;
    [SerializeField] private GameObject _minminPopUp;

    [SerializeField] private Transform _lootBoxGridContent;
    //[SerializeField] private Image giftProgress;
    //[SerializeField] private Text giftText;

    private int _selectedPackIndex;

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

        refreshLootBoxesGrid();

        //if (NetworkManager.LoggedIn)
        //{
        //    NetworkManager.Transaction(NetworkManager.Transactions.GIFT_PROGRESS, new Hashtable(), onGiftProgress);
            
        //} 
        //else
        //{
        //    GameObject.Find("gift_panel").gameObject.SetActive(false);
        //}

        if(!GameNetwork.Instance.IsEnjinLinked)
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

            if (itemIndex < 4)
            {
                int boxTier = _packTiers[itemIndex];
                grantBox(boxTier, 1);
            }
            
            _buyResultPopUp.Open("Thanks for your Purchase!");
        }
        else
        {
            _buyResultPopUp.Open("Purchase Failed. Please try again later.");
        }
    }

    public void OnPackBuyButtonDown(int packIndex)
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);

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
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        _openLootBoxPopUp.Close();
        handleFreeLootBoxGifts();
    }

    public void OnBuyResultPopUpDismissButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        _buyResultPopUp.Close();
    }

    public void onExtraLootBoxPopUpDismissButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        grantBox(GameInventory.Tiers.BRONZE, 2);
        _extraLootBoxPopUp.SetActive(false);
    }

    public void onEnjinPopUpDismissButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);

        if (GameInventory.Instance.GetEnjinAttempts() < 1)
        {
            return;
        }

        GameInventory.Instance.AddMissingEnjinUnits();
        
        _enjinmftPopUp.SetActive(false);
    }

    public void onMinMinPopUpDismissButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        GameInventory.Instance.AddMinMinEnjinUnits();

        _minminPopUp.SetActive(false);
    }

    public void openEnjinPopUp()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);

        int count = setupEnjinLegendsDisplay(_enjinmftPopUp);
        if (count == 0 || !GameNetwork.Instance.HasEnjinMft)
        {
            _enjinmftPopUp.transform.Find("DismissButton").gameObject.SetActive(false);
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

    public void openMinMinPopUp()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);

        int count = setupEnjinLegendsMinMinsDisplay(_minminPopUp);
        Debug.Log("count: " + count);
        if (count == 0 || !GameNetwork.Instance.HasEnjinMinMinsToken)
        {
            _minminPopUp.transform.Find("DismissButton").gameObject.SetActive(false);
        }

        _minminPopUp.SetActive(true);

        if (GameNetwork.Instance.HasEnjinMinMinsToken)
        {
            _minminPopUp.transform.Find("WindowMessage").GetComponent<Text>().text = LocalizationManager.GetTermTranslation("Legend Unit tokens required");
        }
        else
        {
            _enjinmftPopUp.transform.Find("WindowMessage").GetComponent<Text>().text = LocalizationManager.GetTermTranslation("Min-Mins Token required");
        }
    }

    public void closePopup(GameObject obj)
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        obj.SetActive(false);
    }

    public void OnBackButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        SceneManager.LoadScene(EnigmaConstants.Scenes.MAIN);
    }

    private int setupEnjinLegendsDisplay(GameObject popUp)
    {
        Transform enjinContent = popUp.transform.Find("EnjinGrid/Viewport/Content");
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

        return count;
    }

    private bool checkDeactivateLegend(string unitName, string checkName, bool flag)
    {
        return ((unitName == checkName) && (!flag || GameInventory.Instance.HasUnit(unitName))) ;
    }

    private int setupEnjinLegendsMinMinsDisplay(GameObject popUp)
    {
        Transform enjinContent = popUp.transform.Find("EnjinGrid/Viewport/Content");
        int count = enjinContent.childCount;

        GameInventory gameInventory = GameInventory.Instance;
        GameNetwork gameNetwork = GameNetwork.Instance;

        foreach (Transform enjinTransform in enjinContent)
        {
            string unitName = enjinTransform.name;

            if (
                    checkDeactivateLegend(unitName, "100", gameNetwork.HasEnjinMaxim) ||
                    checkDeactivateLegend(unitName, "101", gameNetwork.HasEnjinWitek) ||
                    checkDeactivateLegend(unitName, "102", gameNetwork.HasEnjinBryana) ||
                    checkDeactivateLegend(unitName, "103", gameNetwork.HasEnjinTassio) ||
                    checkDeactivateLegend(unitName, "104", gameNetwork.HasEnjinSimon) ||

                    checkDeactivateLegend(unitName, "122", gameNetwork.HasEnjinAlex) ||
                    checkDeactivateLegend(unitName, "123", gameNetwork.HasEnjinEvan) ||
                    checkDeactivateLegend(unitName, "124", gameNetwork.HasEnjinEsther) ||
                    checkDeactivateLegend(unitName, "125", gameNetwork.HasEnjinBrad) ||
                    checkDeactivateLegend(unitName, "126", gameNetwork.HasEnjinLizz) ||

                    checkDeactivateLegend(unitName, "105", gameNetwork.HasKnightTank) ||
                    checkDeactivateLegend(unitName, "106", gameNetwork.HasKnightHealer) ||
                    checkDeactivateLegend(unitName, "107", gameNetwork.HasKnightScout) ||
                    checkDeactivateLegend(unitName, "108", gameNetwork.HasKnightDestroyer) ||
                    checkDeactivateLegend(unitName, "109", gameNetwork.HasKnightBomber) ||

                    checkDeactivateLegend(unitName, "110", gameNetwork.HasDemonBomber) ||
                    checkDeactivateLegend(unitName, "111", gameNetwork.HasDemonScout) ||
                    checkDeactivateLegend(unitName, "112", gameNetwork.HasDemonDestroyer) ||
                    checkDeactivateLegend(unitName, "113", gameNetwork.HasDemonTank) ||
                    checkDeactivateLegend(unitName, "114", gameNetwork.HasDemonHealer) ||

                    checkDeactivateLegend(unitName, "127", gameNetwork.HasSwissborgCyborg)
                )
            {
                enjinTransform.gameObject.SetActive(false);
                count--;
            }
        }

        return count;
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
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.BuyAndroid)
        {
            handleCurrencyBuyResult(IAPManager.Instance.IAP_IDS[_selectedPackIndex], true);  // Buy hack to work on android
        }
        else
#endif
        {
            IAPManager.BuyConsumable(_selectedPackIndex);
        }

        _lootBoxBuyConfirmPopUp.Close();
    }

    public void onLootBoxBuyCancelButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        _lootBoxBuyConfirmPopUp.Close();
    }

    private void grantBox(int lootBoxTier, int amount)
    {
        GameInventory.Instance.ChangeLootBoxAmount(amount, lootBoxTier, true, true);
        refreshLootBoxesGrid();
    }

    private void onLootBoxOpenButtonDown(int lootBoxTier)
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);

        Dictionary<string, int> unitsWithTier = new Dictionary<string, int>();
        GameInventory gameInventory = GameInventory.Instance;

        List<string> lootBoxUnitNumbers = gameInventory.OpenLootBox(lootBoxTier);
        foreach (string unitName in lootBoxUnitNumbers)
            unitsWithTier.Add(unitName.ToString(), gameInventory.GetUnitTier(unitName));

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
