using GameConstants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostInfoPopUp : MonoBehaviour
{
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _categoryText;
    [SerializeField] private Text _bonusText;

    private void Start()
    {
        Close();
    }

    public void Open(TeamBoostItemGroup boostItemGroup)
    {
        string displayName = "";

        if (boostItemGroup.IsToken)
        {
            displayName = LocalizationManager.GetTermTranslation(boostItemGroup.Name);
        }
        else
        {
            string translatedOreTier = LocalizationManager.GetTermTranslation(TeamBoostItemGroup.GetOreTier(boostItemGroup.Bonus));
            string translatedOreTerm = LocalizationManager.GetTermTranslation(Terms.ORE);

            displayName = translatedOreTier + " " + translatedOreTerm;
        }

        _nameText.text = displayName;
        _categoryText.text = LocalizationManager.GetTermTranslation(boostItemGroup.Category);
        _bonusText.text = "+" + boostItemGroup.Bonus.ToString();

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
