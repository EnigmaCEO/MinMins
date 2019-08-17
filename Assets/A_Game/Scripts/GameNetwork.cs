using Enigma.CoreSystems;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using Tiny;
using UnityEngine;

public class GameNetwork : SingletonMonobehaviour<GameNetwork>
{
    public delegate void OnSendResultsDelegate(string message, int updatedRating);
    private OnSendResultsDelegate _onSendResultsCallback;

    public delegate void OnTeamTurnStartedDelegate(string turnTeam);
    public OnTeamTurnStartedDelegate OnTeamTurnStartedCallback;

    public delegate void OnUnitTurnStartedDelegate(string unitName);
    public OnUnitTurnStartedDelegate OnUnitTurnStartedCallback;

    public delegate void OnUnitHealthSetDelegate(string team, string unitName, int health);
    public OnUnitHealthSetDelegate OnUnitHealthSetCallback;

    public delegate void OnUnitPropertySetDelegate(string team, string unitName, int value);
    public OnUnitPropertySetDelegate OnUnitPropertySetCallback;

    public delegate void OnTeamHealthSetDeleate(string team, int health);
    public OnTeamHealthSetDeleate OnTeamHealthSetCallback;

    public delegate void OnPlayerRatingSetDelegate(int rating);
    public OnPlayerRatingSetDelegate OnPlayerRatingSetCallback;

    public delegate void OnPlayerTeamUnitsSetDelegate(string teamUnits);
    public OnPlayerTeamUnitsSetDelegate OnPlayerTeamUnitsSetCallback;

    public delegate void OnUnitPlayerPropertySetDelegate(string property, string value);
    public OnUnitPlayerPropertySetDelegate OnUnitPlayerPropertySetCallback;

    [SerializeField] private int TeamsAmount = 2;
    [SerializeField] private int TeamSize = 6;

    [SerializeField] private float _statIncreaseByLevel = 1.1f;
    [SerializeField] private float _retrySendingResultsDelay = 5;
    [SerializeField] List<int> _experienceNeededPerUnitLevel = new List<int>() { 10, 30, 70, 150, 310 };  //Rule: Doubles each level
    [SerializeField] List<int> _ratingsForArena = new List<int>() { 0, 300, 600, 900, 1200, 1500 };


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
        public const string TEAM_UNITS = "UnitNames";
    }

    public class RoomCustomProperties
    {
        public const string TEAM_IN_TURN = "TeamInTurn";
        public const string UNIT_IN_TURN = "UnitInTurn";
        public const string START_COUNT_DOWN_TIMER = "st";
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
        public const string UNIT_HEALTH = "Unit Health";
    }

    public class TeamRoomProperties
    {
        public const string TEAM_HEALTH = "Team Health";
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

        NetworkManager.OnPlayerCustomPropertiesChangedCallback += OnPlayerCustomPropertiesChanged;
        NetworkManager.OnRoomCustomPropertiesChangedCallback += OnRoomCustomPropertiesChanged;
    }

    private void OnDestroy()
    {
        NetworkManager.OnPlayerCustomPropertiesChangedCallback -= OnPlayerCustomPropertiesChanged;
        NetworkManager.OnRoomCustomPropertiesChangedCallback -= OnRoomCustomPropertiesChanged;
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
                        team += NetworkManager.Separators.VALUES;

                    team += unitName;
                }
            }
            teams.Add(team);
        }

        NetworkManager.SetLocalPlayerCustomProperty(PlayerCustomProperties.TEAM_UNITS, teams[0], GameConstants.VirtualPlayerIds.ALLIES);
        NetworkManager.SetLocalPlayerCustomProperty(PlayerCustomProperties.TEAM_UNITS, teams[1], GameConstants.VirtualPlayerIds.ENEMIES);
    }

    public void OnRoomCustomPropertiesChanged(Hashtable updatedProperties)
    {
        foreach (string key in updatedProperties.Keys)
        {
            string[] keyParts = key.Split(NetworkManager.Separators.KEYS);
            string idPart = keyParts[0];
            string value = (string)updatedProperties[key];

            if (idPart == RoomCustomProperties.TEAM_IN_TURN)
            {
                if (OnTeamTurnStartedCallback != null)
                    OnTeamTurnStartedCallback(value);
            }
            else if (idPart == RoomCustomProperties.UNIT_IN_TURN)
            {
                if (OnUnitTurnStartedCallback != null)
                    OnUnitTurnStartedCallback(value);
            }
            else if (idPart == UnitRoomProperties.UNIT_HEALTH)
            {
                string team = keyParts[1];
                string unitName = keyParts[2];

                if (OnUnitHealthSetCallback != null)
                    OnUnitHealthSetCallback(team, unitName, int.Parse(value));
            }
            else if (idPart == TeamRoomProperties.TEAM_HEALTH)
            {
                string team = keyParts[1];

                if(OnTeamHealthSetCallback != null)
                    OnTeamHealthSetCallback(team, int.Parse(value));
            }
        }
    }

    public void OnPlayerCustomPropertiesChanged(PhotonPlayer player, Hashtable updatedProperties)
    {
        foreach (string key in updatedProperties.Keys)
        {
            string[] keyParts = key.Split(NetworkManager.Separators.KEYS);
            string idPart = keyParts[0];
            string value = (string)updatedProperties[key];

            if (idPart == PlayerCustomProperties.RATING)
            {
                if (OnPlayerRatingSetCallback != null)
                    OnPlayerRatingSetCallback(int.Parse(value));
            }
            else if (idPart == PlayerCustomProperties.TEAM_UNITS)
            {
                if (OnPlayerTeamUnitsSetCallback != null)
                    OnPlayerTeamUnitsSetCallback(value);
            }
            //else if((idPart == UnitPlayerProperties.LEVEL) || (idPart == UnitPlayerProperties.POSITION) ||
            //        (idPart == UnitPlayerProperties.STRENGHT) || (idPart == UnitPlayerProperties.DEFENSE) ||
            //        (idPart == UnitPlayerProperties.EFFECT_SCALE))
            //{
            //    if (OnUnitPlayerPropertySetCallback != null)
            //        OnUnitPlayerPropertySetCallback(idPart, value);
            //}
        }
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

    public int GetLocalPlayerPvpLevelNumber()
    {
        int rating = GetLocalPlayerRating(GameConstants.VirtualPlayerIds.ALLIES);
        return GetPvpLevelNumberByRating(rating);
    }

    public int GetPvpLevelNumberByRating(int rating)
    {
        int levelNumber = 0;

        int arenasLenght = _ratingsForArena.Count;
        for (int i = (arenasLenght - 1); i >= 0; i--)
        {
            if (rating >= _ratingsForArena[i])
            {
                levelNumber = i + 1;
                break;
            }
        }

        return levelNumber;
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
                _onSendResultsCallback(ServerResponseMessages.SUCCESS, updatedRating);
            }
            else
            {
                StartCoroutine(retryResultsSending());
                _onSendResultsCallback(ServerResponseMessages.SERVER_ERROR, -1);
            }
        }
        else
        {
            StartCoroutine(retryResultsSending());
            _onSendResultsCallback(ServerResponseMessages.CONNECTION_ERROR, -1);
        }
    }

    private IEnumerator retryResultsSending()
    {
        yield return new WaitForSeconds(_retrySendingResultsDelay);
        performResultsSendingTransaction();
    }

    public void SetUnitHealth(string team, string unitName, int value, List<MinMinUnit> teamUnitsForTotalCalculus = null)
    {
        SetRoomUnitStat(UnitRoomProperties.UNIT_HEALTH, team, unitName, value);

        if (teamUnitsForTotalCalculus != null)
        {
            int healthTotal = 0;
            foreach (MinMinUnit unit in teamUnitsForTotalCalculus)
                healthTotal += GetRoomUnitStat(UnitRoomProperties.UNIT_HEALTH, team, unit.name);

            SetRoomTeamStat(UnitRoomProperties.UNIT_HEALTH, team, healthTotal);
        }
    }

    public void SetRoomUnitStat(string stat, string team, string unitName, int value)
    {
        NetworkManager.SetRoomCustomProperty(stat + NetworkManager.Separators.KEYS + team + NetworkManager.Separators.KEYS + unitName, value);
    }

    public int GetRoomUnitStat(string stat, string team, string unitName)
    {
        return NetworkManager.GetRoomCustomPropertyAsInt(stat + NetworkManager.Separators.KEYS + team + NetworkManager.Separators.KEYS + unitName);
    }

    public void SetRoomTeamStat(string stat, string team,  int value)
    {
        NetworkManager.SetRoomCustomProperty(stat + NetworkManager.Separators.KEYS + team, value);
    }

    public int GetRoomTeamStat(string property, string team)
    {
        return NetworkManager.GetRoomCustomPropertyAsInt(property + NetworkManager.Separators.KEYS + team);
    }

    public void ClearTeamUnits(string virtualPlayerId)
    {
        string[] teamUnits = GetLocalPlayerTeamUnits(virtualPlayerId);
        foreach (string unitName in teamUnits)
        {
            SetLocalPlayerUnitProperty(UnitPlayerProperties.LEVEL, unitName, null, virtualPlayerId);
            SetLocalPlayerUnitProperty(UnitPlayerProperties.STRENGHT, unitName, null, virtualPlayerId);
            SetLocalPlayerUnitProperty(UnitPlayerProperties.DEFENSE, unitName, null, virtualPlayerId);
            SetLocalPlayerUnitProperty(UnitPlayerProperties.EFFECT_SCALE, unitName, null, virtualPlayerId);
            SetLocalPlayerUnitProperty(UnitPlayerProperties.POSITION, unitName, null, virtualPlayerId);
        }

        NetworkManager.SetLocalPlayerCustomProperty(PlayerCustomProperties.TEAM_UNITS, null, virtualPlayerId);
    }

    public string[] GetLocalPlayerTeamUnits(string virtualPlayerId)
    {
        string teamString = NetworkManager.GetLocalPlayerCustomProperty(PlayerCustomProperties.TEAM_UNITS, virtualPlayerId);
        return teamString.Split(NetworkManager.Separators.VALUES);
    }

    public void SetLocalPlayerUnitProperty(string property, string unitName, string value, string virtualPlayerId)
    {
        SetAnyPlayerUnitProperty(property, unitName, value, virtualPlayerId, GetLocalPlayer());
    }

    public void SetAnyPlayerUnitProperty(string property, string unitName, string value, string virtualPlayerId, PhotonPlayer player)
    {
        NetworkManager.SetAnyPlayerCustomProperty(property + NetworkManager.Separators.KEYS + unitName, value, virtualPlayerId, player);
    }

    public string GetLocalPlayerUnitProperty(string property, string unitName, string virtualPlayerId)
    {
        return GetAnyPlayerUnitProperty(property, unitName, virtualPlayerId, GetLocalPlayer());
    }

    public int GetLocalPlayerUnitPropertyAsInt(string property, string unitName, string virtualPlayerId)
    {
        return GetAnyPlayerUnitPropertyAsInt(property, unitName, virtualPlayerId, GetLocalPlayer());
    }

    public int GetAnyPlayerUnitPropertyAsInt(string property, string unitName, string virtualPlayerId, PhotonPlayer player)
    {
        return NetworkManager.GetAnyPlayerCustomPropertyAsInt(property + NetworkManager.Separators.KEYS + unitName, virtualPlayerId, player);
    }

    public string GetAnyPlayerUnitProperty(string property, string unitName, string virtualPlayerId, PhotonPlayer player)
    {
        return NetworkManager.GetAnyPlayerCustomProperty(property + NetworkManager.Separators.KEYS + unitName, virtualPlayerId, player);
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

    public void SetTeamInTurn(string team)
    {
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.TEAM_IN_TURN, team);
    }

    public string GetTeamInTurn()
    {
        return NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.TEAM_IN_TURN);
    }

    public void SetUnitInTurn(string unitName)
    {
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.UNIT_IN_TURN, unitName);
    }

    public string GetUnitInTurn()
    {
        return NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.UNIT_IN_TURN);
    }
}
