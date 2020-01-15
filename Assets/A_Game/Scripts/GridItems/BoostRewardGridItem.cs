using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostRewardGridItem : MonoBehaviour
{ 
    [SerializeField] private Image _image;

    [SerializeField] private Text _bonusText;
    [SerializeField] private Text _categoryText;

    public void SetUp(string category, int bonus)
    {
        this.name = category + " +" + bonus;
        _bonusText.text = "+" + bonus.ToString();
        _categoryText.text = LocalizationManager.GetTermTranslation(category);

        string imagePath = TeamBoostItem.GetOreImagePath(category, bonus);

        Sprite itemSprite = Resources.Load<Sprite>(imagePath);

        if (itemSprite != null)
        {
            _image.sprite = itemSprite;
        }
    }
}
