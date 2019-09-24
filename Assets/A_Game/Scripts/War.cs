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
    [SerializeField] private int _fieldRewardChestsAmount = 1;

    [SerializeField] private Transform _teamGridContent;
    [SerializeField] private float _readyCheckDelay = 2;

    [SerializeField] private float _team1_CloudsLocalPosX = 0.07f;

    [SerializeField] private Text _errorText;
    [SerializeField] private MatchResultsPopUp _matchResultsPopUp;

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

    private Dictionary<string, Dictionary<string, List<HealerArea>>> _healerAreasByUnitByTeam;
    private Dictionary<string, Dictionary<string, Dictionary<string, List<TankArea>>>> _tanksAreasByOwnerByUnitByTeam;

    private string _attackingUnitName = "";
    private float _timer;
    private float _playTime;

    private MatchData _matchData = new MatchData();

    private string _localPlayerTeam = "";
    private bool _setupWasCalled = false;

    private bool _canCheckWinner = false;

    private Dictionary<string, bool> _readyByTeam = new Dictionary<string, bool>();

    public string LocalPlayerTeam { get { return _localPlayerTeam; } }

    override protected void Awake()
    {
        base.Awake();

        NetworkManager.OnJoinedRoomCallback += onJoinedRoom;

        GameNetwork.OnPlayerTeamUnitsSetCallback += onPlayerTeamUnitsSet;
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
        _errorText.gameObject.SetActive(false);

        _matchResultsPopUp.DismissButton.onClick.AddListener(() => onMatchResultsDismissButtonDown());
        _matchResultsPopUp.gameObject.SetActive(false);

        UnitTurnHighlightTransform.gameObject.SetActive(false);

        _matchData.PlayerId = NetworkManager.GetPlayerName();

        _hostGrid = _battleField.Find(GridNames.TEAM_1);
        _guestGrid = _battleField.Find(GridNames.TEAM_2);

        determineLocalPlayerTeam();

        if (getIsHost())
        {
            _healerAreasByUnitByTeam = new Dictionary<string, Dictionary<string, List<HealerArea>>>();
            _healerAreasByUnitByTeam.Add(GameNetwork.TeamNames.HOST, new Dictionary<string, List<HealerArea>>());
            _healerAreasByUnitByTeam.Add(GameNetwork.TeamNames.GUEST, new Dictionary<string, List<HealerArea>>());

            _tanksAreasByOwnerByUnitByTeam = new Dictionary<string, Dictionary<string, Dictionary<string, List<TankArea>>>>();
            _tanksAreasByOwnerByUnitByTeam.Add(GameNetwork.TeamNames.HOST, new Dictionary<string, Dictionary<string, List<TankArea>>>());
            _tanksAreasByOwnerByUnitByTeam.Add(GameNetwork.TeamNames.GUEST, new Dictionary<string, Dictionary<string, List<TankArea>>>());
        }

        if (GameStats.Instance.Mode == GameStats.Modes.SinglePlayer)
        {
            GameNetwork.Instance.HostPlayerId = NetworkManager.GetLocalPlayerId();
            GameNetwork.Instance.GuestPlayerId = NetworkManager.GetLocalPlayerId();  
            GameNetwork.Instance.JoinOrCreateRoom();
        }
        else
            setupWar();
    }

    // Update is called once per frame
    void Update()
    {
        _playTime += Time.deltaTime;

        if (_side == 0)
            _timer = _playTime;

        if (((_playTime - _timer) >= _readyCheckDelay) && Ready)
        {
            int guestGridCount = _guestGrid.childCount;
            for (int i = 0; i < guestGridCount; i++)
            {
                Transform guestUnitSlot = _guestGrid.GetChild(i);
                if (guestUnitSlot.childCount == 0)
                    continue;

                //Attack(guestUnitSlot.name + "/" + guestUnitSlot.GetChild(0).name);
                break;
            }
        }
    }

    private void OnDestroy()
    {
        NetworkManager.OnJoinedRoomCallback -= onJoinedRoom;

        GameNetwork.OnPlayerTeamUnitsSetCallback -= onPlayerTeamUnitsSet;
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

    private bool getIsHost()
    {
        return (_localPlayerTeam == GameNetwork.TeamNames.HOST);
    }

    private int getPlayerInTurnId()
    {
        string teamInTurn = GameNetwork.GetTeamInTurn();
        int playerId = -1;

        if (!getIsHost())
            playerId = NetworkManager.GetLocalPlayerId();
        else
            playerId = GameNetwork.Instance.GuestPlayerId;

        return playerId;
    }

    private void onJoinedRoom()
    {
        print("War::OnJoinedRoom -> Is Master Client: " + NetworkManager.GetIsMasterClient());
        if (!_setupWasCalled)
            setupWar();
    }

    private void onPlayerTeamUnitsSet(string teamName, string unitsString)
    {
        if (unitsString == null)
            return;

        string[] teamUnits = unitsString.Split(NetworkManager.Separators.VALUES);

        GameStats gameStats = GameStats.Instance;

        bool localPlayerIsHost = getIsHost();
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

    private void onUnitHealthSet(string team, string unitName, int health)
    {
        if (team == _localPlayerTeam)
        {
            foreach (WarTeamGridItem teamGridItem in _uiTeamGridItems)
            {
                if (teamGridItem.UnitName == unitName)
                {
                    int unitMaxHealth = GameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.MAX_HEALTH, team, unitName);
                    teamGridItem.SetLifeFill((float)health / (float)unitMaxHealth);
                    break;
                }
            }
        }
    }

    private void onTeamHealthSet(string team, int health)
    {
        int teamMaxHealth = GameNetwork.GetRoomTeamProperty(GameNetwork.TeamRoomProperties.MAX_HEALTH, team);

        Image fillToUpdate = (team == _localPlayerTeam) ? _hostTeamHealthFill : _guestTeamHealthFill;
        fillToUpdate.fillAmount = (float)health / (float)teamMaxHealth;

        if (getIsHost())
        {
            if (_canCheckWinner)
            {
                string winner = checkSurvivorWinner();
                if (winner != "")
                    sendMatchResults(winner);
            }
            else
                _canCheckWinner = true;
        }
    }

    private void onRoundStarted(int roundNumber)
    {
        _roundNumberText.text = "Round: " + roundNumber.ToString();

        if (getIsHost())
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

        if (getIsHost())
        {
            string roomProperty = GameNetwork.RoomCustomProperties.HOST_UNIT_INDEX;
            if (teamName == GameNetwork.TeamNames.GUEST)
                roomProperty = GameNetwork.RoomCustomProperties.GUEST_UNIT_INDEX;

            int unitIndex = NetworkManager.GetRoomCustomPropertyAsInt(roomProperty) + 1;
            NetworkManager.SetRoomCustomProperty(roomProperty, unitIndex);
        }
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

        if ((teamName == GameNetwork.TeamNames.HOST) && (unitIndex >= units.Count))
        {
            if (getIsHost())
            {
                processUnitsHealing();

                int roundCount = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.ROUND_COUNT);
                string winner = checkTimeWinner(roundCount);
                if (winner == "")
                    NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ROUND_COUNT, roundCount + 1);
                else
                    sendMatchResults(winner);
            }
        }
        else
        {
            MinMinUnit unit = units.Values.ElementAt(unitIndex);
            string unitName = unit.name;

            if (getIsHost() && (unit.Type == MinMinUnit.Types.Tank))
                checkSpecialDefenseEnded(teamName, unitName);

            _unitTurnText.text = "Unit turn: " + unitName + " | Index: " + unitIndex;
            handleHighlight(unitIndex, teamName);
            _gameCamera.HandleMovement(teamName, units[unitName].Type);
        }
    }

    private void onCameraMovementCompleted(string sideTeam)
    {
        MinMinUnit unit = getUnitInTurn();
        int unitTier = GameInventory.Instance.GetUnitTier(unit.name);

        //unitTier = 1; //Unit tier hack

        _unitTierTurnText.text = "Unit tier: " + unitTier.ToString();
        _unitTypeTurnText.text = "Unit type: " + unit.Type.ToString();

        if (getIsHost())
            NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ACTIONS_LEFT, unitTier.ToString());
    }

    private void onActionStarted(int actionsLeft)
    {
        _actionsLeftText.text = "Actions Left: " + actionsLeft;
        string teamInTurn = GameNetwork.GetTeamInTurn();

        Debug.LogWarning("War::onActionStarted: " + actionsLeft + " teamInTurn: " + teamInTurn);

        if (actionsLeft > 0)
        {
            if (teamInTurn == _localPlayerTeam)
                StartCoroutine(handlePlayerInput());
        }
        else if (getIsHost())
            GameNetwork.SetTeamInTurn(GameNetwork.GetOppositeTeamName(teamInTurn));
    }

    private IEnumerator handlePlayerInput()
    {
        Debug.LogWarning("handlePlayerInput");
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 tapWorldPosition = _gameCamera.MyCamera.ScreenToWorldPoint(Input.mousePosition);
                GameConfig gameConfig = GameConfig.Instance;

                if ((tapWorldPosition.y > gameConfig.BattleFieldMinPos.y) || (tapWorldPosition.y < gameConfig.BattleFieldMaxPos.y)
                || (tapWorldPosition.x < gameConfig.BattleFieldMaxPos.x) || (tapWorldPosition.x > gameConfig.BattleFieldMinPos.x))
                {
                    sendPlayerTargetInput(tapWorldPosition);
                    yield break;
                }
            }
            
            yield return null;
        }
    }

    private void sendPlayerTargetInput(Vector3 playerInputWorldPosition)
    {
        Debug.LogWarning("sendPlayerTargetInput");
        base.SendRpcToMasterClient("receivePlayerTargetInput", playerInputWorldPosition, _localPlayerTeam, NetworkManager.GetLocalPlayerId());
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

        int playerInTurnActionsLeft = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.ACTIONS_LEFT) - 1;
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ACTIONS_LEFT, playerInTurnActionsLeft.ToString());
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
        Debug.LogWarning("onRewardBoxHit");
        rewardWithRandomBox();
    }

    private void setUpSinglePlayerAiTeamUnits()
    {
        print("War::setUpSinglePlayerAiTeamUnits");
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
        string gridName = GetTeamGridName(teamName);
        Transform grid = _battleField.Find(gridName);

        GameNetwork gameNetwork = GameNetwork.Instance;
        GameInventory gameInventory = GameInventory.Instance;
        GameStats gameStats = GameStats.Instance;

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

            object[] instantiationData = { unitName, teamName, i, posX, posY };

            GameObject unitGameObject = NetworkManager.InstantiateObject("Prefabs/MinMinUnits/" + unitName, Vector3.zero, Quaternion.identity, 0, instantiationData);

            MinMinUnit unit = unitGameObject.GetComponent<MinMinUnit>();

            MinMinUnit.Types unitDebugType = MinMinUnit.Types.Bomber;
            //Tanks against Bombers Test hack =======================================
            //if (teamName == GameNetwork.TeamNames.HOST)
            //    unitDebugType = MinMinUnit.Types.Tank;
            //else
            //    unitDebugType = MinMinUnit.Types.Scout;
            //==============================================================

            //Specific Type Test hack =======================================
            //unitDebugType = MinMinUnit.Types.Bomber;
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

    public void SetUnitForHealing(string team, string unitName, HealerArea healerArea)
    {
        if (!_healerAreasByUnitByTeam[team].ContainsKey(unitName))
            _healerAreasByUnitByTeam[team].Add(unitName, new List<HealerArea>());

        _healerAreasByUnitByTeam[team][unitName].Add(healerArea);
    }

    public void SetUnitSpecialDefense(string targetUnitName, TankArea tankArea)
    {
        string targetTeamName = tankArea.OwnerTeamName;
        string ownerUnitName = tankArea.OwnerUnitName;

        if (!_tanksAreasByOwnerByUnitByTeam[targetTeamName].ContainsKey(targetUnitName))
            _tanksAreasByOwnerByUnitByTeam[targetTeamName].Add(targetUnitName, new Dictionary<string, List<TankArea>>());

        if (!_tanksAreasByOwnerByUnitByTeam[targetTeamName][targetUnitName].ContainsKey(ownerUnitName))
            _tanksAreasByOwnerByUnitByTeam[targetTeamName][targetUnitName].Add(ownerUnitName, new List<TankArea>());

        _tanksAreasByOwnerByUnitByTeam[targetTeamName][targetUnitName][ownerUnitName].Add(tankArea);

        Debug.LogWarning("War::SetUnitSpecialDefense -> team: " + targetTeamName + " targetUnitName: " + targetUnitName + " defense: " + tankArea.Defense + " ownerUnitName: " + ownerUnitName);
    }

    public int GetUnitSpecialDefense(string team, string unitName)
    {
        //Debug.LogWarning("War::GetUnitSpecialDefense -> team: " + team + " unitName: " + unitName);
        int unitSpecialDefense = 0;

        if (_tanksAreasByOwnerByUnitByTeam[team].ContainsKey(unitName))
        {
            foreach (string ownerUnitName in _tanksAreasByOwnerByUnitByTeam[team][unitName].Keys)
            {
                foreach (TankArea tankArea in _tanksAreasByOwnerByUnitByTeam[team][unitName][ownerUnitName])
                {
                    int defenseInput = tankArea.Defense;
                    Debug.LogWarning("War::GetUnitSpecialDefense -> ownerUnitName: " + ownerUnitName + " tankArea: " + tankArea + " defenseInput: " + defenseInput);
                    unitSpecialDefense += defenseInput;
                }
            }
        }

        //Debug.LogWarning("War::GetUnitSpecialDefense -> unitSpecialDefense: " + unitSpecialDefense);

        return unitSpecialDefense;
    }

    private void checkSpecialDefenseEnded(string teamName, string ownerUnitName)
    {
        List<ActionArea> tankAreasToDestroy = new List<ActionArea>();

        Debug.LogWarning("War::checkSpecialDefenseEnded -> teamName: " + teamName + " ownerUnitName: " + ownerUnitName);
        foreach (string unitName in _tanksAreasByOwnerByUnitByTeam[teamName].Keys)
        {
            Debug.LogWarning("War::checkSpecialDefenseEnded -> unitName: " + unitName);
            if (_tanksAreasByOwnerByUnitByTeam[teamName][unitName].ContainsKey(ownerUnitName))
            {
                Debug.LogWarning("War::checkSpecialDefenseEnded -> ownerUnitName: " + ownerUnitName);
                foreach (TankArea tankArea in _tanksAreasByOwnerByUnitByTeam[teamName][unitName][ownerUnitName])
                {
                    Debug.LogWarning("War::checkSpecialDefenseEnded -> Tank Area to destroy: " + tankArea);

                    if (!tankAreasToDestroy.Contains(tankArea))
                    {
                        Debug.LogWarning("War::checkSpecialDefenseEnded -> Tank Area to list: " + tankArea);
                        tankAreasToDestroy.Add(tankArea);
                    }
                }

                _tanksAreasByOwnerByUnitByTeam[teamName][unitName].Remove(ownerUnitName);
            }
        }

        ActionArea.DestroyActionAreaList(tankAreasToDestroy);
    }

    public void RemoveHealerArea(string team, HealerArea healingArea)
    {
        foreach (string unitName in _healerAreasByUnitByTeam[team].Keys)
        {
            if (_healerAreasByUnitByTeam[team][unitName].Contains(healingArea))
                _healerAreasByUnitByTeam[team][unitName].Remove(healingArea);
        }

        NetworkManager.NetworkDestroy(healingArea.gameObject);
    }

    public void RemoveTankArea(string team, TankArea tankArea)
    {
        string tankAreaOwner = tankArea.OwnerUnitName;

        foreach (string unitName in _tanksAreasByOwnerByUnitByTeam[team].Keys)
        {
            if (_tanksAreasByOwnerByUnitByTeam[team][unitName].ContainsKey(tankAreaOwner))
            {
                if (_tanksAreasByOwnerByUnitByTeam[team][unitName][tankAreaOwner].Contains(tankArea))
                    _tanksAreasByOwnerByUnitByTeam[team][unitName][tankAreaOwner].Remove(tankArea);
            }
        }

        NetworkManager.NetworkDestroy(tankArea.gameObject);
    }

    /*
    public void HandleUnitScouted(string team, string unitName, int strenght)
    {
        if (_healingByUnitByTeam[team].ContainsKey(unitName))
        {
            int healing = _healingByUnitByTeam[team][unitName];
            if (strenght > healing)
            {
                _healingByUnitByTeam[team].Remove(unitName);  //Remove healing if scouting is stronger
                Debug.LogWarning("War::HandleUnitScouted -> unitName: " + unitName + " at team: " + team + " with healing " + healing + " was removed by scouting with strenght " + strenght);
            }
            else
                Debug.LogWarning("War::HandleUnitScouted -> unitName: " + unitName + " at team: " + team + " with healing " + healing + " endured scouting with strenght " + strenght);
        }

        if (_defenseByAreaByOwnerByUnitByTeam[team].ContainsKey(unitName))
        {
            Dictionary<string, int> defenseOwnerToRemove = new Dictionary<string, int>();
            foreach (string ownerUnitName in _defenseByAreaByOwnerByUnitByTeam[team][unitName].Keys)
            {
                defenseOwnerToRemove.Clear();
                int defense = _defenseByAreaByOwnerByUnitByTeam[team][unitName][ownerUnitName];
                if (strenght > defense)
                {
                    _defenseByAreaByOwnerByUnitByTeam[team][unitName].Remove(ownerUnitName);
                    defenseOwnerToRemove.Add(ownerUnitName, defense);                  
                }
                else
                    Debug.LogWarning("War::HandleUnitScouted -> unitName: " + unitName + " at team: " + team + " with defense " + defense + " by owner: " + ownerUnitName + " endured scouting with strenght " + strenght);
            }

            foreach (KeyValuePair<string, int> defenseByOwner in defenseOwnerToRemove)
            {
                string ownerUnitName = defenseByOwner.Key;
                int defense = defenseByOwner.Value;
                _defenseByAreaByOwnerByUnitByTeam[team][unitName].Remove(defenseByOwner.Key);
                Debug.LogWarning("War::HandleUnitScouted -> unitName: " + unitName + " at team: " + team + " with defense " + defense + " by owner: " + ownerUnitName + " was removed by scouting with strenght " + strenght);
            }
        }
    }
    */

    public void SetUnitHealth(string team, string unitName, int value, bool shouldUpdateTeamHealth)
    {
        GameNetwork.SetUnitHealth(team, unitName, value);

        if (shouldUpdateTeamHealth)
            setTeamHealth(team, false);
    }

    public void RegisterUnit(string teamName, MinMinUnit unit)
    {
        Dictionary<string, MinMinUnit> teamUnits = GetTeamUnitsDictionary(teamName);
        teamUnits.Add(unit.name, unit);

        if (getIsHost())
        {
            int teamPlayersAmount = GameNetwork.GetTeamUnitNames(teamName).Length;
            Debug.LogWarning("War::RegisterUnit -> teamName: " + teamName + " unit: " + unit.name + " units on list: " + teamPlayersAmount + " units registered: " + teamUnits.Count);
            if (teamUnits.Count == teamPlayersAmount)
            {
                _readyByTeam[teamName] = true;

                if (_readyByTeam[GameNetwork.TeamNames.HOST] && _readyByTeam[GameNetwork.TeamNames.GUEST])
                {
                    ////Starts combat cycle
                    NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ROUND_COUNT, 1);
                }
            }
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

        foreach (string team in _healerAreasByUnitByTeam.Keys)
        {
            foreach (string unitName in _healerAreasByUnitByTeam[team].Keys)
            {
                int strongerHealing = 0;
                foreach (HealerArea healerArea in _healerAreasByUnitByTeam[team][unitName])
                {
                    int healing = healerArea.Healing;
                    if (healing > strongerHealing)
                        strongerHealing = healing;

                    if (!healerAreasToDestroy.Contains(healerArea))
                        healerAreasToDestroy.Add(healerArea);
                }

                int unitHealth = GameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.HEALTH, team, unitName);
                int unitMaxHealth = GameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.MAX_HEALTH, team, unitName);

                unitHealth += strongerHealing;  //TODO: Check if healing formula is needed;
                if (unitHealth > unitMaxHealth)
                    unitHealth = unitMaxHealth;

                //TODO: Add healing SFX

                SetUnitHealth(team, unitName, unitHealth, true);
            }

            _healerAreasByUnitByTeam[team].Clear();
        }

        ActionArea.DestroyActionAreaList(healerAreasToDestroy);
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
            int minMinHealth = GameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.HEALTH, _localPlayerTeam, unitName);
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

        //if (GameStats.Instance.Mode == GameStats.Modes.Pvp)
        //    GameNetwork.Instance.SendMatchResultsToServer(_matchData, onSendMatchResultsToServerCallback);
        //else
            setAndDisplayMathResultsPopUp();
    }

    private void rewardWithRandomBox()
    {
        int rewardBoxTier = GameInventory.Instance.GetRandomTier();
        _matchData.BoxTiersWithAmountsRewards[rewardBoxTier]++;
    }

    private void onSendMatchResultsToServerCallback(string message, int updatedRating)
    {
        if (message == GameNetwork.ServerResponseMessages.SUCCESS)
        {
            _errorText.text = "";
            _errorText.gameObject.SetActive(false);

            int oldArenaLevel = GameNetwork.Instance.GetLocalPlayerPvpLevelNumber();
            int updatedLevel = GameNetwork.Instance.GetPvpLevelNumberByRating(updatedRating);

            if (updatedLevel > oldArenaLevel)
                rewardWithRandomBox();

            setAndDisplayMathResultsPopUp();
        }
        else
        {
            _errorText.text = message;
            _errorText.gameObject.SetActive(true);
        }
    }

    private void setAndDisplayMathResultsPopUp()
    {
        _matchResultsPopUp.SetValues(_matchData);
        _matchResultsPopUp.Open();
    }

    //Only master client uses this
    private string checkSurvivorWinner()
    {
        string winner = "";

        bool guestTeamDefeated = isTeamDefeated(GameNetwork.TeamNames.GUEST);
        bool hostTeamDefeated = isTeamDefeated(GameNetwork.TeamNames.HOST);

        if (guestTeamDefeated || hostTeamDefeated)
        {
            if (guestTeamDefeated)
                winner = GameNetwork.TeamNames.HOST;
            else if (hostTeamDefeated)
                winner = GameNetwork.TeamNames.GUEST;
        }

        return winner;
    }

    //Only master client uses this
    private string checkTimeWinner(int roundCount)
    {
        string winner = "";

        if (roundCount >= _maxRoundsCount)
        {
            string hostTeam = GameNetwork.TeamNames.HOST;
            string guestTeam = GameNetwork.TeamNames.GUEST;

            int hostUnitsTotalHealth = getTeamTotalHealth(hostTeam);
            int guestUnitsTotalHealth = getTeamTotalHealth(guestTeam);

            //Team with higher total health wins
            if (hostUnitsTotalHealth > guestUnitsTotalHealth)
                winner = hostTeam;
            else if (guestUnitsTotalHealth > hostUnitsTotalHealth)
                winner = guestTeam;

            if (winner == "")
            {
                int hostPlayerRating = GameNetwork.GetLocalPlayerRating(GameNetwork.TeamNames.HOST);
                int guestPlayerRating = GameNetwork.GetAnyPlayerRating(GameNetwork.Instance.GuestPlayerId, GameNetwork.TeamNames.GUEST);

                //Lowest rated player wins
                if (hostPlayerRating < guestPlayerRating)
                    winner = hostTeam;
                else if (guestPlayerRating < hostPlayerRating)
                    winner = guestTeam;

                if (winner == "")
                    winner = hostTeam; // Master client wins
            }
        }

        return winner;
    }

    //Only master client uses this
    private int getTeamTotalHealth(string team)
    {
        return GameNetwork.GetRoomTeamProperty(GameNetwork.TeamRoomProperties.HEALTH, team);
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
                total += GameNetwork.GetRoomUnitProperty(unitRoomProperty, teamName, unitName);

        GameNetwork.SetRoomTeamProperty(teamRoomProperty, teamName, total.ToString());
    }

    //Only master client uses this
    private bool isTeamDefeated(string team)
    {
        return (GameNetwork.GetRoomTeamProperty(GameNetwork.TeamRoomProperties.HEALTH, team) <= 0);
    }

    private int getUnitHealth(string team, string unitName)
    {
        return GameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.HEALTH, team, unitName);
    }

    public class MatchData
    {
        public string PlayerId = "";
        public int DamageDealt = 0;
        public int DamageReceived = 0;
        public int UnitsKilled = 0;
        public int MatchDuration = 0;

        public Dictionary<string, bool> UnitsAlive = new Dictionary<string, bool>();
        public Dictionary<int, int> BoxTiersWithAmountsRewards = new Dictionary<int, int>();

        public MatchData()
        {
            BoxTiersWithAmountsRewards.Add(GameInventory.Tiers.BRONZE, 0);
            BoxTiersWithAmountsRewards.Add(GameInventory.Tiers.SILVER, 0);
            BoxTiersWithAmountsRewards.Add(GameInventory.Tiers.GOLD, 0);
        }
    }
}
