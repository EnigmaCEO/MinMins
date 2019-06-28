using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class War : MonoBehaviour
{
    [HideInInspector] public bool Ready = true;

    [SerializeField] private Transform _teamGridContent;
    [SerializeField] private float _readyCheckDelay = 2;

    [SerializeField] private float _battlefieldMovementAmount = 5000;
    [SerializeField] private float _battlefieldMovementDelay = 0.1f;
    [SerializeField] private float _battlefieldMovementTime = 1;
    [SerializeField] private iTween.EaseType _battlefieldMovementEaseType = iTween.EaseType.easeInOutExpo;

    private int _side = 0;

    private Transform _battleField;
    private Transform _teamGrid;
    private Transform _enemyGrid;
    //private GameObject _slot;

    private string _attackingUnitName = "";
    private float _timer;
    private float _playTime;

    // Use this for initialization
    void Awake()
    {
        _battleField = GameObject.Find("/Battlefield").transform;
        _teamGrid = _battleField.Find("Team1");
        _enemyGrid = _battleField.Find("Team2");

        MatchManager matchManager = MatchManager.Instance;
        int teamLength = matchManager.Team1.Length;

        for (int i = 0; i < teamLength; i++)
        {
            MatchManager.UnitData unitData = matchManager.Team1[i];
            if (unitData.name == "-1")
                continue;

            int itemNumber = i + 1;

            GameObject unit = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/MinMins/" + unitData.name));
            unit.name = unitData.name;
            Transform unitTransform = unit.transform;
            unitTransform.parent = _teamGrid.Find("slot" + itemNumber);
            unitTransform.localPosition = Vector2.zero;

            Transform spriteTransform = unitTransform.Find("Sprite");
            spriteTransform.localPosition = new Vector2(unitData.position.x, unitData.position.y);
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

        teamLength = matchManager.Team2.Length;
        for (int i = 0; i < teamLength; i++)
        {
            MatchManager.UnitData unitData = matchManager.Team2[i];
            if (unitData.name == "-1")
                continue;

            int itemNumber = i + 1;

            GameObject unit = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/MinMins/" + unitData.name));
            unit.name = unitData.name;
            Transform unitTransform = unit.transform;
            unitTransform.parent = _enemyGrid.Find("slot" + itemNumber);
            unitTransform.localPosition = Vector2.zero;

            Transform spriteTransform = unitTransform.Find("Sprite");
            spriteTransform.localPosition = new Vector2(unitData.position.x, unitData.position.y);
            WarUnit warUnit = spriteTransform.gameObject.AddComponent<WarUnit>();
            warUnit.Unit = unit;
            warUnit.SetWar(this);

            GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
            shadow.transform.parent = spriteTransform;
            shadow.transform.localPosition = new Vector2(0, 0);
            shadow.transform.localScale = new Vector2(-1, 1);
        }

        SetAttacks(_teamGrid);
        SetAttacks(_enemyGrid);
    }

    // Update is called once per frame
    void Update()
    {
        _playTime += Time.deltaTime;

        if (_side == 0)
            _timer = _playTime;

        if (((_playTime - _timer) >= _readyCheckDelay) && Ready)
        {
            int enemiesGridCount = _enemyGrid.childCount;
            for (int i = 0; i < enemiesGridCount; i++)
            {
                Transform enemySlot = _enemyGrid.GetChild(i);
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
            attackerUnitTransform = _teamGrid.Find(attackerUnitName);
        else
            attackerUnitTransform = _enemyGrid.Find(attackerUnitName);

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
            effect_name = _teamGrid.transform.Find(_attackingUnitName + "/Effect").transform.GetChild(0).name;
            attack.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            effect_name = _enemyGrid.transform.Find(_attackingUnitName + "/Effect").transform.GetChild(0).name;
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

    void SetAttacks(Transform val)
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
}
