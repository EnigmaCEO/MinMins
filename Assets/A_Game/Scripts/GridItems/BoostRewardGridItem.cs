using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoostRewardGridItem : MonoBehaviour
{ 
    [SerializeField] private Image _image;

    [SerializeField] private Text _bonusText;
    [SerializeField] private Text _categoryText;

    public void SetUp(string category, int bonus, bool displayBonus)
    {
        this.name = category;
        if (displayBonus)
        {
            this.name += " +" + bonus;
        }

        _bonusText.gameObject.SetActive(displayBonus);

        if (displayBonus)
        {
            _bonusText.text = "+" + bonus.ToString();
        }


        _categoryText.text = LocalizationManager.GetTermTranslation(category);

        string imagePath = TeamBoostItemGroup.GetOreImagePath(category, bonus);

        Sprite itemSprite = Resources.Load<Sprite>(imagePath);

        if (itemSprite != null)
        {
            _image.sprite = itemSprite;
        }
    }

    public void SetTextForQuestReward(string oreTier, string categoryLabel)
    {
        _categoryText.text = LocalizationManager.GetTermTranslation(oreTier) + " " + LocalizationManager.GetTermTranslation("Ore") + ": " + LocalizationManager.GetTermTranslation(categoryLabel);
    }
}
