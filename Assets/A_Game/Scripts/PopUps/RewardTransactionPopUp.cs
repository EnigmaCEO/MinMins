using Enigma.CoreSystems;
using GameConstants;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardTransactionPopUp : MonoBehaviour
{
    public class UiMessages
    {
        public const string GET_ENJIN_WALLET_DESCRIPTION = "Press OK to request an Enjin Wallet so you can withdraw or buy tokens. Close window to go back.";
        public const string JENJ_SPENDING_DESCRIPTION = "Tokens are sent to your wallet in exchange for JENJ. After clicking Start look for the wallet notification to approve the trade.";

        public const string TOKENIZING = "Tokenizing...";
        public const string REQUESTING_PURCHASE_LINK = "Requesting purchase link...";

        public const string WITHDRAWAL_COMPLETED = "Withdrawal completed!";

        public const string WITHDRAWAL_FAILED = "Withdrawal failed. Please try again.";
        public const string PURCHASE_FAILED = "Purchase failed. Please try again.";
    }

    public const string TOKENIZE_TITLE_TERM = "TOKENIZE ITEMS";
    public const string PURCHASE_TITLE_TERM = "PURCHASE ITEMS";

    [SerializeField] private string _getEnjinWalletURL = "https://enjin.io/software/wallet";
    [SerializeField] private float _transactionRetryDelay = 10;

    [SerializeField] private Text _titleText;
    [SerializeField] private Text _statusUiText;
    [SerializeField] private Text _descriptionText;

    [SerializeField] private GameObject _okButton;

    private RewardInventoryItem _tokenSelected = null;
    private Action _inventoryChangedCallback;
    private RewardsInventoryOptions _option;

    void Start()
    {
        Close();
    }

    public void SetInventoryChangedCallback(Action inventoryChangedCallback)
    {
        _inventoryChangedCallback = inventoryChangedCallback;
    }

    public void Open(RewardInventoryItem tokenSelected, RewardsInventoryOptions option)
    {
        _tokenSelected = tokenSelected;
        _option = option;

        if (GameNetwork.Instance.IsEnjinLinked)
        {
            _descriptionText.text = LocalizationManager.GetTermTranslation(UiMessages.JENJ_SPENDING_DESCRIPTION);
        }
        else
        {
            _descriptionText.text = LocalizationManager.GetTermTranslation(UiMessages.GET_ENJIN_WALLET_DESCRIPTION);
        }
        
        _descriptionText.gameObject.SetActive(true);

        _statusUiText.gameObject.SetActive(false);
        _okButton.SetActive(true);

        gameObject.SetActive(true);

        string titleTerm = TOKENIZE_TITLE_TERM;
        if (_option == RewardsInventoryOptions.Buy)
        {
            titleTerm = PURCHASE_TITLE_TERM;
            startTransaction();
        }

        _titleText.text = LocalizationManager.GetTermTranslation(titleTerm);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnOkButtonDown()
    {
        if (GameNetwork.Instance.IsEnjinLinked)
        {
            //if (GameStats.Instance.EnjBalance > 0)
            {
                startTransaction();
            }
        }
        else
        {
            Application.OpenURL(_getEnjinWalletURL);
        }
    }

    private void startTransaction()
    {
        _okButton.SetActive(false);

        _descriptionText.gameObject.SetActive(false);
        string status_UI_term = UiMessages.REQUESTING_PURCHASE_LINK;

        if (_option == RewardsInventoryOptions.Tokenize)
        {
            status_UI_term = UiMessages.TOKENIZING;
        }

        StartCoroutine(handleTransactionProcess(0));

        _statusUiText.text = LocalizationManager.GetTermTranslation(status_UI_term);
        _statusUiText.gameObject.SetActive(true);
    }

    public void OnCloseButtonDown()
    {
        StopAllCoroutines();
        Close();
    }

    private IEnumerator handleTransactionProcess(float delay)
    {
        Debug.Log("handleWithdrawalProcess");

        bool handleEnjinTransactionOnline = true;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.EnjinTransactionResponse.Enabled)
        {
            yield return new WaitForSeconds(GameHacks.Instance.OfflineEnjinTransactionDelay);

            string transactionStatus = GameHacks.Instance.EnjinTransactionResponse.Value;
            if (handleTransactionStatus(transactionStatus))
            {
                GameStats.Instance.EnjBalance -= _tokenSelected.Cost;
                _inventoryChangedCallback();
            }

            handleEnjinTransactionOnline = false;
        }
#endif

        if (handleEnjinTransactionOnline)
        {
            yield return new WaitForSeconds(delay);

            string tokenKey = _tokenSelected.TokenKey + "vEGI_MINMINSv" + GameStats.Instance.EnjBalance + NetworkManager.GetSessionID();

            string fileSec = NetworkManager.md5(tokenKey);

            Hashtable hashTable = new Hashtable();

            int transactionId;

            if (_option == RewardsInventoryOptions.Tokenize)
            {
                hashTable.Add(GameTransactionKeys.TOKEN_KEY, _tokenSelected.TokenKey);
                hashTable.Add(GameTransactionKeys.SEC_CODE, fileSec);
                hashTable.Add(GameTransactionKeys.ENJ_BALANCE, GameStats.Instance.EnjBalance);

                transactionId = EnigmaTransactions.ENJIN_WITHDRAWAL;
            }
            else // Buy
            {
                hashTable.Add(GameTransactionKeys.UUID, _tokenSelected.Uuid);
                transactionId = EnigmaTransactions.ENJIN_PURCHASE;
            }

            NetworkManager.Transaction(transactionId, hashTable, onEnjinTransactionServerResponse);
        }

        yield return 0;
    }

    private void onEnjinTransactionServerResponse(JSONNode response)
    {
        if (response == null)
        {
            Debug.LogError(nameof(onEnjinTransactionServerResponse) + " transaction got null.");
        }
        else
        {
            Debug.Log(nameof(onEnjinTransactionServerResponse) + " response was: " + response.ToString());

            JSONNode response_hash = response[0];
            if (response_hash[EnigmaNodeKeys.STATUS] == null)
            {
                Debug.LogError(nameof(onEnjinTransactionServerResponse) + " response didn't get status.");
            }
            else
            {
                string transactionStatus = response_hash[EnigmaNodeKeys.STATUS].ToString().Trim('"');
                if (handleTransactionStatus(transactionStatus))
                {
                    if (_option == RewardsInventoryOptions.Tokenize)
                    {
                        GameNetwork.Instance.UpdateEnjinGoodies(response_hash, false);
                        GameStats.Instance.EnjBalance -= _tokenSelected.Cost;  //Because it won't be updated in server response
                        _inventoryChangedCallback();
                    }
                    else // Buy
                    {
                        JSONNode urlNode = NetworkManager.CheckValidNode(response_hash, EnigmaNodeKeys.URL);
                        if (urlNode != null)
                        {
                            string url = urlNode.ToString().Trim('"');
                            Application.OpenURL(url);
                            Close();
                        }
                    }
                }
            }
        }
    }

    private bool handleTransactionStatus(string status)
    {
        bool isSuccess = false;
        bool isPopUpOpen = gameObject.activeSelf;

        //status = EnigmaServerStatuses.ERROR;  //Test

        if (status == EnigmaServerStatuses.SUCCESS)
        {
            if (isPopUpOpen)
            {
                if (_option == RewardsInventoryOptions.Tokenize)
                {
                    _statusUiText.text = LocalizationManager.GetTermTranslation(UiMessages.WITHDRAWAL_COMPLETED);
                }
            }

            isSuccess = true;
        }
        else if(isPopUpOpen)
        {
            if (status == EnigmaServerStatuses.PENDING)
            {
                StartCoroutine(handleTransactionProcess(_transactionRetryDelay));
            }
            else //Error
            {
                string failureTerm = UiMessages.PURCHASE_FAILED;

                if (_option == RewardsInventoryOptions.Tokenize)
                {
                    failureTerm = UiMessages.WITHDRAWAL_FAILED;
                }

                _statusUiText.text = LocalizationManager.GetTermTranslation(failureTerm);
            }
        }

        return isSuccess;
    }
}
