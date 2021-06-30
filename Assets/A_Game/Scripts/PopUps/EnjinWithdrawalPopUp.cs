using GameConstants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnjinWithdrawalPopUp : MonoBehaviour
{
    [SerializeField] private Text _statusUiText;
    [SerializeField] private Text _descriptionText;

    [SerializeField] private GameObject _okButton;

    void Start()
    {
        Close();
    }

    public void Open(string tokenCode)
    {
        if (GameNetwork.Instance.IsEnjinLinked)
        {
            _descriptionText.text = LocalizationManager.GetTermTranslation(UiMessages.WITHDRAWAL_DESCRIPTION);
        }
        else
        {
            _descriptionText.text = LocalizationManager.GetTermTranslation(UiMessages.GET_ENJIN_WALLET_DESCRIPTION);
        }
        
        _descriptionText.gameObject.SetActive(true);

        _statusUiText.gameObject.SetActive(false);
        _okButton.SetActive(true);

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnOkButtonDown()
    {
        StartCoroutine(handleWithdrawalProcess());
    }

    public void OnCloseButtonDown()
    {
        StopAllCoroutines();
        Close();
    }

    private IEnumerator handleWithdrawalProcess()
    {
        _okButton.SetActive(false);

        _descriptionText.gameObject.SetActive(false);

        _statusUiText.text = UiMessages.PERFORMING_WITHDRAWL;
        _statusUiText.gameObject.SetActive(true);

        yield return 0;
    }
}
