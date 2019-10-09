using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class CurrencyPack : MonoBehaviour
{
    [SerializeField] private Text _currencyAmountText;
    [SerializeField] private Text _descriptionText;
    [SerializeField] private Text _dollarValueText;
    [SerializeField] private Text _dealDescriptionText;

    private int _amount;
    private string _id;

    public void SetId(string id)
    {
        _id = id;
        gameObject.name = _id.ToString() + "_CurrencyPack";
    }

    public string GetId()
    {
        return _id;
    }

    public void SetLocalizeTerm(string term)
    {
        LocalizationManager.TranslateText(_descriptionText, term);
    }

    public string GetDescription()
    {
        return _descriptionText.GetComponent<Text>().text;
    }

    public void SetCurrencyAmount(int amount, string dealPercentage)
    {
        _amount = amount;
        _currencyAmountText.text = amount.ToString() + dealPercentage;
    }

    public string GetCurrencyAmountString()
    {
        return _currencyAmountText.text;
    }

    public int GetCurrencyAmountInt()
    {
        return _amount;
    }

    public void SetDollarValue(float value)
    {
        _dollarValueText.text = value.ToString() + " $";
    }

    public string GetDollarValueString()
    {
        return _dollarValueText.text;
    }

    public float GetDollarValueFloat()
    {
        return float.Parse(GetDollarValueString());
    }

    public void SetDealDescriptionTerm(string description)
    {
        LocalizationManager.TranslateText(_dealDescriptionText, description);
    }
}
