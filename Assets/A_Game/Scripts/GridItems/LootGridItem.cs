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
            _tierStarsContent.GetChild(i).GetComponent<Image>().enabled = (i < tier);
    }

    public void SetUnitName(string unitName)
    {
        _unitImage.sprite = (Sprite)Resources.Load<Sprite>("Images/Units/" + unitName);
    }
}
