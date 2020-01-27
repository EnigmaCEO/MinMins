using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepMinMinSprite : MonoBehaviour
{
    [HideInInspector] public string UnitName = "-1";

    [SerializeField] private bool _selected = false;

    private float _x;
    private float _y;

    private GameObject _team;
    private GameObject _slot;
    private WarPrepManager _manager;
    private Vector3 _screenPoint;
    private Vector3 _offset;

    private string _unitName;

    // Use this for initialization
    void Start()
    {
        _team = GameObject.Find("/Team1");
        _slot = transform.parent.gameObject;

        _unitName = transform.parent.GetComponentInChildren<MinMinUnit>().gameObject.name;
    }

    // Update is called once per frame
    void Update()
    {
        if (_selected)
        {
            _x = Input.mousePosition.x;
            _y = Input.mousePosition.y;
        }
        else
        {
            limitPosition();
        }
    }

    void OnMouseDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);

        //print("OnMouseDown: " + gameObject.name);
        _screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        _offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenPoint.z));

        _manager.UpdateInfo(_unitName);

        _selected = true;
        //Debug.Log(name + " selected");
        if (transform.parent.parent.name.Contains("slot"))
            transform.parent = _team.transform.Find(transform.parent.parent.name);

        
    }

    void OnMouseDrag()
    {
        if (_selected)
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(_x, _y, _screenPoint.z)) + _offset;
    }

    void OnMouseUp()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        dropOnBattlefield();
    }

    public void SetManager(WarPrepManager manager)
    {
        _manager = manager;
    }

    private void dropOnBattlefield()
    {
        _manager.CloseInfoPopUp();
        _selected = false;

        limitPosition();

        if (_slot.transform.Find("confirm_ok(Clone)") == null)
        {
            GameObject confirm = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/confirm_ok"));
            confirm.transform.parent = _slot.transform;
            confirm.transform.localPosition = new Vector2(0, 0);
            //confirm.transform.localScale = new Vector2(2, 2);

            /*GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
            shadow.transform.parent = transform;
            shadow.transform.localPosition = new Vector2(0, 0);
            shadow.transform.localScale = new Vector2(-1, 1);*/
            GetComponentInChildren<PolygonCollider2D>().isTrigger = false;

            _manager.AddToSlotsReady();
        }
    }

    private void limitPosition()
    {
        GameConfig gameConfig = GameConfig.Instance;

        if (transform.localPosition.y < gameConfig.BattleFieldMinPos.y)
        {
            transform.localPosition = new Vector2(transform.localPosition.x, gameConfig.BattleFieldMinPos.y);
        }
        else if (transform.localPosition.y > gameConfig.BattleFieldMaxPos.y)
        {
            transform.localPosition = new Vector2(transform.localPosition.x, gameConfig.BattleFieldMaxPos.y);
        }

        if (transform.localPosition.x > gameConfig.BattleFieldMaxPos.x)
        {
            transform.localPosition = new Vector2(gameConfig.BattleFieldMaxPos.x, transform.localPosition.y);
        }
        else if (transform.localPosition.x < gameConfig.BattleFieldMinPos.x)
        {
            transform.localPosition = new Vector2(gameConfig.BattleFieldMinPos.x, transform.localPosition.y);
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        //Debug.Log(coll.gameObject.transform.parent.name + " collision");
        if (_selected)
            dropOnBattlefield();
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        //return;

        float x1, y1;

        if (transform.localPosition.x >= coll.transform.localPosition.x)
            x1 = 0.1f;
        else
            x1 = -0.1f;

        if (transform.localPosition.y >= coll.transform.localPosition.y)
            y1 = 0.1f;
        else
            y1 = -0.1f;

        transform.Translate(x1, y1, 0);
    }
}
