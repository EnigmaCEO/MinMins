using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamBoostGridItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _tint;
    [SerializeField] private Text _bonusText;

    public TeamBoostItemGroup BoostItemGroup
    {
        get;
        private set;
    }

    public void SetUp(TeamBoostItemGroup boostItem, bool isOre)
    {
        BoostItemGroup = boostItem;

        this.name = BoostItemGroup.Name;

        _bonusText.text = BoostItemGroup.Bonus.ToString();
        string spritePath = "";

        if (isOre)
        {
            spritePath = TeamBoostItemGroup.GetOreImagePath(BoostItemGroup.Category, BoostItemGroup.Bonus);
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
