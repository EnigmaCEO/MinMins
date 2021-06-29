using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardInventoryItem : MonoBehaviour
{
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _costText;
    [SerializeField] private Image _icon;

    public void Setup(string rewardName, string rewardCost, Sprite sprite)
    {
        _nameText.text = rewardName;
        _costText.text = rewardCost;
        _icon.sprite = sprite;
    }
}
