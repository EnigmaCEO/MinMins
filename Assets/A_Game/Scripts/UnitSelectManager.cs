﻿using SWS;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectManager : MonoBehaviour
{
    [SerializeField] private float _uiMoveDistance = 5;
    [SerializeField] private float _uiMoveTime = 1;
    [SerializeField] private float _uiOutDelay = 0.1f;
    [SerializeField] private float _uiInDelay = 1.5f;
    [SerializeField] private iTween.EaseType _uiMoveEaseType = iTween.EaseType.linear;
    [SerializeField] private Vector2 _attackPatternOffset;

    [SerializeField] private Transform _unitsGridContent;
    [SerializeField] private GameObject _teamGrid;
    [SerializeField] private GameObject _infoPopUp;

    [SerializeField] private Transform _powerGridContent;
    [SerializeField] private Transform _armorGridContent;

    [SerializeField] private Image _slotInfoImage;
    [SerializeField] private WaypointManager _waypointManager;

    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _backButton;

    //[SerializeField] private Transform _patternBG;
    
    private Transform _teamGridContent;
    private int _selectedUnitIndex;

    private int[] _teamUnitIndexes = new int[6];
    private bool _infoEnabled = false;

    private Vector2[] _waypoints;
    private GameObject _attack;

    void Start()
    { 
        int unitsLength = _unitsGridContent.childCount;

        for (int i = 0; i < unitsLength; i++)
        {
            Transform unit =_unitsGridContent.GetChild(i);
            int unitIndex = i;
            unit.Find("FightButton").GetComponent<Button>().onClick.AddListener(() => { onUnitFightButtonDown(unitIndex); });
            unit.Find("InfoButton").GetComponent<Button>().onClick.AddListener(() => { onUnitInfoButtonDown(unitIndex); });
        }

        _teamGridContent = _teamGrid.transform.Find("Viewport/Content");
        int slotsLength = _teamGridContent.childCount;

        for (int i = 0; i < slotsLength; i++)
        {
            Transform slot = _teamGridContent.GetChild(i);
            Image slotImage = slot.Find("Sprite").GetComponent<Image>();
            slotImage.enabled = false;
            Transform btn_aid = slot.Find("btn_aid");
            int slotIndex = i;
            btn_aid.GetComponent<Button>().onClick.AddListener(() => { onTeamSlotButtonDown(slotIndex, slotImage); });
            btn_aid.gameObject.SetActive(false);

            if(i != 5) //Lock last one
                slot.Find("obj_item_locked").GetComponent<Image>().enabled = false;
        }

        _nextButton.onClick.AddListener(() => onNextButtonDown());
        _nextButton.gameObject.SetActive(false);

        _backButton.onClick.AddListener(() => onBackButtonDown());

        disableUnitInfo(true);
    }

    void OnGUI()
    {
        if (_attack && _infoEnabled)
        {
            //Vector2 way = attack.GetComponent<SWS.BezierPathManager>().pathPoints[0]*12;
            Color c = new Color(30f / 255f, 152f / 255f, 0f);

            int wayPointsLength = _waypoints.Length;
            Vector2[] finalwayPoints = new Vector2[wayPointsLength];
            for (int i = 0; i < wayPointsLength; i++)
                finalwayPoints[i] = _waypoints[i] + _attackPatternOffset;

            if (finalwayPoints[2] == Vector2.zero)
            {
                DreamDrawing.DrawLine(finalwayPoints[0], finalwayPoints[1], 100f, 6f, Color.black, finalwayPoints[0], 0f);
                DreamDrawing.DrawLine(finalwayPoints[0], finalwayPoints[1], 100f, 4f, c, finalwayPoints[0], 0f);
            }
            else
            {
                DreamDrawing.DrawCurve(finalwayPoints[0], finalwayPoints[1], finalwayPoints[2], finalwayPoints[3], 100f, 6f, Color.black, finalwayPoints[0], 0f);
                DreamDrawing.DrawCurve(finalwayPoints[0], finalwayPoints[1], finalwayPoints[2], finalwayPoints[3], 100f, 4f, c, finalwayPoints[0], 0f);
            }
        }

    }

    private void enableUnitInfo()
    {
        iTween.MoveBy(_infoPopUp, iTween.Hash("y", _uiMoveDistance, "easeType", _uiMoveEaseType.ToString(), "loopType", iTween.LoopType.none.ToString(), "delay", _uiInDelay, "time", _uiMoveTime));
        _infoEnabled = true;
    }

    private void disableUnitInfo(bool immediate = false)
    {
        float outTime = immediate? 0 : _uiMoveTime;
        float outDelay = immediate ? 0 : _uiOutDelay;
        iTween.MoveBy(_infoPopUp, iTween.Hash("y", -_uiMoveDistance, "easeType", _uiMoveEaseType.ToString(), "loopType", iTween.LoopType.none.ToString(), "delay", outDelay, "time", outTime));

        _infoEnabled = false;
    }

    private void enableTeamGrid()
    {
        iTween.MoveBy(_teamGrid, iTween.Hash("y", _uiMoveDistance, "easeType", _uiMoveEaseType.ToString(), "loopType", iTween.LoopType.none.ToString(), "delay", _uiInDelay, "time", _uiMoveTime));
    }

    private void disableTeamGrid()
    {
        iTween.MoveBy(_teamGrid, iTween.Hash("y", -_uiMoveDistance, "easeType", _uiMoveEaseType.ToString(), "loopType", iTween.LoopType.none.ToString(), "delay", _uiOutDelay, "time", _uiMoveTime));

        //Enable to cancel slot fill when info comes out. ================================
        //int slotsLength = _teamGridContent.childCount;
        //for (int i = 0; i < slotsLength; i++)
        //    _teamGridContent.GetChild(i).Find("btn_aid").gameObject.SetActive(false); 
        //================================================================================
    }

    private void onNextButtonDown()
    {
        SceneManager.LoadScene("WarPrep");
    }

    private void onBackButtonDown()
    {
        disableUnitInfo();
        enableTeamGrid();
    }

    private void onUnitFightButtonDown(int unitIndex)
    {
        _selectedUnitIndex = unitIndex;

        int slotsLength = _teamGridContent.childCount;
        for (int i = 0; i < slotsLength; i++)
        {
            Transform slot = _teamGridContent.GetChild(i);
            if (!slot.Find("Sprite").GetComponent<Image>().enabled && !slot.Find("obj_item_locked").GetComponent<Image>().enabled)
                slot.Find("btn_aid").gameObject.SetActive(true);
        }

        if (_infoEnabled)
        {
            disableUnitInfo();
            enableTeamGrid();
        }
    }

    private void onUnitInfoButtonDown(int unitIndex)
    {
        _selectedUnitIndex = unitIndex;
        loadUnitInfo();

        if (_infoEnabled == false)
        {
            disableTeamGrid();
            enableUnitInfo();
        }
    }

    private void onTeamSlotButtonDown(int slotIndex, Image slotImage)
    {
        _teamUnitIndexes[slotIndex] = _selectedUnitIndex;

        _unitsGridContent.GetChild(_selectedUnitIndex).gameObject.SetActive(false);

        int slotsLength = _teamGridContent.childCount;
        for (int i = 0; i < slotsLength; i++)
            _teamGridContent.GetChild(i).Find("btn_aid").gameObject.SetActive(false);

        GameObject minMinPrefab = Resources.Load<GameObject>("Prefabs/MinMins/" + (_selectedUnitIndex + 1));
        slotImage.sprite = minMinPrefab.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;
        slotImage.enabled = true;

        checkTeamReady();
    }

    private void checkTeamReady()
    {
        bool teamReady = true;

        int slotsLength = _teamGridContent.childCount;
        for (int i = 0; i < slotsLength; i++)
        {
            Transform slot = _teamGridContent.GetChild(i);
            if (!slot.Find("Sprite").GetComponent<Image>().enabled && !slot.Find("obj_item_locked").GetComponent<Image>().enabled)
                teamReady = false;
        }

        if (teamReady)
            _nextButton.gameObject.SetActive(true);
    }

    private void loadUnitInfo()
    {
        print("loadUnitInfo -> selectedIndex: " + _selectedUnitIndex);
        GameObject minMinPrefab = Resources.Load<GameObject>("Prefabs/MinMins/" + (_selectedUnitIndex + 1));
        MinMin minMin = minMinPrefab.GetComponent<MinMin>();
        _slotInfoImage.sprite = minMin.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;

        int powerContentLenght = _powerGridContent.childCount;
        for (int i = 0; i < powerContentLenght; i++)
            _powerGridContent.GetChild(i).GetComponent<Image>().enabled = (i < minMin.strength);

        int armorContentLenght = _armorGridContent.childCount;
        for (int i = 0; i < powerContentLenght; i++)
            _armorGridContent.GetChild(i).GetComponent<Image>().enabled = (i < minMin.defense);

        // Attack Patterns
        GameObject temp = GameObject.Find("Canvas/PopUp/PatternBG/UnitInfo");
        if (temp)
        {
            DestroyImmediate(temp);
            _attack = null;
        }

        GameObject container = new GameObject("UnitInfo");
        //container.transform.SetParent(_patternBG);
        //container.transform.localPosition = Vector3.zero;

        _waypoints = new Vector2[4];
        int pattern = minMin.attack[0];

        _attack = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/Patterns/Attack" + pattern));
        _attack.transform.parent = container.transform;
        _attack.transform.localPosition = new Vector2(53.5f, 33f);
        _attack.GetComponent<SWS.BezierPathManager>().CalculatePath();

        Vector2 add = new Vector2(120, 0);
        Vector2 add2 = new Vector2(60, 0);

        for (int i = 0; i < 4; i++)
        {
            Transform t = null;

            if (_attack.transform.childCount == 2)
            {
                t = _attack.transform.Find("Waypoint " + i);
            }
            else
            {
                if (i == 0) t = _attack.transform.Find("Waypoint " + i);
                if (i == 1) t = _attack.transform.Find("Waypoint " + i + "/Left");
                if (i == 2) t = _attack.transform.Find("Waypoint " + (i - 1) + "/Right");
                if (i == 3) t = _attack.transform.Find("Waypoint " + (i - 1));
            }


            if (t)
            {
                _waypoints[i] = t.position * 12;
                if (_waypoints[i].x == 642 || _waypoints[i].x == 738 || _waypoints[i].x == 582 || _waypoints[i].x == 702) _waypoints[i] -= add2;
                if (_waypoints[i].x == 762) _waypoints[i] -= add;

                _waypoints[i].y += (414 - _waypoints[i].y) * 2;
                Debug.Log(i + ": " + _waypoints[i].y);
            }
            else
            {
                _waypoints[i] = Vector2.zero;
                //Debug.Log(i +": "+waypoints[i].x);
            }
        }
    }
}
