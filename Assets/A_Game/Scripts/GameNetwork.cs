using Enigma.CoreSystems;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameNetwork : SingletonMonobehaviour<GameNetwork>
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
        public const string ROUND_NUMBER = "Team_In_Turn";
        public const string UNIT_IN_TURN = "Unit_In_Turn";
        public const string START_COUNT_DOWN_TIMER = "st";
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

    public class VirtualPlayerIds
    {
        public const string ALLIES = "Allies";
        public const string ENEMIES = "Enemies";
    }

    public delegate void OnTeamTurnStartedDelegate(string turnTeam);
    static public OnTeamTurnStartedDelegate OnRoundStartedCallback;

    public delegate void OnUnitTurnStartedDelegate(string unitName);
    static public OnUnitTurnStartedDelegate OnUnitTurnStartedCallback;

    public delegate void OnUnitHealthSetDelegate(string team, string unitName, int health);
    static public OnUnitHealthSetDelegate OnUnitHealthSetCallback;

    //public delegate void OnUnitPropertySetDelegate(string team, string unitName, int value);
    //public OnUnitPropertySetDelegate OnUnitPropertySetCallback;

    public delegate void OnTeamHealthSetDelegate(string team, int health);
    static public OnTeamHealthSetDelegate OnTeamHealthSetCallback;

    public delegate void OnPlayerRatingSetDelegate(int rating);
    static public OnPlayerRatingSetDelegate OnPlayerRatingSetCallback;

    static public NetworkManager.SimpleDelegate OnStartMatch;

    //public delegate void OnCameraMoveDelegate(Vector3 position);
    //static public OnCameraMoveDelegate OnCameraMoveCallback;

    public delegate void OnPlayerTeamUnitsSetDelegate(string team, string teamUnits);
    static public OnPlayerTeamUnitsSetDelegate OnPlayerTeamUnitsSetCallback;


    public delegate void OnSendResultsDelegate(string message, int updatedRating);
    private OnSendResultsDelegate _onSendResultsCallback;

    [HideInInspector] public PhotonPlayer EnemyPlayer;

    [SerializeField] private int TeamsAmount = 2;
    [SerializeField] private int TeamSize = 6;

    [SerializeField] private float _statIncreaseByLevel = 1.1f;
    [SerializeField] private float _retrySendingResultsDelay = 5;
    [SerializeField] List<int> _experienceNeededPerUnitLevel = new List<int>() { 10, 30, 70, 150, 310 };  //Rule: Doubles each level
    [SerializeField] List<int> _ratingsForArena = new List<int>() { 0, 300, 600, 900, 1200, 1500 };

    [SerializeField] private int _roomMaxPlayers = 2;
    [SerializeField] private int _roomMaxPlayersNotExpectating = 2;

    private Hashtable _matchResultshashTable = new Hashtable();
    private PhotonView _photonView;

    private void Awake()
    {
        NetworkManager.OnPlayerCustomPropertiesChangedCallback += OnPlayerCustomPropertiesChanged;
        NetworkManager.OnRoomCustomPropertiesChangedCallback += OnRoomCustomPropertiesChanged;
    }

    private void Start()
    {
        _photonView = Instance.gameObject.AddComponent<PhotonView>();
        _photonView.viewID = 1001;
    }

    private void OnDestroy()
    {
        NetworkManager.OnPlayerCustomPropertiesChangedCallback -= OnPlayerCustomPropertiesChanged;
        NetworkManager.OnRoomCustomPropertiesChangedCallback -= OnRoomCustomPropertiesChanged;
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

            if (idPart == RoomCustomProperties.ROUND_NUMBER)
            {
                if (OnRoundStartedCallback != null)
                    OnRoundStartedCallback(value);
            }
            else if (idPart == RoomCustomProperties.UNIT_IN_TURN)
            {
                if (OnUnitTurnStartedCallback != null)
                    OnUnitTurnStartedCallback(value);
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

                if(OnTeamHealthSetCallback != null)
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
            string virtualKey = virtualKeyObject.ToString();
            string[] virtualKeyParts = virtualKey.Split(NetworkManager.Separators.VIRTUAL_PLAYER_KEY);

            string virtualPlayerId = virtualKeyParts[0];

            if (virtualKeyParts.Length == 1)
            {
                Debug.LogWarning("Unknown player custom property: " + virtualKeyParts[0].ToString() + " with value: " + value + " . Ignoring.");
                continue;
            }

            string key = virtualKeyParts[1];
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
                    OnPlayerTeamUnitsSetCallback(virtualPlayerId, value);
            }
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

        string maxHealth = getStatByLevel(minMin.MaxHealth, unitLevel);
        SetRoomUnitProperty(UnitRoomProperties.MAX_HEALTH, virtualPlayerId, unitName, maxHealth);
        //SetUnitHealth(virtualPlayerId, unitName, int.Parse(maxHealth));
        SetUnitHealth(virtualPlayerId, unitName, (int.Parse(maxHealth))/2); //TODO: Remove text hack
    }

    public int GetLocalPlayerPvpLevelNumber()
    {
        int rating = GetLocalPlayerRating(VirtualPlayerIds.ALLIES);
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
                SetLocalPlayerRating(updatedRating, VirtualPlayerIds.ALLIES);
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

    public void SetUnitHealth(string team, string unitName, int value)
    {
        SetRoomUnitProperty(UnitRoomProperties.HEALTH, team, unitName, value.ToString());
    }

    public int GetUnitHealth(string team, string unitName)
    {
        return GetRoomUnitProperty(UnitRoomProperties.HEALTH, team, unitName);
    }

    public void SetRoomUnitProperty(string property, string team, string unitName, string value)
    {
        NetworkManager.SetRoomCustomProperty(property + NetworkManager.Separators.KEYS + team + NetworkManager.Separators.KEYS + unitName, value);
    }

    public int GetRoomUnitProperty(string property, string team, string unitName)
    {
        return NetworkManager.GetRoomCustomPropertyAsInt(property + NetworkManager.Separators.KEYS + team + NetworkManager.Separators.KEYS + unitName);
    }

    public void SetRoomTeamProperty(string property, string team,  string value)
    {
        NetworkManager.SetRoomCustomProperty(property + NetworkManager.Separators.KEYS + team, value);
    }

    public int GetRoomTeamProperty(string property, string team)
    {
        return NetworkManager.GetRoomCustomPropertyAsInt(property + NetworkManager.Separators.KEYS + team);
    }

    public void ClearTeamUnits(string virtualPlayerId)
    {
        string teamString = NetworkManager.GetLocalPlayerCustomProperty(PlayerCustomProperties.TEAM_UNITS, virtualPlayerId);
        string[] teamUnits = teamString.Split(NetworkManager.Separators.VALUES);

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
        return NetworkManager.GetLocalPlayerCustomPropertyAsInt(PlayerCustomProperties.RATING, virtualPlayerId);
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

    public void SetRoundNumber(int roundNumber)
    {
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ROUND_NUMBER, roundNumber.ToString());
    }

    public int GetRoundNumber()
    {
        return int.Parse(NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.ROUND_NUMBER));
    }

    public void SetUnitInTurn(string unitName)
    {
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.UNIT_IN_TURN, unitName);
    }

    public string GetUnitInTurn()
    {
        return NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.UNIT_IN_TURN);
    }

    public void SendStartMatch()
    {
        _photonView.RPC("startMatch", PhotonTargets.All);   
    }

    [PunRPC]
    private void startMatch(PhotonMessageInfo messageInfo)
    {
        Debug.Log("startMatch -> sender nickname: " + messageInfo.sender.NickName);

        if (OnStartMatch != null)
            OnStartMatch();
    }

    //public void SendCameraMove(Vector3 position)
    //{
    //    _photonView.RPC("CameraMove", PhotonTargets.All, position);
    //}

    //[PunRPC]
    //private void cameraMove(PhotonMessageInfo messageInfo, Vector3 position)
    //{
    //    if (OnCameraMoveCallback != null)
    //        OnCameraMoveCallback(position);
    //}

    public void JoinOrCreateRoom()
    {
        string roomName = "1v1 - " + NetworkManager.GetPlayerName();
        string[] customPropsForLobby = new string[] { NetworkManager.RoomPropertyOptions.PLAYER_LIST, NetworkManager.RoomPropertyOptions.HOST };
        NetworkManager.JoinOrCreateRoom(roomName, true, true, _roomMaxPlayers, getCustomProps(), customPropsForLobby, _roomMaxPlayersNotExpectating);
    }

    public void CreateRoom(string roomName)
    {
        string[] customPropsForLobby = new string[] { NetworkManager.RoomPropertyOptions.PLAYER_LIST, NetworkManager.RoomPropertyOptions.HOST };
        NetworkManager.CreateRoom(roomName, true, true, _roomMaxPlayers, getCustomProps(), customPropsForLobby, _roomMaxPlayersNotExpectating);
    }

    private Hashtable getCustomProps()
    {
        List<string> playerList = new List<string>();

        playerList.Add(NetworkManager.GetPlayerName());

        Hashtable customProps = new Hashtable();
        customProps.Add(NetworkManager.RoomPropertyOptions.PLAYER_LIST, playerList.ToArray());
        customProps.Add(NetworkManager.RoomPropertyOptions.HOST, NetworkManager.GetPlayerName());

        customProps.Add(NetworkManager.RoomPropertyOptions.MAX_PLAYERS, _roomMaxPlayers);

        return customProps;
    }
}
