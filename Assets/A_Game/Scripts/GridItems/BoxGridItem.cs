using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxGridItem : MonoBehaviour
{
    public Button OpenButton;

    [SerializeField] private Transform _tierStarsContent;
    [SerializeField] private Image _boxImage;

    public void SetUp(int tier)
    {
        int starsContentLenght = _tierStarsContent.childCount;
        for (int i = 0; i < starsContentLenght; i++)
            _tierStarsContent.GetChild(i).GetComponent<Image>().enabled = (i < tier);

        _boxImage.sprite = (Sprite)Resources.Load<Sprite>("Images/shop_chest" + tier);
    }
}
