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

    public string TokenKey
    {
        get;
        private set;
    }

    public float CostNumber
    {
        get;
        private set;
    }

    public void Setup(string tokenKey, string rewardName, string rewardCost, string coinName, Sprite sprite, Action<RewardInventoryItem> buttonDownCallback)
    {
        TokenKey = tokenKey;

        _nameText.text = rewardName;

        CostNumber = float.Parse(rewardCost);
        _costText.text = rewardCost += " " + coinName; ;

        _icon.sprite = sprite;

        _callback = buttonDownCallback;
    }

    public void SetAsWithdrawn()
    {
        Destroy(gameObject);
    }

    public void ButtonDown()
    {
        _callback(this);
    }
}
