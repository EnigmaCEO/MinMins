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

    public PrepMinMinSprite Target
    {
        get;
        private set;
    }

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

    public void SetTarget(PrepMinMinSprite target)
    {
        Target = target;
        _slot = transform.parent;

        Transform targetParent = Target.transform.parent;
        target.transform.SetParent(this.transform);
        this.transform.SetParent(targetParent);

        _unitName = this.transform.parent.GetComponentInChildren<MinMinUnit>().name;
    }

    public void MoveToTeamSlot()
    {
        transform.SetParent(_team.transform.Find(transform.parent.parent.name));
    }

    void OnMouseDown()
    {
        Debug.LogWarning("WarPrepDragger::OnMouseDown -> target: " + Target.name);
        GameSounds.Instance.PlayUiAdvanceSound();

        //print("OnMouseDown: " + gameObject.name);
        _screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

        _offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenPoint.z));

        _manager.UpdateInfo(_unitName);

        _selected = true;
        Target.EnablePolygonCollider();

        //Debug.Log(name + " selected");
        if (transform.parent.parent.name.Contains("slot")) //Check if it is in team slot already
        {
            MoveToTeamSlot();
        }
    }

    void OnMouseDrag()
    {
        Debug.LogWarning("WarPrepDragger::OnMouseDrag -> target: " + Target.name);
        if (_selected)
        {
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(_x, _y, _screenPoint.z)) + _offset;
        }
    }

    void OnMouseUp()
    {
        Debug.LogWarning("WarPrepDragger::OnMouseUp -> target: " + Target.name);
        GameSounds.Instance.PlayUiBackSound();
        DropOnBattlefield();
    }

    public void SetManager(WarPrepManager manager)
    {
        _manager = manager;
    }

    public void HandleDroppedCollision()
    {
        if (_selected)
        {
            DropOnBattlefield();
        }
    }

    public void DropOnBattlefield()
    {
        _manager.CloseInfoPopUp();
        _selected = false;

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
