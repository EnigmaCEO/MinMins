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

    [SerializeField] private int _maxRoundsCount = 3;
    [SerializeField] private int _fieldRewardChestsAmount = 1;

    [SerializeField] private Transform _teamGridContent;
    [SerializeField] private float _readyCheckDelay = 2;

    [SerializeField] private float _team1_CloudsLocalPosX = 0.07f;

    [SerializeField] private Text _errorText;
    [SerializeField] private MatchResultsPopUp _matchResultsPopUp;

    [SerializeField] private Text _teamTurnText;
    [SerializeField] private Text _unitTurnText;
    [SerializeField] private Text _roundNumberText;

    [SerializeField] private Image _hostTeamHealthFill;
    [SerializeField] private Image _guestTeamHealthFill;

    [SerializeField] private RectTransform UnitTurnHighlightTransform;

    [SerializeField] private Transform _battleField;
    [SerializeField] private GameCamera _gameCamera;

    [SerializeField] private GameObject _targetCirclePrefab;

    private Transform _hostGrid;
    private Transform _guestGrid;

    private int _side = 0;
    private bool _hasMatchEnded = false;
    private List<WarTeamGridItem> _uiTeamGridItems = new List<WarTeamGridItem>();

    private Dictionary<string, MinMinUnit> _hostUnits = new Dictionary<string, MinMinUnit>();
    private Dictionary<string, MinMinUnit> _guestUnits = new Dictionary<string, MinMinUnit>();

    private string _attackingUnitName = "";
    private float _timer;
    private float _playTime;

    private MatchData _matchData = new MatchData();

    private int _roundCount = 0;
    private string _localPlayerTeam = "";
    private bool _setupWasCalled = false;


    void Awake()
    {
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
    }

    private void Start()
    {
        _errorText.gameObject.SetActive(false);
        _matchResultsPopUp.gameObject.SetActive(false);
        _matchData.PlayerId = NetworkManager.GetPlayerName();

        _hostGrid = _battleField.Find(GridNames.TEAM_1);
        _guestGrid = _battleField.Find(GridNames.TEAM_2);

        if (GameStats.Instance.Mode == GameStats.Modes.SinglePlayer)
            GameNetwork.Instance.JoinOrCreateRoom();
        else
            setupWar();

        if (getIsHost())
            NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ROUND_NUMBER, 1);
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
        if (teamName == GameNetwork.VirtualPlayerIds.GUEST)
            gridName = GridNames.TEAM_2;

        return gridName;
    }

    static public War GetSceneInstance()
    {
        return GameObject.FindObjectOfType<War>();
    }

    private bool getIsHost()
    {
        return (_localPlayerTeam == GameNetwork.VirtualPlayerIds.HOST);
    }

    private int getPlayerInTurnId()
    {
        string teamInTurn = NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.TEAM_IN_TURN);
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
        if(!_setupWasCalled)
            setupWar();
    }

    private void onPlayerTeamUnitsSet(string teamName, string teamUnits)
    {
        print("War::onPlayerTeamUnitsSet -> teamName: " + teamName + " teamUnits: " + teamUnits);
        if (teamUnits == null)
            return;

        GameStats gameStats = GameStats.Instance;

        bool localPlayerIsHost = getIsHost();
        bool isTeamSetHost = teamName == GameNetwork.VirtualPlayerIds.HOST;

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
            if(isTeamSetHost)
                instantiateRewardChests(GridNames.TEAM_1);
        }
    }

    private void onUnitHealthSet(string team, string unitName, int health)
    {
        GameNetwork gameNetwork = GameNetwork.Instance;
        if (team == _localPlayerTeam)
        {
            foreach (WarTeamGridItem teamGridItem in _uiTeamGridItems)
            {
                if (teamGridItem.UnitName == unitName)
                {
                    int unitMaxHealth = gameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.MAX_HEALTH, team, unitName);
                    teamGridItem.SetLifeFill((float)health/(float)unitMaxHealth); 
                    break; 
                }
            }
        }
    }

    private void onTeamHealthSet(string team, int health)
    {
        GameNetwork gameNetwork = GameNetwork.Instance;
        int teamMaxHealth = gameNetwork.GetRoomTeamProperty(GameNetwork.TeamRoomProperties.MAX_HEALTH, team);

        Image fillToUpdate = (team == _localPlayerTeam) ? _hostTeamHealthFill : _guestTeamHealthFill;
        fillToUpdate.fillAmount = (float)health / (float)teamMaxHealth;
    }

    private void onRoundStarted(int roundNumber)
    {
        _roundNumberText.text = roundNumber.ToString();

        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.TEAM_IN_TURN, GameNetwork.VirtualPlayerIds.HOST);
    }

    private void onTeamTurnChanged(string teamName)
    {
        _teamTurnText.text = "Team turn: " + ((teamName == _localPlayerTeam) ? "Mine" : "Other");

        string roomProperty = GameNetwork.RoomCustomProperties.HOST_UNIT_INDEX;
        if (teamName == GameNetwork.VirtualPlayerIds.GUEST)
            roomProperty = GameNetwork.RoomCustomProperties.GUEST_UNIT_INDEX;

        int unitIndex = NetworkManager.GetRoomCustomPropertyAsInt(roomProperty) + 1;
        NetworkManager.SetRoomCustomProperty(roomProperty, unitIndex);
    }

    private void onHostUnitIndexChanged(int hostUnitIndex)
    {
        MinMinUnit unit = _hostUnits.Values.ElementAt(hostUnitIndex);
        handleUnitIndexChanged(hostUnitIndex, unit.name, GameNetwork.VirtualPlayerIds.HOST);
    }

    private void onGuestUnitIndexChanged(int guestUnitIndex)
    {
        MinMinUnit unit = _guestUnits.Values.ElementAt(guestUnitIndex);
        handleUnitIndexChanged(guestUnitIndex, unit.name, GameNetwork.VirtualPlayerIds.GUEST);
    }

    private void handleUnitIndexChanged(int unitIndex, string unitName, string teamName)
    {
        _unitTurnText.text = "Unit turn: " + unitName;
        handleHighlight(unitIndex, teamName);
        Dictionary<string, MinMinUnit> teamUnits = GetTeamUnitsDictionary(teamName);
        _gameCamera.HandleMovement(teamName, teamUnits[unitName].Type);
    }

    private void onCameraMovementCompleted(string sideTeam)
    {
        if (getIsHost())
        {
            string teamInTurn = NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.TEAM_IN_TURN);
            int unitIndex = -1;
            
            if (teamInTurn == GameNetwork.VirtualPlayerIds.HOST)
                unitIndex = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.HOST_UNIT_INDEX);
            else
                unitIndex = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.GUEST_UNIT_INDEX);

            Dictionary<string, MinMinUnit> teamUnits = GetTeamUnitsDictionary(teamInTurn);
            MinMinUnit unit = teamUnits.Values.ElementAt(unitIndex);
            int unitTier = GameInventory.Instance.GetUnitTier(unit.name);

            NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ACTIONS_LEFT, unitTier.ToString());
        }
    }

    private void onActionStarted(int actionsLeft)
    {
        string teamInTurn = NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.TEAM_IN_TURN);
        if (teamInTurn == _localPlayerTeam)
        {
            if (actionsLeft > 0)
                StartCoroutine(handlePlayerInput());
            else
            {
                //TODO: Increase round and check if match is over
            }
        }
    }

    private IEnumerator handlePlayerInput()
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 tapWorldPosition = _gameCamera.MyCamera.ScreenToWorldPoint(Input.mousePosition);
                sendPlayerTargetInput(tapWorldPosition);
                //TODO: Check click boundaries
                //TODO: Set actions left
                yield break;
            }

            yield return null;
        }
    }

    private void sendPlayerTargetInput(Vector3 playerInputWorldPosition)
    {
        base.SendRpcToMasterClient("receivePlayerTargetInput", playerInputWorldPosition, NetworkManager.GetLocalPlayerId(), _localPlayerTeam);
    }

    [PunRPC]
    private void receivePlayerTargetInput(Vector3 playerInputWorldPosition, int networkPlayerId, string virtualPlayerId)
    {     
        GameObject targetCircle = NetworkManager.Instantiate<GameObject>(_targetCirclePrefab);
        int playerInTurnActionsLeft = NetworkManager.GetRoomCustomPropertyAsInt(GameNetwork.RoomCustomProperties.ACTIONS_LEFT) - 1;
        NetworkManager.SetRoomCustomProperty(GameNetwork.RoomCustomProperties.ACTIONS_LEFT, playerInTurnActionsLeft.ToString());
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
        determineLocalPlayerTeam();

        if (_localPlayerTeam == GameNetwork.VirtualPlayerIds.GUEST)
        {
            //if(_localPlayerTeam == GameNetwork.VirtualPlayerIds.HOST)  //TODO: Remove test hack
            _gameCamera.SetCameraForGuest();
            moveCloudsToOppositeSide();
        }

        setLocalTeamUnits();

        _setupWasCalled = true;
    }

    private void determineLocalPlayerTeam()
    {
        if (NetworkManager.IsPhotonOffline()) //Single player
            _localPlayerTeam = GameNetwork.VirtualPlayerIds.HOST;
        else // Pvp
        {
            if (NetworkManager.GetIsMasterClient())
                _localPlayerTeam = GameNetwork.VirtualPlayerIds.HOST;
            else
                _localPlayerTeam = GameNetwork.VirtualPlayerIds.GUEST;
        }

        print("War::determinLocalPlayerTeam -> _localPlayerTeam: " + _localPlayerTeam);
    }

    private void setLocalTeamUnits()
    {
        GameStats gameStats = GameStats.Instance;
        GameInventory gameInventory = GameInventory.Instance;
        GameNetwork gameNetwork = GameNetwork.Instance;

        List<string> teamUnits = GameStats.Instance.TeamUnits;
        string teamUnitsString = "";

        int teamUnitsLenght = teamUnits.Count;
        for (int i = 0; i < teamUnitsLenght; i++)
        {
            if (i != 0)
                teamUnitsString += NetworkManager.Separators.VALUES;

            string unitName = teamUnits[i];
            teamUnitsString += unitName;

            int itemNumber = i + 1;
            Transform warGridItemTransform = _teamGridContent.Find("WarTeamGridItem" + itemNumber);

            if (unitName != "-1")
            {
                Vector3 pos = gameStats.PreparationPositions[i];
                string positionString = pos.x.ToString() + NetworkManager.Separators.VALUES + pos.y.ToString();
                gameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, unitName, positionString, _localPlayerTeam);

                gameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.EXPERIENCE, unitName, gameInventory.GetLocalUnitExp(unitName).ToString(), _localPlayerTeam);
 
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
            rewardBoxTransform.GetComponent<FieldRewardBox>().OnHit = onRewardBoxHit;

            if (gridName == GridNames.TEAM_1)
            {
                Vector3 localScale = rewardBoxTransform.localScale;
                rewardBoxTransform.localScale = new Vector3(-localScale.x, localScale.y, localScale.z);
            }
        }
    }

    private void onRewardBoxHit()
    {
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

        string hostUnitsString = NetworkManager.GetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.TEAM_UNITS, GameNetwork.VirtualPlayerIds.HOST);
        string[] hostUnits = hostUnitsString.Split(NetworkManager.Separators.VALUES);

        foreach (string unitName in hostUnits)
        {
            if (unitName == "-1")
                continue;

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

        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.TEAM_UNITS, unitsString, GameNetwork.VirtualPlayerIds.GUEST);
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
                int hostUnitExp = gameNetwork.GetLocalPlayerUnitPropertyAsInt(GameNetwork.UnitPlayerProperties.EXPERIENCE, hostUnitNames[i], GameNetwork.VirtualPlayerIds.HOST);
                gameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.EXPERIENCE, guestUnitNames[i], hostUnitExp.ToString(), GameNetwork.VirtualPlayerIds.GUEST);
                
                ////Set random position
                Vector2 pos = getRandomBattlefieldPosition();
                string posString = pos.x.ToString() + NetworkManager.Separators.VALUES + pos.y.ToString();
                gameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, guestUnitNames[i], posString, GameNetwork.VirtualPlayerIds.GUEST);
            }
        }

        return unitsString;
    }

    private Vector2 getRandomBattlefieldPosition()
    {
        GameConfig gameConfig = GameConfig.Instance;
        return new Vector2(Random.Range(gameConfig.BattleFieldMinPos.x, gameConfig.BattleFieldMaxPos.x) + _battleFieldRightSideOffset, Random.Range(gameConfig.BattleFieldMinPos.y, gameConfig.BattleFieldMaxPos.y));
    }

    private void instantiateTeam(string virtualPlayerId, string teamUnitsString)
    {
        bool isHost = getIsHost();
        string gridName = GetTeamGridName(virtualPlayerId);
        Transform grid = _battleField.Find(gridName);

        GameNetwork gameNetwork = GameNetwork.Instance;
        GameInventory gameInventory = GameInventory.Instance;
        GameStats gameStats = GameStats.Instance;

        string[] teamUnits = teamUnitsString.Split(NetworkManager.Separators.VALUES);
        int teamLength = teamUnits.Length;

        for (int i = 0; i < teamLength; i++)
        {
            string unitName = teamUnits[i];
            if (unitName == "-1")
                continue;

            GameObject unit = NetworkManager.InstantiateObject("Prefabs/MinMinUnits/" + unitName, Vector3.zero, Quaternion.identity, 0);
            unit.name = unitName;

            int playerId = NetworkManager.GetLocalPlayerId();
            if (!isHost)
                playerId = gameNetwork.GuestPlayerId;

            int unitExp = gameNetwork.GetAnyPlayerUnitPropertyAsInt(GameNetwork.UnitPlayerProperties.EXPERIENCE, unitName, virtualPlayerId, playerId);
            gameNetwork.BuildUnitLevels(unitName, unitExp, virtualPlayerId);

            string positionString = gameNetwork.GetAnyPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, unitName, virtualPlayerId, playerId);
            string[] positionCoords = positionString.Split(NetworkManager.Separators.VALUES);
            float posX = float.Parse(positionCoords[0]);
            float posY = float.Parse(positionCoords[1]);

            unit.GetComponent<MinMinUnit>().SendSettingsForWar(unitName, virtualPlayerId, i, posX, posY);
        }

        setTeamMaxHealth(virtualPlayerId);
        setTeamHealth(virtualPlayerId);
        setAttacks(grid);
    }

    public void Attack(MinMinUnit minMinUnit)
    {
        if (!Ready)
            return;
    }

    /*
    public void Attack(string attackerUnitName)
    {
        if (!Ready)
            return;

        Transform attackerUnitTransform = null;

        if (_side == 0)
            attackerUnitTransform = _alliesGrid.Find(attackerUnitName);
        else
            attackerUnitTransform = _enemiesGrid.Find(attackerUnitName);

        if (attackerUnitTransform == null)
            return;

        if (attackerUnitTransform.Find("Effect").transform.childCount == 0)
            return;

        Ready = false;
        float movement = (_side == 0) ? -_battlefieldMovementAmount : _battlefieldMovementAmount;
        iTween.MoveBy(_battleField.gameObject, iTween.Hash("x", movement, "easeType", _battlefieldMovementEaseType,
                                            "loopType", iTween.LoopType.none, "delay", _battlefieldMovementDelay,
                                            "time", _battlefieldMovementTime, "oncomplete", "attackReady"));

        _attackingUnitName = attackerUnitName;
    }
    */

    private void attackReady()
    {
        Debug.Log("Attack ready!");
        string effect_name = "";
        GameObject attack = GameObject.Find("Waypoint Manager/" + _attackingUnitName.Split('/')[1] + "/Attack").transform.GetChild(0).gameObject;

        if (_side == 0)
        {
            effect_name = _hostGrid.transform.Find(_attackingUnitName + "/Effect").transform.GetChild(0).name;
            attack.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            effect_name = _guestGrid.transform.Find(_attackingUnitName + "/Effect").transform.GetChild(0).name;
            attack.transform.localEulerAngles = new Vector3(0, 180, 0);
        }

        attack.transform.localPosition = new Vector2(0, 0);
        attack.GetComponent<SWS.BezierPathManager>().CalculatePath();

        GameObject effect = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/Effects/" + effect_name));
        effect.GetComponent<AttackEffect>().SetWar(this);
        effect.GetComponent<SWS.splineMove>().pathContainer = attack.GetComponent<SWS.BezierPathManager>();
        effect.GetComponent<SWS.splineMove>().enabled = true;
        effect.GetComponent<SWS.splineMove>().StartMove();
        effect.AddComponent<VFXSorter>().sortingOrder = 100;

        /* SWS.MessageOptions opt = effect.GetComponent<SWS.splineMove>().messages.GetMessageOption(2); - needs to be replaced
		opt.message [0] = "EndAttack";
		opt.pos = 1;*/
    }

    public void Switch()
    {
        Ready = true;
        if (_side == 0)
            _side = 1;
        else
            _side = 0;
    }

    public void OnMatchResultsExitButtonDown()
    {
        if(getIsHost())
        {
            GameNetwork gameNetwork = GameNetwork.Instance;
            gameNetwork.ClearTeamUnits(GameNetwork.VirtualPlayerIds.HOST);
            gameNetwork.ClearTeamUnits(GameNetwork.VirtualPlayerIds.GUEST);
        }

        SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
    }

    public void SetUnitHealth(string team, string unitName, int value, bool shouldUpdateTeamHealth)
    {
        GameNetwork.Instance.SetUnitHealth(team, unitName, value);

        if (shouldUpdateTeamHealth)
            setTeamHealth(team);
    }

    public Dictionary<string, MinMinUnit> GetTeamUnitsDictionary(string teamName)
    {
        Dictionary<string, MinMinUnit> teamUnits = null;
        if (teamName == GameNetwork.VirtualPlayerIds.HOST)
            teamUnits = _hostUnits;
        else //guest
            teamUnits = _guestUnits;

        return teamUnits;
    }

    private void setAttacks(Transform val)
    {
        for (int i = 0; i < val.childCount; i++)
        {

            GameObject temp = val.GetChild(i).gameObject;

            if (temp.transform.childCount > 0)
            {
                Transform attack = temp.transform.GetChild(0).Find("Attack");
                if (attack != null)
                {
                    GameObject item = new GameObject();
                    item.name = temp.transform.GetChild(0).name;
                    item.transform.parent = GameObject.Find("Waypoint Manager").transform;
                    attack.parent = item.transform;
                    attack.transform.localPosition = new Vector2(0, 0);
                }
            }
        }
    }

    private void handleMatchResults(bool isVictory)
    {
        GameInventory gameInventory = GameInventory.Instance;
        GameNetwork gameNetwork = GameNetwork.Instance;

        foreach (Transform slot in _teamGridContent)
        {
            GameObject minMin = slot.GetChild(0).gameObject;

            int expEarned = 0;
            int minMinHealth = gameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.HEALTH, GameNetwork.VirtualPlayerIds.HOST, minMin.name);
            if (minMinHealth > 0)
            {
                if (isVictory)
                    expEarned = 10;
                else
                    expEarned = 5;
            }

            gameInventory.AddExpToUnit(minMin.name, expEarned);
        }

        gameInventory.SaveUnits();

        if (isVictory)
        {
            rewardWithRandomBox();

            if (GameStats.Instance.Mode == GameStats.Modes.SinglePlayer)
                gameInventory.SetSinglePlayerLevel(gameInventory.GetSinglePlayerLevel() + 1);
        }

        if (GameStats.Instance.Mode == GameStats.Modes.Pvp)
            gameNetwork.SendMatchResults(_matchData, onMatchResultsCallback);
        else
            setAndDisplayMathResultsPopUp();
    }

    private void rewardWithRandomBox()
    {
        int rewardBoxTier = GameInventory.Instance.GetRandomTier();
        _matchData.BoxTiersWithAmountsRewards[rewardBoxTier]++;
    }

    private void onMatchResultsCallback(string message, int updatedRating)
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
    private void checkWinLoseConditions()
    {
        bool guestTeamDefeated = false;
        bool hostTeamDefeated = false;
        string winner = "";

        string hostTeam = GameNetwork.VirtualPlayerIds.HOST;
        string guestTeam = GameNetwork.VirtualPlayerIds.GUEST;

        if (_roundCount > _maxRoundsCount)
            _hasMatchEnded = true;
        else
        { 
            guestTeamDefeated = areAllUnitsDefeated(guestTeam);
            hostTeamDefeated = areAllUnitsDefeated(hostTeam);
            if (guestTeamDefeated  || guestTeamDefeated)
                _hasMatchEnded = true;
        }

        if (_hasMatchEnded)
        {
            if (guestTeamDefeated)
                winner = GameNetwork.VirtualPlayerIds.HOST;
            else if (hostTeamDefeated)
                winner = GameNetwork.VirtualPlayerIds.GUEST;

            if (winner == "")
            {
                int hostUnitsTotalHealth = getTeamTotalHealth(hostTeam);
                int guestUnitsTotalHealth = getTeamTotalHealth(guestTeam);

                //Team with higher total health wins
                if (hostUnitsTotalHealth > guestUnitsTotalHealth)
                    winner = hostTeam;
                else if (guestUnitsTotalHealth > hostUnitsTotalHealth)
                    winner = guestTeam;

                if (winner == "")
                {
                    int hostPlayerRating = GameNetwork.Instance.GetLocalPlayerRating(GameNetwork.VirtualPlayerIds.HOST);
                    int guestPlayerRating = GameNetwork.Instance.GetAnyPlayerRating(GameNetwork.Instance.GuestPlayerId, GameNetwork.VirtualPlayerIds.GUEST);

                    //Lowest rated player wins
                    if (hostPlayerRating < guestPlayerRating)
                        winner = hostTeam;
                    else if (guestPlayerRating < hostPlayerRating)
                        winner = guestTeam;

                    if (winner == "")
                        winner = hostTeam; // Master client wins
                }     
            }
        }
    }

    //Only master client uses this
    private int getTeamTotalHealth(string team)
    {
        return GameNetwork.Instance.GetRoomTeamProperty(GameNetwork.UnitRoomProperties.HEALTH, team);
    }

    private void setTeamHealth(string teamName)
    {
        GameNetwork gameNetwork = GameNetwork.Instance;
        Dictionary<string, MinMinUnit> teamUnitsForTotalCalculus = GetTeamUnitsDictionary(teamName);

        int healthTotal = 0;
        foreach (MinMinUnit unit in teamUnitsForTotalCalculus.Values)
            healthTotal += gameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.HEALTH, teamName, unit.name);

        gameNetwork.SetRoomTeamProperty(GameNetwork.TeamRoomProperties.HEALTH, teamName, healthTotal.ToString());
    }

    private void setTeamMaxHealth(string teamName)
    {
        Dictionary<string, MinMinUnit> teamUnitsForTotalCalculus = GetTeamUnitsDictionary(teamName);

        GameNetwork gameNetwork = GameNetwork.Instance;
        int maxHealthTotal = 0;
        foreach (MinMinUnit unit in teamUnitsForTotalCalculus.Values)
            maxHealthTotal += gameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.MAX_HEALTH, teamName, unit.name);

        gameNetwork.SetRoomTeamProperty(GameNetwork.TeamRoomProperties.MAX_HEALTH, teamName, maxHealthTotal.ToString());
    }

    //Only master client uses this
    private bool areAllUnitsDefeated(string team)
    {
        bool areAllUnitsDefeated = true;
        Dictionary<string, MinMinUnit> units = new Dictionary<string, MinMinUnit>();
        if (team == GameNetwork.VirtualPlayerIds.HOST)
            units = _hostUnits;
        else if (team == GameNetwork.VirtualPlayerIds.GUEST)
            units = _guestUnits;

        foreach (MinMinUnit unit in units.Values)
        {
            int unitHealth = getUnitHealth(team, unit.name);
            if (unitHealth > 0)
            {
                areAllUnitsDefeated = false;
                break;
            }
        }

        return areAllUnitsDefeated;
    }

    private int getUnitHealth(string team, string unitName)
    {
        return GameNetwork.Instance.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.HEALTH, team, unitName);
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
