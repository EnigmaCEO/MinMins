using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class War : MonoBehaviour
{
    [HideInInspector] public bool Ready = true;

    [SerializeField] private int _maxRoundsCount = 3;

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

    // Use this for initialization
    void Awake()
    {
        _battleField = GameObject.Find("/Battlefield").transform;
        _alliesGrid = _battleField.Find("Team1");
        _enemiesGrid = _battleField.Find("Team2");

        GameMatch matchManager = GameMatch.Instance;
        //int teamLength = matchManager.Team1.Length;
        int teamLength = matchManager.GetTeamSize(1);

        for (int i = 0; i < teamLength; i++)
        {
            GameMatch.UnitData unitData = matchManager.GetUnit(1, i);
            if (unitData.Name == "-1")
                continue;

            int itemNumber = i + 1;

            GameObject unit = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/MinMinUnits/" + unitData.Name));
            unit.name = unitData.Name;

            MinMinUnit minMinUnit = unit.GetComponent<MinMinUnit>();
            _allies.Add(minMinUnit);

            Transform unitTransform = unit.transform;
            unitTransform.parent = _alliesGrid.Find("slot" + itemNumber);
            unitTransform.localPosition = Vector2.zero;

            Transform spriteTransform = unitTransform.Find("Sprite");
            spriteTransform.localPosition = new Vector2(unitData.Position.x, unitData.Position.y);
            WarUnit warUnit = spriteTransform.gameObject.AddComponent<WarUnit>();
            warUnit.Unit = unit;
            warUnit.SetWar(this);

            GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
            shadow.transform.parent = spriteTransform;
            shadow.transform.localPosition = new Vector2(0, 0);
            shadow.transform.localScale = new Vector2(-1, 1);

            //Set UI
            Transform warTeamGridItem = _teamGridContent.Find("WarTeamGridItem" + itemNumber);
            Transform uiSpriteTransform = warTeamGridItem.Find("Sprite");
            Image uiSpriteImage = uiSpriteTransform.GetComponent<Image>();
            SpriteRenderer spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
            uiSpriteImage.sprite = spriteRenderer.sprite;
            warUnit.LifeFill = warTeamGridItem.Find("LifeBar/LifeFill").GetComponent<Image>();
        }

        teamLength = matchManager.GetTeamSize(2);
        for (int i = 0; i < teamLength; i++)
        {
            GameMatch.UnitData unitData = matchManager.GetUnit(2, 1);
            if (unitData.Name == "-1")
                continue;

            int itemNumber = i + 1;

            GameObject unit = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/MinMinUnits/" + unitData.Name));
            unit.name = unitData.Name;

            MinMinUnit minMinUnit = unit.GetComponent<MinMinUnit>();
            _enemies.Add(minMinUnit);

            Transform unitTransform = unit.transform;
            unitTransform.parent = _enemiesGrid.Find("slot" + itemNumber);
            unitTransform.localPosition = Vector2.zero;

            Transform spriteTransform = unitTransform.Find("Sprite");
            spriteTransform.localPosition = new Vector2(unitData.Position.x, unitData.Position.y);
            WarUnit warUnit = spriteTransform.gameObject.AddComponent<WarUnit>();
            warUnit.Unit = unit;
            warUnit.SetWar(this);

            GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
            shadow.transform.parent = spriteTransform;
            shadow.transform.localPosition = new Vector2(0, 0);
            shadow.transform.localScale = new Vector2(-1, 1);
        }

        setAttacks(_alliesGrid);
        setAttacks(_enemiesGrid);
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
            //if (stats.Health > 0)
            {
                if (isVictory)
                    expEarned = 10;
                else
                    expEarned = 5;
            }

            GameInventory.Instance.LevelUpUnit(minMin.name, expEarned);
        }
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

        string alliesTeam = GameNetwork.Teams.ALLIES;
        string enemiesTeam = GameNetwork.Teams.ENEMIES;

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
                winner = GameNetwork.Teams.ALLIES;
            else if (alliesDefeated)
                winner = GameNetwork.Teams.ENEMIES;

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
        return GameNetwork.Instance.GetRoomMatchStat(team, GameNetwork.UnitRoomStats.HEALTH);
    }

    //Only master client uses this
    private bool areAllUnitsDefeated(string team)
    {
        bool areAllUnitsDefeated = true;
        List<MinMinUnit> units = new List<MinMinUnit>();
        if (team == GameNetwork.Teams.ALLIES)
            units = _allies;
        else if (team == GameNetwork.Teams.ENEMIES)
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
        return GameNetwork.Instance.GetRoomUnitStat(team, GameNetwork.UnitRoomStats.HEALTH, unitName);
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
