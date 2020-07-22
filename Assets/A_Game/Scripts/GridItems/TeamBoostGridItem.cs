using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamBoostGridItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _tint;
    [SerializeField] private Text _bonusText;

    public TeamBoostItem BoostItem
    {
        get;
        private set;
    }

    public void SetUp(TeamBoostItem boostItem, bool isOre)
    {
        BoostItem = boostItem;

        this.name = BoostItem.Name;

        _bonusText.text = BoostItem.Bonus.ToString();
        string spritePath = "";

        if (isOre)
        {
            spritePath = TeamBoostItem.GetOreImagePath(BoostItem.Category, BoostItem.Bonus);
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
