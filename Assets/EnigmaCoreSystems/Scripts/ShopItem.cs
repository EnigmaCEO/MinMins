using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [HideInInspector] public GameEnums.Powers Power;

    [SerializeField] private Text _amountText;
    [SerializeField] private Text _currencyValueDescriptionText;
    [SerializeField] private Text _dealDescriptionText;

    [SerializeField] private Image _iconImage;

    private int _powerUpAmount;
    private int _currencyValue;
    private float _priceFactor;
    private string _currencyValueDescriptionSuffix;

    public void SetPowerUpAmount(int amount)
    {
        _powerUpAmount = amount;
        _amountText.text = " x " + amount.ToString();
        gameObject.name = amount + "_Item";
    }

    public int GetPowerUpAmount()
    {
        return _powerUpAmount;
    }

    public void SetCurrencyValueDescriptionSuffix(string currencyValueDescriptionSuffix)
    {
        _currencyValueDescriptionSuffix = currencyValueDescriptionSuffix;
        updateCurrencyValueDescription();
    }

    public void SetPriceFactor(float priceFactor)
    {
        _priceFactor = priceFactor;
        updateCurrencyValueDescription();
    }

    public void SetCurrencyValue(int value)
    {
        _currencyValue = value;
        updateCurrencyValueDescription();
    }

    public string GetCurrencyValueString()
    {
        return GetCurrencyValueInt().ToString();
    }

    public int GetCurrencyValueInt()
    {
        return Mathf.RoundToInt(_priceFactor * _currencyValue);
    }

    public void SetDealDescriptionTerm(string term)
    {
        LocalizationManager.LocalizeText(_dealDescriptionText, term);
    }

    //public void SetDealDescriptionText(string text)
    //{
    //    _dealDescriptionText.text = text;
    //}

    public void SetIconSprite(Sprite sprite)
    {
        _iconImage.sprite = sprite;
    }

    public Image GetIconImage()
    {
        return _iconImage;
    }

    private void updateCurrencyValueDescription()
    {
        _currencyValueDescriptionText.text = " = " + GetCurrencyValueString() + _currencyValueDescriptionSuffix;
    }
}
