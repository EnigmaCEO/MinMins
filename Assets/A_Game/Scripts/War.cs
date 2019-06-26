using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class War : MonoBehaviour
{
    public bool Ready = true;

    private int _side = 0;

    private GameObject _field;
    private GameObject _grid;
    private GameObject _enemies;
    private GameObject _slot;

    private string _aUnit = "";
    private float _timer;
    private float _playTime;

    // Use this for initialization
    void Awake()
    {
        _field = GameObject.Find("Battlefield");
        _grid = GameObject.Find("TeamGrid");
        _enemies = GameObject.Find("EnemyGrid");

        MatchManager matchManager = MatchManager.Instance;
        int teamLength = matchManager.Team1.Length;

        for (int i = 0; i < teamLength; i++)
        {
            MatchManager.UnitData unitData = matchManager.Team1[i];
            //if(unitData.)

            _slot = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/Units/" + unitData.name));

            _slot.transform.parent = _grid.transform.Find("slot" + i);
            _slot.name = matchManager.Team1[i - 1].name;
            _slot.transform.localScale = new Vector2(240, 240);
            _slot.AddComponent<MinMin>();

            GameObject obj = (GameObject)Instantiate(_slot.transform.Find("Sprite").gameObject);
            obj.name = matchManager.Team1[i - 1].name;
            obj.transform.parent = _field.transform.Find("Team1/slot" + i);
            obj.transform.localPosition = new Vector2(matchManager.Team1[i - 1].position.x, matchManager.Team1[i - 1].position.y);
            obj.AddComponent<Unit>();
            obj.GetComponent<Unit>().unit = _slot;

            GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
            shadow.transform.parent = obj.transform;
            shadow.transform.localPosition = new Vector2(0, 0);
            shadow.transform.localScale = new Vector2(-1, 1);
        }

        for (int i = 1; i < 6; i++)
        {
            _slot = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/Units/" + matchManager.Team2[i - 1].name));
            _slot.transform.parent = _enemies.transform.Find("slot" + i);
            _slot.name = matchManager.Team2[i - 1].name;
            _slot.transform.localScale = new Vector2(240, 240);
            _slot.transform.localPosition = new Vector2(0, 0);

            GameObject obj = (GameObject)Instantiate(_slot.transform.Find("Sprite").gameObject);
            obj.name = matchManager.Team2[i - 1].name;
            obj.transform.parent = _field.transform.Find("Team2/slot" + i);
            obj.transform.localPosition = new Vector2(matchManager.Team2[i - 1].position.x, matchManager.Team2[i - 1].position.y);
            obj.AddComponent<Unit>();
            obj.transform.localScale = new Vector2(1, 1);
            obj.GetComponent<Unit>().unit = _slot;
            obj.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f / 255f);

        }

        SetAttacks(_grid);
        SetAttacks(_enemies);
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
                Transform eSlot = _enemies.transform.GetChild(i);
                if (eSlot.transform.childCount == 0) continue;

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
            target = _grid.transform.Find(unit);
        }
        else
        {
            target = _enemies.transform.Find(unit);
        }

        if (target == null) return;
        if (target.Find("Effect").transform.childCount == 0) return;

        Ready = false;
        _field.GetComponent<TweenPosition>().ResetToBeginning();
        if (_side == 0)
        {
            _field.GetComponent<TweenPosition>().from = new Vector2(0, 0);
            _field.GetComponent<TweenPosition>().to = new Vector2(-5000, 0);
        }
        else
        {
            _field.GetComponent<TweenPosition>().to = new Vector2(0, 0);
            _field.GetComponent<TweenPosition>().from = new Vector2(-5000, 0);
        }

        _field.GetComponent<TweenPosition>().enabled = true;
        _aUnit = unit;
    }

    public void AttackReady(UITweener tween)
    {
        Debug.Log("Attack ready!");
        string effect_name = "";
        GameObject attack = GameObject.Find("Waypoint Manager/" + _aUnit.Split('/')[1] + "/Attack").transform.GetChild(0).gameObject;

        if (_side == 0)
        {
            effect_name = _grid.transform.Find(_aUnit + "/Effect").transform.GetChild(0).name;

            attack.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            effect_name = _enemies.transform.Find(_aUnit + "/Effect").transform.GetChild(0).name;

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

    void SetAttacks(GameObject val)
    {
        for (int i = 0; i < val.transform.childCount; i++)
        {

            GameObject temp = val.transform.GetChild(i).gameObject;

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
