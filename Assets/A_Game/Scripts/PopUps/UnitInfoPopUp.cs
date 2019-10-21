using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoPopUp : MonoBehaviour
{
    [SerializeField] private Text _level;
    [SerializeField] private Text _exp;
    [SerializeField] private Slider _progress;

    public void UpdateExpInfo(string unitName)
    {
        GameInventory gameInventory = GameInventory.Instance;
        int unitExp = gameInventory.GetLocalUnitExp(unitName);
        GameInventory.ExpData unitExpData = gameInventory.GetUnitExpData(unitExp);

        _level.text = "Level " + unitExpData.Level;
        _exp.text = "(" + unitExp + "/" + unitExpData.ExpForNextLevel + ")";
        _progress.value = (float)(unitExp) / (unitExpData.ExpForNextLevel);
    }
}
