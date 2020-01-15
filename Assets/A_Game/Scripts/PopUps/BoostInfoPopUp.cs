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

    public void Open(string name, string category, int bonus)
    {
        _nameText.text = LocalizationManager.GetTermTranslation(name);
        _categoryText.text = LocalizationManager.GetTermTranslation(category);
        _bonusText.text = "+" + bonus.ToString();

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
