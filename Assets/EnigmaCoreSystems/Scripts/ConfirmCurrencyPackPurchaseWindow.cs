using UnityEngine;
using UnityEngine.UI;

public class ConfirmCurrencyPackPurchaseWindow : MonoBehaviour
{
    public delegate void OnCurrencyPackPurchaseDelegate(CurrencyPack currencyPack);
    public OnCurrencyPackPurchaseDelegate OnCurrencyPackPurchaseCallback;

    [SerializeField] private Text _currencyPackAmount;
    [SerializeField] private Text _descriptionText;
    [SerializeField] private Text _dollarValueText;

    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private CurrencyPack _selectedCurrencyPack;

    private void Awake()
    {
        _confirmButton.onClick.AddListener(() => { onConfirmButtonDown(); });
        _cancelButton.onClick.AddListener(() => { onCancelButtonDown(); });
    }

    public void Open(CurrencyPack currencyPack)
    {
        _selectedCurrencyPack = currencyPack;

        _currencyPackAmount.text = _selectedCurrencyPack.GetCurrencyAmountString();
        _descriptionText.text = _selectedCurrencyPack.GetDescription();
        _dollarValueText.text = _selectedCurrencyPack.GetDollarValueString();

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void onConfirmButtonDown()
    {
        if (OnCurrencyPackPurchaseCallback != null)
            OnCurrencyPackPurchaseCallback(_selectedCurrencyPack);

        Close();
    }

    private void onCancelButtonDown()
    {
        Close();
    }
}

