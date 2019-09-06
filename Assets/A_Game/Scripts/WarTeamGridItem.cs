using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarTeamGridItem : MonoBehaviour
{
    public Image View;

    public string UnitName = "";

    [SerializeField] private Image _lifeFill;

    public void SetLifeFill(float ratio)
    {
        _lifeFill.fillAmount = ratio;
    }
}
