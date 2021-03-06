using Enigma.CoreSystems;
using GameConstants;
using GameEnums;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Enigma.CoreSystems.NetworkManager;


public class GameNetwork : SingletonPersistentPrefab<GameNetwork>
{
    public const string TRANSACTION_GAME_NAME = "MinMins";

    public class PopUpMessages
    {
        public const string OPPONENT_DISCONNECTED = "Your opponent has disconnected.";
        public const string PLAYER_DISCONNECTED = "You have disconnected.";
    }

    public delegate void OnReadyToFightDelegate(string teamName, bool ready);
    static public OnReadyToFightDelegate OnReadyToFightCallback;

    //public delegate void OnSizeBonusChangedDelegate(string teamName, int bonus);
    //static public OnSizeBonusChangedDelegate OnSizeBonusChangedCallback;

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

    public delegate void OnPlayerPingSetDelegate(int ping);
    static public OnPlayerPingSetDelegate OnHostPingSetCallback;
    static public OnPlayerPingSetDelegate OnGuestPingSetCallback;

    public delegate void OnPlayerTeamUnitsSetDelegate(string team, string unitsString);
    static public OnPlayerTeamUnitsSetDelegate OnPlayerTeamUnitsSetCallback;

    public delegate void OnPvpAiSetDelegate(bool enabled);
    public OnPvpAiSetDelegate OnPvpAiSetCallback;

    public delegate void OnSendResultsDelegate(string message, int updatedRating);
    private OnSendResultsDelegate _onSendResultsCallback;

    private const int _DEFAULT_TOKEN_AMOUNT = 1;

    [HideInInspector] public int HostPlayerId = -1;
    [HideInInspector] public int GuestPlayerId = -1;

    [SerializeField] private int TeamsAmount = 2;
    [SerializeField] private int TeamSize = 6;

    [SerializeField] private int _unitHealthByTier = 50;

    [SerializeField] private float _statIncreaseByLevel = 0.1f;
    [SerializeField] private float _retrySendingResultsDelay = 5;
    [SerializeField] List<int> _ratingsForArena = new List<int>() { 0, 300, 600, 900, 1200, 1500 };

    [SerializeField] private int _roomMaxPlayers = 2;
    [SerializeField] private int _roomMaxPlayersNotExpectating = 2;

    [SerializeField] private int _defaultTokenBonus = 5;

    private MessagePopUp _messagePopUp;

    [Header("Only for display. Set at runtime:")]
    public bool IsEnjinLinked = false;

    public int[] rewardedTrainingLevels = new int[100];
    [Header("=================================")]

    private Hashtable _matchResultshashTable = new Hashtable();
    private Dictionary<string, bool> _availabilityByEnjinKey = new Dictionary<string, bool>();

    public string EthAdress
    {
        get;
        private set;
    }

    override protected void Awake()
    {
        base.Awake();

        NetworkManager.OnPlayerCustomPropertiesChangedCallback += OnPlayerCustomPropertiesChanged;
        NetworkManager.OnRoomCustomPropertiesChangedCallback += OnRoomCustomPropertiesChanged;

        NetworkManager.OnPlayerDisconnectedCallback += onPlayerDisconnected;
        NetworkManager.OnDisconnectedFromNetworkCallback += onDisconnectedFromNetwork;

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += onSceneLoaded;
    }

    private void Start()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.EnjinLinked)
        {
            IsEnjinLinked = true;
        }
#endif

        setKeyAvailabilityFalseByDefault();
    }

    private void setKeyAvailabilityFalseByDefault()
    {
        SetEnjinKeyAvailable(EnigmaTokenKeys.ENJIN_MFT, false);
        SetEnjinKeyAvailable(EnigmaTokenKeys.ENIGMA_TOKEN, false);

        SetEnjinKeyAvailable(GameEnjinTokenKeys.MINMINS_TOKEN, false);

        GameInventory gameInventory = GameInventory.Instance;
        List<string> legendUnits = GameInventory.Instance.GetLegendUnitNames();
        foreach (string legendUnit in legendUnits)
        {
            string tokenName = gameInventory.GetUnitNameToken(legendUnit);
            SetEnjinKeyAvailable(tokenName, false);
        }

        SetEnjinKeyAvailable(GameEnjinCodeKeys.QUEST_BLUE_NARWHAL, false);
        SetEnjinKeyAvailable(GameEnjinCodeKeys.QUEST_CHEESE_NARWHAL, false);
        SetEnjinKeyAvailable(GameEnjinCodeKeys.QUEST_EMERALD_NARWHAL, false);
        SetEnjinKeyAvailable(GameEnjinCodeKeys.QUEST_CRIMSON_NARWHAL, false);

        SetEnjinKeyAvailable(GameEnjinTokenKeys.QUEST_WARGOD_SHALWEND, false);
        SetEnjinKeyAvailable(GameEnjinTokenKeys.QUEST_DEADLY_KNIGHT_SHALWEND, false);

        SetEnjinKeyAvailable(GameEnjinTokenKeys.GOD_ENIGMA, false);
    }

    public void UpdateEnjinGoodies(JSONNode response_hash, bool updateEthAddress)
    {
        //GameStats gameStats = GameStats.Instance;
        //        GameInventory gameInventory = GameInventory.Instance;
        //        //gameStats.ActiveQuest = (Quests)response_hash[NetworkManager.TransactionKeys.USER_DATA][GameNetwork.TransactionKeys.QUEST].AsInt;
        //        gameInventory.SetActiveQuest((Quests)response_hash[NetworkManager.TransactionKeys.USER_DATA][GameNetwork.TransactionKeys.QUEST].AsInt);

        //#if DEVELOPMENT_BUILD || UNITY_EDITOR
        //        if (GameHacks.Instance.SetLoginQuest.Enabled)
        //        {
        //            //gameStats.ActiveQuest = GameHacks.Instance.SetLoginQuest.GetValueAsEnum<Quests>();
        //            gameInventory.SetActiveQuest(GameHacks.Instance.SetLoginQuest.GetValueAsEnum<Quests>());
        //        }
        //#endif

        JSONNode userDataNode = response_hash[EnigmaNodeKeys.USER_DATA];
        if (userDataNode != null)
        {
            updateWalletFromUserDataNode(userDataNode, updateEthAddress);
        }

        determineServerTokenAvailable(userDataNode, EnigmaTokenKeys.ENJIN_MFT);
        determineServerTokenAvailable(userDataNode, EnigmaTokenKeys.ENIGMA_TOKEN);

        determineServerTokenAvailable(userDataNode, GameEnjinTokenKeys.MINMINS_TOKEN);
        determineServerTokenAvailable(userDataNode, GameEnjinTokenKeys.GOD_ENIGMA);

        GameInventory gameInventory = GameInventory.Instance;

        //List<string> oreTokens = gameInventory.GetOreTokens();
        //foreach (string oreToken in oreTokens)
        //{
        //    setTokenAvailable(response_hash, oreToken);
        //}

        List<string> legendUnits = gameInventory.GetLegendUnitNames();

        foreach (string legendUnit in legendUnits)
        {
            string tokenName = gameInventory.GetUnitNameToken(legendUnit);
            determineServerTokenAvailable(userDataNode, tokenName);
        }

        CheckAllEnjinTeamBoostTokens(response_hash);
    }

    public bool GetIsEnjinKeyAvailable(string enjinKey)
    {
        return _availabilityByEnjinKey[enjinKey];
    }

    public List<string> GetEnjinKeysAvailable()
    {
        List<string> keysAvailable = new List<string>();

        foreach (string token in _availabilityByEnjinKey.Keys)
        {
            if (_availabilityByEnjinKey[token])
            {
                keysAvailable.Add(token);
            }
        }

        return keysAvailable;
    }

    public void SetEnjinKeyAvailable(string enjinKey, bool available)
    {
        _availabilityByEnjinKey[enjinKey] = available;
    }

    //private void determineTokenAvailable(SimpleJSON.JSONNode response_hash, string tokenKey)
    //{
    //    checkTokenAvailableInNode(response_hash[NetworkManager.TransactionKeys.USER_DATA], tokenKey);
    //}

    private void onPlayerDisconnected(int disconnectedPlayerId)
    {
        if (disconnectedPlayerId == GuestPlayerId)
        {
            if (_messagePopUp)
            {
                _messagePopUp.Open(PopUpMessages.OPPONENT_DISCONNECTED);
            }
        }
    }

    private void onDisconnectedFromNetwork()
    {
        if (GameStats.Instance.Mode == GameModes.Pvp)
        {
            if (_messagePopUp)
            {
                _messagePopUp.Open(PopUpMessages.PLAYER_DISCONNECTED);
            }
        }
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _messagePopUp = GameObject.FindObjectOfType<MessagePopUp>();
        if (_messagePopUp)
        {
            _messagePopUp.OnDismissButtonDownCallback += onMessagePopUpDismissButtonDown;
        }
    }

    private void Update()
    {
        GameHacks gameHacks = GameHacks.Instance;

        if (gameHacks.TriggerPlayerDisconnectPopUp.Enabled)
        {
            if (Input.GetKeyDown(gameHacks.TriggerPlayerDisconnectPopUp.GetValueAsEnum<KeyCode>()))
            {
                if (_messagePopUp)
                {
                    _messagePopUp.Open(PopUpMessages.PLAYER_DISCONNECTED);
                }
                else
                {
                    Debug.LogWarning("There is no message pop up object in scene for message: " + PopUpMessages.PLAYER_DISCONNECTED);
                }
            }
        }

        if (gameHacks.TriggerOpponentDisconnectPopUp.Enabled)
        {
            if (Input.GetKeyDown(gameHacks.TriggerOpponentDisconnectPopUp.GetValueAsEnum<KeyCode>()))
            {
                if (_messagePopUp != null)
                {
                    _messagePopUp.Open(PopUpMessages.OPPONENT_DISCONNECTED);
                }
                else
                {
                    Debug.LogWarning("There is no message pop up object in scene for message: " + PopUpMessages.OPPONENT_DISCONNECTED);
                }
            }
        }
    }

    private void OnDestroy()
    {
        //Debug.LogWarning("GameNetwork::OnDestroy");
        NetworkManager.OnPlayerCustomPropertiesChangedCallback -= OnPlayerCustomPropertiesChanged;
        NetworkManager.OnRoomCustomPropertiesChangedCallback -= OnRoomCustomPropertiesChanged;

        NetworkManager.OnPlayerDisconnectedCallback -= onPlayerDisconnected;
        NetworkManager.OnDisconnectedFromNetworkCallback -= onDisconnectedFromNetwork;

        if (_messagePopUp != null)
        {
            _messagePopUp.OnDismissButtonDownCallback -= onMessagePopUpDismissButtonDown;
        }
    }

    private void onMessagePopUpDismissButtonDown(string message)
    {
        Debug.LogWarning("GameNetwork::onMessagePopUpDismissButtonDown -> message: " + message);

        if ((message == PopUpMessages.OPPONENT_DISCONNECTED) || (message == PopUpMessages.PLAYER_DISCONNECTED))
        {
            NetworkManager.Disconnect();
            //Enigma.CoreSystems.SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
            NetworkManager.LoadScene(GameConstants.Scenes.LEVELS);
        }
    }

    static public string GetNicknameFromPlayerTeam(string teamName)
    {
        int playerId = GetTeamNetworkPlayerId(teamName);
        string nickName = NetworkManager.GetNetworkPlayerNicknameById(playerId);

        return nickName;
    }

    static public string GetOppositeTeamName(string teamName)
    {
        string oppositeTeam = "";

        if (teamName == TeamNames.HOST)
        {
            oppositeTeam = TeamNames.GUEST;
        }
        else if (teamName == TeamNames.GUEST)
        {
            oppositeTeam = TeamNames.HOST;
        }

        return oppositeTeam;
    }

    static public int GetTeamNetworkPlayerId(string teamName)
    {
        int networkPlayerId = -1;  

        if (teamName == TeamNames.HOST)
        {
            networkPlayerId = GameNetwork.Instance.HostPlayerId;
        }
        else if(teamName == TeamNames.GUEST)
        {
            networkPlayerId = GameNetwork.Instance.GuestPlayerId;
        }

        if (networkPlayerId == -1)
        {
            Debug.LogError("Team name: " + teamName + " has no networkPlayerId.");
        }

        //Debug.LogWarning("GameNetwork::GetTeamNetworkPlayerId -> teamName: " + teamName + " networkPlayerId: " + networkPlayerId);

        return networkPlayerId;
    }

    static public string[] GetTeamUnitNames(string teamName)
    {
        int networkPlayerId = GetTeamNetworkPlayerId(teamName);

        string unitsString = NetworkManager.GetAnyPlayerCustomProperty(GamePlayerProperties.TEAM_UNITS, teamName, networkPlayerId);

        if (unitsString == "")
        {
            return null;
        }

        string[] unitNames = unitsString.Split(NetworkManager.Separators.VALUES);
        return unitNames;
    }

    public void ResetLoginValues()
    {
        IsEnjinLinked = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.EnjinLinked)
        {
            IsEnjinLinked = true;
        }
#endif
        List<string> keys = new List<string>(_availabilityByEnjinKey.Keys);
        foreach (string token in keys)
        {
            //Debug.Log("Reset login values -> token: " + token);
            SetEnjinKeyAvailable(token, false);
        }

        GameStats.Instance.TeamBoostTokensOwnedByName.Clear();
    }

    public void OnRoomCustomPropertiesChanged(Hashtable updatedProperties)
    {
        foreach (object keyObject in updatedProperties.Keys)
        {
            string key = keyObject.ToString();
            string[] keyParts = key.Split(NetworkManager.Separators.KEYS);
            string idPart = keyParts[0];

            Debug.LogWarning("OnRoomCustomPropertiesChanged -> key: " + key);
            Debug.LogWarning("OnRoomCustomPropertiesChanged -> updatedProperties: " + updatedProperties.ToStringFull());

            string value = updatedProperties[keyObject].ToString();

            if (idPart == GameRoomProperties.ROUND_COUNT)
            {
                if (OnRoundStartedCallback != null)
                {
                    OnRoundStartedCallback(int.Parse(value));
                }
            }
            else if (idPart == GameRoomProperties.TEAM_IN_TURN)
            {
                if (OnTeamTurnChangedCallback != null)
                {
                    OnTeamTurnChangedCallback(value);
                }
            }
            else if (idPart == GameRoomProperties.HOST_UNIT_INDEX)
            {
                if (OnHostUnitIndexChangedCallback != null)
                {
                    OnHostUnitIndexChangedCallback(int.Parse(value));
                }
            }
            else if (idPart == GameRoomProperties.GUEST_UNIT_INDEX)
            {
                if (OnGuestUnitIndexChangedCallback != null)
                {
                    OnGuestUnitIndexChangedCallback(int.Parse(value));
                }
            }
            else if (idPart == GameRoomProperties.ACTIONS_LEFT)
            {
                if (OnActionStartedCallback != null)
                {
                    OnActionStartedCallback(int.Parse(value));
                }
            }
            else if (idPart == UnitRoomProperties.HEALTH)
            {
                string team = keyParts[1];
                string unitName = keyParts[2];

                if (OnUnitHealthSetCallback != null)
                {
                    OnUnitHealthSetCallback(team, unitName, int.Parse(value));
                }
            }
            else if (idPart == GameTeamRoomProperties.HEALTH)
            {
                string team = keyParts[1];

                if (OnTeamHealthSetCallback != null)
                {
                    OnTeamHealthSetCallback(team, int.Parse(value));
                }
            }
            else if (idPart == GameRoomProperties.HAS_PVP_AI)
            {
                if (OnPvpAiSetCallback != null)
                {
                    OnPvpAiSetCallback(bool.Parse(value));
                }
            }
            else if (idPart == GameRoomProperties.HOST_PING)
            {
                if (OnHostPingSetCallback != null)
                {
                    OnHostPingSetCallback(int.Parse(value));
                }
            }
            else if (idPart == GameRoomProperties.GUEST_PING)
            {
                if (OnGuestPingSetCallback != null)
                {
                    OnGuestPingSetCallback(int.Parse(value));
                }
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

            if (idPart == GamePlayerProperties.RATING)
            {
                if (OnPlayerRatingSetCallback != null)
                {
                    OnPlayerRatingSetCallback(int.Parse(value));
                }
            }
            else if (idPart == GamePlayerProperties.TEAM_UNITS)
            {
                if (OnPlayerTeamUnitsSetCallback != null)
                {
                    OnPlayerTeamUnitsSetCallback(teamName, value);
                }
            }
            else if (idPart == GamePlayerProperties.READY_TO_FIGHT)
            {
                if (OnReadyToFightCallback != null)
                {
                    OnReadyToFightCallback(teamName, bool.Parse(value));
                }
            }
            //else if (idPart == PlayerCustomProperties.SIZE_BONUS)
            //{
            //    if (OnSizeBonusChangedCallback != null)
            //    {
            //        OnSizeBonusChangedCallback(teamName, int.Parse(value));
            //    }
            //}
        }
    }

    public void SendMatchResultsToServer(string winner, OnSendResultsDelegate onSendMatchResultsCallback)
    {
        _onSendResultsCallback = onSendMatchResultsCallback;

        string loser = GameNetwork.GetOppositeTeamName(winner);

        _matchResultshashTable.Clear();

        _matchResultshashTable.Add(GameNodeKeys.WINNER_NICKNAME, NetworkManager.GetRoomCustomProperty(GameRoomProperties.WINNER_NICKNAME));
        _matchResultshashTable.Add(GameNodeKeys.LOSER_NICKNAME, NetworkManager.GetRoomCustomProperty(GameRoomProperties.LOSER_NICKNAME));

        _matchResultshashTable.Add(GameNodeKeys.WINNER_DAMAGE_DEALT, GameNetwork.GetTeamRoomProperty(GameTeamRoomProperties.DAMAGE_DEALT, winner));
        _matchResultshashTable.Add(GameNodeKeys.LOSER_DAMAGE_DEALT, GameNetwork.GetTeamRoomProperty(GameTeamRoomProperties.DAMAGE_DEALT, loser));

        _matchResultshashTable.Add(GameNodeKeys.WINNER_UNITS_KILLED, GameNetwork.GetTeamRoomProperty(GameTeamRoomProperties.UNITS_KILLED, winner));
        _matchResultshashTable.Add(GameNodeKeys.LOSER_UNITS_KILLED, GameNetwork.GetTeamRoomProperty(GameTeamRoomProperties.UNITS_KILLED, loser));

        performResultsSendingTransaction();
    }

    public void BuildUnitLevels(string unitName, int unitLevel, int networkPlayerId, string teamName)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        GameHacks gameHacks = GameHacks.Instance;
        if (gameHacks.RandomizeUnitLevelWithMaxLevel.Enabled)
        {
            unitLevel = UnityEngine.Random.Range(1, gameHacks.RandomizeUnitLevelWithMaxLevel.ValueAsInt);
        }

        if (gameHacks.UnitLevel.Enabled)
        {
            unitLevel = gameHacks.UnitLevel.ValueAsInt;
        }
#endif

        MinMinUnit minMin = GameInventory.Instance.GetMinMinFromResources(unitName);
        SetAnyPlayerUnitProperty(UnitPlayerProperties.LEVEL, unitName, unitLevel.ToString(), teamName, networkPlayerId);

        int unitTier = GameInventory.Instance.GetUnitTier(unitName);
        int baseMaxHealth = unitTier * _unitHealthByTier;

        int healthBonus = NetworkManager.GetAnyPlayerCustomPropertyAsInt(GamePlayerProperties.HEALTH_BONUS, teamName, networkPlayerId);
        int damageBonus = NetworkManager.GetAnyPlayerCustomPropertyAsInt(GamePlayerProperties.DAMAGE_BONUS, teamName, networkPlayerId);
        int defenseBonus = NetworkManager.GetAnyPlayerCustomPropertyAsInt(GamePlayerProperties.DEFENSE_BONUS, teamName, networkPlayerId);
        int powerBonus = NetworkManager.GetAnyPlayerCustomPropertyAsInt(GamePlayerProperties.POWER_BONUS, teamName, networkPlayerId);

        string maxHealth = getIntStatByLevelAsString(baseMaxHealth, unitLevel, healthBonus);

        string strenghtAtLevel = getFloatStatByLevelAsString(minMin.Strength, unitLevel, damageBonus);
        string defenseAtLevel = getFloatStatByLevelAsString(minMin.Defense, unitLevel, defenseBonus);
        string effectScaleAtLevel = getFloatStatByLevelAsString(minMin.EffectScale, unitLevel, powerBonus);

        SetAnyPlayerUnitProperty(UnitPlayerProperties.STRENGHT, unitName, strenghtAtLevel, teamName, networkPlayerId);
        SetAnyPlayerUnitProperty(UnitPlayerProperties.DEFENSE, unitName, defenseAtLevel, teamName, networkPlayerId);
        SetAnyPlayerUnitProperty(UnitPlayerProperties.EFFECT_SCALE, unitName, effectScaleAtLevel, teamName, networkPlayerId);

        SetUnitRoomProperty(UnitRoomProperties.MAX_HEALTH, teamName, unitName, maxHealth);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (gameHacks.FractionOfStartingHealth.Enabled)
        {
            SetUnitHealth(teamName, unitName, (int.Parse(maxHealth)) / gameHacks.FractionOfStartingHealth.ValueAsInt);
        }
        else
#endif
        {
            SetUnitHealth(teamName, unitName, int.Parse(maxHealth));
        }

        //Debug.LogWarning("BuildUnitLevels -> unitName: " + unitName + " tier: " + unitTier + " baseStrenght: " + minMin.Strength + " baseDefense: " + minMin.Defense + " baseEffectScale: " + minMin.EffectScale);
        //Debug.LogWarning("BuildUnitLevels -> unitLevel: " + unitLevel + " maxHealth: " + maxHealth + " strenghtAtLevel: " + strenghtAtLevel + " defenseAtLevel: " + defenseAtLevel + " effectScaleAtLevel: " + effectScaleAtLevel);
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

    private string getFloatStatByLevelAsString(float baseValue, int level, int bonus)
    {
        string result = (baseValue * (1.0f + ((float)bonus / 100.0f)) * (1 + _statIncreaseByLevel * (float)level)).ToString();
        Debug.LogWarning("GameNetwork::getFloatStatByLevelAsString -> baseValue: " + baseValue + " level: " + level + " bonus: " + bonus + " result: " + result);
        return result;
    }

    private string getIntStatByLevelAsString(int baseValue, int level, int bonus)
    {
        string result = (Mathf.RoundToInt((float)baseValue * (1.0f + ((float)bonus / 100.0f)) * (1 + _statIncreaseByLevel * (float)level))).ToString();
        Debug.LogWarning("GameNetwork::getIntStatByLevelAsString -> baseValue: " + baseValue + " level: " + level + " bonus: " + bonus + " result: " + result);
        return result;
    }

    private void performResultsSendingTransaction()
    {
        NetworkManager.Transaction(GameTransactions.CHANGE_RATING, _matchResultshashTable, onSendMatchResults);
    }

    private void onSendMatchResults(JSONNode response)
    {
        if (response != null)
        {
            Debug.Log("onSendMatchResults -> response: " + response.ToString());

            JSONNode response_hash = response[0];
            string status = response_hash[EnigmaNodeKeys.STATUS].ToString().Trim('"');

            if (status == EnigmaServerStatuses.SUCCESS)
            {
                int updatedRating = response_hash[GameNodeKeys.RATING].AsInt;
                GameStats.Instance.Rating = updatedRating;
                SetLocalPlayerRating(updatedRating, TeamNames.HOST);
                _onSendResultsCallback(EnigmaServerStatuses.SUCCESS, updatedRating);
            }
            else
            {
                //StartCoroutine(retryResultsSending());
                _onSendResultsCallback(EnigmaServerStatuses.SERVER_ERROR, -1);
            }
        }
        else
        {
            //StartCoroutine(retryResultsSending());
            _onSendResultsCallback(EnigmaServerStatuses.CONNECTION_ERROR, -1);
        }
    }

    private IEnumerator retryResultsSending()
    {
        yield return new WaitForSeconds(_retrySendingResultsDelay);
        performResultsSendingTransaction();
    }

    static public void SetUnitHealth(string team, string unitName, int value)
    {
        SetUnitRoomProperty(UnitRoomProperties.HEALTH, team, unitName, value.ToString());
    }

    static public int GetUnitHealth(string team, string unitName)
    {
        return GetUnitRoomPropertyAsInt(UnitRoomProperties.HEALTH, team, unitName);
    }

    static public string GetTeamInTurn()
    {
        return NetworkManager.GetRoomCustomProperty(GameRoomProperties.TEAM_IN_TURN);
    }

    static public void SetTeamInTurn(string teamInTurn)
    {
        NetworkManager.SetRoomCustomProperty(GameRoomProperties.TEAM_IN_TURN, teamInTurn);
    }

    static public void SetUnitRoomProperty(string property, string team, string unitName, string value)
    {
        NetworkManager.SetRoomCustomProperty(property + NetworkManager.Separators.KEYS + team + NetworkManager.Separators.KEYS + unitName, value);
    }

    static public int GetUnitRoomPropertyAsInt(string property, string team, string unitName)
    {
        return int.Parse(GetUnitRoomProperty(property, team, unitName));
    }

    static public string GetUnitRoomProperty(string property, string team, string unitName)
    {
        return NetworkManager.GetRoomCustomProperty(property + NetworkManager.Separators.KEYS + team + NetworkManager.Separators.KEYS + unitName);
    }

    static public void SetTeamRoomProperty(string property, string team,  string value)
    {
        NetworkManager.SetRoomCustomProperty(property + NetworkManager.Separators.KEYS + team, value);
    }

    static public int GetTeamRoomPropertyAsInt(string property, string team)
    {
        return int.Parse(GetTeamRoomProperty(property, team));
    }

    static public string GetTeamRoomProperty(string property, string team)
    {
        return NetworkManager.GetRoomCustomProperty(property + NetworkManager.Separators.KEYS + team);
    }

    static public void ClearLocalTeamUnits(string teamName)
    {
        string[] teamUnits = GameNetwork.GetTeamUnitNames(teamName);

        if (teamUnits != null)
        {
            foreach (string unitName in teamUnits)
            {
                SetLocalPlayerUnitProperty(UnitPlayerProperties.LEVEL, unitName, null, teamName);
                SetLocalPlayerUnitProperty(UnitPlayerProperties.STRENGHT, unitName, null, teamName);
                SetLocalPlayerUnitProperty(UnitPlayerProperties.DEFENSE, unitName, null, teamName);
                SetLocalPlayerUnitProperty(UnitPlayerProperties.EFFECT_SCALE, unitName, null, teamName);
                SetLocalPlayerUnitProperty(UnitPlayerProperties.POSITION, unitName, null, teamName);
            }

            NetworkManager.SetLocalPlayerCustomProperty(GamePlayerProperties.TEAM_UNITS, null, teamName);
        }
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
        return NetworkManager.GetLocalPlayerCustomPropertyAsInt(GamePlayerProperties.RATING, teamName);
    }

    static public void SetLocalPlayerRating(int newRating, string teamName)
    {
        NetworkManager.SetLocalPlayerCustomProperty(GamePlayerProperties.RATING, newRating.ToString(), teamName);
    }

    static public int GetAnyPlayerRating(int networkPlayerId, string teamName)
    {
        return NetworkManager.GetAnyPlayerCustomPropertyAsInt(GamePlayerProperties.RATING, teamName, networkPlayerId);
    }

    public void CreatePublicRoom()
    {
        string roomName = "1v1 - " + NetworkManager.GetPlayerName();

        bool isOpen = true;

        bool forcePvpAi = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        forcePvpAi = GameHacks.Instance.ForcePvpAi;
#endif

        if ((GameStats.Instance.Mode == GameModes.Pvp) && forcePvpAi)
        {
            isOpen = false;
        }

        NetworkManager.JoinOrCreateRoom(roomName, true, isOpen, _roomMaxPlayers, getCustomProps(false), getCustomPropsForLobby(), _roomMaxPlayersNotExpectating);
    }

    public void CreateRoom(string roomName)
    {
        NetworkManager.CreateRoom(roomName, true, true, _roomMaxPlayers, getCustomProps(true), getCustomPropsForLobby(), _roomMaxPlayersNotExpectating);
    }

    private string[] getCustomPropsForLobby()
    {
        return new string[] 
        {
            GameRoomProperties.HOST_NAME, GameRoomProperties.HOST_RATING, GameRoomProperties.HOST_PING,
            GameRoomProperties.GUEST_NAME, GameRoomProperties.GUEST_RATING, GameRoomProperties.GUEST_PING,
            GameRoomProperties.IS_PRIVATE, GameRoomProperties.HAS_PVP_AI
        };
    }

    private Hashtable getCustomProps(bool isPrivate)
    {
        List<string> playerList = new List<string>();

        playerList.Add(NetworkManager.GetPlayerName());

        Hashtable customProps = new Hashtable();
        customProps.Add(GameRoomProperties.HOST_NAME, NetworkManager.GetPlayerName());
        customProps.Add(GameRoomProperties.HOST_RATING, GameStats.Instance.Rating);
        customProps.Add(GameRoomProperties.HOST_PING, NetworkManager.GetLocalPlayerPing());
        customProps.Add(GameRoomProperties.HAS_PVP_AI, "False");
        customProps.Add(GameRoomProperties.GUEST_NAME, "");
        customProps.Add(GameRoomProperties.GUEST_RATING, -1);
        customProps.Add(GameRoomProperties.GUEST_PING, -1);
        customProps.Add(GameRoomProperties.IS_PRIVATE, isPrivate.ToString());

        //customProps.Add(RoomCustomProperties.PLAYER_LIST, playerList.ToArray());
        //customProps.Add(RoomCustomProperties.HOST, NetworkManager.GetPlayerName());

        //customProps.Add(RoomCustomProperties.MAX_PLAYERS, _roomMaxPlayers);

        return customProps;
    }

    public void CheckAllEnjinTeamBoostTokens(SimpleJSON.JSONNode response_hash)
    {
        GameStats.Instance.TeamBoostTokensOwnedByName.Clear();

        checkSingleEnjinTeamBoostToken(response_hash, GameEnjinTokenKeys.ENJIN_SWORD, BoostEnjinTokenKeys.SWORD, BoostCategory.DAMAGE);
        checkSingleEnjinTeamBoostToken(response_hash, GameEnjinTokenKeys.ENJIN_ARMOR, BoostEnjinTokenKeys.ARMOR, BoostCategory.DEFENSE);
        checkSingleEnjinTeamBoostToken(response_hash, GameEnjinTokenKeys.ENJIN_SHADOWSONG, BoostEnjinTokenKeys.SHADOW_SONG, BoostCategory.HEALTH);
        checkSingleEnjinTeamBoostToken(response_hash, GameEnjinTokenKeys.ENJIN_BULL, BoostEnjinTokenKeys.BULL, BoostCategory.POWER);
    }

    private void checkSingleEnjinTeamBoostToken(SimpleJSON.JSONNode response_hash, string transactionKey, string name, string category)
    {
        SimpleJSON.JSONNode boostNode = response_hash[EnigmaNodeKeys.USER_DATA][transactionKey];
        bool isAvailable = false;

        if (boostNode != null)
        {
            if (boostNode.AsInt == 1)
            {
                isAvailable = true;
            }
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.AllEnjinTeamBoostTokens)
        {
            isAvailable = true;
        }
#endif

        if (isAvailable)
        {
            GameStats.Instance.TeamBoostTokensOwnedByName.Add(name, new TeamBoostItemGroup(name, _DEFAULT_TOKEN_AMOUNT, _defaultTokenBonus, category, true));
        }
    }

    private void updateWalletFromUserDataNode(JSONNode userDataNode, bool updateAddress)
    {
        JSONNode balancesNode = userDataNode[GameNodeKeys.BALANCES];
        if (balancesNode != null)
        {
            JSONNode balancesData = balancesNode[GameNodeKeys.DATA];
            if (balancesData != null)
            {
                JSONNode walletNode = balancesData[GameNodeKeys.WALLET];
                if (walletNode != null)
                {
                    JSONNode enjBalanceNode = walletNode[GameNodeKeys.ENJ_BALANCE];
                    if (enjBalanceNode != null)
                    {
                        string balanceString = enjBalanceNode.ToString();
                        balanceString = balanceString.Trim('"');
                        GameStats.Instance.EnjBalance = float.Parse(balanceString);
                    }

                    if (updateAddress)
                    {
                        JSONNode ethAddressNode = walletNode[GameNodeKeys.ETH_ADDRESS];
                        string ethString = ethAddressNode.ToString();
                        EthAdress = ethString.Trim('"');

                        Hashtable hashtable = new Hashtable() { { GameNodeKeys.GAME_ID, "1" }, { GameNodeKeys.WALLET, EthAdress } };
                        StartCoroutine(checkNftEnjinTokenAvailable(hashtable, onCheckNftEnjinTokenAvailableResponse));
                    }
                }
            }
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.StartingEnjBalance.Enabled)
        {
            GameStats.Instance.EnjBalance = GameHacks.Instance.StartingEnjBalance.ValueAsInt;
        }
#endif 
    }

    private void onCheckNftEnjinTokenAvailableResponse(JSONNode response)
    {
        if (response == null)
        {
            Debug.LogError("Transaction response " + nameof(checkNftEnjinTokenAvailable) + " transaction got null.");
        }
        else
        {
            Debug.Log(nameof(onCheckNftEnjinTokenAvailableResponse) + " " + response.ToString());

            int count = response.Count;
            for (int i = 0; i < count; i++)
            {
                JSONNode codeNode = response[i][GameNodeKeys.CODE];
                if (codeNode != null)
                {
                    string code = codeNode.ToString();
                    code = code.Trim('"');

                    Debug.Log("Code: " + code);

                    if (code != "")
                    {
                        SetEnjinKeyAvailable(code, true);
                    }
                }
                else
                {
                    Debug.LogError("Code field was not found at response from " + nameof(checkNftEnjinTokenAvailable) + ".");
                }
            }
        }
    }

    private void determineServerTokenAvailable(JSONNode node, string tokenKey)
    {
        string tokenAvailable = "";
        SimpleJSON.JSONNode tokenNode = node[tokenKey];

        if (tokenNode != null)
        {
            tokenAvailable = tokenNode;
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.EnableAllEnjinUnitTokens)
        {
            tokenAvailable = "1";
        }
#endif

        SetEnjinKeyAvailable(tokenKey, (tokenAvailable == "1"));
    }

    static private IEnumerator checkNftEnjinTokenAvailable(Hashtable hashtable, Callback externalCallback, Callback localCallback = null)
    {
        string url = "https://api.playnft.io/getcodes";

        WWWForm formData = new WWWForm();

        foreach (DictionaryEntry pair in hashtable)
        {
            if (pair.Key == null || pair.Value == null)
            {
                continue;
            }

            formData.AddField(pair.Key.ToString(), pair.Value.ToString());
        }

        Debug.Log(nameof(checkNftEnjinTokenAvailable) + " url: " + url);
        var www = UnityEngine.Networking.UnityWebRequest.Post(url, formData);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error + " on " + nameof(checkNftEnjinTokenAvailable));

            JSONNode response = JSON.Parse(www.error);

            if (localCallback != null)
            {
                localCallback(response);
            }

            externalCallback(response);
        }
        else
        {
            if (externalCallback != null)
            {
                string responseString = www.downloadHandler.text;
                responseString = responseString.Trim('"');
                JSONNode response = JSON.Parse(responseString);

                if (localCallback != null)
                {
                    localCallback(response);
                }

                externalCallback(response);
            }
        }
    }
}

public class GameEnjinCodeKeys
{
    public const string QUEST_BLUE_NARWHAL = "quest_bluenarwhal";
    public const string QUEST_CHEESE_NARWHAL = "quest_cheesenarwhal";
    public const string QUEST_EMERALD_NARWHAL = "quest_emeraldnarwhal";
    public const string QUEST_CRIMSON_NARWHAL = "quest_crimsonnarwhal";
}

public class GameEnjinTokenKeys
{
    public const string MINMINS_TOKEN = "minmins_token";

    public const string ENJIN_MAXIM = "enjin_maxim";
    public const string ENJIN_WITEK = "enjin_witek";
    public const string ENJIN_BRYANA = "enjin_bryana";
    public const string ENJIN_TASSIO = "enjin_tassio";
    public const string ENJIN_SIMON = "enjin_simon";

    public const string ENJIN_ESTHER = "enjin_esther"; //fairy 124
    public const string ENJIN_ALEX = "enjin_alex";  //black 122
    public const string ENJIN_LIZZ = "enjin_lizz";  //fire 126
    public const string ENJIN_EVAN = "enjin_evan";  //wizard 123
    public const string ENJIN_BRAD = "enjin_brad";  //book 125

    public const string ENJIN_SWORD = "enjin_sword";
    public const string ENJIN_ARMOR = "enjin_armor";
    public const string ENJIN_SHADOWSONG = "enjin_shadowsong";
    public const string ENJIN_BULL = "enjin_bull";

    public const string KNIGHT_HEALER = "knight_healer";
    public const string KNIGHT_BOMBER = "knight_bomber";
    public const string KNIGHT_SCOUT = "knight_scout";
    public const string KNIGHT_DESTROYER = "knight_destroyer";
    public const string KNIGHT_TANK = "knight_tank";

    public const string DEMON_HEALER = "demon_healer";
    public const string DEMON_BOMBER = "demon_bomber";
    public const string DEMON_SCOUT = "demon_scout";
    public const string DEMON_DESTROYER = "demon_destroyer";
    public const string DEMON_TANK = "demon_tank";

    public const string GOD_HEALER = "god_healer";
    public const string GOD_BOMBER = "god_bomber";
    public const string GOD_SCOUT = "god_scout";
    public const string GOD_DESTROYER_1 = "god_destroyer_1";
    public const string GOD_DESTROYER_2 = "god_destroyer_2";
    public const string GOD_TANK_1 = "god_tank_1";
    public const string GOD_TANK_2 = "god_tank_2";
    public const string GOD_ENIGMA = "god_enigma";

    public const string SWISSBORG_CYBORG = "swissborg_cyborg";
    public const string SHALWEND_WARGOD = "shalwend_wargod";
    public const string SHALWEND_DEADLY_KNIGHT = "shalwend_deadlyknight";

    public const string NARWHAL_BLUE = "narwhal_blue";
    public const string NARWHAL_CHEESE = "narwhal_cheese";
    public const string NARWHAL_EMERALD = "narwhal_emerald";
    public const string NARWHAL_CRIMSON = "narwhal_crimson";
    public const string NARWHAL_DIAMOND = "narwhal_diamond";

    public const string QUEST_WARGOD_SHALWEND = "quest_shalwend";
    public const string QUEST_DEADLY_KNIGHT_SHALWEND = "quest_deadlyknight";

    public const string COZY_BLOBBY = "cozy_blobby";
    public const string ENJINEER_BLOBBY = "enjineer_blobby";
    public const string VR_BLOBBY = "vr_blobby";

    /*
    public const string ENJIN_DEFENSE_ORE_ITEM_1 = "enjin_defense_ore_1";
    public const string ENJIN_DEFENSE_ORE_ITEM_2 = "enjin_defense_ore_2";
    public const string ENJIN_DEFENSE_ORE_ITEM_3 = "enjin_defense_ore_3";
    public const string ENJIN_DEFENSE_ORE_ITEM_4 = "enjin_defense_ore_4";
    public const string ENJIN_DEFENSE_ORE_ITEM_5 = "enjin_defense_ore_5";
    public const string ENJIN_DEFENSE_ORE_ITEM_6 = "enjin_defense_ore_6";
    public const string ENJIN_DEFENSE_ORE_ITEM_7 = "enjin_defense_ore_7";
    public const string ENJIN_DEFENSE_ORE_ITEM_8 = "enjin_defense_ore_8";
    public const string ENJIN_DEFENSE_ORE_ITEM_9 = "enjin_defense_ore_9";
    public const string ENJIN_DEFENSE_ORE_ITEM_10 = "enjin_defense_ore_10";

    public const string ENJIN_HEALTH_ORE_ITEM_1 = "enjin_health_ore_1";
    public const string ENJIN_HEALTH_ORE_ITEM_2 = "enjin_health_ore_2";
    public const string ENJIN_HEALTH_ORE_ITEM_3 = "enjin_health_ore_3";
    public const string ENJIN_HEALTH_ORE_ITEM_4 = "enjin_health_ore_4";
    public const string ENJIN_HEALTH_ORE_ITEM_5 = "enjin_health_ore_5";
    public const string ENJIN_HEALTH_ORE_ITEM_6 = "enjin_health_ore_6";
    public const string ENJIN_HEALTH_ORE_ITEM_7 = "enjin_health_ore_7";
    public const string ENJIN_HEALTH_ORE_ITEM_8 = "enjin_health_ore_8";
    public const string ENJIN_HEALTH_ORE_ITEM_9 = "enjin_health_ore_9";
    public const string ENJIN_HEALTH_ORE_ITEM_10 = "enjin_health_ore_10";

    public const string ENJIN_POWER_ORE_ITEM_1 = "enjin_power_ore_1";
    public const string ENJIN_POWER_ORE_ITEM_2 = "enjin_power_ore_2";
    public const string ENJIN_POWER_ORE_ITEM_3 = "enjin_power_ore_3";
    public const string ENJIN_POWER_ORE_ITEM_4 = "enjin_power_ore_4";
    public const string ENJIN_POWER_ORE_ITEM_5 = "enjin_power_ore_5";
    public const string ENJIN_POWER_ORE_ITEM_6 = "enjin_power_ore_6";
    public const string ENJIN_POWER_ORE_ITEM_7 = "enjin_power_ore_7";
    public const string ENJIN_POWER_ORE_ITEM_8 = "enjin_power_ore_8";
    public const string ENJIN_POWER_ORE_ITEM_9 = "enjin_power_ore_9";
    public const string ENJIN_POWER_ORE_ITEM_10 = "enjin_power_ore_10";

    public const string ENJIN_DAMAGE_ORE_ITEM_1 = "enjin_damage_ore_1";
    public const string ENJIN_DAMAGE_ORE_ITEM_2 = "enjin_damage_ore_2";
    public const string ENJIN_DAMAGE_ORE_ITEM_3 = "enjin_damage_ore_3";
    public const string ENJIN_DAMAGE_ORE_ITEM_4 = "enjin_damage_ore_4";
    public const string ENJIN_DAMAGE_ORE_ITEM_5 = "enjin_damage_ore_5";
    public const string ENJIN_DAMAGE_ORE_ITEM_6 = "enjin_damage_ore_6";
    public const string ENJIN_DAMAGE_ORE_ITEM_7 = "enjin_damage_ore_7";
    public const string ENJIN_DAMAGE_ORE_ITEM_8 = "enjin_damage_ore_8";
    public const string ENJIN_DAMAGE_ORE_ITEM_9 = "enjin_damage_ore_9";
    public const string ENJIN_DAMAGE_ORE_ITEM_10 = "enjin_damage_ore_10";

    public const string ENJIN_SIZE_ORE_ITEM_1 = "enjin_size_ore_1";
    public const string ENJIN_SIZE_ORE_ITEM_2 = "enjin_size_ore_2";
    public const string ENJIN_SIZE_ORE_ITEM_3 = "enjin_size_ore_3";
    public const string ENJIN_SIZE_ORE_ITEM_4 = "enjin_size_ore_4";
    public const string ENJIN_SIZE_ORE_ITEM_5 = "enjin_size_ore_5";
    public const string ENJIN_SIZE_ORE_ITEM_6 = "enjin_size_ore_6";
    public const string ENJIN_SIZE_ORE_ITEM_7 = "enjin_size_ore_7";
    public const string ENJIN_SIZE_ORE_ITEM_8 = "enjin_size_ore_8";
    public const string ENJIN_SIZE_ORE_ITEM_9 = "enjin_size_ore_9";
    public const string ENJIN_SIZE_ORE_ITEM_10 = "enjin_size_ore_10";
    */
}

public class GameTransactions
{
    public const int CHANGE_RATING = 18;
    public const int RANK_CHANGED_TRANSACTION_ID = 19;
    public const int COMPLETED_QUEST_ID = 20;

    public const int SAVE_FILE_TO_SERVER = 25;
    public const int LOAD_FILE_FROM_SERVER = 26;

    public const int GET_QUEST_DATA = 27;
    public const int NEW_TRAINING_LEVEL = 28;

    public const int UPLOAD_REPLAY = 32;
}

public class GameNodeKeys
{
    public const string SEC_CODE = "code";
    public const string DATA = "data";
    public const string BACKUP = "backup";

    public const string RATING = "rating";
    public const string USERNAME = "username";

    public const string QUEST = "quest";
    public const string PROGRESS = "progress";
    public const string LEADERS = "leaders";
    public const string LEVEL = "level";

    public const string REWARDS = "rewards";
    public const string TOKEN_TYPE = "token_type";
    public const string TOKEN_KEY = "token_code";
    //public const string TOKEN_WITHDRAWN = "token_withdrawn";

    public const string WINNER_NICKNAME = "winner_nickname";
    public const string LOSER_NICKNAME = "loser_nickname";

    public const string WINNER_DAMAGE_DEALT = "winner_damage_dealt";
    public const string LOSER_DAMAGE_DEALT = "loser_damage_dealt";

    public const string WINNER_UNITS_KILLED = "winner_units_killed";
    public const string LOSER_UNITS_KILLED = "loser_units_killed";

    public const string MATCH_DURATION = "match_duration";

    public const string BALANCES = "balances";
    public const string ENJ_BALANCE = "enjBalance";
    public const string ETH_ADDRESS = "ethAddress";
    public const string WALLET = "wallet";
    public const string GAME_ID = "gameId";
    public const string CODE = "code";
    public const string UUID = "uuid";

    public const string REWARD = "reward";
}

public class GamePlayerProperties
{
    public const string RATING = "Rating";
    public const string SPECTATING = "Spectating";
    public const string TEAM_UNITS = "Unit_Names";
    public const string READY_TO_FIGHT = "Ready_To_Fight";

    public const string POWER_BONUS = "Effect_Bonus";
    public const string DAMAGE_BONUS = "Damage_Bonus";
    public const string DEFENSE_BONUS = "Defense_Bonus";
    public const string HEALTH_BONUS = "Health_Bonus";
    public const string SIZE_BONUS = "Size_Bonus";
}

public class GameRoomProperties
{
    public const string ROUND_COUNT = "Round_Count";
    public const string TEAM_IN_TURN = "Team_In_Turn";
    public const string HOST_UNIT_INDEX = "Host_Unit_Index";
    public const string GUEST_UNIT_INDEX = "Guest_Unit_Index";
    public const string ACTIONS_LEFT = "Actions_Left";

    public const string START_COUNT_DOWN_TIMER = "st";

    public const string MATCH_START_TIME = "Match_Start_Time";
    public const string MATCH_END_TIME = "Match_End_Time";
    public const string MATCH_DURATION = "Match_Duration";
    public const string WINNER_NICKNAME = "Winner_Nickname";
    public const string LOSER_NICKNAME = "Loser_Nickname";

    //public const string GAME_STATE = "gState";
    //public const string PLAYER_LIST = "playerList";
    //public const string HOST = "host";
    public const string HOST_RATING = "Host_Rating";
    public const string HOST_ID = "Host_Id";
    public const string HOST_NAME = "Host_name";
    public const string HOST_PING = "HostPing";
    public const string GUEST_NAME = "Guest_Name";
    public const string GUEST_RATING = "Guest_Rating";
    public const string GUEST_PING = "Guest_Ping";
    public const string TIME_ROOM_STARTED = "Time_Room_Started";
    public const string IS_PRIVATE = "Is_Private";

    //public const string MAX_PLAYERS = "mp";

    public const string HAS_PVP_AI = "Has_Pvp_Ai";
}

public class GameTeamRoomProperties
{
    public const string HEALTH = "Team_Health";
    public const string MAX_HEALTH = "Team_Max_Health";

    public const string PLAYER_NICKNAME = "Player_Nickname";
    public const string DAMAGE_DEALT = "Damage_Dealt";
    public const string DAMAGE_RECEIVED = "Damage_Received";
    public const string UNITS_KILLED = "Units_Killed";
}

public class UnitRoomProperties
{
    public const string HEALTH = "Unit_Health";
    public const string MAX_HEALTH = "Unit_Max_Health";
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
    public const string SPECTATOR = "Spectator";
}