﻿using Enigma.CoreSystems;
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
    [SerializeField] private float _ultimateCost = 5.0f;

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

        gameObject.SetActive(true);
    }

    private void initializeInventory()
    {
        _currencyText.text = GameStats.Instance.EnjBalance + " " + _coinName;

        _statusUI.text = LocalizationManager.GetTermTranslation(UiMessages.LOADING);
        _gridContent.DestroyChildren();

        bool useOnlineWithdrawnItems = true;

        addOwnedItems();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.UseOfflineTestWithdrawnItems)
        {
            useOnlineWithdrawnItems = false;
            List<string> testWithdrawnTokenKeys = GameHacks.Instance.OfflineTestWithdrawnTokenKeys;

            foreach (string testWithdrawnTokenKey in testWithdrawnTokenKeys)
            {
                setWithdrawnToken(testWithdrawnTokenKey);
            }
        }
#endif

        if (useOnlineWithdrawnItems)
        {
            List<string> availableTokens = GameNetwork.Instance.GetTokensAvailable();
            foreach (string tokenKey in availableTokens)
            {
                setWithdrawnToken(tokenKey);
            }
        }

        _initialized = true;
        _statusUI.gameObject.SetActive(false);
    }

    private void setWithdrawnToken(string tokenKey)
    {
        foreach (Transform tokenTransform in _gridContent)
        {
            RewardInventoryItem rewardItem = tokenTransform.GetComponent<RewardInventoryItem>();
            if (rewardItem.TokenKey == tokenKey)
            {
                rewardItem.SetWithdrawn();
                break;
            }
        }
    }

    private void addOwnedItems()
    {
        GameInventory gameInventory = GameInventory.Instance;

        List<string> ownedUnitNames = gameInventory.GetInventoryUnitNames();

        foreach (string unitName in ownedUnitNames)
        {
            if (gameInventory.CheckCanBeWithdrawn(unitName))
            {
                addRewardItem(gameInventory.GetUnitNameToken(unitName), EnjinTokenTypes.SPECIAL);
            }
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

    private void onInventoryChanged()
    {
        initializeInventory();
    }

    private void addRewardItem(string tokenName, string tokenType)
    {
        GameObject newRewardItem = Instantiate<GameObject>(_rewardItemTemplate, _gridContent);
        newRewardItem.GetComponent<RewardInventoryItem>().Setup(tokenName, GameInventory.Instance.GetTokenUnitName(tokenName), getRewardCost(tokenType), _coinName, getRewardSprite(tokenName, tokenType), onRewardButtonDown);
        newRewardItem.SetActive(true);
    }

    private void onRewardButtonDown(RewardInventoryItem rewardItemSelected)
    {
        Debug.Log("onRewardButtonDown -> code: " + rewardItemSelected.TokenKey);
        _enjinWithdrawalPopUp.Open(rewardItemSelected);
    }

    private string getRewardCost(string rewardType)
    {
        string rewardCost = "";

        switch (rewardType)
        {
            default:
                rewardCost = _commonCost.ToString();
                break;
            case EnjinTokenTypes.PREMIUM:
                rewardCost = _premiumCost.ToString();
                break;
            case EnjinTokenTypes.SPECIAL:
                rewardCost = _specialCost.ToString();
                break;
            case EnjinTokenTypes.ULTIMATE:
                rewardCost = _ultimateCost.ToString();
                break;
        }

        return rewardCost;
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
