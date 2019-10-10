using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitAliveGridItem : MonoBehaviour
{
    [SerializeField] private Image _unitImage;
    [SerializeField] private GameObject _tint;

    public void SetUp(string unitName, bool isAlive)
    {
        this.name = unitName;
        _unitImage.sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
        _tint.SetActive(!isAlive);
    }
}
