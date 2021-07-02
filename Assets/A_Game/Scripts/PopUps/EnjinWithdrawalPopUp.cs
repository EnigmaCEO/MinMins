using Enigma.CoreSystems;
using GameConstants;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnjinWithdrawalPopUp : MonoBehaviour
{
    [SerializeField] private string _getEnjinWalletURL = "https://enjin.io/software/wallet";
    [SerializeField] private float _hackedWithdrawalDelay = 2;

    [SerializeField] private Text _statusUiText;
    [SerializeField] private Text _descriptionText;

    [SerializeField] private GameObject _okButton;

    private RewardInventoryItem _tokenSelected = null;
    private Action _inventoryChangedCallback;

    void Start()
    {
        Close();
    }

    public void SetInventoryChangedCallback(Action inventoryChangedCallback)
    {
        _inventoryChangedCallback = inventoryChangedCallback;
    }

    public void Open(RewardInventoryItem tokenSelected)
    {
        _tokenSelected = tokenSelected;

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
        if (GameNetwork.Instance.IsEnjinLinked)
        {
            _okButton.SetActive(false);

            _descriptionText.gameObject.SetActive(false);

            _statusUiText.text = LocalizationManager.GetTermTranslation(UiMessages.PERFORMING_WITHDRAWAL);
            _statusUiText.gameObject.SetActive(true);

            StartCoroutine(handleWithdrawalProcess());
        }
        else
        {
            Application.OpenURL(_getEnjinWalletURL);
        }
    }

    public void OnCloseButtonDown()
    {
        StopAllCoroutines();
        Close();
    }

    private IEnumerator handleWithdrawalProcess()
    {
        Debug.Log("handleWithdrawalProcess");

        bool handleWithdrawalOnline = true;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.WithdrawalResponse.Enabled)
        {
            yield return new WaitForSeconds(_hackedWithdrawalDelay);

            string withdrawalStatus = GameHacks.Instance.WithdrawalResponse.Value;
            if (handleWithdrawalStatus(withdrawalStatus))
            {
                GameStats.Instance.EnjBalance -= _tokenSelected.CostNumber;
                _inventoryChangedCallback();
            }

            handleWithdrawalOnline = false;
        }
#endif

        if (handleWithdrawalOnline)
        {
            NetworkManager.Transaction(GameNetwork.Transactions.ENJIN_WITHDRAWAL, GameNetwork.TransactionKeys.TOKEN_KEY, _tokenSelected.RewardCode, onWithdrawalServerResponse);
        }

        yield return 0;
    }

    private void onWithdrawalServerResponse(JSONNode response)
    {
        if (response == null)
        {
            Debug.LogError("onWithdrawalServerResponse transaction got null.");
        }
        else
        {
            Debug.Log("onWithdrawalServerResponse response was: " + response.ToString());

            JSONNode response_hash = response[0];
            if (response_hash[NetworkManager.TransactionKeys.STATUS] == null)
            {
                Debug.LogError("onWithdrawalServerResponse response didn't get status.");
            }
            else
            {
                string status = response_hash[NetworkManager.TransactionKeys.STATUS].ToString().Trim('"');
                if (handleWithdrawalStatus(status))
                {
                    GameNetwork.Instance.UpdateBalancesFromNode(response_hash);
                    _inventoryChangedCallback();
                }
            }
        }
    }

    private bool handleWithdrawalStatus(string status)
    {
        bool isSuccess = false;
        bool isPopUpOpen = gameObject.activeSelf;

        if (status == GameNetwork.ServerResponseMessages.SUCCESS)
        {
            if (isPopUpOpen)
            {
                _statusUiText.text = LocalizationManager.GetTermTranslation(UiMessages.WITHDRAWAL_COMPLETED);
            }

            isSuccess = true;
        }
        else if(isPopUpOpen)
        {
            if (status == GameNetwork.ServerResponseMessages.PENDING)
            {
                StartCoroutine(handleWithdrawalProcess());
            }
            else
            {
                _statusUiText.text = LocalizationManager.GetTermTranslation(UiMessages.WITHDRAWAL_FAILED);
            }
        }

        return isSuccess;
    }
}
