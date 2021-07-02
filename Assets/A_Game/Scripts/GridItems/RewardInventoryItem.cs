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
    [SerializeField] private Button _withdrawButton;

    private Action<RewardInventoryItem> _callback;

    public string RewardCode
    {
        get;
        private set;
    }

    public int CostNumber
    {
        get;
        private set;
    }

    public void Setup(string rewardCode, string rewardName, string rewardCost, string coinName, Sprite sprite, string withdrawn, Action<RewardInventoryItem> buttonDownCallback)
    {
        RewardCode = rewardCode;

        _nameText.text = rewardName;

        CostNumber = int.Parse(rewardCost);
        _costText.text = rewardCost += " " + coinName; ;

        _icon.sprite = sprite;

        _callback = buttonDownCallback;

        bool withdrawnBool = bool.Parse(withdrawn);

        _withdrawButton.enabled = !withdrawnBool;
        _withdrawButton.GetComponent<Image>().color = withdrawnBool ? Color.grey : Color.white ;
    }

    public void ButtonDown()
    {
        _callback(this);
    }
}
