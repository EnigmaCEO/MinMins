﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamBoostGridItem : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _tint;
    [SerializeField] private Text _bonusText;

    public void SetUp(string name, string category, int bonus, bool isOre)
    {
        this.name = name;
        _bonusText.text = bonus.ToString();
        string spritePath = "";

        if (isOre)
        {
            spritePath = TeamBoostItem.GetOreImagePath(category, bonus);
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
