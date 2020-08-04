using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootGridItem : MonoBehaviour
{
    [SerializeField] private Transform _tierStarsContent;
    [SerializeField] private Image _unitImage;

    public void SetStars(int tier)
    {
        int starsContentLenght = _tierStarsContent.childCount;
        for (int i = 0; i < starsContentLenght; i++)
        {
            _tierStarsContent.GetChild(i).GetComponent<Image>().enabled = (i < tier);
        }
    }

    public void SetUnitName(string unitName)
    {
        string fullpath = "Images/Units/" + unitName;
        Sprite loadedSprite = (Sprite)Resources.Load<Sprite>(fullpath);

        if (loadedSprite != null)
        {
            _unitImage.sprite = loadedSprite;
        }
        else
        {
            Debug.LogError("Sprite was not found at path: " + fullpath);
        }
    }
}
