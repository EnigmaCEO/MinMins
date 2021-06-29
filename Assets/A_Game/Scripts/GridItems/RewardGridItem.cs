using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardGridItem : MonoBehaviour
{
    [SerializeField] private Transform _tierStarsContent;
    [SerializeField] private Image _boxImage;

    public void SetUp(int tier, bool isEnjin)
    {
        int starsContentLenght = _tierStarsContent.childCount;
        for (int i = 0; i < starsContentLenght; i++)
        {
            _tierStarsContent.GetChild(i).GetComponent<Image>().enabled = (i < tier);
        }

        string path = "Images/shop_chest" + tier;
        if (isEnjin)
        {
            path = "Images/Enjin Logo";
        }

        _boxImage.sprite = (Sprite)Resources.Load<Sprite>(path);
    }
}
