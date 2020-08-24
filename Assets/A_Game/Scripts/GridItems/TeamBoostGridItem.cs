using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamBoostGridItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _tint;
    [SerializeField] private Text _bonusText;

    public TeamBoostItemGroup BoostItem
    {
        get;
        private set;
    }

    public void SetUp(TeamBoostItemGroup boostItem, bool isOre)
    {
        BoostItem = boostItem;

        this.name = BoostItem.Name;

        _bonusText.text = BoostItem.Bonus.ToString();
        string spritePath = "";

        if (isOre)
        {
            spritePath = TeamBoostItemGroup.GetOreImagePath(BoostItem.Category, BoostItem.Bonus);
        }
        else
        {
            spritePath = "Images/EnjinTokens/" + name;
        }

        if (spritePath != "")
        {
            Sprite itemSprite = Resources.Load<Sprite>(spritePath);

            if (itemSprite != null)
            {
                _image.sprite = itemSprite;
            }
        }

        Deselect();
    }

    public void Select()
    {
        _tint.SetActive(true);
    }

    public void Deselect()
    {
        _tint.SetActive(false);
    }
}
