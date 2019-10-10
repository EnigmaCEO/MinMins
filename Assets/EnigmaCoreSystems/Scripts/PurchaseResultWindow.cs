using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseResultWindow : MonoBehaviour
{
    [SerializeField] private Text _messageText;

#if (UNITY_ANDROID || UNITY_IOS)
    [SerializeField] private Button _freeCoinsButton;
#endif

    private void Start()
    {
        _freeCoinsButton.gameObject.SetActive(false);
    }

    public void Open(string localizationTerm)
    {
#if (UNITY_ANDROID || UNITY_IOS)
        _freeCoinsButton.gameObject.SetActive((localizationTerm == "NotEnoughCurrency") || (localizationTerm == "PurchaseFailed"));
#endif

        LocalizationManager.LocalizeText(_messageText, localizationTerm);
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnCloseButtonDown()
    {
        Close();
    }

    public void OnFreeCoinsButtonDown()
    {
        IAPManager.Instance.ShowOfferwall();
    }
}
