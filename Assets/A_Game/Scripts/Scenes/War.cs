using Enigma.CoreSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.Storage;
using SimpleJSON;
using System.Runtime.ConstrainedExecution;
using GameEnums;
using GameConstants;

public class War : NetworkEntity
{
    public class GridNames
    {
        public const string TEAM_1 = "Team1";
        public const string TEAM_2 = "Team2";
    }

    [HideInInspector] public bool Ready = true;

    [SerializeField] private float _battleFieldRightSideBaseOffset = 0;
    [SerializeField] private float _actionAreaPosZ = -0.1f;

    [SerializeField] private int _maxRoundsCount = 3;
    [SerializeField] private float _roundFinishDelay = 2;
    [SerializeField] private float _roundPopUpDuration = 2;
    [SerializeField] private double _maxDecisionTime = 10;
    [SerializeField] private int _fieldRewardChestsAmount = 1;

    [SerializeField] private Transform _teamGridContent;
    [SerializeField] private float _readyCheckDelay = 2;

    [SerializeField] private float _team1_CloudsLocalPosX = 0.07f;
    [SerializeField] private float _rightSideOffsetAdjustment = 0.3f;

    [SerializeField] private int _baseEnjinItemChance = 5;
    [SerializeField] private int _mftEnjinItemChance = 10;

    [SerializeField] private Text _errorText;
    [SerializeField] private MatchResultsPopUp _matchResultsPopUp;

    [SerializeField] private Text _timeLeftText;

    [SerializeField] private Text _teamTurnText;
    [SerializeField] private Text _unitTurnText;
    [SerializeField] private Text _unitTypeTurnText;
    [SerializeField] private Text _unitTierTurnText;

    [SerializeField] private Text _roundNumberText;
    [SerializeField] private Transform _roundDisplayGridContent;

    [SerializeField] private Text _actionsLeftText;

    [SerializeField] private Text _teamNameText1;
    [SerializeField] private Text _teamNameText2;

    [SerializeField] private Image _hostTeamHealthFill;
    [SerializeField] private Image _guestTeamHealthFill;

    [SerializeField] private Text _localPlayerPingText;
    [SerializeField] private Text _remotePlayerPingText;

    [SerializeField] private RectTransform UnitTurnHighlightTransform;

    [SerializeField] private Transform _battleField;
    [SerializeField] private GameCamera _gameCamera;

    [SerializeField] private Transform _cloudsContainer;

    [SerializeField] private GameObject _roundPopUp;
    [SerializeField] private Text _roundPopUpText;

    [SerializeField] private float _pingUpdateDelay = 2;

    [SerializeField] private QuestCompletePopUp _questCompletePopUp;

    private Transform _hostGrid;
    private Transform _guestGrid;

    private int _side = 0;
    private List<WarTeamGridItem> _uiTeamGridItems = new List<WarTeamGridItem>();

    private Dictionary<string, MinMinUnit> _hostUnits = new Dictionary<string, MinMinUnit>();
    private Dictionary<string, MinMinUnit> _guestUnits = new Dictionary<string, MinMinUnit>();

    //private Dictionary<string, Dictionary<string, Dictionary<string, List<HealerArea>>>> _healerAreasByOwnerByTargetByTeam;
    //private Dictionary<string, Dictionary<string, Dictionary<string, List<TankArea>>>> _tanksAreasByOwnerByTargetByTeam;

    private Dictionary<string, Dictionary<string, List<HealerArea>>> _healerAreasByOwnerByTeam;
    private Dictionary<string, Dictionary<string, List<HealerArea>>> _healerAreasByTargetByTeam;

    private Dictionary<string, Dictionary<string, List<TankArea>>> _tankAreasByOwnerByTeam;
    private Dictionary<string, Dictionary<string, List<TankArea>>> _tankAreasByTargetByTeam;

    private Dictionary<string, List<MinMinUnit>> _exposedUnitsByTeam = new Dictionary<string, List<MinMinUnit>>();


    private double _timeLeftCount = 0;

    private MatchLocalData _matchLocalData = new MatchLocalData();

    private string _localPlayerTeam = "";
    private bool _setupWasCalled = false;

    private Dictionary<string, bool> _readyByTeam = new Dictionary<string, bool>();

    public string LocalPlayerTeam { get { return _localPlayerTeam; } }

    private AiPlayer _aiPlayer;
    private double _lastNetworkTime;

    public GameObject ReadyPopup;
    public Button ActionButton;

    [SerializeField] private ActionPopUp _actionPopUp;

    LineRenderer _lineRenderer1;
    LineRenderer _lineRenderer2;

    private bool _isVictory = false;

    override protected void Awake()
    {
        base.Awake();

        NetworkManager.OnJoinedRoomCallback += onJoinedRoom;
        NetworkManager.OnPlayerDisconnectedCallback += onPlayerDisconnected;
        NetworkManager.OnDisconnectedFromNetworkCallback += onDisconnectedFromNetwork;

        GameNetwork.OnPlayerTeamUnitsSetCallback += onPlayerTeamUnitsSet;
        GameNetwork.OnReadyToFightCallback += onReadyToFight;
        GameNetwork.OnUnitHealthSetCallback += onUnitHealthSet;
        GameNetwork.OnTeamHealthSetCallback += onTeamHealthSet;
        GameNetwork.OnRoundStartedCallback += onRoundStarted;
        GameNetwork.OnTeamTurnChangedCallback += onTeamTurnChanged;
        GameNetwork.OnHostUnitIndexChangedCallback += onHostUnitIndexChanged;
        GameNetwork.OnGuestUnitIndexChangedCallback += onGuestUnitIndexChanged;
        GameNetwork.OnActionStartedCallback += onActionStarted;
        GameNetwork.OnHostPingSetCallback += onHostPingSet;
        GameNetwork.OnGuestPingSetCallback += onGuestPingSet;

        GameCamera.OnMovementCompletedCallback += onCameraMovementCompleted;

        _readyByTeam.Add(GameNetwork.TeamNames.HOST, false);
        _readyByTeam.Add(GameNetwork.TeamNames.GUEST, false);

        _actionPopUp.DismissButton.onClick.AddListener(() => { onActionPopUpDismissButtonDown(); });
    }

    private void Start()
    {
        GameStats gameStats = GameStats.Instance;

        SoundManager.FadeCurrentSong(1f, () => {
                                                    SoundManager.Stop();
                                                    SoundManager.Play("war", SoundManager.AudioTypes.Music, "", true);
                                                });

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.HideClouds)
        {
            _cloudsContainer.gameObject.SetActive(false);
        }
#endif

        _actionPopUp.Close();
        _roundPopUp.SetActive(false);

        _localPlayerPingText.gameObject.SetActive(false);
        _remotePlayerPingText.gameObject.SetActive(false);

        if (GetUsesAi())
        {
            _aiPlayer = new AiPlayer();

            _exposedUnitsByTeam.Add(GameNetwork.TeamNames.HOST, new List<MinMinUnit>());
            _exposedUnitsByTeam.Add(GameNetwork.TeamNames.GUEST, new List<MinMinUnit>());
        }

        _errorText.gameObject.SetActive(false);
        enableTimeLeftDisplay(false);

        _matchResultsPopUp.DismissButton.onClick.AddListener(() => OnMatchResultsDismissButtonDown());
        _matchResultsPopUp.gameObject.SetActive(false);

        UnitTurnHighlightTransform.gameObject.SetActive(false);

        //_matchData.PlayerId = NetworkManager.GetPlayerName();

        _hostGrid = _battleField.Find(GridNames.TEAM_1);
        _guestGrid = _battleField.Find(GridNames.TEAM_2);

        _battleFieldRightSideBaseOffset = _guestGrid.transform.position.x - _hostGrid.transform.position.x;

        determineLocalPlayerTeam();

        if (!NetworkManager.LoggedIn)
        {
            NetworkManager.SetLocalPlayerNickName("Guest Player");
        }

        _teamNameText1.text = NetworkManager.GetPlayerName();

        if (GetUsesAi())
        {
            if (gameStats.Mode == GameStats.Modes.Pvp)
            {
                _teamNameText2.text = NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.GUEST_NAME); //NetworkManager.GetRandomOnlineName();
            }
            else if (gameStats.Mode == GameStats.Modes.Quest)
            {
                if (gameStats.SelectedLevelNumber > 0)
                {
                    _teamNameText2.text = GameInventory.Instance.GetActiveQuestName();
                }
            }
            else
            {
                _teamNameText2.text = LocalizationManager.GetTermTranslation("Arena") + " " + gameStats.SelectedLevelNumber;
            }         
        }
        else
        {
            string opponentTeamName = GameNetwork.GetOppositeTeamName(_localPlayerTeam);
            int opponentNetworkPlayerId = GameNetwork.GetTeamNetworkPlayerId(opponentTeamName);
            _teamNameText2.text = NetworkManager.GetNetworkPlayerNicknameById(opponentNetworkPlayerId);
        }

        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.READY_TO_FIGHT, false.ToString(), _localPlayerTeam);

        if (GetIsHost())
        {
            _healerAreasByOwnerByTeam = new Dictionary<string, Dictionary<string, List<HealerArea>>>();
            _healerAreasByOwnerByTeam.Add(GameNetwork.TeamNames.HOST, new Dictionary<string, List<HealerArea>>());
            _healerAreasByOwnerByTeam.Add(GameNetwork.TeamNames.GUEST, new Dictionary<string, List<HealerArea>>());

            _healerAreasByTargetByTeam = new Dictionary<string, Dictionary<string, List<HealerArea>>>();
            _healerAreasByTargetByTeam.Add(GameNetwork.TeamNames.HOST, new Dictionary<string, List<HealerArea>>());
            _healerAreasByTargetByTeam.Add(GameNetwork.TeamNames.GUEST, new Dictionary<string, List<HealerArea>>());

            _tankAreasByOwnerByTeam = new Dictionary<string, Dictionary<string, List<TankArea>>>();
            _tankAreasByOwnerByTeam.Add(GameNetwork.TeamNames.HOST, new Dictionary<string, List<TankArea>>());
            _tankAreasByOwnerByTeam.Add(GameNetwork.TeamNames.GUEST, new Dictionary<string, List<TankArea>>());

            _tankAreasByTargetByTeam = new Dictionary<string, Dictionary<string, List<TankArea>>>();
            _tankAreasByTargetByTeam.Add(GameNetwork.TeamNames.HOST, new Dictionary<string, List<TankArea>>());
            _tankAreasByTargetByTeam.Add(GameNetwork.TeamNames.GUEST, new Dictionary<string, List<TankArea>>());
        }

        if ((gameStats.Mode == GameStats.Modes.SinglePlayer) || (gameStats.Mode == GameStats.Modes.Quest))
        {
            GameNetwork.Instance.CreatePublicRoom();
        }
        else
        {
            setupWar();
        }
        
        _lineRenderer1 = _battleField.Find(GridNames.TEAM_1).gameObject.AddComponent<LineRenderer>();
        _lineRenderer1.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer1.widthMultiplier = 0.1f;
        _lineRenderer1.positionCount = 4;
        _lineRenderer1.loop = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.cyan, 0.0f), new GradientColorKey(Color.cyan, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        _lineRenderer1.colorGradient = gradient;

        GameConfig gameConfig = GameConfig.Instance;

        _lineRenderer1.SetPosition(0, new Vector3(gameConfig.BattleFieldMinPos.x - gameConfig.BoundsLineRendererOutwardOffset, gameConfig.BattleFieldMinPos.y - gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
        _lineRenderer1.SetPosition(1, new Vector3(gameConfig.BattleFieldMinPos.x - gameConfig.BoundsLineRendererOutwardOffset, gameConfig.BattleFieldMaxPos.y + gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
        _lineRenderer1.SetPosition(2, new Vector3(gameConfig.BattleFieldMaxPos.x + gameConfig.BoundsLineRendererOutwardOffset, gameConfig.BattleFieldMaxPos.y + gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
        _lineRenderer1.SetPosition(3, new Vector3(gameConfig.BattleFieldMaxPos.x + gameConfig.BoundsLineRendererOutwardOffset, gameConfig.BattleFieldMinPos.y - gameConfig.BoundsLineRendererOutwardOffset, 0.0f));

        _lineRenderer1.GetComponent<Renderer>().sortingOrder = 303;

        _lineRenderer2 = _battleField.Find(GridNames.TEAM_2).gameObject.AddComponent<LineRenderer>();
        _lineRenderer2.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer2.widthMultiplier = 0.1f;
        _lineRenderer2.positionCount = 4;
        _lineRenderer2.loop = true;
        gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.cyan, 0.0f), new GradientColorKey(Color.cyan, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        _lineRenderer2.colorGradient = gradient;

        _lineRenderer2.SetPosition(0, new Vector3(gameConfig.BattleFieldMinPos.x - gameConfig.BoundsLineRendererOutwardOffset + 17, gameConfig.BattleFieldMinPos.y - gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
        _lineRenderer2.SetPosition(1, new Vector3(gameConfig.BattleFieldMinPos.x - gameConfig.BoundsLineRendererOutwardOffset + 17, gameConfig.BattleFieldMaxPos.y + gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
        _lineRenderer2.SetPosition(2, new Vector3(gameConfig.BattleFieldMaxPos.x + gameConfig.BoundsLineRendererOutwardOffset + 17, gameConfig.BattleFieldMaxPos.y + gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
        _lineRenderer2.SetPosition(3, new Vector3(gameConfig.BattleFieldMaxPos.x + gameConfig.BoundsLineRendererOutwardOffset + 17, gameConfig.BattleFieldMinPos.y - gameConfig.BoundsLineRendererOutwardOffset, 0.0f));

        _lineRenderer1.enabled = false;
        _lineRenderer2.enabled = false;

        _lineRenderer2.GetComponent<Renderer>().sortingOrder = 303;
    }

    private void OnDestroy()
    {
        //Debug.LogWarning("War::OnDestroy");
        NetworkManager.OnJoinedRoomCallback -= onJoinedRoom;
        NetworkManager.OnPlayerDisconnectedCallback -= onPlayerDisconnected;
        NetworkManager.OnDisconnectedFromNetworkCallback -= onDisconnectedFromNetwork;

        GameNetwork.OnPlayerTeamUnitsSetCallback -= onPlayerTeamUnitsSet;
        GameNetwork.OnReadyToFightCallback -= onReadyToFight;
        GameNetwork.OnUnitHealthSetCallback -= onUnitHealthSet;
        GameNetwork.OnTeamHealthSetCallback -= onTeamHealthSet;
        GameNetwork.OnRoundStartedCallback -= onRoundStarted;
        GameNetwork.OnTeamTurnChangedCallback -= onTeamTurnChanged;
        GameNetwork.OnHostUnitIndexChangedCallback -= onHostUnitIndexChanged;
        GameNetwork.OnGuestUnitIndexChangedCallback -= onGuestUnitIndexChanged;
        GameNetwork.OnActionStartedCallback -= onActionStarted;

        GameNetwork.OnHostPingSetCallback -= onHostPingSet;
        GameNetwork.OnGuestPingSetCallback -= onGuestPingSet;

        GameCamera.OnMovementCompletedCallback -= onCameraMovementCompleted;
    }

    static public string GetTeamGridName(string teamName)
    {
        string gridName = GridNames.TEAM_1;

        if (teamName == GameNetwork.TeamNames.GUEST)
        {
            gridName = GridNames.TEAM_2;
        }

        return gridName;
    }

    static public War GetSceneInstance()
    {
        return GameObject.FindObjectOfType<War>();
    }

    public bool GetIsHost()
    {
        return (_localPlayerTeam == GameNetwork.TeamNames.HOST);
    }

    public bool GetIsGuest()
    {
        return (_localPlayerTeam == GameNetwork.TeamNames.GUEST);
    }

    public bool GetUsesAi()
    {
        return ((GameStats.Instance.Mode == GameStats.Modes.SinglePlayer) || (GameStats.Instance.Mode == GameStats.Modes.Quest) || GameStats.Instance.UsesAiForPvp);
    }

    public bool GetIsAiTurn()
    {
        string teamInTurn = GameNetwork.GetTeamInTurn();
        return ((teamInTurn == GameNetwork.TeamNames.GUEST) && GetUsesAi());
    }

    public void HandleAiSuccessfulAttack(ActionArea actionArea)
    {
        if (actionArea is BomberArea)
        {
            _aiPlayer.LastBomberAttackWasSuccessful = true;
        }
        else if (actionArea is DestroyerArea)
        {
            _aiPlayer.LastDestroyerAttackWasSuccessful = true;
        }
    }

    public void HandleAddExposedUnit(string teamName, MinMinUnit unit)
    {         
        if (!_exposedUnitsByTeam[teamName].Contains(unit))
        {
            Debug.LogWarning("War::HandleExposedUnit Added to exposed units: -> teamName: " + teamName + " unit name: " + unit.name + " unit type: " + unit.Type);
            _exposedUnitsByTeam[teamName].Add(unit);
        }
    }

    public void HandleRemoveExposedUnit(string teamName, MinMinUnit unit)
    {
        if (_exposedUnitsByTeam[teamName].Contains(unit))
        {
            Debug.LogWarning("War::HandleExposedUnit Removed from exposed units: -> teamName: " + teamName + " unit name: " + unit.name + " unit type: " + unit.Type);
            _exposedUnitsByTeam[teamName].Remove(unit);
        }
    }

    private int getPlayerInTurnId()
    {
        string teamInTurn = GameNetwork.GetTeamInTurn();
        int playerId = -1;

        if (GetIsHost())
        {
            playerId = NetworkManager.GetLocalPlayerId();
        }
        else
        {
            playerId = GameNetwork.Instance.GuestPlayerId;
        }

        return playerId;
    }

    private void onJoinedRoom()
    {
        print("War::OnJoinedRoom -> Is Master Client: " + NetworkManager.GetIsMasterClient());

        if ((GameStats.Instance.Mode == GameStats.Modes.SinglePlayer) || (GameStats.Instance.Mode == GameStats.Modes.Quest))
        {
            GameNetwork.Instance.HostPlayerId = NetworkManager.GetLocalPlayerId();
            GameNetwork.Instance.GuestPlayerId = NetworkManager.GetLocalPlayerId();
        }

        if (!_setupWasCalled)
        {
            setupWar();
        }
    }

    private void onPlayerDisconnected(int disconnectedPlayerId)
    {
        Debug.LogWarning("War::onPlayerDisconnected: " + disconnectedPlayerId);
        if (GameNetwork.Instance.GuestPlayerId == disconnectedPlayerId)
        {
            handleDisconnection();
        }
    }

    private void onDisconnectedFromNetwork()
    {
        Debug.LogWarning("War::onDisconnectedFromNetwork");
        handleDisconnection();
    }

    private void handleDisconnection()
    {
        bool isPvp = (GameStats.Instance.Mode == GameStats.Modes.Pvp);
        Debug.LogWarning("War::HandleDisconnection -> isPvp: " + isPvp);

        if(isPvp)   //Shouldn't be called, but just in case.
        {
            StopAllCoroutines();
            CancelInvoke();
        }
    }

    private void onPlayerTeamUnitsSet(string teamName, string unitsString)
    {
        if (unitsString == null)
        {
            return;
        }

        string[] teamUnits = unitsString.Split(NetworkManager.Separators.VALUES);

        GameStats gameStats = GameStats.Instance;

        bool localPlayerIsHost = GetIsHost();
        bool isTeamSetHost = teamName == GameNetwork.TeamNames.HOST;

        print("War::onPlayerTeamUnitsSet -> localPlayerIsHost: " + localPlayerIsHost);

        if (localPlayerIsHost)
        {
            instantiateTeam(teamName, teamUnits);

            if (isTeamSetHost)
            {
                if (gameStats.Mode == GameStats.Modes.SinglePlayer)
                {
                    setUpSinglePlayerAiTeamUnits();
                }
                else if (gameStats.Mode == GameStats.Modes.Quest)
                {
                    setupQuestAiTeamUnits();
                }
                else //Pvp
                {
                    if (gameStats.UsesAiForPvp)
                        setUpPvpAiTeamUnits();
                }
            }
            else // Team set is Host
            {
                instantiateRewardChests(GridNames.TEAM_2);
            }
        }
        else // Local player is Guest
        {
            if (isTeamSetHost)
            {
                instantiateRewardChests(GridNames.TEAM_1);
            }
        }
        
    }

    private void onReadyToFight(string teamName, bool ready)
    {
        Debug.LogWarning("War::onReadyToFight -> teamName: " + teamName + " ready: " + ready);
        if (GetIsHost())
        {
            if (ready)
            {
                bool hostReady = false;
                bool guestReady = false;

                if (teamName == GameNetwork.TeamNames.HOST)
                {
                    hostReady = true;

                    GameStats gameStats = GameStats.Instance;
                    if (GetUsesAi())
                    {
                        guestReady = true;
                        Debug.LogWarning("War::onReadyToFight -> Guest was set ready because of Pvp Ai.");
                    }
                    else
                    {
                        guestReady = bool.Parse(NetworkManager.GetAnyPlayerCustomProperty(GameNetwork.PlayerCustomProperties.READY_TO_FIGHT, GameNetwork.TeamNames.GUEST, GameNetwork.GetTeamNetworkPlayerId(GameNetwork.TeamNames.GUEST)));
                    }
                }
                else if (teamName == GameNetwork.TeamNames.GUEST)
                {
                    guestReady = true;
                    hostReady = bool.Parse(NetworkManager.GetAnyPlayerCustomProperty(GameNetwork.PlayerCustomProperties.READY_TO_FIGHT, GameNetwork.TeamNames.HOST, GameNetwork.GetTeamNetworkPlayerId(GameNetwork.TeamNames.HOST)));
                }

                Debug.LogWarning("War::onReadyToFight -> hostReady: " + hostReady + " guestReady: " + guestReady);

                if (hostReady && guestReady)
                {
                    sendSetupLoadTeams();
                    Invoke(nameof(startBattle), 8.0f);
                }
            }
        }
    }

    private void startBattle()
    {
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.MATCH_START_TIME, NetworkManager.GetNetworkTime());
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ROUND_COUNT, 1); //Starts combat cycle

        sendReadyPopUpDismiss();
    }

    private IEnumerator handlePingUpdate()
    {
        while (true)
        {
            Debug.LogWarning("handlePingUpdate -> GetIsHost(): " + GetIsHost() + " GetIsGuest(): " + GetIsGuest());
            if (GetIsHost())
            {
                NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.HOST_PING, NetworkManager.GetLocalPlayerPing());
            }
            else if (GetIsGuest())
            {
                NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.GUEST_PING, NetworkManager.GetLocalPlayerPing());
            }

            yield return new WaitForSeconds(_pingUpdateDelay);
        }
    }

    private void sendReadyPopUpDismiss()
    {
        Debug.LogWarning("sendReadyPopUpDismiss");
        base.SendRpcToAll(nameof(receiveReadyPopUpDismiss));
    }

    [PunRPC]
    private void receiveReadyPopUpDismiss()
    {
        Debug.LogWarning("receiveReadyPopUpDismiss");
        ReadyPopup.SetActive(false);

        if (GameStats.Instance.Mode == GameStats.Modes.Pvp)
        {
            if (GetIsHost() || GetIsGuest())
            {
                StartCoroutine(handlePingUpdate());
            }
        }
    }

    private void onHostPingSet(int value)
    {
        Debug.LogWarning("War::onHostPingSet -> value: " + value + " GetIsHost(): " + GetIsHost() + " GetIsGuest(): " + GetIsGuest());
        string text = LocalizationManager.GetTermTranslation(GameConstants.Terms.PING) + " " + value.ToString(); 

        if (GetIsHost())
        {
            _localPlayerPingText.text = text;

            if (!_localPlayerPingText.gameObject.activeSelf)
            {
                _localPlayerPingText.gameObject.SetActive(true);
            }
        }
        else if (GetIsGuest())
        {
            _remotePlayerPingText.text = text;

            if (!_remotePlayerPingText.gameObject.activeSelf)
            {
                _remotePlayerPingText.gameObject.SetActive(true);
            }
        }
    }

    private void onGuestPingSet(int value)
    {
        Debug.LogWarning("War::onGuestPingSet -> value: " + value + " GetIsHost(): " + GetIsHost() + " GetIsGuest(): " + GetIsGuest());
        string text = LocalizationManager.GetTermTranslation(GameConstants.Terms.PING) + " " + value.ToString();

        if (GetIsHost())
        {
            _remotePlayerPingText.text = text;

            if (!_remotePlayerPingText.gameObject.activeSelf)
            {
                _remotePlayerPingText.gameObject.SetActive(true);
            }
        }
        else if (GetIsGuest())
        {
            _localPlayerPingText.text = text;

            if (!_localPlayerPingText.gameObject.activeSelf)
            {
                _localPlayerPingText.gameObject.SetActive(true);
            }
        }
    }

    private void onUnitHealthSet(string teamName, string unitName, int health)
    {
        if (health <= 0)
        {
            handleUnitDeath(teamName, unitName);
        }

        if (teamName == _localPlayerTeam)
        {
            foreach (WarTeamGridItem teamGridItem in _uiTeamGridItems)
            {
                if (teamGridItem.UnitName == unitName)
                {
                    int unitMaxHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.MAX_HEALTH, teamName, unitName);
                    teamGridItem.SetLifeFill((float)health / (float)unitMaxHealth);
                    break;
                }
            }
        }
    }

    private void onTeamHealthSet(string team, int health)
    {
        int teamMaxHealth = GameNetwork.GetTeamRoomPropertyAsInt(GameNetwork.TeamRoomProperties.MAX_HEALTH, team);

        Image fillToUpdate = (team == _localPlayerTeam) ? _hostTeamHealthFill : _guestTeamHealthFill;
        fillToUpdate.fillAmount = (float)health / (float)teamMaxHealth;
        fillToUpdate.transform.parent.GetComponentInChildren<Text>().text = health + "/" + teamMaxHealth;
    }

    private void handleUnitDeath(string teamName, string unitName)
    {
        Dictionary<string, MinMinUnit> teamUnits = GetTeamUnitsDictionary(teamName);
        MinMinUnit unit = teamUnits[unitName];
        unit.gameObject.SetActive(false);

        //if ((GetUsesAi() && (teamName == GameNetwork.TeamNames.HOST)) || (teamName == _localPlayerTeam))
        if (teamName == _localPlayerTeam)
        {
            GameInventory.Instance.HandleUnitDeath(unitName);
        }

        SoundManager.Play(GameConstants.SoundNames.DEATH, SoundManager.AudioTypes.Sfx);

        if (GetIsHost())
        {
            Debug.LogWarning("War::handleUnitDeath ->  Removing areas for owner teamName: " + teamName + " unitName: " + unitName);
            removeAreasForOwner<HealerArea>(teamName, unitName, _healerAreasByOwnerByTeam, _healerAreasByTargetByTeam);
            removeAreasForOwner<TankArea>(teamName, unitName, _tankAreasByOwnerByTeam, _tankAreasByTargetByTeam);

            removeTargetFromAreas<HealerArea>(teamName, unitName, _healerAreasByTargetByTeam);
            removeTargetFromAreas<TankArea>(teamName, unitName, _tankAreasByTargetByTeam);

            string killerTeam = GameNetwork.GetOppositeTeamName(teamName);
            int unitsKilled = GameNetwork.GetTeamRoomPropertyAsInt(GameNetwork.TeamRoomProperties.UNITS_KILLED, killerTeam);
            GameNetwork.SetTeamRoomProperty(GameNetwork.TeamRoomProperties.UNITS_KILLED, killerTeam, (unitsKilled + 1).ToString());

            if (GetUsesAi())
            {
                HandleRemoveExposedUnit(teamName, unit);
            }
        }
    }

    private void onRoundStarted(int roundNumber)
    {
        StartCoroutine(endRoundFinishPause(roundNumber));
    }

    private IEnumerator endRoundFinishPause(int roundNumber)
    {
        bool isThereDelay = (roundNumber > 1);
        float delay = isThereDelay? _roundFinishDelay : 0;

        yield return new WaitForSeconds(delay);

        string roundText = LocalizationManager.GetTermTranslation("Round") + ": " + roundNumber.ToString();

        if (isThereDelay)
        {
            _roundPopUpText.text = roundText;
            _roundPopUp.SetActive(true);
        }

        _roundNumberText.text = roundText;
        updateRoundDisplay(roundNumber);

        delay = isThereDelay ? _roundPopUpDuration : 0;

        yield return new WaitForSeconds(delay);

        if (isThereDelay)
        {
            _roundPopUp.SetActive(false);
        }

        if (GetIsHost())
        {
            //Initial values
            NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.HOST_UNIT_INDEX, -1);
            NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.GUEST_UNIT_INDEX, -1);

            GameNetwork.SetTeamInTurn(GameNetwork.TeamNames.HOST);
        }
    }

    private void onTeamTurnChanged(string teamName)
    {
        //_teamTurnText.text = "Turn: " + teamName + " | Mine: " + _localPlayerTeam;
        if (teamName == _localPlayerTeam)
        {
            _teamTurnText.text = LocalizationManager.GetTermTranslation("YOUR TURN");
        }
        else
        {
            _teamTurnText.text = LocalizationManager.GetTermTranslation("ENEMY TURN");
        }

        if (GetIsHost())
        {
            advanceUnitIndex(teamName);
        }
    }

    private int getTeamUnitIndex(string teamName)
    {
        string roomProperty = GameNetwork.RoomCustomProperties.HOST_UNIT_INDEX;
        if (teamName == GameNetwork.TeamNames.GUEST)
        {
            roomProperty = GameNetwork.RoomCustomProperties.GUEST_UNIT_INDEX;
        }

        int unitIndex = NetworkManager.GetRoomCustomPropertyAsInt(roomProperty);
        return unitIndex;
    }

    private void advanceUnitIndex(string teamName)
    {
        string roomProperty = GameNetwork.RoomCustomProperties.HOST_UNIT_INDEX;
        if (teamName == GameNetwork.TeamNames.GUEST)
        {
            roomProperty = GameNetwork.RoomCustomProperties.GUEST_UNIT_INDEX;
        }

        int unitIndex = NetworkManager.GetRoomCustomPropertyAsInt(roomProperty) + 1;
        NetworkManager.SetRoomCustomProperty(roomProperty, unitIndex);
    }

    private void changeTurn()
    {
        changeTurn(GameNetwork.GetTeamInTurn());
    }

    private void changeTurn(string teamName)
    {
        GameNetwork.SetTeamInTurn(GameNetwork.GetOppositeTeamName(teamName));
    }

    private void onHostUnitIndexChanged(int hostUnitIndex)
    {
        handleUnitIndexChanged(_hostUnits, hostUnitIndex, GameNetwork.TeamNames.HOST);
    }

    private void onGuestUnitIndexChanged(int guestUnitIndex)
    {
        handleUnitIndexChanged(_guestUnits, guestUnitIndex, GameNetwork.TeamNames.GUEST);
    }

    private void handleUnitIndexChanged(Dictionary<string, MinMinUnit> units, int unitIndex, string teamName)
    {
        print("handleUnitIndexChanged:-> units.Count: " + units.Count + " unitIndex: " + unitIndex + " teamName: " + teamName);
        if (unitIndex == -1)
        {
            return;
        }

        _lineRenderer1.enabled = false;
        _lineRenderer2.enabled = false;

        int teamUnitsCount = units.Count;
        if (unitIndex >= teamUnitsCount)
        {
            if (GetIsHost())
            {
                if (teamName == GameNetwork.TeamNames.HOST)
                {
                    processUnitsHealing();

                    int roundCount = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.ROUND_COUNT);
                    string winner = checkTimeWinner(roundCount);
                    if (winner == "")
                    {
                        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ROUND_COUNT, roundCount + 1);
                    }
                    else
                    {
                        handleMatchEnd(winner);
                    }
                }
                else if (teamName == GameNetwork.TeamNames.GUEST)// Is GUEST
                {
                    changeTurn(teamName);
                }
            }
        }
        else
        {
            MinMinUnit unit = units.Values.ElementAt(unitIndex);
            string unitName = unit.name;

            if (GetIsHost() && (unit.Type == MinMinUnit.Types.Tank))
            {
                Debug.LogWarning("War::handleUnitIndexChanged ->  Removing tank areas for owner teamName: " + teamName + " unitName: " + unitName);
                removeAreasForOwner<TankArea>(teamName, unitName, _tankAreasByOwnerByTeam, _tankAreasByTargetByTeam);
            }

            int unitHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.HEALTH, teamName, unitName);

            if (unitHealth > 0)
            {
                _unitTurnText.text = LocalizationManager.GetTermTranslation("Unit turn") + ": " + unitName + " | " + LocalizationManager.GetTermTranslation("Index") + ": " + unitIndex;
                handleHighlight(unitIndex, teamName);
                _gameCamera.HandleMovement(teamName, units[unitName].Type);
            }
            else if (GetIsHost())
            {
                if ((unitIndex + 1) >= teamUnitsCount)
                {
                    changeTurn(teamName);
                }
                else
                {
                    advanceUnitIndex(teamName);
                }
            }
        }
    }

    private void onCameraMovementCompleted(string sideTeam)
    {
        MinMinUnit unit = getUnitInTurn();
        int unitTier = GameInventory.Instance.GetUnitTier(unit.name);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.UnitTier.Enabled)
        {
            unitTier = GameHacks.Instance.UnitTier.ValueAsInt;
        }
#endif

        _unitTierTurnText.text = LocalizationManager.GetTermTranslation("Unit tier") + ": " + unitTier.ToString();
        _unitTypeTurnText.text = LocalizationManager.GetTermTranslation("Unit type") + ": " + LocalizationManager.GetTermTranslation(unit.Type.ToString());

        if (GetIsHost())
        {
            NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ACTIONS_LEFT, unitTier.ToString());
        }

        string teamInTurn = GameNetwork.GetTeamInTurn();
        if (_localPlayerTeam == teamInTurn)
        {
            if (sideTeam == GameNetwork.TeamNames.HOST)
            {
                _lineRenderer1.enabled = true;
                _lineRenderer2.enabled = false;
            }
            else if (sideTeam == GameNetwork.TeamNames.GUEST)
            {
                _lineRenderer1.enabled = false;
                _lineRenderer2.enabled = true;
            }
        }
    }

    private void onActionStarted(int actionsLeft)
    {
        _actionsLeftText.text = LocalizationManager.GetTermTranslation("Actions Left") + ": " + actionsLeft;
        string teamInTurn = GameNetwork.GetTeamInTurn();

        //Debug.LogWarning("War::onActionStarted: " + actionsLeft + " teamInTurn: " + teamInTurn);

        if (actionsLeft > 0)
        {
            bool isthisPlayerTurn = (teamInTurn == _localPlayerTeam);

            if (GetIsAiTurn())
            {
                StartCoroutine(handleAiPlayerInput(teamInTurn));
            }
            else if (isthisPlayerTurn)
            {
                StartCoroutine(handleHumanPlayerInput());
            }

            if (GetIsHost())
            {
                MinMinUnit unit = getUnitInTurn();
                sendActionPopUpOpen(unit.name, unit.Type);
            }
        }
        else if (GetIsHost())
        {
            changeTurn(teamInTurn);
        }
    }

    private void sendActionPopUpOpen(string unitName, MinMinUnit.Types unitType)
    {
        base.SendRpcToAll(nameof(receiveActionPopUpOpen), unitName, unitType);
    }

    [PunRPC]
    private void receiveActionPopUpOpen(string unitName, MinMinUnit.Types unitType)
    {
        string teamInTurn = GameNetwork.GetTeamInTurn();
        _actionPopUp.Open(unitName, unitType, (teamInTurn == _localPlayerTeam));
    }

    private void sendActionPopUpClose()
    {
        base.SendRpcToAll(nameof(receiveActionPopUpClose));
    }

    [PunRPC]
    private void receiveActionPopUpClose()
    {
        _actionPopUp.Close();
    }

    private IEnumerator handleAiPlayerInput(string teamInTurn)
    {
        float delay = UnityEngine.Random.Range(AiPlayer._MIN_DECISION_DELAY, AiPlayer._MAX_DECISION_DELAY);

        while (true)
        {
            delay -= Time.deltaTime;
            if (delay <= 0)
            {
                _actionPopUp.Close();

                MinMinUnit unit = getUnitInTurn();
                Dictionary<string, MinMinUnit> teamInTurnUnits = GetTeamUnitsDictionary(teamInTurn);
                Vector2 aiWorldInput2D = _aiPlayer.GetWorldInput2D(unit.Type, teamInTurnUnits, _exposedUnitsByTeam, _healerAreasByTargetByTeam);
                Vector3 aiWorldInput3D = new Vector3(aiWorldInput2D.x, aiWorldInput2D.y, _actionAreaPosZ);
                sendPlayerTargetInput(aiWorldInput3D, GameNetwork.TeamNames.GUEST);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator handleHumanPlayerInput()
    {
        resetDecisionTimeCount();

        //Debug.LogWarning("handlePlayerInput");
        while (true)
        {
            if (!_actionPopUp.gameObject.GetActive() && Input.GetMouseButtonDown(0))
            {
                GameSounds.Instance.PlayUiAdvanceSound();

                Vector3 tapWorldPosition = _gameCamera.MyCamera.ScreenToWorldPoint(Input.mousePosition);
                GameConfig gameConfig = GameConfig.Instance;

                float minPosX = gameConfig.BattleFieldMinPos.x;
                float maxPosX = gameConfig.BattleFieldMaxPos.x;

                if ((_gameCamera.IsAtOppponentSide && (_localPlayerTeam == GameNetwork.TeamNames.HOST))
                    || (!_gameCamera.IsAtOppponentSide && (_localPlayerTeam == GameNetwork.TeamNames.GUEST)))
                {
                    minPosX += _battleFieldRightSideBaseOffset + _rightSideOffsetAdjustment;
                    maxPosX += _battleFieldRightSideBaseOffset + _rightSideOffsetAdjustment;
                }

                if ((tapWorldPosition.y > (gameConfig.BattleFieldMinPos.y - gameConfig.BoundsActionOutwardOffset)) && (tapWorldPosition.y < (gameConfig.BattleFieldMaxPos.y + gameConfig.BoundsActionOutwardOffset))
                && (tapWorldPosition.x < (maxPosX + gameConfig.BoundsActionOutwardOffset)) && (tapWorldPosition.x > (minPosX - gameConfig.BoundsActionOutwardOffset)))
                {
                    sendPlayerTargetInput(tapWorldPosition, _localPlayerTeam);
                    enableTimeLeftDisplay(false);
                    yield break;
                }
            }
            else if (checkDecisionTimeOut())
            {
                yield break;
            }
           
            yield return null;
        }
    }

    private bool checkDecisionTimeOut()
    {
        bool timeIsOver = reduceDecisionTimeCount();

        if (timeIsOver)
        {
            enableTimeLeftDisplay(false);
            changeTurn();
        }

        return timeIsOver;
    }

    private void resetDecisionTimeCount()
    {
        _timeLeftCount = _maxDecisionTime;
        enableTimeLeftDisplay(true);
        updateTimeLeftDisplay();
        _lastNetworkTime = NetworkManager.GetNetworkTime();
    }

    private void enableTimeLeftDisplay(bool enabled)
    {
        _timeLeftText.enabled = enabled;
    }

    private bool reduceDecisionTimeCount()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.DecisionTimeFreeze)
        {
            return false;
        }
#endif

        bool isOver = false;

        double networkTime = NetworkManager.GetNetworkTime();
        double deltaTime = networkTime - _lastNetworkTime;
        _lastNetworkTime = networkTime;

        _timeLeftCount -= deltaTime;

        if (_timeLeftCount <= 0)
        {
            _timeLeftCount = 0;
            isOver = true;
        }

        updateTimeLeftDisplay();

        return isOver;
    }

    private void updateTimeLeftDisplay()
    {
        _timeLeftText.text = LocalizationManager.GetTermTranslation("Time Left:") + " " + (Mathf.CeilToInt(((float)_timeLeftCount))).ToString();
    }

    private void sendPlayerTargetInput(Vector3 playerInputWorldPosition, string teamName)
    {
        //Debug.LogWarning("sendPlayerTargetInput");
        base.SendRpcToMasterClient(nameof(receivePlayerTargetInput), playerInputWorldPosition, teamName, NetworkManager.GetLocalPlayerId());
    }

    //Only Master Client uses this
    [PunRPC]
    private void receivePlayerTargetInput(Vector3 inputWorldPosition, string teamName, int networkPlayerId)
    {
        MinMinUnit unitInTurn = getUnitInTurn();
        Vector3 actionAreaPos = inputWorldPosition;
        actionAreaPos.z = _actionAreaPosZ;

        List<Vector3> directions = new List<Vector3>();

        if (unitInTurn.Type == MinMinUnit.Types.Destroyer)
        {
            int unitTier = GameInventory.Instance.GetUnitTier(unitInTurn.name);
            GameConfig gameConfig = GameConfig.Instance;

            if (unitTier >= GameInventory.Tiers.GOLD)
            {
                directions.Add(gameConfig.GoldProjectileBaseDirection);
                directions.Add(-gameConfig.GoldProjectileBaseDirection);
            }

            if (unitTier >= GameInventory.Tiers.SILVER)
            {
                directions.Add(gameConfig.SilverProjectileBaseDirection);
                directions.Add(-gameConfig.SilverProjectileBaseDirection);
            }

            if (unitTier >= GameInventory.Tiers.BRONZE)
            {
                directions.Add(gameConfig.BronzeProjectileBaseDirection);
                directions.Add(-gameConfig.BronzeProjectileBaseDirection);
            }

            GameStats.Instance.UnitsDamagedInSingleDestroyerAction.Clear();
        }
        else
        {
            directions.Add(Vector3.zero);
        }

        handleActionAreaCreation(actionAreaPos, directions, unitInTurn, teamName, networkPlayerId);
    }

    //Only Master Client uses this
    private void handleActionAreaCreation(Vector3 inputWorldPosition, List<Vector3> directions, MinMinUnit unit, string teamName, int networkPlayerId)
    {
        string actionAreaNetworkViewIdsString = "";
        float actionTime = 0;
        foreach (Vector3 direction in directions)
        {
            string unitTypeString = unit.Type.ToString();
            string actionAreaPrefabName = unitTypeString + "Area";

            MinMinUnit.EffectNames effectName = MinMinUnit.GetEffectName(unit);

            object[] instantiationData = { actionAreaPrefabName, inputWorldPosition, direction, unit.name, effectName, teamName, networkPlayerId };
            GameObject actionAreaObject = NetworkManager.InstantiateObject(ActionArea.ACTION_AREAS_RESOURCES_FOLDER_PATH + actionAreaPrefabName, Vector3.zero, Quaternion.identity, 0, instantiationData);

            if (actionAreaNetworkViewIdsString != "")
            {
                actionAreaNetworkViewIdsString += NetworkManager.Separators.KEYS;
            }

            int actionAreaNetworkViewId = actionAreaObject.GetComponent<NetworkEntity>().GetNetworkViewId();
            actionAreaNetworkViewIdsString += actionAreaNetworkViewId.ToString();

            ActionArea actionArea = actionAreaObject.GetComponent<ActionArea>();
            actionTime = actionArea.ActionTime;
        }

        sendActionAreaNetworkViewsIds(actionAreaNetworkViewIdsString);
        StartCoroutine(HandleActionTime(actionTime));
    }

    

    private void sendActionAreaNetworkViewsIds(string actionAreaNetworkViewIdsString)
    {
        base.SendRpcToAll(nameof(receiveActionAreaNetworkViewsIds), actionAreaNetworkViewIdsString);
    }

    [PunRPC]
    private void receiveActionAreaNetworkViewsIds(string actionAreaNetworkViewIdsString)
    {
        string[] actionAreasNetworkViewsIds = actionAreaNetworkViewIdsString.Split(NetworkManager.Separators.KEYS);
        foreach (string actionAreaNetworkViewId in actionAreasNetworkViewsIds)
        {
            NetworkEntity areaNetworkEntity = NetworkEntity.Find(int.Parse(actionAreaNetworkViewId));
            ActionArea area = areaNetworkEntity.GetComponent<ActionArea>();

            string parentPath = ActionArea.ACTION_AREAS_PARENT_FIND_PATH + area.name + "sContainer";
            Transform parent = GameObject.Find(parentPath).transform;

            area.transform.SetParent(parent);
            area.SetWarRef(this);
        }
    }

    //Only Master Client uses this
    private IEnumerator HandleActionTime(float actionTime)
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.ActionTimeHack.Enabled)
        {
            actionTime = GameHacks.Instance.ActionTimeHack.ValueAsFloat;
        }
#endif

        yield return new WaitForSeconds(actionTime);

        string winnerTeam = checkSurvivorWinner();
        if (winnerTeam != "")
        {
            handleMatchEnd(winnerTeam);
        }
        else
        {
            int playerInTurnActionsLeft = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.ACTIONS_LEFT) - 1;
            NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ACTIONS_LEFT, playerInTurnActionsLeft.ToString());
        }
    }

    private MinMinUnit getUnitInTurn()
    {
        string teamInTurn = GameNetwork.GetTeamInTurn();
        int unitIndex = -1;

        if (teamInTurn == GameNetwork.TeamNames.HOST)
        {
            unitIndex = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.HOST_UNIT_INDEX);
        }
        else if (teamInTurn == GameNetwork.TeamNames.GUEST)
        {
            unitIndex = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.GUEST_UNIT_INDEX);
        }

        Dictionary<string, MinMinUnit> teamUnits = GetTeamUnitsDictionary(teamInTurn);
        MinMinUnit unit = teamUnits.Values.ElementAt(unitIndex);

        return unit;
    }

    private void handleHighlight(int unitIndex, string teamName)
    {
        if (teamName == _localPlayerTeam)
        {
            WarTeamGridItem warTeamGridItem = _uiTeamGridItems[unitIndex];

            UnitTurnHighlightTransform.SetParent(warTeamGridItem.transform);
            UnitTurnHighlightTransform.SetAsFirstSibling();
            UnitTurnHighlightTransform.localPosition = Vector3.zero;
            UnitTurnHighlightTransform.gameObject.SetActive(true);
        }
        else
        {
            UnitTurnHighlightTransform.gameObject.SetActive(false);
        }
    }

    private void setupWar()
    {
        if ((_localPlayerTeam == GameNetwork.TeamNames.GUEST) || GameHacks.Instance.GuestCameraAsHost)
        {
            _gameCamera.SetCameraForGuest();
            moveCloudsToOppositeSide();
        }

        setLocalTeamBonuses();
        setLocalTeamUnits();

        _setupWasCalled = true;
    }

    private void determineLocalPlayerTeam()
    {
        if (NetworkManager.IsPhotonOffline()) //Single player
        {
            _localPlayerTeam = GameNetwork.TeamNames.HOST;
        }
        else // Pvp
        {
            if (NetworkManager.GetIsMasterClient())
            {
                _localPlayerTeam = GameNetwork.TeamNames.HOST;
            }
            else
            {
                _localPlayerTeam = GameNetwork.TeamNames.GUEST;
            }
        }

        print("War::determinLocalPlayerTeam -> _localPlayerTeam: " + _localPlayerTeam);
    }

    private void setLocalTeamBonuses()
    {
        TeamBoostItemGroup selectedBoostItem = GameStats.Instance.TeamBoostGroupSelected;

        int healthBonus = 0;
        int damageBonus = 0;
        int defenseBonus = 0;
        int powerBonus = 0;
        int sizeBonus = 0;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.SetTeamBoostBonuses.Enabled)
        {
            int boostBonus = GameHacks.Instance.SetTeamBoostBonuses.ValueAsInt;

            healthBonus = boostBonus;
            damageBonus = boostBonus;
            defenseBonus = boostBonus;
            powerBonus = boostBonus;
            sizeBonus = boostBonus;
        }
        else
#endif
        {
            if (selectedBoostItem != null)
            {
                string boostCategory = selectedBoostItem.Category;
                int boostBonus = selectedBoostItem.Bonus;

                if (boostCategory == GameConstants.TeamBoostCategory.DAMAGE)
                {
                    damageBonus = boostBonus;
                }
                else if (boostCategory == GameConstants.TeamBoostCategory.DEFENSE)
                {
                    defenseBonus = boostBonus;
                }
                else if (boostCategory == GameConstants.TeamBoostCategory.HEALTH)
                {
                    healthBonus = boostBonus;
                }
                else if (boostCategory == GameConstants.TeamBoostCategory.POWER)
                {
                    powerBonus = boostBonus;
                }
                else if (boostCategory == GameConstants.TeamBoostCategory.SIZE)
                {
                    sizeBonus = boostBonus;
                }
            }
        }

        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.DAMAGE_BONUS, damageBonus.ToString(), LocalPlayerTeam);
        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.DEFENSE_BONUS, defenseBonus.ToString(), LocalPlayerTeam);
        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.HEALTH_BONUS, healthBonus.ToString(), LocalPlayerTeam);
        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.POWER_BONUS, powerBonus.ToString(), LocalPlayerTeam);
        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.SIZE_BONUS, sizeBonus.ToString(), LocalPlayerTeam);

        GameStats gameStats = GameStats.Instance;
        if ((gameStats.TeamBoostGroupSelected != null) && (!gameStats.TeamBoostGroupSelected.IsToken))
        {     
            TeamBoostItemGroup boostItemGroup = GameInventory.Instance.GetOreItemGroup(gameStats.TeamBoostGroupSelected.Name);
            boostItemGroup.Amount -= 1;
            GameInventory.Instance.UpdateTeamBoostOreItem(boostItemGroup, true);

            gameStats.TeamBoostGroupSelected = null;
        }
    }

    private void setLocalTeamUnits()
    {
        GameNetwork.ClearLocalTeamUnits(_localPlayerTeam);

        GameStats gameStats = GameStats.Instance;
        GameInventory gameInventory = GameInventory.Instance;

        List<string> teamUnits = GameStats.Instance.TeamUnits;
        string teamUnitsString = "";

        int teamUnitsLenght = teamUnits.Count;
        for (int i = 0; i < teamUnitsLenght; i++)
        {
            int itemNumber = i + 1;
            Transform warGridItemTransform = _teamGridContent.Find("WarTeamGridItem" + itemNumber);

            string unitName = teamUnits[i];

            if (unitName != "-1")
            {
                if (teamUnitsString != "")
                {
                    teamUnitsString += NetworkManager.Separators.VALUES;
                }

                teamUnitsString += unitName;

                Vector3 pos = gameStats.PreparationPositions[i];
                string positionString = pos.x.ToString() + NetworkManager.Separators.VALUES + pos.y.ToString();
                GameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, unitName, positionString, _localPlayerTeam);

                GameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.EXPERIENCE, unitName, gameInventory.GetLocalUnitExp(unitName).ToString(), _localPlayerTeam);

                WarTeamGridItem warTeamGridItem = warGridItemTransform.GetComponent<WarTeamGridItem>();
                _uiTeamGridItems.Add(warTeamGridItem);
                warTeamGridItem.UnitName = unitName;

                warTeamGridItem.View.sprite = Resources.Load<Sprite>("Images/Units/" + unitName);

                int unitTier = GameInventory.Instance.GetUnitTier(unitName);
                warTeamGridItem.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/unit_frame_t" + unitTier);

                warTeamGridItem.transform.Find("level/txt_level").GetComponent<Text>().text = GameInventory.Instance.GetLocalUnitLevel(unitName).ToString();

            }
            else
            {
                warGridItemTransform.gameObject.SetActive(false);
            }
        }

        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.TEAM_UNITS, teamUnitsString, _localPlayerTeam);
    }

    private void moveCloudsToOppositeSide()
    {
        _cloudsContainer.SetParent(_battleField.Find(GridNames.TEAM_1));
        _cloudsContainer.SetAsLastSibling();
        Vector3 cloudsContainerLocalScale = _cloudsContainer.localScale;
        _cloudsContainer.localScale = new Vector3(-cloudsContainerLocalScale.x, cloudsContainerLocalScale.y, cloudsContainerLocalScale.z);
        _cloudsContainer.localPosition = new Vector3(_team1_CloudsLocalPosX, 0, 0);
    }

    private void instantiateRewardChests(string gridName)
    {
        GameInventory gameInventory = GameInventory.Instance;
        GameStats gameStats = GameStats.Instance;

        Debug.Log("GetSinglePlayerLevel: " + gameInventory.GetHigherSinglePlayerLevelCompleted());

        if ((gameStats.Mode == GameStats.Modes.SinglePlayer) && (gameStats.SelectedLevelNumber <= gameInventory.GetHigherSinglePlayerLevelCompleted()))
        {
            if (GameNetwork.Instance.rewardedTrainingLevels[GameStats.Instance.SelectedLevelNumber - 1] == 0)  //By pass check if they didn't got enjin drop, and give it to player
            {
                ObscuredPrefs.SetInt("EnjinWins", 99);  //Will ensure a 100% chance of getting the drop at the end of battle
            }
            else
            {
                return; // no chest for replaying levels, unless it is guaranteed enjin drop
            }
        }

        //if ((gameStats.Mode == GameStats.Modes.Quest) && (gameStats.SelectedLevelNumber <= gameInventory.GetHighestQuestLevelCompleted()))
        //{
        //        return; // no chests for replaying quest levels
        //}

        //No chests in private matches
        if (((string)NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.IS_PRIVATE)) == "True")
        {
            return;
        }

        Transform rewardBoxesContainer = _battleField.Find(gridName + "/RewardBoxesContainer");

        for (int i = 0; i < _fieldRewardChestsAmount; i++)
        {
            Transform rewardBoxTransform = (Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/FieldRewardBox"))).transform;
            rewardBoxTransform.SetParent(rewardBoxesContainer);

            Vector2 randomPos = GameConfig.Instance.GetRandomBattlefieldPosition();
            rewardBoxTransform.localPosition = randomPos;
            rewardBoxTransform.GetComponent<FieldRewardBox>().OnHitCallback = onRewardBoxHit;

            if (gridName == GridNames.TEAM_1)
            {
                Vector3 localScale = rewardBoxTransform.localScale;
                rewardBoxTransform.localScale = new Vector3(-localScale.x, localScale.y, localScale.z);
            }
        }
    }

    private void updateRoundDisplay(int round)
    {
        int roundContentLenght = _roundDisplayGridContent.childCount;
        for (int i = 0; i < roundContentLenght; i++)
        {
            _roundDisplayGridContent.GetChild(i).GetComponent<Image>().enabled = (i < round);
        }
    }

    private void onRewardBoxHit()
    {
        //Debug.LogWarning("onRewardBoxHit");
        _matchLocalData.RewardChestWasHit = true;
    }

    private void setupQuestAiTeamUnits()
    {
        print("War::setupQuestAiTeamUnits");
        int level = GameStats.Instance.SelectedLevelNumber;
        string unitsString = "";
        Quests activeQuest = GameInventory.Instance.GetActiveQuest(); //GameStats.Instance.ActiveQuest;

        if ((activeQuest == Quests.EnjinLegend122) || (activeQuest == Quests.EnjinLegend123) || (activeQuest == Quests.EnjinLegend124)
            || (activeQuest == Quests.EnjinLegend125) || (activeQuest == Quests.EnjinLegend126))
        {
            switch (level)
            {
                case 0:
                    int randomTeamNumber = UnityEngine.Random.Range(1, 4); //1 to 3

                    if (randomTeamNumber == 1)
                    {
                        unitsString = "27|31|4|10|5";
                    }
                    else if (randomTeamNumber == 2)
                    {
                        unitsString = "60|57|62|51|67";
                    }
                    else if (randomTeamNumber == 3)
                    {
                        unitsString = "72|75|80|78|77";
                    }

                    _teamNameText2.text = LocalizationManager.GetTermTranslation("Quest Team #") + randomTeamNumber;

                    break;
                case 1:
                    unitsString = "122|124|123|126|125";
                    break;
                case 2:
                    unitsString = "123|126|122|125|124";
                    break;
                case 3:
                    unitsString = "124|126|123|122|125";
                    break;
                case 4:
                    unitsString = "125|126|122|124|123";
                    break;
                case 5:
                    unitsString = "126|125|122|123|124";
                    break;
            }
        }

        //if (activeQuest == Quests.Swissborg)
        //{
        //    switch (level)
        //    {
        //        case 1:
        //            unitsString = "27|31|4|10|5"; //27, bomber, 31 Destroyer, 4 Tank, 10 healer, 5 Scout
        //            exp = 150;
        //            break;
        //        case 2:
        //            unitsString = "60|57|62|51|67"; //60 bomber, 57 scout, 62 healer, 51 destroyer, 67 Tank
        //            exp = 150;
        //            break;
        //        case 3:
        //            unitsString = "72|75|80|78|77"; // 72 Tank, 75 Healer, 80 Destroyer, 78 scout, 77 Bomber
        //            exp = 150;
        //            break;
        //        case 4:
        //            unitsString = "125|116|104|109|102"; //125 Healer, 116 Tank, 104 Destroyer, 109 bomber, 102 Scout 
        //            exp = 150;
        //            break;
        //    }
        //}

        finalizeUnitsExpAndPos(unitsString, GameConfig.Instance.QuestUnitExp);
        finelizeAiSetup(unitsString);
    }
    
    private void setUpSinglePlayerAiTeamUnits()
    {
        print("War::setUpSinglePlayerAiTeamUnits");
        int level = GameStats.Instance.SelectedLevelNumber;
        string unitsString = "";
        int exp = 0;

        switch (level)
        {
            case 1:
                unitsString = "1|2|3|4|5";
                exp = 0;
                break;
            case 2:
                unitsString = "6|7|8|9|10";
                exp = 0;
                break;
            case 3:
                unitsString = "11|12|13|14|15";
                exp = 0;
                break;
            case 4:
                unitsString = "16|17|18|19|20";
                exp = 10;
                break;
            case 5:
                unitsString = "21|22|23|24|25";
                exp = 10;
                break;
            case 6:
                unitsString = "26|27|28|29|30";
                exp = 30;
                break;
            case 7:
                unitsString = "31|32|33|34|35";
                exp = 30;
                break;
            case 8:
                unitsString = "36|37|38|39|40";
                exp = 70;
                break;
            case 9:
                unitsString = "41|42|43|44|45";
                exp = 30;
                break;
            case 10:
                unitsString = "46|47|48|49|50";
                exp = 30;
                break;
            case 11:
                unitsString = "51|52|53|54|55";
                exp = 70;
                break;
            case 12:
                unitsString = "56|57|58|59|60";
                exp = 70;
                break;
            case 13:
                unitsString = "61|62|63|64|65";
                exp = 150;
                break;
            case 14:
                unitsString = "66|67|68|69|70";
                exp = 150;
                break;
            case 15:
                unitsString = "71|72|73|74|75";
                exp = 70;
                break;
            case 16:
                unitsString = "76|77|78|79|80";
                exp = 70;
                break;
            case 17:
                unitsString = "71|72|78|79|80";
                exp = 150;
                break;
            case 18:
                unitsString = "76|77|73|74|75";
                exp = 150;
                break;
            case 19:
                unitsString = "61|62|63|64|65";
                exp = 310;
                break;
            case 20:
                unitsString = "66|67|68|69|70";
                exp = 310;
                break;
            case 21:
                unitsString = "71|72|73|74|75";
                exp = 310;
                break;
            case 22:
                unitsString = "76|77|78|79|80";
                exp = 310;
                break;
            case 23:
                unitsString = "100|101|78|79|80";
                exp = 310;
                break;
            case 24:
                unitsString = "100|101|78|74|75";
                exp = 310;
                break;
            case 25:
                unitsString = "71|72|102|103|80";
                exp = 310;
                break;
            case 26:
                unitsString = "76|101|78|79|104";
                exp = 310;
                break;
            case 27:
                unitsString = "100|101|75|76|104";
                exp = 310;
                break;
            case 28:
                unitsString = "80|79|75|72|101";
                exp = 310;
                break;
            case 29:
                unitsString = "100|102|104|74|73";
                exp = 310;
                break;
            case 30:
                unitsString = "100|101|102|103|104";
                exp = 310;
                break;
            case 31:
                unitsString = "100|105|106|107|104";
                exp = 310;
                break;
            case 32:
                unitsString = "108|105|106|107|109";
                exp = 310;
                break;
            case 33:
                unitsString = "108|110|111|112|109";
                exp = 310;
                break;
            case 34:
                unitsString = "113|110|111|112|114";
                exp = 310;
                break;
            case 35:
                unitsString = "113|115|106|107|114";
                exp = 310;
                break;
            case 36:
                unitsString = "111|105|108|107|114";
                exp = 310;
                break;
            case 37:
                unitsString = "113|112|110|109|104";
                exp = 310;
                break;
            case 38:
                unitsString = "100|112|114|107|105";
                exp = 310;
                break;
            case 39:
                unitsString = "100|111|110|109|104";
                exp = 310;
                break;
            case 40:
                unitsString = "114|105|113|107|112";
                exp = 310;
                break;
            case 41:
                unitsString = "114|115|116|117|118";
                exp = 310;
                break;
            case 42:
                unitsString = "117|119|120|121|114";
                exp = 310;
                break;
            case 43:
                unitsString = "109|110|120|121|112";
                exp = 310;
                break;
            case 44:
                unitsString = "117|115|113|120|119";
                exp = 310;
                break;
            case 45:
                unitsString = "116|115|119|117|120";
                exp = 310;
                break;
            case 46:
                unitsString = "104|105|112|119|121";
                exp = 310;
                break;
            case 47:
                unitsString = "111|118|110|117|105";
                exp = 310;
                break;
            case 48:
                unitsString = "114|105|113|107|112";
                exp = 310;
                break;
            case 49:
                unitsString = "115|116|117|118|119";
                exp = 310;
                break;
            case 50:
                unitsString = "121|120|119|118|117";
                exp = 310;
                break;
        }

        finalizeUnitsExpAndPos(unitsString, exp);
        finelizeAiSetup(unitsString);
    }

    private void finalizeUnitsExpAndPos(string unitsString, int exp)
    {
        string guestTeam = GameNetwork.TeamNames.GUEST;

        for (int i = 0; i < 5; i++)
        {
            GameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.EXPERIENCE, unitsString.Split("|"[0])[i], exp.ToString(), guestTeam);

            Vector2 pos = GameConfig.Instance.GetRandomBattlefieldPosition();
            string posString = pos.x.ToString() + NetworkManager.Separators.VALUES + pos.y.ToString();
            GameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, unitsString.Split("|"[0])[i], posString, guestTeam);
        }
    }

    private void setUpPvpAiTeamUnits()
    {
        print("War::setupPvpAiTeamUnits");
        GameInventory gameInventory = GameInventory.Instance;

        List<string> hostBronzeUnits = new List<string>();
        List<string> hostSilverUnits = new List<string>();
        List<string> hostGoldUnits = new List<string>();

        string[] hostUnits = GameNetwork.GetTeamUnitNames(GameNetwork.TeamNames.HOST);

        foreach (string unitName in hostUnits)
        {
            int unitTier = gameInventory.GetUnitTier(unitName);

            if (unitTier == GameInventory.Tiers.BRONZE)
            {
                hostBronzeUnits.Add(unitName);
            }
            else if (unitTier == GameInventory.Tiers.SILVER)
            {
                hostSilverUnits.Add(unitName);
            }
            else if (unitTier == GameInventory.Tiers.GOLD)
            {
                hostGoldUnits.Add(unitName);
            }
        }

        List<string> guestBronzeUnits = gameInventory.GetRandomUnitsFromTier(hostBronzeUnits.Count, GameInventory.Tiers.BRONZE);
        List<string> guestSilverUnits = gameInventory.GetRandomUnitsFromTier(hostSilverUnits.Count, GameInventory.Tiers.SILVER);
        List<string> guestGoldUnits = gameInventory.GetRandomUnitsFromTier(hostGoldUnits.Count, GameInventory.Tiers.GOLD);

        string unitsString = "";
        unitsString = setGuestAiUnits(hostBronzeUnits, guestBronzeUnits, unitsString);
        unitsString = setGuestAiUnits(hostSilverUnits, guestSilverUnits, unitsString);
        unitsString = setGuestAiUnits(hostGoldUnits, guestGoldUnits, unitsString);

        finelizeAiSetup(unitsString);
    }

    private void finelizeAiSetup(string unitsString)
    {
        string guestTeam = GameNetwork.TeamNames.GUEST;

        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.DAMAGE_BONUS, "0", guestTeam);
        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.DEFENSE_BONUS, "0", guestTeam);
        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.HEALTH_BONUS, "0", guestTeam);
        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.POWER_BONUS, "0", guestTeam);
        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.SIZE_BONUS, "0", guestTeam);

        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.TEAM_UNITS, unitsString, guestTeam);
    }

    private string setGuestAiUnits(List<string> hostUnitNames, List<string> guestUnitNames, string unitsString)
    {
        print("War::setGuestAiUnits");
        int unitCount = hostUnitNames.Count;
        if (unitCount > 0)
        {
            GameNetwork gameNetwork = GameNetwork.Instance;
            GameInventory gameInventory = GameInventory.Instance;

            for (int i = 0; i < unitCount; i++)
            {
                //Build string
                if (unitsString != "")
                {
                    unitsString += NetworkManager.Separators.VALUES;
                }

                unitsString += guestUnitNames[i];

                //Give guest AI same exp as host units of the same tier. When team is created the levels will be built.  
                int hostUnitExp = GameNetwork.GetLocalPlayerUnitPropertyAsInt(GameNetwork.UnitPlayerProperties.EXPERIENCE, hostUnitNames[i], GameNetwork.TeamNames.HOST) + 1;
                //if (hostUnitExp > 5) hostUnitExp = 5;

                GameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.EXPERIENCE, guestUnitNames[i], hostUnitExp.ToString(), GameNetwork.TeamNames.GUEST);

                ////Set random position
                Vector2 pos = GameConfig.Instance.GetRandomBattlefieldPosition();
                string posString = pos.x.ToString() + NetworkManager.Separators.VALUES + pos.y.ToString();
                GameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, guestUnitNames[i], posString, GameNetwork.TeamNames.GUEST);
            }
        }

        return unitsString;
    }

    private void instantiateTeam(string teamName, string[] teamUnits)
    {
        string playerNickname = GameNetwork.GetNicknameFromPlayerTeam(teamName);
        GameNetwork.SetTeamRoomProperty(GameNetwork.TeamRoomProperties.PLAYER_NICKNAME, teamName, playerNickname);
        GameNetwork.SetTeamRoomProperty(GameNetwork.TeamRoomProperties.DAMAGE_DEALT, teamName, "0");
        GameNetwork.SetTeamRoomProperty(GameNetwork.TeamRoomProperties.DAMAGE_RECEIVED, teamName, "0");
        GameNetwork.SetTeamRoomProperty(GameNetwork.TeamRoomProperties.UNITS_KILLED, teamName, "0");

        string gridName = GetTeamGridName(teamName);
        Transform grid = _battleField.Find(gridName);

        GameNetwork gameNetwork = GameNetwork.Instance;
        GameInventory gameInventory = GameInventory.Instance;
        GameStats gameStats = GameStats.Instance;
        GameConfig gameConfig = GameConfig.Instance;

        bool requiresHorizontalInversion = (!GetUsesAi() && (teamName == GameNetwork.TeamNames.GUEST));

        float battlefieldCenterX = 0;

        if (requiresHorizontalInversion)
        {
            battlefieldCenterX = (gameConfig.BattleFieldMaxPos.x + gameConfig.BattleFieldMinPos.x) * 0.5f;
        }

        int teamLength = teamUnits.Length;
        int networkPlayerId = GameNetwork.GetTeamNetworkPlayerId(teamName);
        string unitNetworkViewsIdsString = "";

        int sizeBonus = NetworkManager.GetAnyPlayerCustomPropertyAsInt(GameNetwork.PlayerCustomProperties.SIZE_BONUS, teamName, networkPlayerId);

        for (int i = 0; i < teamLength; i++)
        {
            string unitName = teamUnits[i];

            int unitExp = GameNetwork.GetAnyPlayerUnitPropertyAsInt(GameNetwork.UnitPlayerProperties.EXPERIENCE, unitName, teamName, networkPlayerId);
            int unitLevel = GameInventory.Instance.GetUnitExpData(unitExp).Level;
            gameNetwork.BuildUnitLevels(unitName, unitLevel, networkPlayerId, teamName);

            string positionString = GameNetwork.GetAnyPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, unitName, teamName, networkPlayerId);
            string[] positionCoords = positionString.Split(NetworkManager.Separators.VALUES);
            float posX = float.Parse(positionCoords[0]);
            float posY = float.Parse(positionCoords[1]);

            if (requiresHorizontalInversion)
            {
                posX = battlefieldCenterX - (posX - battlefieldCenterX);
            }

            object[] instantiationData = { unitName, teamName, i, posX, posY, sizeBonus };

            GameObject unitGameObject = NetworkManager.InstantiateObject("Prefabs/MinMinUnits/" + unitName, Vector3.zero, Quaternion.identity, 0, instantiationData);
            int unitNetworkViewId = unitGameObject.GetComponent<NetworkEntity>().GetNetworkViewId();

            if (unitNetworkViewsIdsString != "")
            {
                unitNetworkViewsIdsString += NetworkManager.Separators.KEYS;
            }

            unitNetworkViewsIdsString += unitNetworkViewId.ToString();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            MinMinUnit unit = unitGameObject.GetComponent<MinMinUnit>();

            GameHacks gameHacks = GameHacks.Instance;
            MinMinUnit.Types unitHackType = MinMinUnit.Types.None;

            if (GameHacks.Instance.RandomizeUnitTypes)
            {
                unitHackType = (MinMinUnit.Types)UnityEngine.Random.Range(1, 6);
            }

            if (gameHacks.SetHostUnitType.Enabled && (teamName == GameNetwork.TeamNames.HOST))
            {
                unitHackType = gameHacks.SetHostUnitType.GetValueAsEnum<MinMinUnit.Types>();
            }

            if (gameHacks.SetGuestUnitType.Enabled && (teamName == GameNetwork.TeamNames.GUEST))
            {
                unitHackType = gameHacks.SetGuestUnitType.GetValueAsEnum<MinMinUnit.Types>();
            }

            if (gameHacks.SetAllUnitsType.Enabled)
            {
                unitHackType = gameHacks.SetAllUnitsType.GetValueAsEnum<MinMinUnit.Types>();
            }

            if (unitHackType != MinMinUnit.Types.None)
            {
                unit.SendDebugSettingsForWar(unitHackType);
            }
#endif
        }

        sendUnitsNetworkIds(teamName, unitNetworkViewsIdsString);
        
        setTeamHealth(teamName, true);
        setTeamHealth(teamName, false);
    }

    private void sendSetupLoadTeams()
    {
        Debug.LogWarning("sendSetupLoadTeams");
        base.SendRpcToAll(nameof(receiveSetupLoadTeams));
    }

    [PunRPC]
    private void receiveSetupLoadTeams()
    {
        Debug.LogWarning("receiveSetupLoadTeams");

        string[] hostUnits = GameNetwork.GetTeamUnitNames(GameNetwork.TeamNames.HOST);
        string[] guestUnits = GameNetwork.GetTeamUnitNames(GameNetwork.TeamNames.GUEST);

        int itemNumber = 1;
        foreach (string unitName in hostUnits)
        {
            // Setup team for loading screen
            Transform Team1ItemTransform = ReadyPopup.transform.Find("panel1/ReadyTeam1/Viewport/Content/WarTeamGridItem" + itemNumber++);
            WarTeamGridItem Team1GridItem = Team1ItemTransform.GetComponent<WarTeamGridItem>();

            int unitTier = GameInventory.Instance.GetUnitTier(unitName);
            Dictionary<string, MinMinUnit> teamUnits = GetTeamUnitsDictionary(GameNetwork.TeamNames.HOST);
            MinMinUnit unit = teamUnits[unitName];
            Team1ItemTransform.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation(unit.Type.ToString());

            Team1GridItem.View.sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
            Team1GridItem.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/unit_frame_t" + unitTier);

            Team1ItemTransform.Find("level/txt_level").GetComponent<Text>().text = GameNetwork.GetAnyPlayerUnitProperty(GameNetwork.UnitPlayerProperties.LEVEL, unitName, GameNetwork.TeamNames.HOST, GameNetwork.Instance.HostPlayerId);
        }

        itemNumber = 1;
        foreach (string unitName in guestUnits)
        {
            // Setup team for loading screen
            Transform Team2ItemTransform = ReadyPopup.transform.Find("panel2/ReadyTeam2/Viewport/Content/WarTeamGridItem" + itemNumber++);
            WarTeamGridItem Team2GridItem = Team2ItemTransform.GetComponent<WarTeamGridItem>();

            int unitTier = GameInventory.Instance.GetUnitTier(unitName);
            Dictionary<string, MinMinUnit> teamUnits = GetTeamUnitsDictionary(GameNetwork.TeamNames.GUEST);
            MinMinUnit unit = teamUnits[unitName];
            Team2ItemTransform.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation(unit.Type.ToString());

            Team2GridItem.View.sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
            Team2GridItem.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/unit_frame_t" + unitTier);

            //Team2ItemTransform.Find("level/txt_level").GetComponent<Text>().text = "?";
            Team2ItemTransform.Find("level/txt_level").GetComponent<Text>().text = GameNetwork.GetAnyPlayerUnitProperty(GameNetwork.UnitPlayerProperties.LEVEL, unitName, GameNetwork.TeamNames.GUEST, GameNetwork.Instance.GuestPlayerId);
        }
    }

    private void sendUnitsNetworkIds(string teamName, string unitNetworkViewsIdsString)
    {
        base.SendRpcToAll(nameof(receiveUnitsNetworkIds), teamName, unitNetworkViewsIdsString);
    }

    [PunRPC]
    private void receiveUnitsNetworkIds(string teamName, string unitNetworkViewsIdsString)
    {
        string[] unitNetworkViewsIds = unitNetworkViewsIdsString.Split(NetworkManager.Separators.KEYS);

        foreach (string unitNetworkViewId in unitNetworkViewsIds)
        {
            NetworkEntity networkEntity = NetworkEntity.Find(int.Parse(unitNetworkViewId));
            MinMinUnit unit = networkEntity.GetComponent<MinMinUnit>();

            string gridName = War.GetTeamGridName(teamName);
            int slotNumber = unit.TeamIndex + 1;
            string slotFindPath = "Battlefield/" + gridName + "/slot" + slotNumber;

            GameObject sceneObject = GameObject.Find(slotFindPath);

            Transform parent = sceneObject.transform;
            unit.transform.SetParent(parent);
            unit.transform.localPosition = Vector2.zero;

            RegisterUnit(unit);
        }

        _readyByTeam[teamName] = true;

        //Debug.LogWarning("War::receiveTeamInstantiated -> _readyByTeam[GameNetwork.TeamNames.HOST]: " + _readyByTeam[GameNetwork.TeamNames.HOST] + " _readyByTeam[GameNetwork.TeamNames.GUEST] " + _readyByTeam[GameNetwork.TeamNames.GUEST]);
        if (_readyByTeam[GameNetwork.TeamNames.HOST] && _readyByTeam[GameNetwork.TeamNames.GUEST])
        {
            NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.READY_TO_FIGHT, true.ToString(), _localPlayerTeam);
        }
    }

    public void onActionPopUpDismissButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();
        sendActionPopUpClose();
    }

    public void OnMatchResultsDismissButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();

        //GameNetwork.ClearLocalTeamUnits(_localPlayerTeam);
        NetworkManager.Disconnect();
        //SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
        GameStats gameStats = GameStats.Instance;

        if (gameStats.Mode == GameStats.Modes.Quest)
        {
            if (GameInventory.Instance.GetAllQuestLevelsCompleted())
            {
                NetworkManager.LoadScene(EnigmaConstants.Scenes.MAIN);
            }
            else
            {
                NetworkManager.LoadScene(GameConstants.Scenes.QUEST_SELECTION);
            }
        }
        else
        {
            NetworkManager.LoadScene(GameConstants.Scenes.LEVELS);
        }
    }

    public void SetUnitForHealing(string targetName, HealerArea healerArea)
    {
        //Debug.LogWarning("War::SetUnitForHealing -> healing: " + healerArea.Strenght);
        setUnitForActionArea<HealerArea>(targetName, healerArea, _healerAreasByTargetByTeam);
    }

    public void SetUnitSpecialDefense(string targetName, TankArea tankArea)
    {
        //Debug.LogWarning("War::SetUnitSpecialDefense -> strenght: " + tankArea.Strenght);
        setUnitForActionArea<TankArea>(targetName, tankArea, _tankAreasByTargetByTeam);
    }

    public void AddHealerArea(HealerArea area)
    {
        addActionArea<HealerArea>(area, _healerAreasByOwnerByTeam);
    }

    public void AddTankArea(TankArea area)
    {
        addActionArea<TankArea>(area, _tankAreasByOwnerByTeam);
    }

    private void addActionArea<T>(T area, Dictionary<string, Dictionary<string, List<T>>> areasByOwnerByTeam) where T:ActionArea
    {
        string ownerTeamName = area.OwnerTeamName;
        string ownerUnitName = area.OwnerUnitName;

        if (!areasByOwnerByTeam[ownerTeamName].ContainsKey(ownerUnitName))
        {
            areasByOwnerByTeam[ownerTeamName].Add(ownerUnitName, new List<T>());
        }

        areasByOwnerByTeam[ownerTeamName][ownerUnitName].Add(area);
    }

    private void setUnitForActionArea<T>(string targetName, T area, Dictionary<string, Dictionary<string, List<T>>> areasByTargetByTeam) where T : ActionArea
    {
        Debug.LogWarning("War::setUnitForActionArea -> targetName: " + targetName);
        string team = area.OwnerTeamName;

        if (!areasByTargetByTeam[team].ContainsKey(targetName))
        {
            areasByTargetByTeam[team].Add(targetName, new List<T>());
        }

        areasByTargetByTeam[team][targetName].Add(area);
    }

    public float GetUnitTankDefense(string team, string unitName)
    {
        //Debug.LogWarning("War::GetUnitTankDefense -> team: " + team + " unitName: " + unitName);
        float strongerTankDefense = 0;

        if (_tankAreasByTargetByTeam[team].ContainsKey(unitName))
        {
            foreach (TankArea tankArea in _tankAreasByTargetByTeam[team][unitName])
            {
                float tankDefense = tankArea.Strenght;
                //Debug.LogWarning("War::GetUnitSpecialDefense -> ownerUnitName: " + ownerUnitName + " tankArea: " + tankArea + " tankDefense: " + tankDefense);
                if (tankDefense > strongerTankDefense)
                {
                    strongerTankDefense = tankDefense;
                }
            }
        }

        //Debug.LogWarning("War::GetUnitTankDefense -> strongerTankDefense: " + strongerTankDefense);

        return strongerTankDefense;
    }

    private void removeTargetFromAreas<T>(string teamName, string targetName, Dictionary<string, Dictionary<string, List<T>>> areasByTargetByTeam) where T : ActionArea
    {
        if (areasByTargetByTeam[teamName].ContainsKey(targetName))
        {
            areasByTargetByTeam[teamName].Remove(targetName);
        }
    }

    private void removeAreasForOwner<T>(string teamName, string ownerUnitName, Dictionary<string, Dictionary<string, List<T>>> areasByOwnerByTeam, Dictionary<string, Dictionary<string, List<T>>> areasByTargetByTeam) where T : ActionArea
    {
        var areasToRemove = new List<ActionArea>();

        Debug.LogWarning("War::removeAreaForOwner -> teamName: " + teamName + " ownerUnitName: " + ownerUnitName);
        if (areasByOwnerByTeam[teamName].ContainsKey(ownerUnitName))
        {
            foreach (T area in areasByOwnerByTeam[teamName][ownerUnitName])
            {
                areasToRemove.Add(area);
            }
        }

        foreach (T area in areasToRemove)
        {
            removeArea<T>(area, areasByOwnerByTeam, areasByTargetByTeam);
        }
    }

    public void RemoveHealerArea(HealerArea healerArea)
    {
        removeArea<HealerArea>(healerArea, _healerAreasByOwnerByTeam, _healerAreasByTargetByTeam);
    }

    public void RemoveTankArea(TankArea tankArea)
    {
        removeArea<TankArea>(tankArea, _tankAreasByOwnerByTeam, _tankAreasByTargetByTeam);
    }

    private void removeArea<T>(T area, Dictionary<string, Dictionary<string, List<T>>> areasByOwnerByTeam, Dictionary<string, Dictionary<string, List<T>>> areasByTargetByTeam) where T : ActionArea
    {
        string areaTeam = area.OwnerTeamName;
        string areaOwner = area.OwnerUnitName;

        areasByOwnerByTeam[areaTeam][areaOwner].Remove(area);

        if (areasByOwnerByTeam[areaTeam][areaOwner].Count == 0)
        {
            areasByOwnerByTeam[areaTeam].Remove(areaOwner);
        }

        var targetsToRemove = new List<string>();
        foreach (string targetUnitName in areasByTargetByTeam[areaTeam].Keys)
        {
            var targetAreaList = areasByTargetByTeam[areaTeam][targetUnitName];
            if (targetAreaList.Contains(area))
            {
                targetAreaList.Remove(area);

                if (targetAreaList.Count == 0)
                {
                    targetsToRemove.Add(targetUnitName);
                }
            }
        }

        foreach (string targetUnitName in targetsToRemove)
        {
            areasByTargetByTeam[areaTeam].Remove(targetUnitName);
        }

        NetworkManager.NetworkDestroy(area.gameObject);
    }

    public void handleAiAttackSuccessful(ActionArea actionArea)
    {
        if (GetIsAiTurn())
        {
            if (actionArea is BomberArea)
            {
                _aiPlayer.LastBomberAttackWasSuccessful = true;
            }
            else if (actionArea is DestroyerArea)
            {
                _aiPlayer.LastDestroyerAttackWasSuccessful = true;
            }
        }
    }

    public void SetUnitHealth(string team, string unitName, int value, bool shouldUpdateTeamHealth)
    {
        GameNetwork.SetUnitHealth(team, unitName, value);

        if (shouldUpdateTeamHealth)
        {
            setTeamHealth(team, false);
        }
    }

    public void RegisterUnit(MinMinUnit unit)
    {
        string teamName = unit.TeamName;

        //Debug.LogWarning("War::RegisterUnit -> teamName: " + teamName + " unit: " + unit.name);
        Dictionary<string, MinMinUnit> teamUnits = GetTeamUnitsDictionary(teamName);
        teamUnits.Add(unit.name, unit);
    }

    public Dictionary<string, MinMinUnit> GetTeamUnitsDictionary(string teamName)
    {
        Dictionary<string, MinMinUnit> teamUnits = null;
        if (teamName == GameNetwork.TeamNames.HOST)
        {
            teamUnits = _hostUnits;
        }
        else if (teamName == GameNetwork.TeamNames.GUEST)
        {
            teamUnits = _guestUnits;
        }

        return teamUnits;
    }

    private void processUnitsHealing()
    {
        List<ActionArea> healerAreasToRemove = new List<ActionArea>();

        foreach (string targetTeamName in _healerAreasByTargetByTeam.Keys)
        {
            foreach (string targetUnitName in _healerAreasByTargetByTeam[targetTeamName].Keys)
            {
                float highestStrenght = 0;
                foreach (HealerArea healerArea in _healerAreasByTargetByTeam[targetTeamName][targetUnitName])
                {              
                    float strenght = healerArea.Strenght;
                    if (strenght > highestStrenght)
                    {
                        highestStrenght = strenght;
                    }
                }

                int unitHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.HEALTH, targetTeamName, targetUnitName);
                int unitMaxHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.MAX_HEALTH, targetTeamName, targetUnitName);

                Dictionary<string, MinMinUnit> teamUnits = GetTeamUnitsDictionary(targetTeamName);
                MinMinUnit unit = teamUnits[targetUnitName];

                int healing = Mathf.RoundToInt((float)unitMaxHealth * (highestStrenght / 15));
                Debug.LogWarning("War::processUnitsHealing -> highestStrenght: " + highestStrenght + " unitMaxHealth: " + unitMaxHealth + " heal: " + healing);

                ScoreFlash.Instance.PushWorld(unit.gameObject.transform.localPosition, unit.gameObject.transform.position, healing, Color.green);

            
                unitHealth += healing;
                if (unitHealth > unitMaxHealth)
                {
                    unitHealth = unitMaxHealth;
                }

                //TODO: Add healing SFX

                SetUnitHealth(targetTeamName, targetUnitName, unitHealth, true);
            }

            foreach (string ownerUnitName in _healerAreasByOwnerByTeam[targetTeamName].Keys)
            {
                foreach (HealerArea healerArea in _healerAreasByOwnerByTeam[targetTeamName][ownerUnitName])
                {
                    healerAreasToRemove.Add(healerArea);
                }
            }
        }

        foreach (HealerArea healerArea in healerAreasToRemove)
        {
            removeArea<HealerArea>(healerArea, _healerAreasByOwnerByTeam, _healerAreasByTargetByTeam);
        }
    }

    private void handleMatchEnd(string winnerTeam)
    {
        _lineRenderer1.gameObject.SetActive(false);
        _lineRenderer2.gameObject.SetActive(false);

        string winnerNickname = "";
        if (GetUsesAi() && (winnerTeam == GameNetwork.TeamNames.GUEST))
        {
            winnerNickname = "AI";
        }
        else
        {
            winnerNickname = GameNetwork.GetNicknameFromPlayerTeam(winnerTeam);
        }
        
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.WINNER_NICKNAME, winnerNickname);

        string loserTeam = GameNetwork.GetOppositeTeamName(winnerTeam);
        string loserNickname = GameNetwork.GetNicknameFromPlayerTeam(loserTeam);

        if (GetUsesAi() && (loserTeam == GameNetwork.TeamNames.GUEST))
        {
            loserNickname = "AI";
        }

        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.LOSER_NICKNAME, loserNickname);

        double endTime = NetworkManager.GetNetworkTime();
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.MATCH_END_TIME, endTime);

        double startTime = double.Parse(NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.MATCH_START_TIME));
        double matchDuration = endTime - startTime;

        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.MATCH_DURATION, matchDuration.ToString());

        sendMatchResults(winnerTeam);
    }

    private void sendMatchResults(string winner)
    {
        base.SendRpcToAll(nameof(receiveMatchResults), winner);
    }

    [PunRPC]
    private void receiveMatchResults(string winner)
    {
        _isVictory = (winner == _localPlayerTeam);

        GameStats gameStats = GameStats.Instance;
        GameHacks gameHacks = GameHacks.Instance;
        GameNetwork gameNetwork = GameNetwork.Instance;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (gameHacks.WarVictory)
        {
            _isVictory = true;
        }
#endif

        bool isPrivateRoom = (((string)NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.IS_PRIVATE)) == "True");

        if (isPrivateRoom)
        {
            setAndDisplayMatchResultsPopUp(true);
            return;
        }

        GameInventory gameInventory = GameInventory.Instance;

        foreach (Transform slot in _teamGridContent)
        {
            WarTeamGridItem teamGridItem = slot.GetComponent<WarTeamGridItem>();
            string unitName = teamGridItem.UnitName;
            if (unitName == "")
            {
                continue;
            }

            int expEarned = 0;
            int minMinHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.HEALTH, _localPlayerTeam, unitName);
            if (minMinHealth > 0)
            {
                if (gameStats.Mode == GameStats.Modes.SinglePlayer)
                {
                    if (_isVictory)
                    {
                        expEarned = gameStats.SelectedLevelNumber * 2;
                    }
                    else
                    {
                        expEarned = 0;
                    }
                }
                else  //pvp and quest
                {
                    if (_isVictory)
                    {
                        expEarned = 10;
                    }
                    else
                    {
                        expEarned = 5;
                    }
                }

                gameInventory.AddExpToUnit(unitName, expEarned);
            }
        }

        gameInventory.SaveUnits();

        bool enjinItemCollectedTransactionSent = false;

        if (_isVictory)
        {
            if (gameStats.Mode == GameStats.Modes.SinglePlayer)
            {
                int levelWon = gameStats.SelectedLevelNumber;
                if (NetworkManager.LoggedIn && (levelWon > gameInventory.GetHigherSinglePlayerLevelCompleted()))
                {
                    NetworkManager.Transaction(GameNetwork.Transactions.NEW_TRAINING_LEVEL, GameNetwork.TransactionKeys.LEVEL, levelWon, onNewTrainingLevel);
                }
            }
            else if (gameStats.Mode == GameStats.Modes.Quest)
            {
                if (gameStats.SelectedLevelNumber == 0)
                {
                    gameStats.QuestScoutPending = true;
                }
                else
                {
                    gameInventory.SetQuestLevelCompleted(gameStats.SelectedLevelNumber);
                }
            }

            bool chestHitHack = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            chestHitHack = gameHacks.ChestHit;
#endif

            if (_matchLocalData.RewardChestWasHit || chestHitHack)
            {
                
                if (NetworkManager.LoggedIn && gameNetwork.IsEnjinLinked)
                {
                    int chance = 5;
                    int enjinWinsRequired = 10;

                    if (gameNetwork.GetIsTokenAvailable(EnjinTokenKeys.MINMINS_TOKEN))
                    {
                        chance = 25;
                        enjinWinsRequired = 5;
                    }

                    Debug.Log("EnjinWins: " + ObscuredPrefs.GetInt("EnjinWins", 0));

                    if(ObscuredPrefs.GetInt("EnjinWins", 0) >= enjinWinsRequired)
                    {
                        chance = 100;
                        ObscuredPrefs.SetInt("EnjinWins", 0);
                    } 
                    else
                    {
                        ObscuredPrefs.SetInt("EnjinWins", ObscuredPrefs.GetInt("EnjinWins", 0) + 1);
                    }

                    int random = UnityEngine.Random.Range(1, 101);
                    //random = 25;
                    Debug.Log("Chance: " + chance + " random: " + random);

                    bool forceEnjinRewardsOnChest = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    forceEnjinRewardsOnChest = gameHacks.ForceEnjinRewardOnChest;
#endif

                    if ((random <= chance) || forceEnjinRewardsOnChest)
                    {
                        NetworkManager.Instance.SendEnjinCollectedTransaction(GameNetwork.TRANSACTION_GAME_NAME, gameStats.SelectedLevelNumber, onEnjinItemCollectedTransactionExternal);
                        enjinItemCollectedTransactionSent = true;
                    }                 
                }

                if (!enjinItemCollectedTransactionSent)
                {
                    rewardWithTierBox(GameInventory.Tiers.BRONZE);
                }
            }
        }

        if (gameStats.Mode == GameStats.Modes.Pvp)
        {
            if (GetIsHost())
            {
                GameNetwork.Instance.SendMatchResultsToServer(winner, onSendMatchResultsToServerCallback);
            }
        }
        else if(!enjinItemCollectedTransactionSent) //Training or Quest
        {
            setAndDisplayMatchResultsPopUp(false);
        }
    }

    private void onNewTrainingLevel(JSONNode response)
    {
        if (NetworkManager.CheckInvalidServerResponse(response, nameof(onNewTrainingLevel)))
        {
            _errorText.text = LocalizationManager.GetTermTranslation(GameNetwork.ServerResponseMessages.SERVER_ERROR);
            _errorText.gameObject.SetActive(true);
        }
    }

    //private void onCompletedQuestResponse(JSONNode response)
    //{
    //    if (GameHacks.Instance.CompleteLevelQuestFail)
    //    {
    //        _errorText.text = LocalizationManager.GetTermTranslation(GameNetwork.ServerResponseMessages.SERVER_ERROR);
    //        _errorText.gameObject.SetActive(true);

    //        _matchResultsPopUp.Open();
    //    }
    //    else if (NetworkManager.CheckInvalidServerResponse(response, nameof(onCompletedQuestResponse)))
    //    {
    //        JSONNode response_hash = response[0];
    //        if (response_hash != null)
    //        {
    //            JSONNode statusNode = NetworkManager.CheckValidNode(response_hash, NetworkManager.TransactionKeys.STATUS);

    //            if (statusNode != null)
    //            {
    //                _errorText.text = LocalizationManager.GetTermTranslation(statusNode.ToString().Trim('"'));
    //            }
    //            else
    //            {
    //                _errorText.text = LocalizationManager.GetTermTranslation(GameNetwork.ServerResponseMessages.SERVER_ERROR);
    //            }

    //            _errorText.gameObject.SetActive(true);
    //            _matchResultsPopUp.Open();
    //        }
    //    }
    //    else
    //    {          
    //        _questCompletePopUp.Open(_matchResultsPopUp);
    //    }
    //}

    private void onEnjinItemCollectedTransactionExternal(JSONNode response)
    {
        JSONNode response_hash = response[0];
        Debug.LogWarning("onEnjinItemCollectedTransactionExternal response: " + response_hash.ToString());
        string status = response_hash["status"].ToString().Trim('"');

        if ((status == "SUCCESS") && !GameHacks.Instance.EnjinItemCollectedFailure)
        {
            _matchLocalData.EnjinCollected = true;
            GameNetwork.Instance.rewardedTrainingLevels[GameStats.Instance.SelectedLevelNumber - 1] = 1;
        }
        else
        {
            rewardWithTierBox(GameInventory.Tiers.BRONZE);
        }

        bool isPrivateRoom = (((string)NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.IS_PRIVATE)) == "True");
        setAndDisplayMatchResultsPopUp(isPrivateRoom);
    }

    private void rewardWithTierBox(int rewardBoxTier)
    {
        _matchLocalData.BoxTiersWithAmountsRewards[rewardBoxTier]++;
    }

    private void onSendMatchResultsToServerCallback(string message, int updatedRating)
    {
        sendMatchResultsServerResponse(message, updatedRating);
    }

    private void sendMatchResultsServerResponse(string message, int updatedRating)
    {
        base.SendRpcToAll(nameof(receiveMatchResultsServerResponse), message, updatedRating);
    }

    [PunRPC]
    private void receiveMatchResultsServerResponse(string message, int updatedRating)
    {
        Debug.LogWarning("War::receiveMatchResultsServerResponse -> message: " + message + " updatedRating: " + updatedRating);
        if (message == GameNetwork.ServerResponseMessages.SUCCESS)
        {
            _errorText.text = "";
            _errorText.gameObject.SetActive(false);

            int oldArenaLevel = GameNetwork.Instance.GetLocalPlayerPvpLevelNumber();
            int updatedLevel = GameNetwork.Instance.GetPvpLevelNumberByRating(updatedRating);

            if (updatedLevel > oldArenaLevel)
            {
                rewardWithTierBox(GameInventory.Tiers.GOLD);
            }
        }
        else
        {
            _errorText.text = LocalizationManager.GetTermTranslation(message);
            _errorText.gameObject.SetActive(true);
        }

        setAndDisplayMatchResultsPopUp(false);
    }

    private void setAndDisplayMatchResultsPopUp(bool isPrivateMatch)
    {
        _actionPopUp.Close();

        GameHacks gameHacks = GameHacks.Instance;
        GameStats gameStats = GameStats.Instance;
        GameInventory gameInventory = GameInventory.Instance;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (gameHacks.TreatBattleResultsAsPrivateMatch)
        {
            isPrivateMatch = true;
        }
#endif

        TeamBoostItemGroup boostGroupReward = null;
        bool gotBoostReward = false;

        if (!isPrivateMatch && ((gameStats.Mode == GameStats.Modes.Pvp) || (gameStats.Mode == GameStats.Modes.Quest)))
        {
            int randomInt = UnityEngine.Random.Range(1, 101);
            Debug.LogWarning("Ore reward random int: " + randomInt);
            gotBoostReward = (randomInt <= 20); //20% chance
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.ForceTeamBoostReward.Enabled)
        {
            gotBoostReward = GameHacks.Instance.ForceTeamBoostReward.ValueAsBool;
        }
#endif

        if (gotBoostReward)
        {
            int arenaNumber = gameStats.SelectedLevelNumber;

            if (arenaNumber == 0) // Quest scout fights are Level 0
            {
                arenaNumber = 1;
            }

            int minBonus = arenaNumber;
            int maxBonus = arenaNumber * 2;

            int bonus = UnityEngine.Random.Range(minBonus, maxBonus);
            string[] categories = { GameConstants.TeamBoostCategory.DAMAGE, GameConstants.TeamBoostCategory.DEFENSE,
                                    GameConstants.TeamBoostCategory.HEALTH, GameConstants.TeamBoostCategory.POWER,
                                    GameConstants.TeamBoostCategory.SIZE };

            string[] baseNames = { GameConstants.TeamBoostEnjinOreItems.DAMAGE, GameConstants.TeamBoostEnjinOreItems.DEFENSE,
                                   GameConstants.TeamBoostEnjinOreItems.HEALTH, GameConstants.TeamBoostEnjinOreItems.POWER,
                                   GameConstants.TeamBoostEnjinOreItems.SIZE };

            int randomNumber = UnityEngine.Random.Range(0, 5);  //Picks from index 0 to 4

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (gameHacks.TeamBoostRewardCategory.Enabled)
            {
                for (int i = 0; i < categories.Length; i++)
                {
                    if (categories[i] == gameHacks.TeamBoostRewardCategory.Value)
                    {
                        randomNumber = i;
                        break;
                    }
                }
            }
#endif

            string rewardCategory = categories[randomNumber];
            string rewardBaseName = baseNames[randomNumber];

            string oreItemName = rewardBaseName + " " + bonus;

            TeamBoostItemGroup boostItemGroup = GameInventory.Instance.GetOreItemGroup(oreItemName);
            boostGroupReward = new TeamBoostItemGroup(oreItemName, boostItemGroup.Amount + 1, bonus, rewardCategory, false);
            GameInventory.Instance.UpdateTeamBoostOreItem(boostGroupReward, true);
        }

        _matchResultsPopUp.SetValues(_matchLocalData, boostGroupReward);
        bool questCompletedTransactionSent = false;

        if (_isVictory)
        {
            int levelCompleted = GameStats.Instance.SelectedLevelNumber;

            if (gameStats.Mode == GameStats.Modes.SinglePlayer)
            {
                gameInventory.SetHigherSinglePlayerLevelCompleted(levelCompleted);
            }
            else if (gameStats.Mode == GameStats.Modes.Quest)
            {
                if (levelCompleted != 0)  //0 is to reveal enemies
                {
                    gameInventory.SetQuestLevelCompleted(levelCompleted);
                }

                if (gameInventory.GetQuestCompleted())
                //if (levelCompleted == gameInventory.GetActiveQuestMaxLevel())
                {
                    //if (gameHacks.CompleteQuestOffline)
                    //{
                    //    _questCompletePopUp.Open(_matchResultsPopUp);
                    //    GameStats.Instance.ActiveQuest = Quests.None;
                    //}
                    //else
                    {
                        //NetworkManager.Transaction(GameNetwork.Transactions.COMPLETED_QUEST_ID, onCompletedQuestResponse);
                        grantQuestRewards();
                        _questCompletePopUp.Open(_matchResultsPopUp);
                        questCompletedTransactionSent = true;
                    }
                }
            }
        }

        if(!questCompletedTransactionSent)
        {
            _matchResultsPopUp.Open();
        }
    }

    private void grantQuestRewards()
    {
        Debug.Log("grantQuestRewards");

        GameInventory gameInventory = GameInventory.Instance;
        Quests activeQuest = gameInventory.GetActiveQuest();

        if (activeQuest == Quests.EnjinLegend122)
        {
            gameInventory.HandleAddUnitOrExp("122");
        }
        else if (activeQuest == Quests.EnjinLegend123)
        {
            gameInventory.HandleAddUnitOrExp("123");
        }
        else if (activeQuest == Quests.EnjinLegend124)
        {
            gameInventory.HandleAddUnitOrExp("124");
        }
        else if (activeQuest == Quests.EnjinLegend125)
        {
            gameInventory.HandleAddUnitOrExp("125");
        }
        else if (activeQuest == Quests.EnjinLegend126)
        {
            gameInventory.HandleAddUnitOrExp("126");
        }
    }

    //Only master client uses this
    private string checkSurvivorWinner()
    {
        string winnerTeam = "";

        bool guestTeamDefeated = isTeamDefeated(GameNetwork.TeamNames.GUEST);
        bool hostTeamDefeated = isTeamDefeated(GameNetwork.TeamNames.HOST);

        if (guestTeamDefeated || hostTeamDefeated)
        {
            if (guestTeamDefeated)
            {
                winnerTeam = GameNetwork.TeamNames.HOST;
            }
            else if (hostTeamDefeated)
            {
                winnerTeam = GameNetwork.TeamNames.GUEST;
            }
        }

        return winnerTeam;
    }

    //Only master client uses this
    private string checkTimeWinner(int roundCount)
    {
        string winnerTeam  = "";

        int maxRounds = _maxRoundsCount;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.RoundCount.Enabled)
        {
            maxRounds = GameHacks.Instance.RoundCount.ValueAsInt;
        }
#endif

        if (roundCount >= maxRounds)
        {
            string hostTeam = GameNetwork.TeamNames.HOST;
            string guestTeam = GameNetwork.TeamNames.GUEST;

            int hostUnitsTotalHealth = getTeamTotalHealth(hostTeam);
            int guestUnitsTotalHealth = getTeamTotalHealth(guestTeam);

            //Team with higher total health wins
            if (hostUnitsTotalHealth > guestUnitsTotalHealth)
            {
                winnerTeam = hostTeam;
            }
            else if (guestUnitsTotalHealth > hostUnitsTotalHealth)
            {
                winnerTeam = guestTeam;
            }

            if (winnerTeam == "")
            {
                if (!GetUsesAi())
                {
                    int hostPlayerRating = GameNetwork.GetLocalPlayerRating(GameNetwork.TeamNames.HOST);
                    int guestPlayerRating = GameNetwork.GetAnyPlayerRating(GameNetwork.Instance.GuestPlayerId, GameNetwork.TeamNames.GUEST);

                    //Lowest rated player wins
                    if (hostPlayerRating < guestPlayerRating)
                    {
                        winnerTeam = hostTeam;
                    }
                    else if (guestPlayerRating < hostPlayerRating)
                    {
                        winnerTeam = guestTeam;
                    }
                }

                if (winnerTeam == "")
                {
                    winnerTeam = hostTeam; // Master client wins
                }
            }
        }

        return winnerTeam;
    }

    //Only master client uses this
    private int getTeamTotalHealth(string team)
    {
        return GameNetwork.GetTeamRoomPropertyAsInt(GameNetwork.TeamRoomProperties.HEALTH, team);
    }

    private void setTeamHealth(string teamName, bool isMaxHealth)
    {
        int networkPlayerId = GameNetwork.GetTeamNetworkPlayerId(teamName);
        string[] teamUnitsForTotalCalculus = GameNetwork.GetTeamUnitNames(teamName); 

        string unitRoomProperty = isMaxHealth ? GameNetwork.UnitRoomProperties.MAX_HEALTH : GameNetwork.UnitRoomProperties.HEALTH;
        string teamRoomProperty = isMaxHealth ? GameNetwork.TeamRoomProperties.MAX_HEALTH : GameNetwork.TeamRoomProperties.HEALTH;

        GameNetwork gameNetwork = GameNetwork.Instance;
        int total = 0;
        foreach (string unitName in teamUnitsForTotalCalculus)
        {
            total += GameNetwork.GetUnitRoomPropertyAsInt(unitRoomProperty, teamName, unitName);
        }

        GameNetwork.SetTeamRoomProperty(teamRoomProperty, teamName, total.ToString());
    }

    //Only master client uses this
    private bool isTeamDefeated(string team)
    {
        return (GameNetwork.GetTeamRoomPropertyAsInt(GameNetwork.TeamRoomProperties.HEALTH, team) <= 0);
    }

    private int getUnitHealth(string team, string unitName)
    {
        return GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.HEALTH, team, unitName);
    }

    public class MatchLocalData
    {
        public Dictionary<int, int> BoxTiersWithAmountsRewards = new Dictionary<int, int>();
        public bool RewardChestWasHit = false;
        public bool EnjinCollected = false;

        public MatchLocalData()
        {
            BoxTiersWithAmountsRewards.Add(GameInventory.Tiers.BRONZE, 0);
            BoxTiersWithAmountsRewards.Add(GameInventory.Tiers.SILVER, 0);
            BoxTiersWithAmountsRewards.Add(GameInventory.Tiers.GOLD, 0);
        }
    }

    public void ClosePopup(GameObject obj)
    {
        obj.transform.parent.gameObject.SetActive(false);
    }
}
