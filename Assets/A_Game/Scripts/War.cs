using Enigma.CoreSystems;
using System.Collections;
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

    [SerializeField] private int _maxRoundsCount = 3;

    [SerializeField] private int PvpTeamsToCreate = 2;
    [SerializeField] private int SinglePlayerOrAiTeamsToCreate = 1;

    [SerializeField] private Transform _teamGridContent;
    [SerializeField] private float _readyCheckDelay = 2;

    [SerializeField] private float _battlefieldMovementAmount = 5000;
    [SerializeField] private float _battlefieldMovementDelay = 0.1f;
    [SerializeField] private float _battlefieldMovementTime = 1;
    [SerializeField] private iTween.EaseType _battlefieldMovementEaseType = iTween.EaseType.easeInOutExpo;

    [SerializeField] private Text _errorText;
    [SerializeField] private MatchResultsPopUp _matchResultsPopUp;

    private int _side = 0;
    private bool _hasMatchEnded = false;

    private Transform _battleField;
    private Transform _alliesGrid;
    private Transform _enemiesGrid;
    //private GameObject _slot;

    private List<MinMinUnit> _allies;
    private List<MinMinUnit> _enemies;

    private string _attackingUnitName = "";
    private float _timer;
    private float _playTime;

    private MatchData _matchData = new MatchData();

    private int _roundCount = 0;

    void Awake()
    {
        _battleField = GameObject.Find("/Battlefield").transform;

        if (NetworkManager.IsPhotonOffline())
        {
            instantiateAllies();
            instantiateEnemies(false);
        }
        else
        {
            if (NetworkManager.GetIsMasterClient())
            {
                instantiateAllies();

                if (GameStats.Instance.UsesAiForPvp)
                    instantiateEnemies(false);
            }
            else
                instantiateEnemies(true);
        }
    }

    private void instantiateAllies()
    {
        instantiateTeam(GridNames.TEAM_1, GameConstants.TeamNames.ALLIES, true);
    }

    private void instantiateEnemies(bool localPlayerIsEnemy)
    {
        instantiateTeam(GridNames.TEAM_2, GameConstants.TeamNames.ENEMIES, localPlayerIsEnemy);
    }

    private void instantiateTeam(string gridName, string teamName, bool localPlayerIsTeam)
    {
        Transform grid = _battleField.Find(gridName);

        GameNetwork gameNetwork = GameNetwork.Instance;
        string[] teamUnits = gameNetwork.GetLocalPlayerTeamUnits(teamName);
        int teamLength = teamUnits.Length;

        for (int i = 0; i < teamLength; i++)
        {
            string unitName = teamUnits[i];
            if (unitName == "-1")
                continue;

            int itemNumber = i + 1;

            GameObject unit = NetworkManager.InstantiateObject("Prefabs/MinMinUnits/" + unitName, Vector3.zero, Quaternion.identity, 0);
            unit.name = unitName;

            MinMinUnit minMinUnit = unit.GetComponent<MinMinUnit>();
            _allies.Add(minMinUnit);

            if (localPlayerIsTeam)
                gameNetwork.BuildUnitLevels(unitName);
            else
            {
                //TODO: Level up AI unit. 
            }

            Transform unitTransform = unit.transform;
            unitTransform.parent = grid.Find("slot" + itemNumber);
            unitTransform.localPosition = Vector2.zero;

            string positionString = gameNetwork.GetLocalPlayerUnitProperty(unitName, GameNetwork.UnitPlayerProperties.POSITION);
            string[] positionCoords = positionString.Split(Constants.Separators.First);
            float posX = float.Parse(positionCoords[0]);
            float posY = float.Parse(positionCoords[1]);

            Transform spriteTransform = unitTransform.Find("Sprite");
            spriteTransform.localPosition = new Vector2(posX, posY);
            WarUnit warUnit = spriteTransform.gameObject.AddComponent<WarUnit>();
            warUnit.Unit = unit;
            warUnit.SetWar(this);

            GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
            shadow.transform.parent = spriteTransform;
            shadow.transform.localPosition = new Vector2(0, 0);
            shadow.transform.localScale = new Vector2(-1, 1);

            if (localPlayerIsTeam)
            {
                //Set UI
                Transform warTeamGridItem = _teamGridContent.Find("WarTeamGridItem" + itemNumber);
                Transform uiSpriteTransform = warTeamGridItem.Find("Sprite");
                Image uiSpriteImage = uiSpriteTransform.GetComponent<Image>();
                SpriteRenderer spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
                uiSpriteImage.sprite = spriteRenderer.sprite;
                warUnit.LifeFill = warTeamGridItem.Find("LifeBar/LifeFill").GetComponent<Image>();
            }
        }

        setAttacks(grid);
    }

    private void Start()
    {
        _errorText.gameObject.SetActive(false);
        _matchResultsPopUp.gameObject.SetActive(false);
        _matchData.PlayerId = NetworkManager.GetPlayerName();
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

                Attack(enemySlot.name + "/" + enemySlot.GetChild(0).name);
                break;
            }
        }
    }

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
        SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
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
        _matchResultsPopUp.SetValues(_matchData);
        _matchResultsPopUp.Open();

        GameNetwork.Instance.SendMatchResults(_matchData, onMatchResultsCallback);

        foreach (Transform slot in _teamGridContent)
        {
            GameObject minMin = slot.GetChild(0).gameObject;
            //int Health = 

            int expEarned = 0;
            int minMinHealth = GameNetwork.Instance.GetRoomUnitStat(GameConstants.TeamNames.ALLIES, minMin.name, GameNetwork.UnitRoomProperties.HEALTH);
            if (minMinHealth > 0)
            {
                if (isVictory)
                    expEarned = 10;
                else
                    expEarned = 5;
            }

            GameInventory.Instance.AddExpToUnit(minMin.name, expEarned);
        }

        GameInventory.Instance.SaveUnits();
    }

    private void onMatchResultsCallback(string message)
    {
        if (message == GameNetwork.ServerResponseMessages.SUCCESS)
        {
            _errorText.text = "";
            _errorText.gameObject.SetActive(false);
        }
        else
        {
            _errorText.text = message;
            _errorText.gameObject.SetActive(true);
        }
    }

    //Only master client uses this
    private void checkWinLoseConditions()
    {
        bool enemiesDefeated = false;
        bool alliesDefeated = false;
        string winner = "";

        string alliesTeam = GameConstants.TeamNames.ALLIES;
        string enemiesTeam = GameConstants.TeamNames.ENEMIES;

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
                winner = GameConstants.TeamNames.ALLIES;
            else if (alliesDefeated)
                winner = GameConstants.TeamNames.ENEMIES;

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
                    int localPlayerRating = GameNetwork.Instance.GetLocalPlayerRating();
                    int enemyRating = GameNetwork.Instance.GetAnyPlayerRating(_enemies[0].GetComponent<PhotonView>());

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
        return GameNetwork.Instance.GetRoomMatchStat(team, GameNetwork.UnitRoomProperties.HEALTH);
    }

    //Only master client uses this
    private bool areAllUnitsDefeated(string team)
    {
        bool areAllUnitsDefeated = true;
        List<MinMinUnit> units = new List<MinMinUnit>();
        if (team == GameConstants.TeamNames.ALLIES)
            units = _allies;
        else if (team == GameConstants.TeamNames.ENEMIES)
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
        return GameNetwork.Instance.GetRoomUnitStat(team, unitName, GameNetwork.UnitRoomProperties.HEALTH);
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
