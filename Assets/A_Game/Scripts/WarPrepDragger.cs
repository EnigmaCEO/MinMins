using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarPrepDragger : MonoBehaviour
{
    [HideInInspector] public string UnitName = "-1";

    private bool _selected = false;

    private float _x;
    private float _y;

    private GameObject _team;
    private Transform _slot;

    private WarPrepManager _manager;
    private Vector3 _screenPoint;
    private Vector3 _offset;

    private string _unitName;

    private Transform _target;

    // Use this for initialization
    void Awake()
    {
        _team = GameObject.Find("/Team1");
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

    public void SetTarget(Transform target)
    {
        _target = target;
        _slot = transform.parent;

        Transform targetParent = _target.parent;
        target.SetParent(this.transform);
        this.transform.SetParent(targetParent);

        _unitName = this.transform.parent.GetComponentInChildren<MinMinUnit>().name;
    }

    void OnMouseDown()
    {
        Debug.LogWarning("WarPrepDragger::OnMouseDown -> target: " + _target.name);
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);

        //print("OnMouseDown: " + gameObject.name);
        _screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        _offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenPoint.z));

        _manager.UpdateInfo(_unitName);

        _selected = true;
        _target.GetComponent<PrepMinMinSprite>().EnablePolygonCollider();


        //Debug.Log(name + " selected");
        if (transform.parent.parent.name.Contains("slot"))
        {
            transform.parent = _team.transform.Find(transform.parent.parent.name);
        }
    }

    void OnMouseDrag()
    {
        Debug.LogWarning("WarPrepDragger::OnMouseDrag -> target: " + _target.name);
        if (_selected)
        {
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(_x, _y, _screenPoint.z)) + _offset;
        }
    }

    void OnMouseUp()
    {
        Debug.LogWarning("WarPrepDragger::OnMouseUp -> target: " + _target.name);
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        dropOnBattlefield();
    }

    public void SetManager(WarPrepManager manager)
    {
        _manager = manager;
    }

    public void HandleDroppedCollision()
    {
        if (_selected)
        {
            dropOnBattlefield();
        }
    }

    private void dropOnBattlefield()
    {
        _manager.CloseInfoPopUp();
        _selected = false;

        //_target.GetComponent<PrepMinMinSprite>().EnablePolygonCollider();
        //GetComponent<BoxCollider2D>().enabled = false;

        limitPosition();

        if (_slot.Find("confirm_ok(Clone)") == null)
        {
            GameObject confirm = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/confirm_ok"));
            confirm.transform.parent = _slot;
            confirm.transform.localPosition = new Vector2(0, 0);

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
}
