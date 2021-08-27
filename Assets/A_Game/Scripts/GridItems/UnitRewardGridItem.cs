using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitRewardGridItem : MonoBehaviour
{
    [SerializeField] private Text _nameText;
    [SerializeField] private Image _icon;

    public void Setup(string unitName)
    {
        _nameText.text = "#" + unitName;
        _icon.sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
    }
}
