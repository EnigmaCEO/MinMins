using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardInventoryItem : MonoBehaviour
{
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _costText;
    [SerializeField] private Image _icon;

    [SerializeField] private Text OptionButtonText;

    private Action<RewardInventoryItem> _callback;

    public string EthereumId
    {
        get;
        private set;
    }

    public string TokenKey
    {
        get;
        private set;
    }

    public string Uuid
    {
        get;
        private set;
    }

    public float Cost
    {
        get;
        private set;
    }

    public RewardsInventoryOptions Option
    {
        get;
        private set;
    }

    public void Setup(string tokenKey, string unitName, string ethereumId, string uuid, string optionCostString, string coinName, Sprite sprite, Action<RewardInventoryItem> buttonDownCallback, RewardsInventoryOptions option)
    {
        TokenKey = tokenKey;
        EthereumId = ethereumId;
        Uuid = uuid;
        Option = option;
       
        _nameText.text = "#" + unitName;

        if (option == RewardsInventoryOptions.Sell)
        {
            _costText.gameObject.SetActive(false);
        }
        else
        { 
            Cost = float.Parse(optionCostString);
            _costText.text = optionCostString += " " + coinName;
        }

        _icon.sprite = sprite;

        _callback = buttonDownCallback;

        OptionButtonText.text = LocalizationManager.GetTermTranslation(option.ToString());
    }

    public void ButtonDown()
    {
        _callback(this);
    }
}
