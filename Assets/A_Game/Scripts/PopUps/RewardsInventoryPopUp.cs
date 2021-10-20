using GameConstants;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Enigma.CoreSystems;
using SimpleJSON;
using System;


public class RewardsInventoryPopUp : MonoBehaviour
{
    [SerializeField] private float _commonCost = 0.5f;
    [SerializeField] private float _premiumCost = 1.0f;
    [SerializeField] private float _specialCost = 2.0f;
    [SerializeField] private float _ultimateCost = 5.0f;

    [SerializeField] private string _coinName = "JENJ";
    //[SerializeField] private string _defaultTokenName = "Unknown";

    [SerializeField] private string _imagesFolder = "Images/";
    [SerializeField] private string _enigmaCollectiblesFolder = "EnigmaCollectibles/";
    [SerializeField] private string _oreFolder = "EnjinTokens/";
    [SerializeField] private string _unitsFolder = "Units/";

    [SerializeField] private Sprite _defaultRewardSprite;
    [SerializeField] private Transform _gridContent;
    [SerializeField] private Text _statusUI;
    [SerializeField] private Text _currencyText;

    [SerializeField] private RewardTransactionPopUp _enjinTransactionPopUp;

    private const string SERVER_ERROR_TERM = "There is a Server Error. Please try again later.";
    private const string TOKEN_WEBSITE_PREFIX = "https://jumpnet.enjinx.io/eth/asset/";
    private GameObject _rewardItemTemplate;

    private void Awake()
    {
        _rewardItemTemplate = _gridContent.GetChild(0).gameObject;
        _rewardItemTemplate.transform.SetParent(_gridContent.parent);
        _rewardItemTemplate.SetActive(false);

        _enjinTransactionPopUp.SetInventoryChangedCallback(onInventoryChanged);
    }

    public void Open()
    {
        initializeInventory();
        gameObject.SetActive(true);
    }

    private void initializeInventory()
    {
        _currencyText.text = GameStats.Instance.EnjBalance + " " + _coinName;

        _statusUI.text = LocalizationManager.GetTermTranslation(UiMessages.LOADING);
        _gridContent.DestroyChildren();

        NetworkManager.Transaction(EnigmaTransactions.GET_ENJIN_MARKETPLACE, onGetEnjinMarketPlace);
    }

    private float getPriceFromListing(JSONNode listing)
    {
        float purchaseCost = -1;

        JSONNode purchaseCostNode = NetworkManager.CheckValidNode(listing, EnigmaNodeKeys.PRICE);
        if (purchaseCostNode != null)
        {
            string purchaseCostString = purchaseCostNode.ToString().Trim('"');
            purchaseCost = float.Parse(purchaseCostString);
        }

        return purchaseCost;
    }

    private void onGetEnjinMarketPlace(JSONNode response)
    {
        if (NetworkManager.CheckInvalidServerResponse(response, nameof(onGetEnjinMarketPlace)))
        {
            _statusUI.text = LocalizationManager.GetTermTranslation(SERVER_ERROR_TERM);
            return;
        }

        GameInventory gameInventory = GameInventory.Instance;

        List<string> ownedUnitNames = gameInventory.GetInventoryUnitNames();
        string shalwendDeadlyKnightUnitName = gameInventory.GetTokenUnitName(GameEnjinTokenKeys.SHALWEND_DEADLY_KNIGHT);
        string shalwendWarGodUnitName = gameInventory.GetTokenUnitName(GameEnjinTokenKeys.SHALWEND_WARGOD);

        bool useOnlineWithdrawnItems = true; 
        List<string> availableTokenKeys = null;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.UseOfflineTestWithdrawnItems)
        {
            useOnlineWithdrawnItems = false;
            availableTokenKeys = GameHacks.Instance.OfflineTestWithdrawnTokenKeys;
        }
#endif

        if (useOnlineWithdrawnItems)
        {
            availableTokenKeys = GameNetwork.Instance.GetEnjinKeysAvailable();
        }

        JSONNode response_hash = response[0];
        JSONNode listings = NetworkManager.CheckValidNode(response_hash, EnigmaNodeKeys.LISTINGS);
        Dictionary<string, JSONNode> listingByUnitName = new Dictionary<string, JSONNode>();
        List<string> listingsUnitNames = new List<string>();

        if (listings != null)
        {
            foreach (JSONNode listing in listings.AsArray)
            {
                JSONNode tokenCodeNode = NetworkManager.CheckValidNode(listing, EnigmaNodeKeys.ENJIN_CODE);
                if (tokenCodeNode != null)
                {
                    string tokenCode = tokenCodeNode.ToString().Trim('"');
                    string unitName = gameInventory.GetTokenUnitName(tokenCode);

                    if (listingByUnitName.ContainsKey(unitName))
                    {
                        float newPrice = getPriceFromListing(listingByUnitName[unitName]);
                        float currentPrice = getPriceFromListing(listing);

                        if (newPrice < currentPrice)
                        {
                            listingByUnitName[unitName] = listing;
                        }
                    }
                    else
                    {
                        listingByUnitName.Add(unitName, listing);
                        listingsUnitNames.Add(unitName);
                    }
                }
            }
        }

        List<string> descendingListingNames = listingsUnitNames.OrderByDescending(x => int.Parse(x)).ToList();

        foreach (string unitName in descendingListingNames)
        {         
            JSONNode listingNode = listingByUnitName[unitName];

            JSONNode tokenCodeNode = NetworkManager.CheckValidNode(listingNode, EnigmaNodeKeys.ENJIN_CODE);
            if (tokenCodeNode != null)
            {
                string tokenCode = tokenCodeNode.ToString().Trim('"');

                JSONNode etherumIdNode = NetworkManager.CheckValidNode(listingNode, EnigmaNodeKeys.ETHEREUM_ID);
                if (etherumIdNode != null)
                {
                    string ethereumId = etherumIdNode.ToString().Trim('"');

                    JSONNode uuidNode = NetworkManager.CheckValidNode(listingNode, EnigmaNodeKeys.UUID);
                    if (uuidNode != null)
                    {
                        string uuid = uuidNode.ToString().Trim('"');

                        string purchaseCostString = getPriceFromListing(listingNode).ToString();
                        if(purchaseCostString != "-1")
                        { 
                            RewardsInventoryOptions option = RewardsInventoryOptions.None;
                            if (availableTokenKeys.Contains(tokenCode))
                            {
                                option = RewardsInventoryOptions.Sell;
                            }
                            else if (ownedUnitNames.Contains(unitName))
                            {
                                option = RewardsInventoryOptions.Tokenize;
                            }
                            else
                            {
                                option = RewardsInventoryOptions.Purchase;
                            }

                            string tokenType = EnjinTokenTypes.SPECIAL;

                            if ((unitName == shalwendDeadlyKnightUnitName) || (unitName == shalwendWarGodUnitName))
                            {
                                tokenType = EnjinTokenTypes.ULTIMATE;
                            }

                            addRewardItem(tokenCode, tokenType, ethereumId, uuid, purchaseCostString, option);
                        }
                    }
                }
            }
        }

        _statusUI.gameObject.SetActive(false);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnCloseButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        Close();
    }

    private void onInventoryChanged()
    {
        initializeInventory();
    }

    private void addRewardItem(string tokenKey, string tokenType, string ethereumId, string uuid, string purchaseCost, RewardsInventoryOptions option)
    {
        string costString = purchaseCost;

        if (option == RewardsInventoryOptions.Tokenize)
        {
            costString = getTokenizeCost(tokenType);
        }

        GameObject newRewardItem = Instantiate<GameObject>(_rewardItemTemplate, _gridContent);
        string tokenUnitName = GameInventory.Instance.GetTokenUnitName(tokenKey);
        newRewardItem.GetComponent<RewardInventoryItem>().Setup(tokenKey, tokenUnitName, ethereumId, uuid, costString, _coinName, getRewardSprite(tokenKey, tokenType), onRewardButtonDown, option);
        newRewardItem.name = "#" + tokenKey;
        newRewardItem.SetActive(true);
    }

    private void onRewardButtonDown(RewardInventoryItem rewardItemSelected)
    {
        Debug.Log("onRewardButtonDown -> code: " + rewardItemSelected.TokenKey);

        RewardsInventoryOptions option = rewardItemSelected.Option;

        if (option == RewardsInventoryOptions.Sell)
        {
            string ethereumIdWebSuffix = getEthereumIdWebSuffix(rewardItemSelected.EthereumId);
            string fullRewardWebUrl = TOKEN_WEBSITE_PREFIX + ethereumIdWebSuffix;
            Application.OpenURL(fullRewardWebUrl);
        }
        else //Tokenize or Buy
        {
            _enjinTransactionPopUp.Open(rewardItemSelected, option);
        }
    }

    private string getEthereumIdWebSuffix(string ethereumId)
    {
        string ethereumIdeWebSuffix = ethereumId;
        int stringLength = ethereumId.Length;
        int lastReleventCharIndex = 0;

        for (int i = 0; i < stringLength; i++)
        {
            if (ethereumId[i] != '0')
            {
                lastReleventCharIndex = i;
            }
        }

        if (lastReleventCharIndex != (stringLength - 1))
        {
            ethereumIdeWebSuffix.Remove(lastReleventCharIndex + 1);
        }

        return ethereumIdeWebSuffix;
    }

    private string getTokenizeCost(string rewardType)
    {
        float rewardCost = 0.0f;

        switch (rewardType)
        {
            default:
                rewardCost = _commonCost;
                break;
            case EnjinTokenTypes.PREMIUM:
                rewardCost = _premiumCost;
                break;
            case EnjinTokenTypes.SPECIAL:
                rewardCost = _specialCost;
                break;
            case EnjinTokenTypes.ULTIMATE:
                rewardCost = _ultimateCost;
                break;
        }

        if (GameNetwork.Instance.GetIsEnjinKeyAvailable(GameEnjinTokenKeys.GOD_ENIGMA))
        {
            rewardCost *= 0.5f;
        }

        return rewardCost.ToString();
    }

    private Sprite getRewardSprite(string enjinToken, string rewardType)
    {
        string path = _imagesFolder;
        Sprite sprite = _defaultRewardSprite;
        GameInventory gameInventory = GameInventory.Instance;

        switch (rewardType)
        {
            default:
                path += _enigmaCollectiblesFolder + enjinToken;
                break;
            case EnjinTokenTypes.PREMIUM:
                path += _oreFolder + gameInventory.GetTokenUnitName(enjinToken);
                break;
            case EnjinTokenTypes.SPECIAL:
            case EnjinTokenTypes.ULTIMATE:
                path += _unitsFolder + gameInventory.GetTokenUnitName(enjinToken);
                break;
        }

        Sprite loadedSprite = (Sprite)Resources.Load<Sprite>(path);

        if (loadedSprite != null)
        {
            sprite = loadedSprite;
        }
        else
        {
            Debug.LogError("RewardsInventoryPopUp::getRewardSprite -> Reward sprite at path: " + path + " was not found. Returning default sprite.");
        }

        return sprite;
    }
}

public enum RewardsInventoryOptions
{
    None,
    Tokenize,
    Purchase,
    Sell
}
