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

    private Action<RewardInventoryItem> _callback;

    public string RewardCode
    {
        get;
        private set;
    }

    public void Setup(string rewardCode, string rewardName, string rewardCost, Sprite sprite, Action<RewardInventoryItem> buttonDownCallback)
    {
        RewardCode = rewardCode;

        _nameText.text = rewardName;
        _costText.text = rewardCost;
        _icon.sprite = sprite;

        _callback = buttonDownCallback;
    }

    public void ButtonDown()
    {
        _callback(this);
    }
}
