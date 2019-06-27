using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class War : MonoBehaviour
{
    public bool Ready = true;

    [SerializeField] private Transform _teamGridContent;

    private int _side = 0;

    private Transform _battleField;
    private Transform _teamGrid;
    private Transform _enemyGrid;
    //private GameObject _slot;

    private string _aUnit = "";
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

            GameObject unit = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/Units/" + unitData.name));
            unit.name = unitData.name;
            Transform unitTransform = unit.transform;
            unitTransform.parent = _battleField.Find("Team1/slot" + (i + 1));
            unitTransform.localPosition = Vector2.zero;

            Transform spriteTransform = unitTransform.Find("Sprite");
            spriteTransform.localPosition = new Vector2(unitData.position.x, unitData.position.y);
            WarUnit warUnit = spriteTransform.gameObject.AddComponent<WarUnit>();
            warUnit.Unit = unit; 
            warUnit.SetWar(this);

            GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
            shadow.transform.parent = unit.transform;
            shadow.transform.localPosition = new Vector2(0, 0);
            shadow.transform.localScale = new Vector2(-1, 1);

            _teamGridContent.Find("WarTeamGridItem" + (i + 1) + "/Sprite").GetComponent<Image>().sprite = spriteTransform.GetComponent<Image>().sprite;
        }

        teamLength = matchManager.Team2.Length;
        for (int i = 0; i < teamLength; i++)
        {
            MatchManager.UnitData unitData = matchManager.Team2[i];
            if (unitData.name == "-1")
                continue;

            GameObject unit = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/Units/" + unitData.name));
            unit.name = unitData.name;
            Transform unitTransform = unit.transform;
            unitTransform.parent = _battleField.Find("Team2/slot" + (i + 1));
            unitTransform.localPosition = Vector2.zero;

            Transform spriteTransform = unitTransform.Find("Sprite");
            spriteTransform.localPosition = new Vector2(unitData.position.x, unitData.position.y);
            WarUnit warUnit = spriteTransform.gameObject.AddComponent<WarUnit>();
            warUnit.Unit = unit;
            warUnit.SetWar(this);

            GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
            shadow.transform.parent = unit.transform;
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

        if (_side == 0) _timer = _playTime;

        if (_playTime - _timer >= 2 && Ready)
        {
            for (int i = 0; i < 6; i++)
            {
                Transform eSlot = _enemyGrid.transform.GetChild(i);
                if (eSlot.transform.childCount == 0)
                    continue;

                Attack(eSlot.name + "/" + eSlot.GetChild(0).name);
                break;
            }
        }
    }

    public void Attack(string unit)
    {
        if (!Ready) return;
        Transform target = null;

        if (_side == 0)
        {
            target = _teamGrid.transform.Find(unit);
        }
        else
        {
            target = _enemyGrid.transform.Find(unit);
        }

        if (target == null) return;
        if (target.Find("Effect").transform.childCount == 0) return;

        Ready = false;
        _battleField.GetComponent<TweenPosition>().ResetToBeginning();
        if (_side == 0)
        {
            _battleField.GetComponent<TweenPosition>().from = new Vector2(0, 0);
            _battleField.GetComponent<TweenPosition>().to = new Vector2(-5000, 0);
        }
        else
        {
            _battleField.GetComponent<TweenPosition>().to = new Vector2(0, 0);
            _battleField.GetComponent<TweenPosition>().from = new Vector2(-5000, 0);
        }

        _battleField.GetComponent<TweenPosition>().enabled = true;
        _aUnit = unit;
    }

    public void AttackReady(UITweener tween)
    {
        Debug.Log("Attack ready!");
        string effect_name = "";
        GameObject attack = GameObject.Find("Waypoint Manager/" + _aUnit.Split('/')[1] + "/Attack").transform.GetChild(0).gameObject;

        if (_side == 0)
        {
            effect_name = _teamGrid.transform.Find(_aUnit + "/Effect").transform.GetChild(0).name;

            attack.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            effect_name = _enemyGrid.transform.Find(_aUnit + "/Effect").transform.GetChild(0).name;

            attack.transform.localEulerAngles = new Vector3(0, 180, 0);
        }

        attack.transform.localPosition = new Vector2(0, 0);
        attack.GetComponent<SWS.BezierPathManager>().CalculatePath();

        GameObject effect = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/Attacks/" + effect_name));
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
