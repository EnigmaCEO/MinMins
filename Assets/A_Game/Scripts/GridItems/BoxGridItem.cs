using GameConstants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxGridItem : MonoBehaviour
{
    public Button OpenButton;

    [SerializeField] private Transform _tierStarsContent;
    [SerializeField] private Image _boxImage;

    public void SetUp(int boxIndex)
    {
        int starsContentLenght = _tierStarsContent.childCount;
        int tier = GameInventory.Instance.GetPackTier(boxIndex);
        for (int i = 0; i < starsContentLenght; i++)
        {
            _tierStarsContent.GetChild(i).GetComponent<Image>().enabled = (i < tier);
        }

        if (boxIndex == BoxIndexes.SPECIAL) //Cheaper Master Box
        {
            boxIndex = BoxIndexes.MASTER; //Master Box
        }

        _boxImage.sprite = (Sprite)Resources.Load<Sprite>("Images/shop_chest" + (boxIndex + 1));
    }
}
