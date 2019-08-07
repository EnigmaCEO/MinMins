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

    public class TeamPlayerProperties
    {
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
        storeDefaultTeams();
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

    public void BuildUnitLevels(string unitName)
    {
        GameInventory gameInventory = GameInventory.Instance;
        int unitExp = gameInventory.GetAllyUnitExp(unitName);

        int unitLevel = 1;
        int maxLevelIndexToCheck = _experienceNeededPerUnitLevel.Count - 2;
        int maxExp = _experienceNeededPerUnitLevel[maxLevelIndexToCheck + 1];

        if (unitExp > maxExp)
            unitExp = maxExp;

        for (int i = maxLevelIndexToCheck; i >= 0; i++)
        {
            if (unitExp >= _experienceNeededPerUnitLevel[i])
            {
                unitLevel = i + 1;
                break;
            }
        }

        MinMinUnit minMin = gameInventory.GetMinMinFromResources(unitName);
        SetLocalPlayerUnitProperty(UnitPlayerProperties.LEVEL, unitName, unitLevel.ToString());
        SetLocalPlayerUnitProperty(UnitPlayerProperties.STRENGHT, unitName, getStatByLevel(minMin.Strength, unitLevel));
        SetLocalPlayerUnitProperty(UnitPlayerProperties.DEFENSE, unitName, getStatByLevel(minMin.Defense, unitLevel));
        SetLocalPlayerUnitProperty(UnitPlayerProperties.EFFECT_SCALE, unitName, getStatByLevel(minMin.EffectScale, unitLevel));
    }

    private string getStatByLevel(int baseValue, float level)
    {
        return (Mathf.RoundToInt((float)baseValue * _statIncreaseByLevel * level)).ToString();
    }

    private void storeDefaultTeams()
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
                        team += Constants.Separators.First;

                    team += unitName;
                }
            }

            teams.Add(team);
        }

        SetLocalPlayerTeamProperty(GameConstants.TeamNames.ALLIES, TeamPlayerProperties.UNIT_NAMES, teams[0]);
        SetLocalPlayerTeamProperty(GameConstants.TeamNames.ENEMIES, TeamPlayerProperties.UNIT_NAMES, teams[1]);
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

    public void ClearTeamUnits(string teamName)
    {
        string[] teamUnits = GetLocalPlayerTeamUnits(teamName);
        foreach (string unitName in teamUnits)
        {
            SetLocalPlayerUnitProperty(unitName, UnitPlayerProperties.LEVEL, null);
            SetLocalPlayerUnitProperty(unitName, UnitPlayerProperties.STRENGHT, null);
            SetLocalPlayerUnitProperty(unitName, UnitPlayerProperties.DEFENSE, null);
            SetLocalPlayerUnitProperty(unitName, UnitPlayerProperties.EFFECT_SCALE, null);
            SetLocalPlayerUnitProperty(unitName, UnitPlayerProperties.POSITION, null);
        }

        SetLocalPlayerTeamProperty(teamName, TeamPlayerProperties.UNIT_NAMES, null);
    }

    public string[] GetLocalPlayerTeamUnits(string teamName)
    {
        string teamString = GetLocalPlayerUnitProperty(teamName, TeamPlayerProperties.UNIT_NAMES);
        return teamString.Split(Constants.Separators.First);
    }

    public void SetLocalPlayerTeamProperty(string teamName, string property, string value)
    {
        SetAnyPlayerTeamProperty(teamName, property, value, GetLocalPlayerPhotonView());
    }

    public void SetAnyPlayerTeamProperty(string teamName, string property, string value, PhotonView photonView)
    {
        NetworkManager.SetAnyPlayerCustomProperty(teamName + property, value, photonView);
    }

    public string GetLocalPlayerTeamProperty(string teamName, string property)
    {
        return GetAnyPlayerTeamProperty(teamName, property, GetLocalPlayerPhotonView());
    }

    public string GetAnyPlayerTeamProperty(string teamName, string property, PhotonView photonView)
    {
        return NetworkManager.GetAnyPlayerCustomProperty(teamName + property, photonView);
    }

    public void SetLocalPlayerUnitProperty(string unitName, string property, string value)
    {
        SetAnyPlayerUnitProperty(unitName, property, value, GetLocalPlayerPhotonView());
    }

    public void SetAnyPlayerUnitProperty(string unitName, string property, string value, PhotonView photonView)
    {
        NetworkManager.SetAnyPlayerCustomProperty(unitName + property, value, photonView);
    }

    public string GetLocalPlayerUnitProperty(string unitName, string property)
    {
        return GetAnyPlayerUnitProperty(unitName, property, GetLocalPlayerPhotonView());
    }

    public int GetLocalPlayerUnitPropertyAsInt(string unitName, string property)
    {
        return GetAnyPlayerUnitPropertyAsInt(unitName, property, GetLocalPlayerPhotonView());
    }

    public int GetAnyPlayerUnitPropertyAsInt(string unitName, string property, PhotonView photonView)
    {
        return NetworkManager.GetAnyPlayerCustomPropertyAsInt(unitName + property, photonView);
    }

    public string GetAnyPlayerUnitProperty(string unitName, string property, PhotonView photonView)
    {
        return NetworkManager.GetAnyPlayerCustomProperty(unitName + property, photonView);
    }

    public int GetLocalPlayerRating()
    {
        return NetworkManager.GetLocalPlayerCustomPropertyAsInt(PlayerCustomProperties.RATING);
    }

    public void SetLocalPlayerRating(int newRating)
    {
        NetworkManager.SetLocalPlayerCustomProperty(PlayerCustomProperties.RATING, newRating.ToString());
    }

    public int GetAnyPlayerRating(PhotonView photonView)
    {
        return NetworkManager.GetAnyPlayerCustomPropertyAsInt(PlayerCustomProperties.RATING, photonView);
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
