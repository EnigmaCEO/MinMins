using Enigma.CoreSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class War : MonoBehaviour
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

    [SerializeField] private float _battlefieldMovementAmount = 5000;
    [SerializeField] private float _battlefieldMovementDelay = 0.1f;
    [SerializeField] private float _battlefieldMovementTime = 1;
    [SerializeField] private iTween.EaseType _battlefieldMovementEaseType = iTween.EaseType.easeInOutExpo;

    [SerializeField] private Text _errorText;
    [SerializeField] private MatchResultsPopUp _matchResultsPopUp;

    [SerializeField] private Text _teamTurnText;
    [SerializeField] private Text _unitTurnText;

    [SerializeField] private Image _alliesHealthFill;
    [SerializeField] private Image _enemiesHealthFill;

    [SerializeField] private RectTransform UnitTurnHighlightTransform;

    [SerializeField] private Transform _battleField;
    [SerializeField] private GameCamera _gameCamera;

    private Transform _alliesGrid;
    private Transform _enemiesGrid;

    private int _side = 0;
    private bool _hasMatchEnded = false;
    private List<WarTeamGridItem> _uiTeamGridItems = new List<WarTeamGridItem>();

    private List<MinMinUnit> _allies = new List<MinMinUnit>();
    private List<MinMinUnit> _enemies = new List<MinMinUnit>();

    private string _attackingUnitName = "";
    private float _timer;
    private float _playTime;

    private MatchData _matchData = new MatchData();

    private int _roundCount = 0;
    private string _localPlayerTeam = "";
    private bool _isLocalPlayerTurn = false;
    private bool _setupWasCalled = false;

    void Awake()
    {
        NetworkManager.OnJoinedRoomCallback += onJoinedRoom;

        GameNetwork.OnPlayerTeamUnitsSetCallback += onPlayerTeamUnitsSet;
        GameNetwork.OnUnitHealthSetCallback += onUnitHealthSet;
        GameNetwork.OnTeamHealthSetCallback += onTeamHealthSet;
        GameNetwork.OnRoundStartedCallback += onRoundStarted;
        GameNetwork.OnUnitTurnStartedCallback += onUnitTurnStarted;
    }

    private void Start()
    {
        _errorText.gameObject.SetActive(false);
        _matchResultsPopUp.gameObject.SetActive(false);
        _matchData.PlayerId = NetworkManager.GetPlayerName();

        _alliesGrid = _battleField.Find(GridNames.TEAM_1);
        _enemiesGrid = _battleField.Find(GridNames.TEAM_2);

        if (GameStats.Instance.Mode == GameStats.Modes.SinglePlayer)
            GameNetwork.Instance.JoinOrCreateRoom();
        else
            setupWar();

        if (_localPlayerTeam == GameNetwork.VirtualPlayerIds.ALLIES)
        {
            GameNetwork.Instance.SetRoundNumber(1);
            GameNetwork.Instance.SetUnitInTurn(_allies[0].name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _playTime += Time.deltaTime;

        if (_side == 0)
            _timer = _playTime;

        if (((_playTime - _timer) >= _readyCheckDelay) && Ready)
        {
            int enemiesGridCount = _enemiesGrid.childCount;
            for (int i = 0; i < enemiesGridCount; i++)
            {
                Transform enemySlot = _enemiesGrid.GetChild(i);
                if (enemySlot.childCount == 0)
                    continue;

                //Attack(enemySlot.name + "/" + enemySlot.GetChild(0).name);
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
        GameNetwork.OnUnitTurnStartedCallback -= onUnitTurnStarted;
    }

    static public string GetTeamGridName(string teamName)
    {
        string gridName = GridNames.TEAM_1;
        if (teamName == GameNetwork.VirtualPlayerIds.ENEMIES)
            gridName = GridNames.TEAM_2;

        return gridName;
    }

    static public War GetSceneInstance()
    {
        return GameObject.Find("War").GetComponent<War>();
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

        bool localPlayerIsAllies = (_localPlayerTeam == GameNetwork.VirtualPlayerIds.ALLIES);
        bool isTeamSetAllies = teamName == GameNetwork.VirtualPlayerIds.ALLIES;

        print("War::onPlayerTeamUnitsSet -> localPlayerIsAllies: " + localPlayerIsAllies);

        if (localPlayerIsAllies)
        {
            instantiateTeam(teamName, teamUnits);

            if (isTeamSetAllies)
            {
                if (gameStats.Mode == GameStats.Modes.SinglePlayer)
                    setUpSinglePlayerAiTeamUnits();
                else //Pvp
                {
                    if (gameStats.UsesAiForPvp)
                        setUpPvpAiTeamUnits();
                }
            }
            else // Team set is enemy
                instantiateRewardChests(GridNames.TEAM_2);
        }
        else // Local player is Enemy
        {
            if(isTeamSetAllies)
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

        Image fillToUpdate = (team == _localPlayerTeam) ? _alliesHealthFill : _enemiesHealthFill;
        fillToUpdate.fillAmount = (float)health / (float)teamMaxHealth;
    }

    private void onRoundStarted(string team)
    {
        _isLocalPlayerTurn = (team == _localPlayerTeam);
        _teamTurnText.text = "Team turn: " + (_isLocalPlayerTurn ? "Mine" : "Other");
    }

    private void onUnitTurnStarted(string unitName)
    {
        _unitTurnText.text = "Unit turn: " + unitName;

        if (_isLocalPlayerTurn)
        {
            foreach (WarTeamGridItem teamGridItem in _uiTeamGridItems)
            {
                if (teamGridItem.UnitName == unitName)
                {
                    UnitTurnHighlightTransform.SetParent(teamGridItem.transform);
                    UnitTurnHighlightTransform.SetAsFirstSibling();
                    UnitTurnHighlightTransform.localPosition = Vector3.zero;
                    UnitTurnHighlightTransform.gameObject.SetActive(true);
                    break;
                }
            }
        }
        else
            UnitTurnHighlightTransform.gameObject.SetActive(false);
    }

    private void setupWar()
    {
        determineLocalPlayerTeam();

        if (_localPlayerTeam == GameNetwork.VirtualPlayerIds.ENEMIES)
        {
            //if(_localPlayerTeam == GameNetwork.VirtualPlayerIds.ALLIES)  //TODO: Remove test hack
            _gameCamera.SetCameraForEnemies();
            moveCloudsToOppositeSide();
        }

        setLocalTeamUnits();

        _setupWasCalled = true;
    }

    private void determineLocalPlayerTeam()
    {
        if (NetworkManager.IsPhotonOffline()) //Single player
            _localPlayerTeam = GameNetwork.VirtualPlayerIds.ALLIES;
        else // Pvp
        {
            if (NetworkManager.GetIsMasterClient())
                _localPlayerTeam = GameNetwork.VirtualPlayerIds.ALLIES;
            else
                _localPlayerTeam = GameNetwork.VirtualPlayerIds.ENEMIES;
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

        List<string> allyBronzeUnits = new List<string>();
        List<string> allySilverUnits = new List<string>();
        List<string> allyGoldUnits = new List<string>();

        string alliesUnitsString = NetworkManager.GetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.TEAM_UNITS, GameNetwork.VirtualPlayerIds.ALLIES);
        string[] alliesUnits = alliesUnitsString.Split(NetworkManager.Separators.VALUES);

        foreach (string unitName in alliesUnits)
        {
            if (unitName == "-1")
                continue;

            int unitTier = gameInventory.GetUnitTier(unitName);

            if (unitTier == GameInventory.Tiers.BRONZE)
                allyBronzeUnits.Add(unitName);
            else if (unitTier == GameInventory.Tiers.SILVER)
                allySilverUnits.Add(unitName);
            else if (unitTier == GameInventory.Tiers.GOLD)
                allyGoldUnits.Add(unitName);
        }

        List<string> enemyBronzeUnits = gameInventory.GetRandomUnitsFromTier(allyBronzeUnits.Count, GameInventory.Tiers.BRONZE);
        List<string> enemySilverUnits = gameInventory.GetRandomUnitsFromTier(allySilverUnits.Count, GameInventory.Tiers.SILVER);
        List<string> enemyGoldUnits = gameInventory.GetRandomUnitsFromTier(allyGoldUnits.Count, GameInventory.Tiers.GOLD);

        string unitsString = "";
        unitsString = setEnemyAiUnits(allyBronzeUnits, enemyBronzeUnits, unitsString);
        unitsString = setEnemyAiUnits(allySilverUnits, enemySilverUnits, unitsString);
        unitsString = setEnemyAiUnits(allyGoldUnits, enemyGoldUnits, unitsString);

        NetworkManager.SetLocalPlayerCustomProperty(GameNetwork.PlayerCustomProperties.TEAM_UNITS, unitsString, GameNetwork.VirtualPlayerIds.ENEMIES);
    }

    private string setEnemyAiUnits(List<string> allyUnitNames, List<string> enemyUnitNames, string unitsString)
    {
        print("War::setEnemyAiUnits");
        int unitCount = allyUnitNames.Count;
        if (unitCount > 0)
        {
            GameNetwork gameNetwork = GameNetwork.Instance;
            GameInventory gameInventory = GameInventory.Instance;

            for (int i = 0; i < unitCount; i++)
            {
                //Build string
                if (unitsString != "")
                    unitsString += NetworkManager.Separators.VALUES;

                unitsString += enemyUnitNames[i];

                //Give enemy AI same exp as allies of the same tier. When team is created the levels will be built.  
                int allyUnitExp = gameNetwork.GetLocalPlayerUnitPropertyAsInt(GameNetwork.UnitPlayerProperties.EXPERIENCE, allyUnitNames[i], GameNetwork.VirtualPlayerIds.ALLIES);
                gameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.EXPERIENCE, enemyUnitNames[i], allyUnitExp.ToString(), GameNetwork.VirtualPlayerIds.ENEMIES);
                
                ////Set random position
                Vector2 pos = getRandomBattlefieldPosition();
                string posString = pos.x.ToString() + NetworkManager.Separators.VALUES + pos.y.ToString();
                gameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, enemyUnitNames[i], posString, GameNetwork.VirtualPlayerIds.ENEMIES);
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
        bool isAllies = (virtualPlayerId == GameNetwork.VirtualPlayerIds.ALLIES);
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

            PhotonPlayer player = NetworkManager.GetLocalPlayer();
            if (!isAllies && (gameStats.Mode == GameStats.Modes.Pvp) && !gameStats.UsesAiForPvp)
                player = gameNetwork.EnemyPlayer;

            int unitExp = gameNetwork.GetAnyPlayerUnitPropertyAsInt(GameNetwork.UnitPlayerProperties.EXPERIENCE, unitName, virtualPlayerId, player);
            gameNetwork.BuildUnitLevels(unitName, unitExp, virtualPlayerId);

            string positionString = gameNetwork.GetAnyPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, unitName, virtualPlayerId, player);
            string[] positionCoords = positionString.Split(NetworkManager.Separators.VALUES);
            float posX = float.Parse(positionCoords[0]);
            float posY = float.Parse(positionCoords[1]);

            unit.GetComponent<MinMinUnit>().SendSetForWar(unitName, virtualPlayerId, i, posX, posY);
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
            effect_name = _alliesGrid.transform.Find(_attackingUnitName + "/Effect").transform.GetChild(0).name;
            attack.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            effect_name = _enemiesGrid.transform.Find(_attackingUnitName + "/Effect").transform.GetChild(0).name;
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
        if (_localPlayerTeam == GameNetwork.VirtualPlayerIds.ALLIES)
        {
            GameNetwork gameNetwork = GameNetwork.Instance;
            gameNetwork.ClearTeamUnits(GameNetwork.VirtualPlayerIds.ALLIES);
            gameNetwork.ClearTeamUnits(GameNetwork.VirtualPlayerIds.ENEMIES);
        }

        SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
    }

    public void SetUnitHealth(string team, string unitName, int value, bool shouldUpdateTeamHealth)
    {
        GameNetwork.Instance.SetUnitHealth(team, unitName, value);

        if (shouldUpdateTeamHealth)
            setTeamHealth(team);
    }

    public List<MinMinUnit> GetTeamUnitsList(string teamName)
    {
        List<MinMinUnit> teamUnits = null;
        if (teamName == GameNetwork.VirtualPlayerIds.ALLIES)
            teamUnits = _allies;
        else //Enemies
            teamUnits = _enemies;

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
            //int Health = 

            int expEarned = 0;
            int minMinHealth = gameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.HEALTH, GameNetwork.VirtualPlayerIds.ALLIES, minMin.name);
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
        bool enemiesDefeated = false;
        bool alliesDefeated = false;
        string winner = "";

        string alliesTeam = GameNetwork.VirtualPlayerIds.ALLIES;
        string enemiesTeam = GameNetwork.VirtualPlayerIds.ENEMIES;

        if (_roundCount > _maxRoundsCount)
            _hasMatchEnded = true;
        else
        { 
            enemiesDefeated = areAllUnitsDefeated(enemiesTeam);
            alliesDefeated = areAllUnitsDefeated(alliesTeam);
            if (enemiesDefeated  || enemiesDefeated)
                _hasMatchEnded = true;
        }

        if (_hasMatchEnded)
        {
            if (enemiesDefeated)
                winner = GameNetwork.VirtualPlayerIds.ALLIES;
            else if (alliesDefeated)
                winner = GameNetwork.VirtualPlayerIds.ENEMIES;

            if (winner == "")
            {
                int alliesTotalHealth = getTeamTotalHealth(alliesTeam);
                int enemiesTotalHealth = getTeamTotalHealth(enemiesTeam);

                //Team with higher total health wins
                if (alliesTotalHealth > enemiesTotalHealth)
                    winner = alliesTeam;
                else if (enemiesTotalHealth > alliesTotalHealth)
                    winner = enemiesTeam;

                if (winner == "")
                {
                    int localPlayerRating = GameNetwork.Instance.GetLocalPlayerRating(GameNetwork.VirtualPlayerIds.ALLIES);
                    int enemyRating = GameNetwork.Instance.GetAnyPlayerRating(_enemies[0].GetComponent<PhotonView>().owner, GameNetwork.VirtualPlayerIds.ENEMIES);

                    //Lowest rated player wins
                    if (localPlayerRating < enemyRating)
                        winner = alliesTeam;
                    else if (enemyRating < localPlayerRating)
                        winner = enemiesTeam;

                    if (winner == "")
                        winner = alliesTeam; // Master client wins
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
        List<MinMinUnit> teamUnitsForTotalCalculus = GetTeamUnitsList(teamName);

        int healthTotal = 0;
        foreach (MinMinUnit unit in teamUnitsForTotalCalculus)
            healthTotal += gameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.HEALTH, teamName, unit.name);

        gameNetwork.SetRoomTeamProperty(GameNetwork.TeamRoomProperties.HEALTH, teamName, healthTotal.ToString());
    }

    private void setTeamMaxHealth(string teamName)
    {
        List<MinMinUnit> teamUnitsForTotalCalculus = GetTeamUnitsList(teamName);

        GameNetwork gameNetwork = GameNetwork.Instance;
        int maxHealthTotal = 0;
        foreach (MinMinUnit unit in teamUnitsForTotalCalculus)
            maxHealthTotal += gameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.MAX_HEALTH, teamName, unit.name);

        gameNetwork.SetRoomTeamProperty(GameNetwork.TeamRoomProperties.MAX_HEALTH, teamName, maxHealthTotal.ToString());
    }

    //Only master client uses this
    private bool areAllUnitsDefeated(string team)
    {
        bool areAllUnitsDefeated = true;
        List<MinMinUnit> units = new List<MinMinUnit>();
        if (team == GameNetwork.VirtualPlayerIds.ALLIES)
            units = _allies;
        else if (team == GameNetwork.VirtualPlayerIds.ENEMIES)
            units = _enemies;

        foreach (MinMinUnit unit in units)
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
