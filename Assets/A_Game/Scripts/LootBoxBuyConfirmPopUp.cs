using UnityEngine;
using UnityEngine.UI;

public class LootBoxBuyConfirmPopUp : MonoBehaviour
{
    public Button ConfirmButton;
    public Button CancelButton;

    [SerializeField] private Text _packNameText;
    [SerializeField] private Text _packNameDescriptionText;
    [SerializeField] private Text _packPriceText;

    [SerializeField] private Transform _tierStarsContent;

    [SerializeField] private Image _packImage;

    public void SetStars(int tier)
    {
        int starsContentLenght = _tierStarsContent.childCount;
        for (int i = 0; i < starsContentLenght; i++)
            _tierStarsContent.GetChild(i).GetComponent<Image>().enabled = (i < tier);
    }

    public void SetPackName(string name)
    {
        _packNameText.text = name;
    }

    public void SetPackDescription(string description)
    {
        _packNameDescriptionText.text = description;
    }

    public void SetPrice(string price)
    {
        _packPriceText.text = price + " USD";
    }

    public void SetPackSprite(Sprite sprite)
    {
        _packImage.sprite = sprite;
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
