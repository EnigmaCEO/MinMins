using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class War : NetworkEntity
{
    public class GridNames
    {
        public const string TEAM_1 = "Team1";
        public const string TEAM_2 = "Team2";
    }

    [HideInInspector] public bool Ready = true;

    [SerializeField] private float _battleFieldRightSideOffset = 0;
    [SerializeField] private float _actionAreaPosZ = -0.1f;

    [SerializeField] private int _maxRoundsCount = 3;
    [SerializeField] private double _maxDecisionTime = 10;
    [SerializeField] private int _fieldRewardChestsAmount = 1;

    [SerializeField] private Transform _teamGridContent;
    [SerializeField] private float _readyCheckDelay = 2;

    [SerializeField] private float _team1_CloudsLocalPosX = 0.07f;

    [SerializeField] private Text _errorText;
    [SerializeField] private MatchResultsPopUp _matchResultsPopUp;

    [SerializeField] private Text _timeLeftText;

    [SerializeField] private Text _teamTurnText;
    [SerializeField] private Text _unitTurnText;
    [SerializeField] private Text _unitTypeTurnText;
    [SerializeField] private Text _unitTierTurnText;
    [SerializeField] private Text _roundNumberText;
    [SerializeField] private Text _actionsLeftText;

    [SerializeField] private Image _hostTeamHealthFill;
    [SerializeField] private Image _guestTeamHealthFill;

    [SerializeField] private RectTransform UnitTurnHighlightTransform;

    [SerializeField] private Transform _battleField;
    [SerializeField] private GameCamera _gameCamera;

    private Transform _hostGrid;
    private Transform _guestGrid;

    private int _side = 0;
    private List<WarTeamGridItem> _uiTeamGridItems = new List<WarTeamGridItem>();

    private Dictionary<string, MinMinUnit> _hostUnits = new Dictionary<string, MinMinUnit>();
    private Dictionary<string, MinMinUnit> _guestUnits = new Dictionary<string, MinMinUnit>();

    private Dictionary<string, Dictionary<string, Dictionary<string, List<HealerArea>>>> _healerAreasByOwnerByTargetByTeam;
    private Dictionary<string, Dictionary<string, Dictionary<string, List<TankArea>>>> _tanksAreasByOwnerByTargetByTeam;

    private Dictionary<string, List<MinMinUnit>> _exposedUnitsByTeam = new Dictionary<string, List<MinMinUnit>>();


    private double _timeLeftCount = 0;

    private MatchLocalData _matchLocalData = new MatchLocalData();

    private string _localPlayerTeam = "";
    private bool _setupWasCalled = false;

    //private bool _canCheckWinner = false;

    private Dictionary<string, bool> _readyByTeam = new Dictionary<string, bool>();

    public string LocalPlayerTeam { get { return _localPlayerTeam; } }

    private AiPlayer _aiPlayer;
    private double _lastNetworkTime;

    override protected void Awake()
    {
        base.Awake();

        NetworkManager.OnJoinedRoomCallback += onJoinedRoom;

        GameNetwork.OnPlayerTeamUnitsSetCallback += onPlayerTeamUnitsSet;
        GameNetwork.OnReadyToFightCallback += onReadyToFight;
        GameNetwork.OnUnitHealthSetCallback += onUnitHealthSet;
        GameNetwork.OnTeamHealthSetCallback += onTeamHealthSet;
        GameNetwork.OnRoundStartedCallback += onRoundStarted;
        GameNetwork.OnTeamTurnChangedCallback += onTeamTurnChanged;
        GameNetwork.OnHostUnitIndexChangedCallback += onHostUnitIndexChanged;
        GameNetwork.OnGuestUnitIndexChangedCallback += onGuestUnitIndexChanged;
        GameNetwork.OnActionStartedCallback += onActionStarted;

        GameCamera.OnMovementCompletedCallback += onCameraMovementCompleted;

        _readyByTeam.Add(GameNetwork.TeamNames.HOST, false);
        _readyByTeam.Add(GameNetwork.TeamNames.GUEST, false);
    }

    private void Start()
    {
        if (GetUsesAi())
        {
            _aiPlayer = new AiPlayer();

            _exposedUnitsByTeam.Add(GameNetwork.TeamNames.HOST, new List<MinMinUnit>());
            _exposedUnitsByTeam.Add(GameNetwork.TeamNames.GUEST, new List<MinMinUnit>());
        }

        _errorText.gameObject.SetActive(false);
        enableTimeLeftDisplay(false);

        _matchResultsPopUp.DismissButton.onClick.AddListener(() => onMatchResultsDismissButtonDown());
        _matchResultsPopUp.gameObject.SetActive(false);

        UnitTurnHighlightTransform.gameObject.SetActive(false);

        //_matchData.PlayerId = NetworkManager.GetPlayerName();

        _hostGrid = _battleField.Find(GridNames.TEAM_1);
        _guestGrid = _battleField.Find(GridNames.TEAM_2);

        determineLocalPlayerTeam();

        NetworkManager.SetLocalPlayerNickName(_localPlayerTeam + "_Player");  // Nickname hack

        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.READY_TO_FIGHT, false.ToString(), _localPlayerTeam);

        if (GetIsHost())
        {
            _healerAreasByOwnerByTargetByTeam = new Dictionary<string, Dictionary<string, Dictionary<string, List<HealerArea>>>>();
            _healerAreasByOwnerByTargetByTeam.Add(GameNetwork.TeamNames.HOST, new Dictionary<string, Dictionary<string, List<HealerArea>>>());
            _healerAreasByOwnerByTargetByTeam.Add(GameNetwork.TeamNames.GUEST, new Dictionary<string, Dictionary<string, List<HealerArea>>>());

            _tanksAreasByOwnerByTargetByTeam = new Dictionary<string, Dictionary<string, Dictionary<string, List<TankArea>>>>();
            _tanksAreasByOwnerByTargetByTeam.Add(GameNetwork.TeamNames.HOST, new Dictionary<string, Dictionary<string, List<TankArea>>>());
            _tanksAreasByOwnerByTargetByTeam.Add(GameNetwork.TeamNames.GUEST, new Dictionary<string, Dictionary<string, List<TankArea>>>());
        }

        if (GameStats.Instance.Mode == GameStats.Modes.SinglePlayer)
            GameNetwork.Instance.JoinOrCreateRoom();
        else
            setupWar();
    }

    private void OnDestroy()
    {
        NetworkManager.OnJoinedRoomCallback -= onJoinedRoom;

        GameNetwork.OnPlayerTeamUnitsSetCallback -= onPlayerTeamUnitsSet;
        GameNetwork.OnReadyToFightCallback -= onReadyToFight;
        GameNetwork.OnUnitHealthSetCallback -= onUnitHealthSet;
        GameNetwork.OnTeamHealthSetCallback -= onTeamHealthSet;
        GameNetwork.OnRoundStartedCallback -= onRoundStarted;
        GameNetwork.OnTeamTurnChangedCallback -= onTeamTurnChanged;
        GameNetwork.OnHostUnitIndexChangedCallback -= onHostUnitIndexChanged;
        GameNetwork.OnGuestUnitIndexChangedCallback -= onGuestUnitIndexChanged;
        GameNetwork.OnActionStartedCallback -= onActionStarted;

        GameCamera.OnMovementCompletedCallback -= onCameraMovementCompleted;
    }

    static public string GetTeamGridName(string teamName)
    {
        string gridName = GridNames.TEAM_1;
        if (teamName == GameNetwork.TeamNames.GUEST)
            gridName = GridNames.TEAM_2;

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

    public bool GetUsesAi()
    {
        return ((GameStats.Instance.Mode == GameStats.Modes.SinglePlayer) || GameStats.Instance.UsesAiForPvp);
    }

    public bool GetIsAiTurn()
    {
        string teamInTurn = GameNetwork.GetTeamInTurn();
        return ((teamInTurn == GameNetwork.TeamNames.GUEST) && GetUsesAi());
    }

    public void HandleAiSuccessfulAttack(ActionArea actionArea)
    {
        if (actionArea is BomberArea)
            _aiPlayer.LastBomberAttackWasSuccessful = true;
        else if (actionArea is DestroyerArea)
            _aiPlayer.LastDestroyerAttackWasSuccessful = true;
    }

    public void HandleExposedUnit(string teamName, MinMinUnit unit)
    {
        if (!_exposedUnitsByTeam[teamName].Contains(unit))
        {
            Debug.LogWarning("War::HandleExposedUnit Added to exposed units: -> teamName: " + teamName + " unit name: " + unit.name + " unit type: " + unit.Type);
            _exposedUnitsByTeam[teamName].Add(unit);
        }
    }

    private int getPlayerInTurnId()
    {
        string teamInTurn = GameNetwork.GetTeamInTurn();
        int playerId = -1;

        if (!GetIsHost())
            playerId = NetworkManager.GetLocalPlayerId();
        else
            playerId = GameNetwork.Instance.GuestPlayerId;

        return playerId;
    }

    private void onJoinedRoom()
    {
        print("War::OnJoinedRoom -> Is Master Client: " + NetworkManager.GetIsMasterClient());

        if (GameStats.Instance.Mode == GameStats.Modes.SinglePlayer)
        {
            GameNetwork.Instance.HostPlayerId = NetworkManager.GetLocalPlayerId();
            GameNetwork.Instance.GuestPlayerId = NetworkManager.GetLocalPlayerId();
        }

        if (!_setupWasCalled)
            setupWar();
    }

    private void onPlayerTeamUnitsSet(string teamName, string unitsString)
    {
        if (unitsString == null)
            return;

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
                    setUpSinglePlayerAiTeamUnits();
                else //Pvp
                {
                    if (gameStats.UsesAiForPvp)
                        setUpPvpAiTeamUnits();
                }
            }
            else // Team set is Host
                instantiateRewardChests(GridNames.TEAM_2);
        }
        else // Local player is Guest
        {
            if (isTeamSetHost)
                instantiateRewardChests(GridNames.TEAM_1);
        }
    }

    private void onReadyToFight(string teamName, bool ready)
    {
        //Debug.LogWarning("War::onReadyToFight -> teamName: " + teamName + " ready: " + ready);
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
                        guestReady = true;
                    else
                        guestReady = bool.Parse(NetworkManager.GetAnyPlayerCustomProperty(GameNetwork.PlayerCustomProperties.READY_TO_FIGHT, GameNetwork.TeamNames.GUEST, GameNetwork.GetTeamNetworkPlayerId(GameNetwork.TeamNames.GUEST)));
                }
                else if (teamName == GameNetwork.TeamNames.GUEST)
                {
                    guestReady = true;
                    hostReady = bool.Parse(NetworkManager.GetAnyPlayerCustomProperty(GameNetwork.PlayerCustomProperties.READY_TO_FIGHT, GameNetwork.TeamNames.HOST, GameNetwork.GetTeamNetworkPlayerId(GameNetwork.TeamNames.HOST)));
                }

                //Debug.LogWarning("War::onReadyToFight -> hostReady: " + hostReady + " guestReady: " + guestReady);

                if (hostReady && guestReady)
                {
                    NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.MATCH_START_TIME, NetworkManager.GetNetworkTime());
                    NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ROUND_COUNT, 1); //Starts combat cycle
                }
            }
        }
    }

    private void onUnitHealthSet(string teamName, string unitName, int health)
    {
        if (health <= 0)
            handleUnitDeath(teamName, unitName);

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

        //if (GetIsHost())
        //{
        //    if (_canCheckWinner)
        //    {
        //        string winnerTeam = checkSurvivorWinner();
        //        if (winnerTeam != "")
        //            handlMatchEnd(winnerTeam);
        //    }
        //    else
        //        _canCheckWinner = true;
        //}
    }

    private void handleUnitDeath(string teamName, string unitName)
    {
        Dictionary<string, MinMinUnit> teamUnits = GetTeamUnitsDictionary(teamName);
        MinMinUnit unit = teamUnits[unitName];
        unit.gameObject.SetActive(false);

        if (GetIsHost())
        {
            removeAreaForOwner<HealerArea>(teamName, unitName, _healerAreasByOwnerByTargetByTeam);
            removeAreaForOwner<TankArea>(teamName, unitName, _tanksAreasByOwnerByTargetByTeam);

            removeTargetFromAreas<HealerArea>(teamName, unitName, _healerAreasByOwnerByTargetByTeam);
            removeTargetFromAreas<TankArea>(teamName, unitName, _tanksAreasByOwnerByTargetByTeam);

            string killerTeam = GameNetwork.GetOppositeTeamName(teamName);
            int unitsKilled = GameNetwork.GetTeamRoomPropertyAsInt(GameNetwork.TeamRoomProperties.UNITS_KILLED, killerTeam);
            GameNetwork.SetTeamRoomProperty(GameNetwork.TeamRoomProperties.UNITS_KILLED, killerTeam, (unitsKilled + 1).ToString());
        }
    }

    private void onRoundStarted(int roundNumber)
    {
        _roundNumberText.text = "Round: " + roundNumber.ToString();

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
        _teamTurnText.text = "Turn: " + teamName + " | Mine: " + _localPlayerTeam;

        if (GetIsHost())
            skipUnitIndex(teamName);
    }

    private int getTeamUnitIndex(string teamName)
    {
        string roomProperty = GameNetwork.RoomCustomProperties.HOST_UNIT_INDEX;
        if (teamName == GameNetwork.TeamNames.GUEST)
            roomProperty = GameNetwork.RoomCustomProperties.GUEST_UNIT_INDEX;

        int unitIndex = NetworkManager.GetRoomCustomPropertyAsInt(roomProperty);
        return unitIndex;
    }

    private void skipUnitIndex(string teamName)
    {
        string roomProperty = GameNetwork.RoomCustomProperties.HOST_UNIT_INDEX;
        if (teamName == GameNetwork.TeamNames.GUEST)
            roomProperty = GameNetwork.RoomCustomProperties.GUEST_UNIT_INDEX;

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
            return;

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
                        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ROUND_COUNT, roundCount + 1);
                    else
                        handlMatchEnd(winner);
                }
                else // Is GUEST
                    changeTurn(teamName);
            }
        }
        else
        {
            MinMinUnit unit = units.Values.ElementAt(unitIndex);
            string unitName = unit.name;

            if (GetIsHost() && (unit.Type == MinMinUnit.Types.Tank))
                removeAreaForOwner<TankArea>(teamName, unitName, _tanksAreasByOwnerByTargetByTeam);

            int unitHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.HEALTH, teamName, unitName);

            if (unitHealth > 0)
            {
                _unitTurnText.text = "Unit turn: " + unitName + " | Index: " + unitIndex;
                handleHighlight(unitIndex, teamName);
                _gameCamera.HandleMovement(teamName, units[unitName].Type);
            }
            else if (GetIsHost())
            {
                if ((unitIndex + 1) >= teamUnitsCount)
                    changeTurn(teamName);
                else
                    skipUnitIndex(teamName);
            }
        }
    }

    private void onCameraMovementCompleted(string sideTeam)
    {
        MinMinUnit unit = getUnitInTurn();
        int unitTier = GameInventory.Instance.GetUnitTier(unit.name);

        //unitTier = 1; //Unit tier hack

        _unitTierTurnText.text = "Unit tier: " + unitTier.ToString();
        _unitTypeTurnText.text = "Unit type: " + unit.Type.ToString();

        if (GetIsHost())
            NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ACTIONS_LEFT, unitTier.ToString());
    }

    private void onActionStarted(int actionsLeft)
    {
        _actionsLeftText.text = "Actions Left: " + actionsLeft;
        string teamInTurn = GameNetwork.GetTeamInTurn();

        //Debug.LogWarning("War::onActionStarted: " + actionsLeft + " teamInTurn: " + teamInTurn);

        if (actionsLeft > 0)
        {
            if (GetIsAiTurn())
                StartCoroutine(handleAiPlayerInput(teamInTurn));
            else if (teamInTurn == _localPlayerTeam)
                StartCoroutine(handleHumanPlayerInput());
        }
        else if (GetIsHost())
            changeTurn(teamInTurn);
    }

    private IEnumerator handleAiPlayerInput(string teamInTurn)
    {
        //resetDecisionTimeCount();
        float delay = Random.Range(AiPlayer._MIN_DECISION_DELAY, AiPlayer._MAX_DECISION_DELAY);

        while (true)
        {
            //reduceDecisionTimeCount();

            delay -= Time.deltaTime;
            if (delay <= 0)
            {
                MinMinUnit unit = getUnitInTurn();
                Dictionary<string, MinMinUnit> teamInTurnUnits = GetTeamUnitsDictionary(teamInTurn);
                Vector2 aiWorldInput2D = _aiPlayer.GetWorldInput2D(unit.Type, teamInTurnUnits, _exposedUnitsByTeam, _healerAreasByOwnerByTargetByTeam);
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
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 tapWorldPosition = _gameCamera.MyCamera.ScreenToWorldPoint(Input.mousePosition);
                GameConfig gameConfig = GameConfig.Instance;

                if ((tapWorldPosition.y > gameConfig.BattleFieldMinPos.y) || (tapWorldPosition.y < gameConfig.BattleFieldMaxPos.y)
                || (tapWorldPosition.x < gameConfig.BattleFieldMaxPos.x) || (tapWorldPosition.x > gameConfig.BattleFieldMinPos.x))
                {
                    sendPlayerTargetInput(tapWorldPosition, _localPlayerTeam);
                    enableTimeLeftDisplay(false);
                    yield break;
                }
            }
            else if (checkDecisionTimeOut())
                yield break;
           
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
        //return false; // decisition time hack
        
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
        _timeLeftText.text = "Time Left: " + (Mathf.CeilToInt(((float)_timeLeftCount))).ToString();
    }

    private void sendPlayerTargetInput(Vector3 playerInputWorldPosition, string teamName)
    {
        //Debug.LogWarning("sendPlayerTargetInput");
        base.SendRpcToMasterClient("receivePlayerTargetInput", playerInputWorldPosition, teamName, NetworkManager.GetLocalPlayerId());
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
            directions.Add(Vector3.zero);


        handleActionAreaCreation(actionAreaPos, directions, unitInTurn, teamName, networkPlayerId);
    }

    //Only Master Client uses this
    private void handleActionAreaCreation(Vector3 inputWorldPosition, List<Vector3> directions, MinMinUnit unit, string teamName, int networkPlayerId)
    {
        float actionTime = 0;
        foreach (Vector3 direction in directions)
        {
            string unitTypeString = unit.Type.ToString();
            string actionAreaPrefabName = unitTypeString + "Area";
            object[] instantiationData = { actionAreaPrefabName, inputWorldPosition, direction, unit.name, unit.EffectName, teamName, networkPlayerId };
            GameObject actionAreaObject = NetworkManager.InstantiateObject(ActionArea.ACTION_AREAS_RESOURCES_FOLDER_PATH + actionAreaPrefabName, Vector3.zero, Quaternion.identity, 0, instantiationData);

            ActionArea actionArea = actionAreaObject.GetComponent<ActionArea>();
            actionTime = actionArea.ActionTime;
            //actionArea.SendSetupData(actionAreaPrefabName, inputWorldPosition, direction, unit.name, unit.EffectName, teamName, networkPlayerId);
        }

        StartCoroutine(HandleActionTime(actionTime));
    }

    //Only Master Client uses this
    private IEnumerator HandleActionTime(float actionTime)
    {
        //time = 60; // Actions time hack

        yield return new WaitForSeconds(actionTime);

        string winnerTeam = checkSurvivorWinner();
        if (winnerTeam != "")
            handlMatchEnd(winnerTeam);
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
            unitIndex = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.HOST_UNIT_INDEX);
        else
            unitIndex = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.GUEST_UNIT_INDEX);

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
            UnitTurnHighlightTransform.gameObject.SetActive(false);
    }

    private void setupWar()
    {
        if (_localPlayerTeam == GameNetwork.TeamNames.GUEST)
        {
            //if(_localPlayerTeam == GameNetwork.VirtualPlayerIds.HOST)  //Test hack
            _gameCamera.SetCameraForGuest();
            moveCloudsToOppositeSide();
        }

        setLocalTeamUnits();

        _setupWasCalled = true;
    }

    private void determineLocalPlayerTeam()
    {
        if (NetworkManager.IsPhotonOffline()) //Single player
            _localPlayerTeam = GameNetwork.TeamNames.HOST;
        else // Pvp
        {
            if (NetworkManager.GetIsMasterClient())
                _localPlayerTeam = GameNetwork.TeamNames.HOST;
            else
                _localPlayerTeam = GameNetwork.TeamNames.GUEST;
        }

        print("War::determinLocalPlayerTeam -> _localPlayerTeam: " + _localPlayerTeam);
    }

    private void setLocalTeamUnits()
    {
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
                    teamUnitsString += NetworkManager.Separators.VALUES;

                teamUnitsString += unitName;

                Vector3 pos = gameStats.PreparationPositions[i];
                string positionString = pos.x.ToString() + NetworkManager.Separators.VALUES + pos.y.ToString();
                GameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, unitName, positionString, _localPlayerTeam);

                GameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.EXPERIENCE, unitName, gameInventory.GetLocalUnitExp(unitName).ToString(), _localPlayerTeam);

                WarTeamGridItem warTeamGridItem = warGridItemTransform.GetComponent<WarTeamGridItem>();
                _uiTeamGridItems.Add(warTeamGridItem);
                warTeamGridItem.UnitName = unitName;

                warTeamGridItem.View.sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
            }
            else
                warGridItemTransform.gameObject.SetActive(false);
        }

        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.TEAM_UNITS, teamUnitsString, _localPlayerTeam);
    }

    private void moveCloudsToOppositeSide()
    {
        Transform cloudsContainer = _battleField.Find(GridNames.TEAM_2 + "/CloudsContainer");

        if (cloudsContainer == null)
            return;

        cloudsContainer.SetParent(_battleField.Find(GridNames.TEAM_1));
        cloudsContainer.SetAsLastSibling();
        Vector3 cloudsContainerLocalScale = cloudsContainer.localScale;
        cloudsContainer.localScale = new Vector3(-cloudsContainerLocalScale.x, cloudsContainerLocalScale.y, cloudsContainerLocalScale.z);
        cloudsContainer.localPosition = new Vector3(_team1_CloudsLocalPosX, 0, 0);
    }

    private void instantiateRewardChests(string gridName)
    {
        Transform rewardBoxesContainer = _battleField.Find(gridName + "/RewardBoxesContainer");

        for (int i = 0; i < _fieldRewardChestsAmount; i++)
        {
            Transform rewardBoxTransform = (Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/FieldRewardBox"))).transform;
            rewardBoxTransform.SetParent(rewardBoxesContainer);

            Vector2 randomPos = getRandomBattlefieldPosition();
            rewardBoxTransform.localPosition = randomPos;
            rewardBoxTransform.GetComponent<FieldRewardBox>().OnHitCallback = onRewardBoxHit;

            if (gridName == GridNames.TEAM_1)
            {
                Vector3 localScale = rewardBoxTransform.localScale;
                rewardBoxTransform.localScale = new Vector3(-localScale.x, localScale.y, localScale.z);
            }
        }
    }

    private void onRewardBoxHit()
    {
        //Debug.LogWarning("onRewardBoxHit");
        rewardWithRandomBox();
    }

    private void setUpSinglePlayerAiTeamUnits()
    {
        //print("War::setUpSinglePlayerAiTeamUnits");
        setUpPvpAiTeamUnits();
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
                hostBronzeUnits.Add(unitName);
            else if (unitTier == GameInventory.Tiers.SILVER)
                hostSilverUnits.Add(unitName);
            else if (unitTier == GameInventory.Tiers.GOLD)
                hostGoldUnits.Add(unitName);
        }

        List<string> guestBronzeUnits = gameInventory.GetRandomUnitsFromTier(hostBronzeUnits.Count, GameInventory.Tiers.BRONZE);
        List<string> guestSilverUnits = gameInventory.GetRandomUnitsFromTier(hostSilverUnits.Count, GameInventory.Tiers.SILVER);
        List<string> guestGoldUnits = gameInventory.GetRandomUnitsFromTier(hostGoldUnits.Count, GameInventory.Tiers.GOLD);

        string unitsString = "";
        unitsString = setGuestAiUnits(hostBronzeUnits, guestBronzeUnits, unitsString);
        unitsString = setGuestAiUnits(hostSilverUnits, guestSilverUnits, unitsString);
        unitsString = setGuestAiUnits(hostGoldUnits, guestGoldUnits, unitsString);

        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.TEAM_UNITS, unitsString, GameNetwork.TeamNames.GUEST);
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
                    unitsString += NetworkManager.Separators.VALUES;

                unitsString += guestUnitNames[i];

                //Give guest AI same exp as host units of the same tier. When team is created the levels will be built.  
                int hostUnitExp = GameNetwork.GetLocalPlayerUnitPropertyAsInt(GameNetwork.UnitPlayerProperties.EXPERIENCE, hostUnitNames[i], GameNetwork.TeamNames.HOST);
                GameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.EXPERIENCE, guestUnitNames[i], hostUnitExp.ToString(), GameNetwork.TeamNames.GUEST);

                ////Set random position
                Vector2 pos = getRandomBattlefieldPosition();
                string posString = pos.x.ToString() + NetworkManager.Separators.VALUES + pos.y.ToString();
                GameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, guestUnitNames[i], posString, GameNetwork.TeamNames.GUEST);
            }
        }

        return unitsString;
    }

    private Vector2 getRandomBattlefieldPosition()
    {
        GameConfig gameConfig = GameConfig.Instance;
        return new Vector2(Random.Range(gameConfig.BattleFieldMinPos.x, gameConfig.BattleFieldMaxPos.x) + _battleFieldRightSideOffset, Random.Range(gameConfig.BattleFieldMinPos.y, gameConfig.BattleFieldMaxPos.y));
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
        //requiresHorizontalInversion = true;  // test hack
        float battlefieldCenterX = 0;
        if(requiresHorizontalInversion)
            battlefieldCenterX = (gameConfig.BattleFieldMaxPos.x + gameConfig.BattleFieldMinPos.x) * 0.5f;

        int teamLength = teamUnits.Length;
        int networkPlayerId = GameNetwork.GetTeamNetworkPlayerId(teamName);

        for (int i = 0; i < teamLength; i++)
        {
            string unitName = teamUnits[i];

            int unitExp = GameNetwork.GetAnyPlayerUnitPropertyAsInt(GameNetwork.UnitPlayerProperties.EXPERIENCE, unitName, teamName, networkPlayerId);
            gameNetwork.BuildUnitLevels(unitName, unitExp, networkPlayerId, teamName);

            string positionString = GameNetwork.GetAnyPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, unitName, teamName, networkPlayerId);
            string[] positionCoords = positionString.Split(NetworkManager.Separators.VALUES);
            float posX = float.Parse(positionCoords[0]);
            float posY = float.Parse(positionCoords[1]);

            if (requiresHorizontalInversion)
                posX = battlefieldCenterX - (posX - battlefieldCenterX); 

            object[] instantiationData = { unitName, teamName, i, posX, posY };

            GameObject unitGameObject = NetworkManager.InstantiateObject("Prefabs/MinMinUnits/" + unitName, Vector3.zero, Quaternion.identity, 0, instantiationData);

            MinMinUnit unit = unitGameObject.GetComponent<MinMinUnit>();

            MinMinUnit.Types unitDebugType = MinMinUnit.Types.Bomber;
            //Type vs Type Test hack =======================================
            //if (teamName == GameNetwork.TeamNames.HOST)
            //    unitDebugType = MinMinUnit.Types.Scout;
            //else
            //    unitDebugType = MinMinUnit.Types.Healer;
            //==============================================================

            //Specific Type Test hack =======================================
            //unitDebugType = MinMinUnit.Types.Scout;
            //==============================================================

            //Tanks against Bombers Test hack =======================================
            //if (teamName == GameNetwork.TeamNames.HOST)
            //    unitDebugType = (MinMinUnit.Types)Random.Range(0, 5);
            //else
            //    unitDebugType = MinMinUnit.Types.Tank;
            //==============================================================

            //Random type Test hack =======================================
            unitDebugType = (MinMinUnit.Types)Random.Range(0, 5);
            //==============================================================

            unit.SendDebugSettingsForWar(unitDebugType);
        }

        setTeamHealth(teamName, true);
        setTeamHealth(teamName, false);
    }

    public void onMatchResultsDismissButtonDown()
    {
        GameNetwork.ClearLocalTeamUnits(_localPlayerTeam);
        SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
    }

    public void SetUnitForHealing(string targetName, HealerArea healerArea)
    {
        Debug.LogWarning("War::SetUnitForHealing -> healing: " + healerArea.Healing);
        setUnitForActionArea<HealerArea>(targetName, healerArea, _healerAreasByOwnerByTargetByTeam);
    }

    public void SetUnitSpecialDefense(string targetName, TankArea tankArea)
    {
        Debug.LogWarning("War::SetUnitSpecialDefense -> defense: " + tankArea.Defense);
        setUnitForActionArea<TankArea>(targetName, tankArea, _tanksAreasByOwnerByTargetByTeam);
    }

    private void setUnitForActionArea<T>(string targetName, T area, Dictionary<string, Dictionary<string, Dictionary<string, List<T>>>> areasByOwnerByTargetByTeam) where T : ActionArea
    {
        Debug.LogWarning("War::setUnitForActionArea -> targetName: " + targetName + " owner: " + area.OwnerUnitName + " team " + area.OwnerTeamName);
        string team = area.OwnerTeamName;
        string owner = area.OwnerUnitName;

        if (!areasByOwnerByTargetByTeam[team].ContainsKey(targetName))
            areasByOwnerByTargetByTeam[team].Add(targetName, new Dictionary<string, List<T>>());

        if (!areasByOwnerByTargetByTeam[team][targetName].ContainsKey(owner))
            areasByOwnerByTargetByTeam[team][targetName].Add(owner, new List<T>());

        areasByOwnerByTargetByTeam[team][targetName][owner].Add(area);
    }

    public int GetUnitSpecialDefense(string team, string unitName)
    {
        //Debug.LogWarning("War::GetUnitSpecialDefense -> team: " + team + " unitName: " + unitName);
        int unitSpecialDefense = 0;

        if (_tanksAreasByOwnerByTargetByTeam[team].ContainsKey(unitName))
        {
            foreach (string ownerUnitName in _tanksAreasByOwnerByTargetByTeam[team][unitName].Keys)
            {
                foreach (TankArea tankArea in _tanksAreasByOwnerByTargetByTeam[team][unitName][ownerUnitName])
                {
                    int defenseInput = tankArea.Defense;
                    //Debug.LogWarning("War::GetUnitSpecialDefense -> ownerUnitName: " + ownerUnitName + " tankArea: " + tankArea + " defenseInput: " + defenseInput);
                    unitSpecialDefense += defenseInput;
                }
            }
        }

        //Debug.LogWarning("War::GetUnitSpecialDefense -> unitSpecialDefense: " + unitSpecialDefense);

        return unitSpecialDefense;
    }

    private void removeTargetFromAreas<T>(string teamName, string targetName, Dictionary<string, Dictionary<string, Dictionary<string, List<T>>>> areasByOwnerByTargetByTeam) where T : ActionArea
    {
        if (areasByOwnerByTargetByTeam[teamName].ContainsKey(targetName))
            areasByOwnerByTargetByTeam[teamName].Remove(targetName);
    }

    //private void removeTankAreaForOwner(string teamName, string ownerUnitName)
    //{
    //    removeAreaForOwner<TankArea>(teamName, ownerUnitName, _tanksAreasByOwnerByTargetByTeam);
    //}

    //private void removeHealingAreaForOwner(string teamName, string ownerUnitName)
    //{
    //    removeAreaForOwner<HealerArea>(teamName, ownerUnitName, _healerAreasByOwnerByTargetByTeam);
    //}

    private void removeAreaForOwner<T>(string teamName, string ownerUnitName, Dictionary<string, Dictionary<string, Dictionary<string, List<T>>>> areasByOwnerByTargetByTeam) where T: ActionArea
    {
        List<ActionArea> areasToDestroy = new List<ActionArea>();

        //Debug.LogWarning("War::removeAreaForOwner -> teamName: " + teamName + " ownerUnitName: " + ownerUnitName);
        foreach (string unitName in areasByOwnerByTargetByTeam[teamName].Keys)
        {
            //Debug.LogWarning("War::removeAreaForOwner -> unitName: " + unitName);
            if (areasByOwnerByTargetByTeam[teamName][unitName].ContainsKey(ownerUnitName))
            {
                //Debug.LogWarning("War::removeAreaForOwner -> ownerUnitName: " + ownerUnitName);
                foreach (T area in areasByOwnerByTargetByTeam[teamName][unitName][ownerUnitName])
                {
                    //Debug.LogWarning("War::removeAreaForOwner -> Area to destroy: " + area);

                    if (!areasToDestroy.Contains(area))
                    {
                        //Debug.LogWarning("War::removeAreaForOwner -> Area to list: " + area);
                        areasToDestroy.Add(area);
                    }
                }

                areasByOwnerByTargetByTeam[teamName][unitName].Remove(ownerUnitName);
            }
        }

        ActionArea.DestroyActionAreaList(areasToDestroy);
    }

    public void RemoveHealerArea(HealerArea healerArea)
    {
        removeArea<HealerArea>(healerArea, _healerAreasByOwnerByTargetByTeam);
    }

    public void RemoveTankArea(TankArea tankArea)
    {
        removeArea<TankArea>(tankArea, _tanksAreasByOwnerByTargetByTeam);
    }

    private void removeArea<T>(T area, Dictionary<string, Dictionary<string, Dictionary<string, List<T>>>> areasByOwnerByTargetByTeam) where T : ActionArea
    {
        string team = area.OwnerTeamName;
        string owner = area.OwnerUnitName;

        foreach (string unitName in areasByOwnerByTargetByTeam[team].Keys)
        {
            if (areasByOwnerByTargetByTeam[team][unitName].ContainsKey(owner))
            {
                if (areasByOwnerByTargetByTeam[team][unitName][owner].Contains(area))
                    areasByOwnerByTargetByTeam[team][unitName][owner].Remove(area);
            }
        }

        NetworkManager.NetworkDestroy(area.gameObject);
    }

    public void handleAiAttackSuccessful(ActionArea actionArea)
    {
        if (GetIsAiTurn())
        {
            if (actionArea is BomberArea)
                _aiPlayer.LastBomberAttackWasSuccessful = true;
            else if (actionArea is DestroyerArea)
                _aiPlayer.LastDestroyerAttackWasSuccessful = true;
        }
    }

    public void SetUnitHealth(string team, string unitName, int value, bool shouldUpdateTeamHealth)
    {
        GameNetwork.SetUnitHealth(team, unitName, value);

        if (shouldUpdateTeamHealth)
            setTeamHealth(team, false);
    }

    public void RegisterUnit(string teamName, MinMinUnit unit)
    {
        //Debug.LogWarning("War::RegisterUnit -> teamName: " + teamName + " unit: " + unit.name);
        Dictionary<string, MinMinUnit> teamUnits = GetTeamUnitsDictionary(teamName);
        teamUnits.Add(unit.name, unit);

        int teamPlayersAmount = GameNetwork.GetTeamUnitNames(teamName).Length;
        //Debug.LogWarning("War::RegisterUnit -> teamName: " + teamName + " unit: " + unit.name + " units on list: " + teamPlayersAmount + " units registered: " + teamUnits.Count);
        if (teamUnits.Count == teamPlayersAmount)
        {
            _readyByTeam[teamName] = true;

            if (_readyByTeam[GameNetwork.TeamNames.HOST] && _readyByTeam[GameNetwork.TeamNames.GUEST])
                NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.READY_TO_FIGHT, true.ToString(), _localPlayerTeam);

        }
    }

    public Dictionary<string, MinMinUnit> GetTeamUnitsDictionary(string teamName)
    {
        Dictionary<string, MinMinUnit> teamUnits = null;
        if (teamName == GameNetwork.TeamNames.HOST)
            teamUnits = _hostUnits;
        else //guest
            teamUnits = _guestUnits;

        return teamUnits;
    }

    private void processUnitsHealing()
    {
        List<ActionArea> healerAreasToDestroy = new List<ActionArea>();

        foreach (string team in _healerAreasByOwnerByTargetByTeam.Keys)
        {
            foreach (string unitName in _healerAreasByOwnerByTargetByTeam[team].Keys)
            {
                int strongerHealing = 0;
                foreach (string owner in _healerAreasByOwnerByTargetByTeam[team][unitName].Keys)
                {              
                    foreach (HealerArea healerArea in _healerAreasByOwnerByTargetByTeam[team][unitName][owner])
                    {
                        int healing = healerArea.Healing;
                        if (healing > strongerHealing)
                            strongerHealing = healing;

                        if (!healerAreasToDestroy.Contains(healerArea))
                            healerAreasToDestroy.Add(healerArea);
                    }
                }

                int unitHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.HEALTH, team, unitName);
                int unitMaxHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.MAX_HEALTH, team, unitName);

                unitHealth += strongerHealing;  //TODO: Check if healing formula is needed;
                if (unitHealth > unitMaxHealth)
                    unitHealth = unitMaxHealth;

                //TODO: Add healing SFX

                SetUnitHealth(team, unitName, unitHealth, true);
            }

            _healerAreasByOwnerByTargetByTeam[team].Clear();
        }

        ActionArea.DestroyActionAreaList(healerAreasToDestroy);
    }

    private void handlMatchEnd(string winnerTeam)
    {
        string winnerNickname = "";
        if (GetUsesAi() && (winnerTeam == GameNetwork.TeamNames.GUEST))
            winnerNickname = "Ai_Player";
        else
            winnerNickname = GameNetwork.GetNicknameFromPlayerTeam(winnerTeam);
        
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.WINNER_NICKNAME, winnerNickname);

        string loserTeam = GameNetwork.GetOppositeTeamName(winnerTeam);
        string loserNickname = GameNetwork.GetNicknameFromPlayerTeam(loserTeam);
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
        base.SendRpcToAll("receiveMatchResults", winner);
    }

    //Master Client Method
    [PunRPC]
    private void receiveMatchResults(string winner)
    {
        bool isVictory = (winner == _localPlayerTeam);

        GameInventory gameInventory = GameInventory.Instance;

        foreach (Transform slot in _teamGridContent)
        {
            WarTeamGridItem teamGridItem = slot.GetComponent<WarTeamGridItem>();
            string unitName = teamGridItem.UnitName;
            if (unitName == "")
                continue;

            int expEarned = 0;
            int minMinHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.HEALTH, _localPlayerTeam, unitName);
            if (minMinHealth > 0)
            {
                if (isVictory)
                    expEarned = 10;
                else
                    expEarned = 5;
            }

            gameInventory.AddExpToUnit(unitName, expEarned);
        }

        gameInventory.SaveUnits();

        if (isVictory)
        {
            rewardWithRandomBox();

            if (GameStats.Instance.Mode == GameStats.Modes.SinglePlayer)
                gameInventory.SetSinglePlayerLevel(gameInventory.GetSinglePlayerLevel() + 1);
        }

        if (GameStats.Instance.Mode == GameStats.Modes.Pvp)
        {
            if(GetIsHost())
                GameNetwork.Instance.SendMatchResultsToServer(winner, onSendMatchResultsToServerCallback);
        }
        else
            setAndDisplayMathResultsPopUp();
    }

    private void rewardWithRandomBox()
    {
        int rewardBoxTier = GameInventory.Instance.GetRandomTier();
        _matchLocalData.BoxTiersWithAmountsRewards[rewardBoxTier]++;
    }

    private void onSendMatchResultsToServerCallback(string message, int updatedRating)
    {
        sendMatchResultsServerResponse(message, updatedRating);
    }

    private void sendMatchResultsServerResponse(string message, int updatedRating)
    {
        base.SendRpcToAll("receiveMatchResultsServerResponse", message, updatedRating);
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
                rewardWithRandomBox();
        }
        else
        {
            _errorText.text = message;
            _errorText.gameObject.SetActive(true);
        }

        setAndDisplayMathResultsPopUp();
    }

    private void setAndDisplayMathResultsPopUp()
    {
        _matchResultsPopUp.SetValues(_matchLocalData);
        _matchResultsPopUp.Open();
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
                winnerTeam = GameNetwork.TeamNames.HOST;
            else if (hostTeamDefeated)
                winnerTeam = GameNetwork.TeamNames.GUEST;
        }

        return winnerTeam;
    }

    //Only master client uses this
    private string checkTimeWinner(int roundCount)
    {
        string winnerTeam  = "";

        if (roundCount >= _maxRoundsCount)
        {
            string hostTeam = GameNetwork.TeamNames.HOST;
            string guestTeam = GameNetwork.TeamNames.GUEST;

            int hostUnitsTotalHealth = getTeamTotalHealth(hostTeam);
            int guestUnitsTotalHealth = getTeamTotalHealth(guestTeam);

            //Team with higher total health wins
            if (hostUnitsTotalHealth > guestUnitsTotalHealth)
                winnerTeam = hostTeam;
            else if (guestUnitsTotalHealth > hostUnitsTotalHealth)
                winnerTeam = guestTeam;

            if (winnerTeam == "")
            {
                if (!GetUsesAi())
                {
                    int hostPlayerRating = GameNetwork.GetLocalPlayerRating(GameNetwork.TeamNames.HOST);
                    int guestPlayerRating = GameNetwork.GetAnyPlayerRating(GameNetwork.Instance.GuestPlayerId, GameNetwork.TeamNames.GUEST);

                    //Lowest rated player wins
                    if (hostPlayerRating < guestPlayerRating)
                        winnerTeam = hostTeam;
                    else if (guestPlayerRating < hostPlayerRating)
                        winnerTeam = guestTeam;
                }

                if (winnerTeam == "")
                    winnerTeam = hostTeam; // Master client wins
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
                total += GameNetwork.GetUnitRoomPropertyAsInt(unitRoomProperty, teamName, unitName);

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

        public MatchLocalData()
        {
            BoxTiersWithAmountsRewards.Add(GameInventory.Tiers.BRONZE, 0);
            BoxTiersWithAmountsRewards.Add(GameInventory.Tiers.SILVER, 0);
            BoxTiersWithAmountsRewards.Add(GameInventory.Tiers.GOLD, 0);
        }
    }
}
