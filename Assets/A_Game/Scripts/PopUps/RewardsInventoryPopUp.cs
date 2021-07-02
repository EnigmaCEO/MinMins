using Enigma.CoreSystems;
using GameConstants;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardsInventoryPopUp : MonoBehaviour
{
    [SerializeField] private float _commonCost = 0.5f;
    [SerializeField] private float _premiumCost = 1.0f;
    [SerializeField] private float _specialCost = 2.0f;

    [SerializeField] private string _coinName = "JENJ";
    [SerializeField] private string _defaultTokenName = "Unknown";

    [SerializeField] private string _imagesFolder = "Images/";
    [SerializeField] private string _enigmaCollectiblesFolder = "EnigmaCollectibles/";
    [SerializeField] private string _oreFolder = "EnjinTokens/";
    [SerializeField] private string _unitsFolder = "Units/";

    [SerializeField] private Sprite _defaultRewardSprite;
    [SerializeField] private Transform _gridContent;
    [SerializeField] private Text _statusUI;
    [SerializeField] private Text _currencyText;

    [SerializeField] private EnjinWithdrawalPopUp _enjinWithdrawalPopUp;

    private GameObject _rewardItemTemplate;
    private bool _initialized = false;

    private void Awake()
    {
        _rewardItemTemplate = _gridContent.GetChild(0).gameObject;
        _rewardItemTemplate.transform.SetParent(_gridContent.parent);
        _rewardItemTemplate.SetActive(false);

        _enjinWithdrawalPopUp.SetInventoryChangedCallback(onInventoryChanged);
    }

    public void Open()
    {
        if (!_initialized)
        {
            initializeInventory();
        }

        UpdateCurrencyUI();
        gameObject.SetActive(true);
    }

    private void initializeInventory()
    {
        _statusUI.text = LocalizationManager.GetTermTranslation(UiMessages.LOADING);
        _gridContent.DestroyChildren();

        bool useOnlineRewardsInventory = true;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.UseOfflineTestInventoryRewards)
        {
            useOnlineRewardsInventory = false;
            List<GameHacks.InventoryRewardsTestItem> testRewardItems = GameHacks.Instance.OfflineTestInventoryRewards;

            foreach (GameHacks.InventoryRewardsTestItem testRewardItem in testRewardItems)
            {
                addRewardItem(testRewardItem.TokenCode, testRewardItem.TokenType, testRewardItem.Withdrawn);
            }

            _initialized = true;
            _statusUI.gameObject.SetActive(false);
        }
#endif

        if (useOnlineRewardsInventory)
        {
            NetworkManager.Transaction(GameNetwork.Transactions.GET_REWARDS_INVENTORY, onRewardInventoryReceived);
        }
    }

    public void UpdateCurrencyUI()
    {
        _currencyText.text = GameStats.Instance.EnjBalance + " " + _coinName;
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
        UpdateCurrencyUI();
    }

    private void onRewardInventoryReceived(JSONNode response)
    {
        if (NetworkManager.CheckInvalidServerResponse(response, nameof(onRewardInventoryReceived)))
        {
            _statusUI.text = LocalizationManager.GetTermTranslation(GameNetwork.ServerResponseMessages.SERVER_ERROR);
        }
        else
        {
            JSONNode rewardsNode = NetworkManager.CheckValidNode(response[0], GameNetwork.TransactionKeys.REWARDS);

            if (rewardsNode != null)
            {
                foreach (JSONNode reward in rewardsNode.AsArray)
                {
                    string tokenKey = rewardsNode[GameNetwork.TransactionKeys.TOKEN_KEY].ToString().Trim('"');
                    string tokenType = rewardsNode[GameNetwork.TransactionKeys.TOKEN_TYPE].ToString().Trim('"');
                    string withdrawn = rewardsNode[GameNetwork.TransactionKeys.TOKEN_WITHDRAWN].ToString().Trim('"');

                    addRewardItem(tokenKey, tokenType, withdrawn);
                }

                _initialized = true;
                _statusUI.gameObject.SetActive(false);
            }
            else
            {
                _statusUI.text = LocalizationManager.GetTermTranslation(GameNetwork.ServerResponseMessages.SERVER_ERROR);
            }
        }
    }

    private void addRewardItem(string tokenCode, string tokenType, string withdrawn)
    {
        GameObject newRewardItem = Instantiate<GameObject>(_rewardItemTemplate, _gridContent);
        newRewardItem.GetComponent<RewardInventoryItem>().Setup(tokenCode, getRewardName(tokenCode), getRewardCost(tokenType), _coinName, getRewardSprite(tokenCode, tokenType), withdrawn, onRewardButtonDown);
        newRewardItem.SetActive(true);
    }

    private void onRewardButtonDown(RewardInventoryItem rewardItemSelected)
    {
        Debug.Log("onRewardButtonDown -> code: " + rewardItemSelected.RewardCode);
        _enjinWithdrawalPopUp.Open(rewardItemSelected);
    }

    private string getRewardName(string tokenKey)
    { 
        string tokenName = "";

        switch (tokenKey)
        {
            case TokenKeys.ENJIN_MAXIM:
                tokenName = "100";
                break;
            case TokenKeys.ENJIN_WITEK:
                tokenName = "101";
                break;
            case TokenKeys.ENJIN_BRYANA:
                tokenName = "102";
                break;
            case TokenKeys.ENJIN_TASSIO:
                tokenName = "103";
                break;
            case TokenKeys.ENJIN_SIMON:
                tokenName = "104";
                break;


            case TokenKeys.KNIGHT_TANK:
                tokenName = "105";
                break;
            case TokenKeys.KNIGHT_HEALER:
                tokenName = "106";
                break;
            case TokenKeys.KNIGHT_SCOUT:
                tokenName = "107";
                break;
            case TokenKeys.KNIGHT_DESTROYER:
                tokenName = "108";
                break;
            case TokenKeys.KNIGHT_BOMBER:
                tokenName = "109";
                break;

            case TokenKeys.DEMON_BOMBER:
                tokenName = "110";
                break;
            case TokenKeys.DEMON_SCOUT:
                tokenName = "111";
                break;
            case TokenKeys.DEMON_DESTROYER:
                tokenName = "113";
                break;
            case TokenKeys.DEMON_TANK:
                tokenName = "114";
                break;
            case TokenKeys.DEMON_HEALER:
                tokenName = "115";
                break;

            case TokenKeys.ENJIN_ALEX:
                tokenName = "122";
                break;
            case TokenKeys.ENJIN_EVAN:
                tokenName = "123";
                break;
            case TokenKeys.ENJIN_ESTHER:
                tokenName = "124";
                break;
            case TokenKeys.ENJIN_BRAD:
                tokenName = "125";
                break;
            case TokenKeys.ENJIN_LIZZ:
                tokenName = "126";
                break;

            case TokenKeys.SWISSBORG_CYBORG:
                tokenName = "127";
                break;

            case TokenKeys.ENJIN_SWORD:
                tokenName = TeamBoostEnjinTokens.SWORD;
                break;
            case TokenKeys.ENJIN_ARMOR:
                tokenName = TeamBoostEnjinTokens.ARMOR;
                break;
            case TokenKeys.ENJIN_SHADOWSONG:
                tokenName = TeamBoostEnjinTokens.SHADOW_SONG;
                break;
            case TokenKeys.ENJIN_BULL:
                tokenName = TeamBoostEnjinTokens.BULL;
                break;

            default:
                    tokenName = _defaultTokenName;
                    break;
        }

        return tokenName;
    }

    private string getRewardCost(string rewardType)
    {
        string rewardCost = "";

        switch (rewardType)
        {
            default:
                rewardCost = _commonCost.ToString();
                break;
            case RewardTypes.PREMIUM:
                rewardCost = _premiumCost.ToString();
                break;
            case RewardTypes.SPECIAL:
                rewardCost = _specialCost.ToString();
                break;
        }

        return rewardCost;
    }

    private Sprite getRewardSprite(string enjinToken, string rewardType)
    {
        string path = _imagesFolder;
        Sprite sprite = _defaultRewardSprite;

        switch (rewardType)
        {
            default:
                path += _enigmaCollectiblesFolder + enjinToken;
                break;
            case RewardTypes.PREMIUM:
                path += _oreFolder + getRewardName(enjinToken);
                break;
            case RewardTypes.SPECIAL:
                path += _unitsFolder + getRewardName(enjinToken);
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
