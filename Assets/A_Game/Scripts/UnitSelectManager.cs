using SWS;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enigma.CoreSystems;

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
    private string _selectedUnitName;

    private bool _infoEnabled = false;

    private Vector2[] _waypoints;
    private GameObject _attack;

    private List<string> _selectedUnits = new List<string>();

    void Start()
    {
        NetworkManager.Connect(true);

        List<string> inventoryUnitNames = GameInventory.Instance.GetInventoryUnitNames();  //TODO: Check if this needs to return stats
        int unitsLength = inventoryUnitNames.Count;
        GameObject unitGridItemTemplate = _unitsGridContent.GetChild(0).gameObject;

        for (int i = 0; i < unitsLength; i++)
        {
            Transform unitTransform = Instantiate<GameObject>(unitGridItemTemplate, _unitsGridContent).transform;
            string unitName = inventoryUnitNames[i];
            unitTransform.name = unitName;
            unitTransform.Find("Sprite").GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
            unitTransform.Find("FightButton").GetComponent<Button>().onClick.AddListener(() => { onUnitFightButtonDown(unitName); });
            unitTransform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(() => { onUnitInfoButtonDown(unitName); });
        }

        unitGridItemTemplate.SetActive(false);

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

        createDefaultSelectedUnits(slotsLength);
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

    private void createDefaultSelectedUnits(int amount)
    {
        for (int i = 0; i < amount; i++)
            _selectedUnits.Add("-1");
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
        string teamUnits = "";
        int selectedUnitLenght = _selectedUnits.Count;
        for (int i = 0; i < selectedUnitLenght; i++)
        {
            if (i != 0)
                teamUnits += Constants.Separators.First;

            teamUnits += _selectedUnits[i];
        }

        GameNetwork.Instance.ClearTeamUnits(GameConstants.TeamNames.ALLIES);
        GameNetwork.Instance.SetLocalPlayerTeamProperty(GameConstants.TeamNames.ALLIES, GameNetwork.TeamPlayerProperties.UNIT_NAMES, teamUnits);

        SceneManager.LoadScene(GameConstants.Scenes.WAR_PREP);
    }

    private void onBackButtonDown()
    {
        disableUnitInfo();
        enableTeamGrid();
    }

    private void onUnitFightButtonDown(string unitName)
    {
        _selectedUnitName = unitName;

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

    private void onUnitInfoButtonDown(string unitName)
    {
        _selectedUnitName = unitName;
        loadUnitInfo();

        if (_infoEnabled == false)
        {
            disableTeamGrid();
            enableUnitInfo();
        }
    }

    private void onTeamSlotButtonDown(int slotIndex, Image slotImage)
    {
        GameObject unitGameObject = _unitsGridContent.Find(_selectedUnitName).gameObject;
        unitGameObject.SetActive(false);

        int slotsLength = _teamGridContent.childCount;
        for (int i = 0; i < slotsLength; i++)
            _teamGridContent.GetChild(i).Find("btn_aid").gameObject.SetActive(false);

        string unitName = unitGameObject.name;
        slotImage.sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
        slotImage.enabled = true;

        _selectedUnits[slotIndex] = unitName;

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
        print("loadUnitInfo -> selectedUnitName: " + _selectedUnitName);
        MinMinUnit minMin = GameInventory.Instance.GetMinMinFromResources(_selectedUnitName);
        _slotInfoImage.sprite = minMin.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;

        int powerContentLenght = _powerGridContent.childCount;
        for (int i = 0; i < powerContentLenght; i++)
            _powerGridContent.GetChild(i).GetComponent<Image>().enabled = (i < minMin.Strength);

        int armorContentLenght = _armorGridContent.childCount;
        for (int i = 0; i < powerContentLenght; i++)
            _armorGridContent.GetChild(i).GetComponent<Image>().enabled = (i < minMin.Defense);

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
        int pattern = minMin.Attacks[0];

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
