using UnityEngine;
using UnityEngine.UI;

public class ConfirmItemPurchaseWindow : MonoBehaviour
{
    public delegate void OnItemPurchaseDelegate(ShopItem item);
    public OnItemPurchaseDelegate OnItemPurchaseCallback;

    [SerializeField] private Text _descriptionText;
    [SerializeField] private Text _currencyValueText;

    [SerializeField] private Image _itemIcon;

    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    private ShopItem _selectedShopItem;

    private void Awake()
    {
        _confirmButton.onClick.AddListener(() => { onConfirmButtonDown(); });
        _cancelButton.onClick.AddListener(() => { onCancelButtonDown(); });
    }

    public void Open(ShopItem selectedItem)
    {
        _selectedShopItem = selectedItem;

        _descriptionText.text = _selectedShopItem.Power.ToString() + " x " + selectedItem.GetPowerUpAmount();
        _currencyValueText.text = _selectedShopItem.GetCurrencyValueString();
        _itemIcon.sprite = _selectedShopItem.GetIconImage().sprite;

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void onConfirmButtonDown()
    {
        if (OnItemPurchaseCallback != null)
            OnItemPurchaseCallback(_selectedShopItem);

        Close();
    }

    private void onCancelButtonDown()
    {
        Close();
    }
}
