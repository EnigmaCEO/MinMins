using Enigma.CoreSystems;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameNetwork : SingletonNetworkEntity<GameNetwork>
{
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
        public const string TEAM_UNITS = "Unit_Names";
    }

    public class RoomCustomProperties
    {
        public const string ROUND_COUNT = "Round_Count";
        public const string TEAM_IN_TURN = "Team_In_Turn";
        public const string HOST_UNIT_INDEX = "Host_Unit_Index";
        public const string GUEST_UNIT_INDEX = "Guest_Unit_Index";
        public const string ACTIONS_LEFT = "Actions_Left";
        public const string START_COUNT_DOWN_TIMER = "st";

        //public const string GAME_STATE = "gState";
        //public const string PLAYER_LIST = "playerList";
        //public const string HOST = "host";
        public const string HOST_RATING = "Host_Rating";
        public const string HOST_ID = "Host_Id";
        //public const string MAX_PLAYERS = "mp";
    }

    public class UnitRoomProperties
    {
        public const string HEALTH = "Unit_Health";
        public const string MAX_HEALTH = "Unit_Max_Health";
    }

    public class TeamRoomProperties
    {
        public const string HEALTH = "Team_Health";
        public const string MAX_HEALTH = "Team_Max_Health";
    }

    public class UnitPlayerProperties
    {
        public const string POSITION = "Position";
        public const string EXPERIENCE = "Experience";

        public const string LEVEL = "Level";
        public const string STRENGHT = "Strenght";
        public const string DEFENSE = "Defense";

        public const string EFFECT_SCALE = "EffectScale";
    }

    public class TeamNames
    {
        public const string HOST = "Host";
        public const string GUEST = "Guest";
    }

    public delegate void OnRoundStartedDelegate(int roundNumber);
    static public OnRoundStartedDelegate OnRoundStartedCallback;

    public delegate void OnTeamTurnChangedDelegate(string teamName);
    static public OnTeamTurnChangedDelegate OnTeamTurnChangedCallback;

    public delegate void OnHostUnitIndexChangedDelegate(int hostUnitIndex);
    static public OnHostUnitIndexChangedDelegate OnHostUnitIndexChangedCallback;

    public delegate void OnGuestUnitIndexChangedDelegate(int guestUnitIndex);
    static public OnGuestUnitIndexChangedDelegate OnGuestUnitIndexChangedCallback;

    public delegate void OnActionStartedDelegate(int actionsLeft);
    static public OnActionStartedDelegate OnActionStartedCallback; 

    public delegate void OnUnitHealthSetDelegate(string team, string unitName, int health);
    static public OnUnitHealthSetDelegate OnUnitHealthSetCallback;

    public delegate void OnTeamHealthSetDelegate(string team, int health);
    static public OnTeamHealthSetDelegate OnTeamHealthSetCallback;

    public delegate void OnPlayerRatingSetDelegate(int rating);
    static public OnPlayerRatingSetDelegate OnPlayerRatingSetCallback;

    public delegate void OnPlayerTeamUnitsSetDelegate(string team, string unitsString);
    static public OnPlayerTeamUnitsSetDelegate OnPlayerTeamUnitsSetCallback;

    public delegate void OnSendResultsDelegate(string message, int updatedRating);
    private OnSendResultsDelegate _onSendResultsCallback;

    [HideInInspector] public int HostPlayerId = -1;
    [HideInInspector] public int GuestPlayerId = -1;

    [SerializeField] private int TeamsAmount = 2;
    [SerializeField] private int TeamSize = 6;

    [SerializeField] private float _statIncreaseByLevel = 1.1f;
    [SerializeField] private float _retrySendingResultsDelay = 5;
    [SerializeField] List<int> _experienceNeededPerUnitLevel = new List<int>() { 10, 30, 70, 150, 310 };  //Rule: Doubles each level
    [SerializeField] List<int> _ratingsForArena = new List<int>() { 0, 300, 600, 900, 1200, 1500 };

    [SerializeField] private int _roomMaxPlayers = 2;
    [SerializeField] private int _roomMaxPlayersNotExpectating = 2;

    private Hashtable _matchResultshashTable = new Hashtable();

    override protected void Awake()
    {
        base.Awake();

        NetworkManager.OnPlayerCustomPropertiesChangedCallback += OnPlayerCustomPropertiesChanged;
        NetworkManager.OnRoomCustomPropertiesChangedCallback += OnRoomCustomPropertiesChanged;
    }

    private void Start()
    {
        base.setNetworkViewId(1001);
    }

    private void OnDestroy()
    {
        NetworkManager.OnPlayerCustomPropertiesChangedCallback -= OnPlayerCustomPropertiesChanged;
        NetworkManager.OnRoomCustomPropertiesChangedCallback -= OnRoomCustomPropertiesChanged;
    }

    static public string GetOppositeTeamName(string teamName)
    {
        string oppositeTeam = "";

        if (teamName == GameNetwork.TeamNames.HOST)
            oppositeTeam = GameNetwork.TeamNames.GUEST;
        else if (teamName == GameNetwork.TeamNames.GUEST)
            oppositeTeam = GameNetwork.TeamNames.HOST;

        return oppositeTeam;
    }

    static public int GetTeamNetworkPlayerId(string teamName)
    { 
        int networkPlayerId = (teamName == TeamNames.HOST)? GameNetwork.Instance.HostPlayerId : GameNetwork.Instance.GuestPlayerId;

        //Debug.LogWarning("GameNetwork::GetTeamNetworkPlayerId -> teamName: " + teamName + " networkPlayerId: " + networkPlayerId);

        return networkPlayerId;
    }

    static public string[] GetTeamUnitNames(string teamName)
    {
        int networkPlayerId = GetTeamNetworkPlayerId(teamName);

        string unitsString = NetworkManager.GetAnyPlayerCustomProperty(GameNetwork.PlayerCustomProperties.TEAM_UNITS, teamName, networkPlayerId);

        if (unitsString == null)
            return null;

        string[] unitNames = unitsString.Split(NetworkManager.Separators.VALUES);
        return unitNames;
    }

    public void OnRoomCustomPropertiesChanged(Hashtable updatedProperties)
    {
        foreach (object keyObject in updatedProperties.Keys)
        {
            string key = keyObject.ToString();
            string[] keyParts = key.Split(NetworkManager.Separators.KEYS);
            string idPart = keyParts[0];

            //print("OnRoomCustomPropertiesChanged -> key: " + key);
            //print("OnRoomCustomPropertiesChanged -> updatedProperties: " + updatedProperties.ToStringFull());

            string value = updatedProperties[keyObject].ToString();

            if (idPart == RoomCustomProperties.ROUND_COUNT)
            {
                if (OnRoundStartedCallback != null)
                    OnRoundStartedCallback(int.Parse(value));
            }
            else if (idPart == RoomCustomProperties.TEAM_IN_TURN)
            {
                if (OnTeamTurnChangedCallback != null)
                    OnTeamTurnChangedCallback(value);
            }
            else if (idPart == RoomCustomProperties.HOST_UNIT_INDEX)
            {
                if (OnHostUnitIndexChangedCallback != null)
                    OnHostUnitIndexChangedCallback(int.Parse(value));
            }
            else if (idPart == RoomCustomProperties.GUEST_UNIT_INDEX)
            {
                if (OnGuestUnitIndexChangedCallback != null)
                    OnGuestUnitIndexChangedCallback(int.Parse(value));
            }
            else if (idPart == RoomCustomProperties.ACTIONS_LEFT)
            {
                if (OnActionStartedCallback != null)
                    OnActionStartedCallback(int.Parse(value));
            }
            else if (idPart == UnitRoomProperties.HEALTH)
            {
                string team = keyParts[1];
                string unitName = keyParts[2];

                if (OnUnitHealthSetCallback != null)
                    OnUnitHealthSetCallback(team, unitName, int.Parse(value));
            }
            else if (idPart == TeamRoomProperties.HEALTH)
            {
                string team = keyParts[1];

                if (OnTeamHealthSetCallback != null)
                    OnTeamHealthSetCallback(team, int.Parse(value));
            }
        }
    }

    public void OnPlayerCustomPropertiesChanged(PhotonPlayer player, Hashtable updatedProperties)
    {
        print("GameNetwork::OnPlayerCustomPropertiesChanged -> player: " + player.NickName + " updatedProperties: " + updatedProperties.ToStringFull());
        foreach (object virtualKeyObject in updatedProperties.Keys)
        {
            string value = (string)updatedProperties[virtualKeyObject];
            string teamKey = virtualKeyObject.ToString();
            string[] teamKeyParts = teamKey.Split(NetworkManager.Separators.VIRTUAL_PLAYER_KEY);

            string teamName = teamKeyParts[0];

            if (teamKeyParts.Length == 1)
            {
                Debug.LogWarning("Unknown player custom property: " + teamKeyParts[0].ToString() + " with value: " + value + " . Ignoring.");
                continue;
            }

            string key = teamKeyParts[1];
            string[] keyParts = key.Split(NetworkManager.Separators.KEYS);
            string idPart = keyParts[0];

            if (idPart == PlayerCustomProperties.RATING)
            {
                if (OnPlayerRatingSetCallback != null)
                    OnPlayerRatingSetCallback(int.Parse(value));
            }
            else if (idPart == PlayerCustomProperties.TEAM_UNITS)
            {
                if (OnPlayerTeamUnitsSetCallback != null)
                    OnPlayerTeamUnitsSetCallback(teamName, value);
            }
        }
    }

    public void SendMatchResultsToServer(War.MatchData matchData, OnSendResultsDelegate onSendMatchResultsCallback)
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

    public void BuildUnitLevels(string unitName, int unitExp, int networkPlayerId, string teamName)
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
        SetAnyPlayerUnitProperty(UnitPlayerProperties.LEVEL, unitName, unitLevel.ToString(), teamName, networkPlayerId);
        SetAnyPlayerUnitProperty(UnitPlayerProperties.STRENGHT, unitName, getStatByLevel(minMin.Strength, unitLevel), teamName, networkPlayerId);
        SetAnyPlayerUnitProperty(UnitPlayerProperties.DEFENSE, unitName, getStatByLevel(minMin.Defense, unitLevel), teamName, networkPlayerId);
        SetAnyPlayerUnitProperty(UnitPlayerProperties.EFFECT_SCALE, unitName, getStatByLevel(minMin.EffectScale, unitLevel), teamName, networkPlayerId);

        string maxHealth = getStatByLevel(minMin.MaxHealth, unitLevel);
        SetRoomUnitProperty(UnitRoomProperties.MAX_HEALTH, teamName, unitName, maxHealth);
        SetUnitHealth(teamName, unitName, int.Parse(maxHealth));
        //SetUnitHealth(teamName, unitName, (int.Parse(maxHealth))/4); //TODO: Remove text hack
    }

    public int GetLocalPlayerPvpLevelNumber()
    {
        int rating = GameStats.Instance.Rating;
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

    private string getStatByLevel(float baseValue, int level)
    {
        return (baseValue * _statIncreaseByLevel * (float)level).ToString();
    }

    private string getStatByLevel(int baseValue, int level)
    {
        return (Mathf.RoundToInt((float)baseValue * _statIncreaseByLevel * (float)level)).ToString();
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
                SetLocalPlayerRating(updatedRating, TeamNames.HOST);
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

    static public void SetUnitHealth(string team, string unitName, int value)
    {
        SetRoomUnitProperty(UnitRoomProperties.HEALTH, team, unitName, value.ToString());
    }

    static public int GetUnitHealth(string team, string unitName)
    {
        return GetRoomUnitProperty(UnitRoomProperties.HEALTH, team, unitName);
    }

    static public string GetTeamInTurn()
    {
        return NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.TEAM_IN_TURN);
    }

    static public void SetTeamInTurn(string teamInTurn)
    {
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.TEAM_IN_TURN, teamInTurn);
    }

    static public void SetRoomUnitProperty(string property, string team, string unitName, string value)
    {
        NetworkManager.SetRoomCustomProperty(property + NetworkManager.Separators.KEYS + team + NetworkManager.Separators.KEYS + unitName, value);
    }

    static public int GetRoomUnitProperty(string property, string team, string unitName)
    {
        return NetworkManager.GetRoomCustomPropertyAsInt(property + NetworkManager.Separators.KEYS + team + NetworkManager.Separators.KEYS + unitName);
    }

    static public void SetRoomTeamProperty(string property, string team,  string value)
    {
        NetworkManager.SetRoomCustomProperty(property + NetworkManager.Separators.KEYS + team, value);
    }

    static public int GetRoomTeamProperty(string property, string team)
    {
        return NetworkManager.GetRoomCustomPropertyAsInt(property + NetworkManager.Separators.KEYS + team);
    }

    static public void ClearLocalTeamUnits(string teamName)
    {
        string[] teamUnits = GameNetwork.GetTeamUnitNames(teamName);

        foreach (string unitName in teamUnits)
        {
            SetLocalPlayerUnitProperty(UnitPlayerProperties.LEVEL, unitName, null, teamName);
            SetLocalPlayerUnitProperty(UnitPlayerProperties.STRENGHT, unitName, null, teamName);
            SetLocalPlayerUnitProperty(UnitPlayerProperties.DEFENSE, unitName, null, teamName);
            SetLocalPlayerUnitProperty(UnitPlayerProperties.EFFECT_SCALE, unitName, null, teamName);
            SetLocalPlayerUnitProperty(UnitPlayerProperties.POSITION, unitName, null, teamName);
        }

        NetworkManager.SetLocalPlayerCustomProperty(PlayerCustomProperties.TEAM_UNITS, null, teamName);
    }

    static public void SetLocalPlayerUnitProperty(string property, string unitName, string value, string teamName)
    {
        SetAnyPlayerUnitProperty(property, unitName, value, teamName, NetworkManager.GetLocalPlayerId());
    }

    static public void SetAnyPlayerUnitProperty(string property, string unitName, string value, string teamName, int networkPlayerId)
    {
        NetworkManager.SetAnyPlayerCustomProperty(property + NetworkManager.Separators.KEYS + unitName, value, teamName, networkPlayerId);
    }

    static public string GetLocalPlayerUnitProperty(string property, string unitName, string teamName)
    {
        return GetAnyPlayerUnitProperty(property, unitName, teamName, NetworkManager.GetLocalPlayerId());
    }

    static public int GetLocalPlayerUnitPropertyAsInt(string property, string unitName, string teamName)
    {
        return GetAnyPlayerUnitPropertyAsInt(property, unitName, teamName, NetworkManager.GetLocalPlayerId());
    }

    static public int GetAnyPlayerUnitPropertyAsInt(string property, string unitName, string teamName, int networkPlayerId)
    {
        return NetworkManager.GetAnyPlayerCustomPropertyAsInt(property + NetworkManager.Separators.KEYS + unitName, teamName, networkPlayerId);
    }

    static public string GetAnyPlayerUnitProperty(string property, string unitName, string teamName, int networkPlayerId)
    {
        return NetworkManager.GetAnyPlayerCustomProperty(property + NetworkManager.Separators.KEYS + unitName, teamName, networkPlayerId);
    }

    static public int GetLocalPlayerRating(string teamName)
    {
        return NetworkManager.GetLocalPlayerCustomPropertyAsInt(PlayerCustomProperties.RATING, teamName);
    }

    static public void SetLocalPlayerRating(int newRating, string teamName)
    {
        NetworkManager.SetLocalPlayerCustomProperty(PlayerCustomProperties.RATING, newRating.ToString(), teamName);
    }

    static public int GetAnyPlayerRating(int networkPlayerId, string teamName)
    {
        return NetworkManager.GetAnyPlayerCustomPropertyAsInt(PlayerCustomProperties.RATING, teamName, networkPlayerId);
    }

    public void JoinOrCreateRoom()
    {
        string roomName = "1v1 - " + NetworkManager.GetPlayerName();
        NetworkManager.JoinOrCreateRoom(roomName, true, true, _roomMaxPlayers, getCustomProps(), getCustomPropsForLobby(), _roomMaxPlayersNotExpectating);
    }

    public void CreateRoom(string roomName)
    {
        NetworkManager.CreateRoom(roomName, true, true, _roomMaxPlayers, getCustomProps(), getCustomPropsForLobby(), _roomMaxPlayersNotExpectating);
    }

    private string[] getCustomPropsForLobby()
    {
        return new string[] { RoomCustomProperties.HOST_RATING };
    }

    private Hashtable getCustomProps()
    {
        List<string> playerList = new List<string>();

        playerList.Add(NetworkManager.GetPlayerName());

        Hashtable customProps = new Hashtable();
        customProps.Add(RoomCustomProperties.HOST_RATING, GameStats.Instance.Rating);
        //customProps.Add(RoomCustomProperties.PLAYER_LIST, playerList.ToArray());
        //customProps.Add(RoomCustomProperties.HOST, NetworkManager.GetPlayerName());

        //customProps.Add(RoomCustomProperties.MAX_PLAYERS, _roomMaxPlayers);

        return customProps;
    }
}
