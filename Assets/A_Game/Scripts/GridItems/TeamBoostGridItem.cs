using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamBoostGridItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _tint;
    [SerializeField] private Text _bonusText;

    //[SerializeField] private Text _nameText;
    //[SerializeField] private Text _descriptionText;

    public void SetUp(string name, string category, int bonus, bool isOre)
    {
        this.name = name;
        _bonusText.text = bonus.ToString();
        string spritePath = "";

        if (isOre)
        {
            spritePath = TeamBoostItem.GetOreImagePath(category, bonus);
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
