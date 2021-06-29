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

    [SerializeField] private string _imagesFolder = "Images/";
    [SerializeField] private string _enigmaCollectiblesFolder = "EnigmaCollectibles/";
    [SerializeField] private string _oreFolder = "Ore/";
    [SerializeField] private string _unitsFolder = "Units/";

    [SerializeField] private Sprite _defaultRewardSprite;
    [SerializeField] private Transform _gridContent;
    [SerializeField] private Text _statusUI;

    private GameObject _rewardItemTemplate;
    private bool _initialized = false;

    private void Start()
    {
        _rewardItemTemplate = _gridContent.GetChild(0).gameObject;
        _rewardItemTemplate.transform.SetParent(_gridContent.parent);
        _rewardItemTemplate.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);

        if (!_initialized)
        {
            _statusUI.text = LocalizationManager.GetTermTranslation(UiMessages.LOADING);
            NetworkManager.Transaction(GameNetwork.Transactions.GET_REWARDS_INVENTORY, onRewardInventoryReceived);
        }
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
                    string rewardCode = rewardsNode[GameNetwork.TransactionKeys.REWARD_CODE];
                    string rewardType = rewardsNode[GameNetwork.TransactionKeys.REWARD_TYPE];

                    addRewardItem(rewardCode, rewardType);
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

    private void addRewardItem(string rewardCode, string rewardType)
    {
        GameObject newRewardItem = Instantiate<GameObject>(_rewardItemTemplate, _gridContent);
        newRewardItem.GetComponent<RewardInventoryItem>().Setup(rewardCode, getRewardName(rewardCode), getRewardCost(rewardType), getRewardSprite(rewardCode, rewardType), onRewardButtonDown);
        newRewardItem.SetActive(true);
    }

    private void onRewardButtonDown(RewardInventoryItem rewardItemSelected)
    {
        Debug.Log("onRewardButtonDown -> code: " + rewardItemSelected.RewardCode);
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

            default:
                    tokenName = "100";
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

        rewardCost += " " + _coinName;

        return rewardCost;
    }

    private Sprite getRewardSprite(string rewardCode, string rewardType)
    {
        string path = "Images/";
        Sprite sprite = _defaultRewardSprite;

        switch (rewardType)
        {
            default:
                path += _enigmaCollectiblesFolder + rewardCode;
                break;
            case RewardTypes.PREMIUM:
                path += _oreFolder + rewardCode;
                break;
            case RewardTypes.SPECIAL:
                path += _unitsFolder + getRewardName(rewardCode);
                break;
        }

        Sprite loadedSprite = (Sprite)Resources.Load<Sprite>(path);

        if (loadedSprite != null)
        {
            Debug.LogError("RewardsInventoryPopUp::getRewardSprite -> Reward sprite at path: " + path + " was not found. Returning default sprite.");
            sprite = loadedSprite;
        }

        return sprite;
    }
}
