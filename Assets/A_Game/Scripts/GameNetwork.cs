using Enigma.CoreSystems;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using Tiny;
using UnityEngine;

public class GameNetwork : SingletonMonobehaviour<GameNetwork>
{
    public delegate void OnSendResultsDelegate(string message);

    [SerializeField] private int TeamsAmount = 2;
    [SerializeField] private int TeamSize = 6;

    [SerializeField] private float _statIncreaseByLevel = 1.1f;
    [SerializeField] private float _retrySendingResultsDelay = 5;
    [SerializeField] List<int> _experienceNeededPerUnitLevel = new List<int>() { 10, 30, 70, 150, 310 };  //Rule: Doubles each level

    private OnSendResultsDelegate _onSendResultsCallback;
    private Hashtable _matchResultshashTable = new Hashtable();

    public class Separators
    {
        public const char PARSE = '|';
        public const char UNIT_PROPERTY = '-';
    }

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
        public const string UNIT_NAMES = "UnitNames";
    }

    //public class MatchStats
    //{
    //    public const string DAMAGE_DEALT = "DamageDealt";
    //    public const string DAMAGE_RECEIVED = "DamageReceived";
    //    public const string UNITS_KILLED = "UnitsKilled";
    //    public const string MATCH_DURATION = "MatchDuration";
    //    public const string ALLIES_TOTAL_HEALTH = "AlliesTotalHealth";
    //    public const string ENEMIES_TOTAL_HEALTH = "EnemiesTotalHealth";
    //}

    public class UnitRoomProperties
    {
        public const string HEALTH = "Health";
    }

    public class UnitPlayerProperties
    {
        public const string POSITION = "Position";

        public const string LEVEL = "Level";
        public const string STRENGHT = "Strenght";
        public const string DEFENSE = "Defense";
        public const string EFFECT_SCALE = "EffectScale";
    }

    private void Awake()
    {
        StoreDefaultTeams();
    }

    public void StoreDefaultTeams()
    {
        List<string> teams = new List<string>();
        for (int i = 0; i < TeamsAmount; i++)
        {
            string team = "";

            for (int j = 0; j < TeamSize; j++)
            {
                if (j < (TeamSize - 1)) //Last one is Locked
                {
                    string unitName = "";
                    unitName = (j + 1).ToString();  //default values

                    if (j != 0)
                        team += Separators.PARSE;

                    team += unitName;
                }
            }

            teams.Add(team);
        }

        NetworkManager.SetLocalPlayerCustomProperty(PlayerCustomProperties.UNIT_NAMES, teams[0], GameConstants.VirtualPlayerIds.ALLIES);
        NetworkManager.SetLocalPlayerCustomProperty(PlayerCustomProperties.UNIT_NAMES, teams[1], GameConstants.VirtualPlayerIds.ENEMIES);
    }

    public void SendMatchResults(War.MatchData matchData, OnSendResultsDelegate onSendMatchResultsCallback)
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

    public int GetMaxUnitExperience()
    {
        int maxLevelIndexToCheck = _experienceNeededPerUnitLevel.Count - 2;
        int maxExp = _experienceNeededPerUnitLevel[maxLevelIndexToCheck + 1];
        return maxExp;
    }

    public void BuildUnitLevels(string unitName, int unitExp, string virtualPlayerId)
    {
        int unitLevel = 1;
        int maxLevelIndexToCheck = _experienceNeededPerUnitLevel.Count - 2;

        for (int i = maxLevelIndexToCheck; i >= 0; i--)
        {
            if (unitExp >= _experienceNeededPerUnitLevel[i])
            {
                unitLevel = i + 1;
                break;
            }
        }

        MinMinUnit minMin = GameInventory.Instance.GetMinMinFromResources(unitName);
        SetLocalPlayerUnitProperty(UnitPlayerProperties.LEVEL, unitName, unitLevel.ToString(), virtualPlayerId);
        SetLocalPlayerUnitProperty(UnitPlayerProperties.STRENGHT, unitName, getStatByLevel(minMin.Strength, unitLevel), virtualPlayerId);
        SetLocalPlayerUnitProperty(UnitPlayerProperties.DEFENSE, unitName, getStatByLevel(minMin.Defense, unitLevel), virtualPlayerId);
        SetLocalPlayerUnitProperty(UnitPlayerProperties.EFFECT_SCALE, unitName, getStatByLevel(minMin.EffectScale, unitLevel), virtualPlayerId);
    }

    private string getStatByLevel(int baseValue, float level)
    {
        return (Mathf.RoundToInt((float)baseValue * _statIncreaseByLevel * level)).ToString();
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
                int updatedRating = response_hash[TransactionKeys.RATING].AsInt;
                SetLocalPlayerRating(updatedRating, GameConstants.VirtualPlayerIds.ALLIES);
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
        SetRoomUnitStat(team, unitName, UnitRoomProperties.HEALTH, value);

        if (teamUnitsForTotalCalculus != null)
        {
            int healthTotal = 0;
            foreach (MinMinUnit unit in teamUnitsForTotalCalculus)
                healthTotal += GetRoomUnitStat(team, unit.name, UnitRoomProperties.HEALTH);

            SetRoomMatchStat(team, UnitRoomProperties.HEALTH, healthTotal);
        }
    }

    public void SetRoomUnitStat(string team, string unitName, string stat, int value)
    {
        NetworkManager.SetRoomCustomProperty(team + unitName + stat, value);
    }

    public int GetRoomUnitStat(string team, string unitName, string stat)
    {
        return NetworkManager.GetRoomCustomPropertyAsInt(team + unitName + stat);
    }

    public void SetRoomMatchStat(string team, string stat, int value)
    {
        NetworkManager.SetRoomCustomProperty(team + stat, value);
    }

    public int GetRoomMatchStat(string team, string property)
    {
        return NetworkManager.GetRoomCustomPropertyAsInt(team + property);
    }

    public void ClearTeamUnits(string virtualPlayerId)
    {
        string[] teamUnits = GetLocalPlayerTeamUnits(virtualPlayerId);
        foreach (string unitName in teamUnits)
        {
            SetLocalPlayerUnitProperty(unitName, UnitPlayerProperties.LEVEL, null, virtualPlayerId);
            SetLocalPlayerUnitProperty(unitName, UnitPlayerProperties.STRENGHT, null, virtualPlayerId);
            SetLocalPlayerUnitProperty(unitName, UnitPlayerProperties.DEFENSE, null, virtualPlayerId);
            SetLocalPlayerUnitProperty(unitName, UnitPlayerProperties.EFFECT_SCALE, null, virtualPlayerId);
            SetLocalPlayerUnitProperty(unitName, UnitPlayerProperties.POSITION, null, virtualPlayerId);
        }

        NetworkManager.SetLocalPlayerCustomProperty(PlayerCustomProperties.UNIT_NAMES, null, virtualPlayerId);
    }

    public string[] GetLocalPlayerTeamUnits(string virtualPlayerId)
    {
        string teamString = NetworkManager.GetLocalPlayerCustomProperty(PlayerCustomProperties.UNIT_NAMES, virtualPlayerId);
        return teamString.Split(Separators.PARSE);
    }

    public void SetLocalPlayerUnitProperty(string unitName, string property, string value, string virtualPlayerId)
    {
        SetAnyPlayerUnitProperty(unitName, property, value, virtualPlayerId, GetLocalPlayer());
    }

    public void SetAnyPlayerUnitProperty(string unitName, string property, string value, string virtualPlayerId, PhotonPlayer player)
    {
        NetworkManager.SetAnyPlayerCustomProperty(unitName + Separators.UNIT_PROPERTY + property, value, virtualPlayerId, player);
    }

    public string GetLocalPlayerUnitProperty(string unitName, string property, string virtualPlayerId)
    {
        return GetAnyPlayerUnitProperty(unitName, property, virtualPlayerId, GetLocalPlayer());
    }

    public int GetLocalPlayerUnitPropertyAsInt(string unitName, string property, string virtualPlayerId)
    {
        return GetAnyPlayerUnitPropertyAsInt(unitName, property, virtualPlayerId, GetLocalPlayer());
    }

    public int GetAnyPlayerUnitPropertyAsInt(string unitName, string property, string virtualPlayerId, PhotonPlayer player)
    {
        return NetworkManager.GetAnyPlayerCustomPropertyAsInt(unitName + Separators.UNIT_PROPERTY + property, virtualPlayerId, player);
    }

    public string GetAnyPlayerUnitProperty(string unitName, string property, string virtualPlayerId, PhotonPlayer player)
    {
        return NetworkManager.GetAnyPlayerCustomProperty(unitName + Separators.UNIT_PROPERTY + property, virtualPlayerId, player);
    }

    public int GetLocalPlayerRating(string virtualPlayerId)
    {
        return 10; //TODO: Remove hack when rating is received from Login response
        //return NetworkManager.GetLocalPlayerCustomPropertyAsInt(PlayerCustomProperties.RATING, virtualPlayerId);
    }

    public void SetLocalPlayerRating(int newRating, string virtualPlayerId)
    {
        NetworkManager.SetLocalPlayerCustomProperty(PlayerCustomProperties.RATING, newRating.ToString(), virtualPlayerId);
    }

    public int GetAnyPlayerRating(PhotonPlayer player, string virtualPlayerId)
    {
        return NetworkManager.GetAnyPlayerCustomPropertyAsInt(PlayerCustomProperties.RATING, virtualPlayerId, player);
    }

    public PhotonPlayer GetLocalPlayer()
    {
        return NetworkManager.GetLocalPlayer();
    }

    //public void SetLocalPlayerCustomPropertyArray(string key, string[] val)
    //{
    //    string value = "";
    //    if (val.Length == 0)
    //        value = Json.Encode(val);

    //    NetworkManager.SetLocalPlayerCustomProperty(key, value);
    //}

    //public string[] GetLocalPlayerCustomPropertyArray(string key)
    //{
    //    string value = NetworkManager.GetLocalPlayerCustomProperty(key);
    //    return Json.Decode<string[]>(value);
    //}
}
