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

    public class MatchStats
    {
        public const string DAMAGE_DEALT = "DamageDealt";
        public const string DAMAGE_RECEIVED = "DamageReceived";
        public const string UNITS_KILLED = "UnitsKilled";
        public const string MATCH_DURATION = "MatchDuration";
        public const string ALLIES_TOTAL_HEALTH = "AlliesTotalHealth";
        public const string ENEMIES_TOTAL_HEALTH = "EnemiesTotalHealth";
    }

    public class Teams
    {
        public const string ALLIES = "Allies";
        public const string ENEMIES = "Enemies";
    }

    public class UnitRoomStats
    {
        public const string HEALTH = "Health";
    }

    public class UnitPlayerStats
    {
        public const string LEVEL = "Level";
        public const string STRENGHT = "Strenght";
        public const string DEFENSE = "Defense";
        public const string EFFECT_SCALE = "EffectScale";
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
                SetLocalPlayerRating(updatedRating);
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

    public void setRoomUnitHealth(string team, string unitName, int value, List<MinMinUnit> teamUnitsForTotalCalculus = null)
    {
        SetRoomUnitStat(team, UnitRoomStats.HEALTH, unitName, value);

        if (teamUnitsForTotalCalculus != null)
        {
            int healthTotal = 0;
            foreach (MinMinUnit unit in teamUnitsForTotalCalculus)
                healthTotal += GetRoomUnitStat(team, UnitRoomStats.HEALTH, unit.name);

            SetRoomMatchStat(team, UnitRoomStats.HEALTH, healthTotal);
        }
    }

    public void SetRoomUnitStat(string team, string stat, string unitName, int value)
    {
        NetworkManager.SetRoomCustomProperty(team + stat + unitName, value);
    }

    public int GetRoomUnitStat(string team, string stat, string unitName)
    {
        return NetworkManager.GetRoomCustomPropertyAsInt(team + stat + unitName);
    }

    public void SetRoomMatchStat(string team, string stat, int value)
    {
        NetworkManager.SetRoomCustomProperty(team + stat, value);
    }

    public int GetRoomMatchStat(string team, string stat)
    {
        return NetworkManager.GetRoomCustomPropertyAsInt(team + stat);
    }

    public void SetLocalPlayerUnitStat(string stat, string unitName, int value)
    {
        SetAnyPlayerUnitStat(stat, unitName, value, GetLocalPlayerPhotonView());
    }

    public void SetAnyPlayerUnitStat(string stat, string unitName, int value, PhotonView photonView)
    {
        NetworkManager.SetAnyPlayerCustomProperty(stat + unitName, value.ToString(), photonView);
    }

    public int GetLocalPlayerUnitStat(string stat, string unitName)
    {
        return GetAnyPlayerUnitStat(stat, unitName, GetLocalPlayerPhotonView());
    }

    public int GetAnyPlayerUnitStat(string stat, string unitName, PhotonView photonView)
    {
        return NetworkManager.GetAnyPlayerCustomPropertyAsInt(stat + unitName, photonView);
    }

    public int GetLocalPlayerRating()
    {
        return NetworkManager.GetLocalPlayerCustomPropertyAsInt(PlayerCustomProperties.RATING);
    }

    public void SetLocalPlayerRating(int newRating)
    {
        NetworkManager.SetLocalPlayerCustomProperty(PlayerCustomProperties.RATING, newRating.ToString());
    }

    public PhotonView GetLocalPlayerPhotonView()
    {
        return NetworkManager.GetLocalPlayerPhotonView();
    }

    public void SetLocalPlayerCustomPropertyArray(string key, string[] val)
    {
        string value = "";
        if (val.Length == 0)
            value = Json.Encode(val);

        NetworkManager.SetLocalPlayerCustomProperty(key, value);
    }

    public string[] GetLocalPlayerCustomPropertyArray(string key)
    {
        string value = NetworkManager.GetLocalPlayerCustomProperty(key);
        return Json.Decode<string[]>(value);
    }
}
