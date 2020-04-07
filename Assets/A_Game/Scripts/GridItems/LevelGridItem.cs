using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma.CoreSystems;
using UnityEngine.UI;

public class LevelGridItem : MonoBehaviour
{
    public Button FightButton;
    public GameObject enjinReward;

    [SerializeField] private Text _label;
    //[SerializeField] private Text _labelShadow;

    private void Start()
    {
        int level = GameInventory.Instance.GetSinglePlayerLevel();
        Debug.Log("Status: " + level);
    }


    public void SetLabel(string text)
    {
        _label.text = text;
        //_labelShadow.text = text;
    }

    public void OnFightButtonDown()
    {
        
    }

    public void EnableEnjinReward()
    {
        enjinReward.SetActive(true);
    }
}
