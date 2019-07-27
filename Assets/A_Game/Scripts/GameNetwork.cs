using Enigma.CoreSystems;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using Tiny;
using UnityEngine;

public class GameNetwork : SingletonMonobehaviour<GameNetwork>
{
    public delegate void OnSendResultsDelegate(string message);
    private OnSendResultsDelegate _onSendResultsCallback;
    private Hashtable _matchResultshashTable = new Hashtable();

    [SerializeField] private float _retrySendingResultsDelay = 5;

    public class Transactions
    {
        public const int CHANGE_RATING = 15;
    }

    public class TransactionKeys
    {
        public const string RATING = "rating";
        public const string PLAYER_ID = "player_id";
        public const string UNITS_KILLED = "units_killed";
        public const string DAMAGE_DEALT = "damage_dealt";
        public const string DAMAGE_RECEIVED = "damage_received";
        public const string MATCH_DURATION = "match_duration";
    }

    public class ServerResponseMessages
    {
        public const string SUCCESS = "Success";
        public const string SERVER_ERROR = "Server Error";
        public const string CONNECTION_ERROR = "Connection Error";
    }

    public class PlayerCustomProperties
    {
        public const string RATING = "Rating";
    }

    public void SendMatchResults(War.MatchData matchData,  OnSendResultsDelegate onSendMatchResultsCallback)
    {
        _onSendResultsCallback = onSendMatchResultsCallback;

        _matchResultshashTable.Clear();
        _matchResultshashTable.Add(TransactionKeys.PLAYER_ID, matchData.PlayerId);
        _matchResultshashTable.Add(TransactionKeys.MATCH_DURATION, matchData.MatchDuration);
        _matchResultshashTable.Add(TransactionKeys.DAMAGE_DEALT, matchData.DamageDealt);
        _matchResultshashTable.Add(TransactionKeys.DAMAGE_RECEIVED, matchData.DamageReceived);
        _matchResultshashTable.Add(TransactionKeys.MATCH_DURATION, matchData.MatchDuration);
        _matchResultshashTable.Add(TransactionKeys.UNITS_KILLED, matchData.UnitsKilled);

        performResultsSendingTransaction();
    }

    private void performResultsSendingTransaction()
    {
        NetworkManager.Transaction(Transactions.CHANGE_RATING, _matchResultshashTable, onSendMatchResults);
    }

    private void onSendMatchResults(JSONNode response)
    {
        if (response != null)
        {
            Debug.Log("onSendMatchResults -> response: " + response.ToString());

            JSONNode response_hash = response[0];
            string status = response_hash[NetworkManager.TransactionKeys.STATUS].ToString().Trim('"');

            if (status == NetworkManager.StatusOptions.SUCCESS)
            {
                int updatedRating = response_hash[GameNetwork.TransactionKeys.RATING].AsInt;
                SetRating(updatedRating);
                _onSendResultsCallback(ServerResponseMessages.SUCCESS);
            }
            else
            {
                StartCoroutine(retryResultsSending());
                _onSendResultsCallback(ServerResponseMessages.SERVER_ERROR);
            }
        }
        else
        {
            StartCoroutine(retryResultsSending());
            _onSendResultsCallback(ServerResponseMessages.CONNECTION_ERROR);
        }
    }

    private IEnumerator retryResultsSending()
    {
        yield return new WaitForSeconds(_retrySendingResultsDelay);
        performResultsSendingTransaction();
    }

    public int GetRating()
    {
        return (int)NetworkManager.GetCustomProperty(PlayerCustomProperties.RATING);
    }

    public void SetRating(int newRating)
    {
        NetworkManager.SetCustomProperty(PlayerCustomProperties.RATING, newRating);
    }

    public void SetValue(string key, string val)
    {
        NetworkManager.SetUserInfo(key, val);
    }

    public void SetValue(string key, string[] val)
    {
        string value = "";
        if (val.Length == 0)
            value = Json.Encode(val);

        NetworkManager.SetUserInfo(key, value);
    }

    public string GetValue(string key, string defaultValue = "")
    {
        return NetworkManager.GetUserInfo(key, defaultValue);
    }

    public string[] GetValueArray(string key, string[] defaultValue = null)
    {
        string value = NetworkManager.GetUserInfo(key);

        if ((value == "") && (defaultValue != null))
            return defaultValue;
        else
            return Json.Decode<string[]>(value);
    }
}
