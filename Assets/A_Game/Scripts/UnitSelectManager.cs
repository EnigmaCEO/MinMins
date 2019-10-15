using SWS;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enigma.CoreSystems;

public class UnitSelectManager : EnigmaScene
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
    [SerializeField] private Button _infoBackButton;
    [SerializeField] private Button _sceneBackButton;

    //[SerializeField] private Transform _patternBG;
    
    private Transform _teamGridContent;
    private string _selectedUnitName;

    private bool _infoEnabled = false;

    private Vector2[] _waypoints;
    private GameObject _attack;


    void Start()
    {
        List<string> inventoryUnitNames = GameInventory.Instance.GetInventoryUnitNames();  //TODO: Check if this needs to return stats
        int unitsLength = inventoryUnitNames.Count;
        GameObject unitGridItemTemplate = _unitsGridContent.GetChild(0).gameObject;

        for (int i = 0; i < unitsLength; i++)
        {
            Transform unitTransform = Instantiate<GameObject>(unitGridItemTemplate, _unitsGridContent).transform;
            string unitName = inventoryUnitNames[i];
            unitTransform.name = unitName;
            unitTransform.Find("Sprite").GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
            unitTransform.GetComponent<Button>().onClick.AddListener(() => { onUnitFightButtonDown(unitName); });
            unitTransform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(() => { onUnitInfoButtonDown(unitName); });

            int unitTier = GameInventory.Instance.GetUnitTier(unitName);
            unitTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/unit_frame_t" + unitTier);
            
            for(int x = 1; x < 4; x++)
            {
                if (unitTier < x) unitTransform.Find("StarsGrid/Viewport/Content/star" + x).gameObject.SetActive(false);
            }
                
        }

        unitGridItemTemplate.SetActive(false);
        GameObject.Find("UnitsGrid").GetComponent<ScrollRect>().horizontalNormalizedPosition = 0;

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
            iTween.Pause(btn_aid.gameObject);
            btn_aid.gameObject.SetActive(false);

            if(i != 5) //Lock last one
                slot.Find("obj_item_locked").GetComponent<Image>().enabled = false;
        }

        _nextButton.onClick.AddListener(() => onNextButtonDown());
        _nextButton.gameObject.SetActive(false);

        _infoBackButton.onClick.AddListener(() => onInfoBackButtonDown());
        _sceneBackButton.onClick.AddListener(() => onSceneBackButtonDown());

        disableUnitInfo(true);

        createDefaultSelectedUnits(slotsLength);
    }

    private void createDefaultSelectedUnits(int amount)
    {
        GameStats gameStats = GameStats.Instance;
        gameStats.TeamUnits.Clear();
        for (int i = 0; i < amount; i++)
            gameStats.TeamUnits.Add("-1");
    }

    private void enableUnitInfo()
    {
        /*iTween.MoveBy(_infoPopUp, iTween.Hash("y", _uiMoveDistance, "easeType", _uiMoveEaseType.ToString(), "loopType", iTween.LoopType.none.ToString(), "delay", _uiInDelay, "time", _uiMoveTime));
        _infoEnabled = true;*/
        _infoPopUp.SetActive(true);
    }

    private void disableUnitInfo(bool immediate = false)
    {
        /*float outTime = immediate? 0 : _uiMoveTime;
        float outDelay = immediate ? 0 : _uiOutDelay;
        iTween.MoveBy(_infoPopUp, iTween.Hash("y", -_uiMoveDistance, "easeType", _uiMoveEaseType.ToString(), "loopType", iTween.LoopType.none.ToString(), "delay", outDelay, "time", outTime));

        _infoEnabled = false;*/
        _infoPopUp.SetActive(false);
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
        SceneManager.LoadScene(GameConstants.Scenes.WAR_PREP);
    }

    private void onInfoBackButtonDown()
    {
        disableUnitInfo();
        enableTeamGrid();
    }

    private void onSceneBackButtonDown()
    {
        SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
    }

    private void onUnitFightButtonDown(string unitName)
    {
        _selectedUnitName = unitName;

        int slotsLength = _teamGridContent.childCount;
        for (int i = 0; i < slotsLength; i++)
        {
            Transform slot = _teamGridContent.GetChild(i);
            if (!slot.Find("Sprite").GetComponent<Image>().enabled && !slot.Find("obj_item_locked").GetComponent<Image>().enabled)
            {
                GameObject btnAidGameObject = slot.Find("btn_aid").gameObject;
                btnAidGameObject.SetActive(true);
                iTween.Resume(btnAidGameObject);
            }
        }

        if (_infoEnabled)
        {
            disableUnitInfo();
            enableTeamGrid();
        }

        loadUnitInfo();
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
        {
            GameObject btnAidGameObject = _teamGridContent.GetChild(i).Find("btn_aid").gameObject;
            iTween.Pause(btnAidGameObject);
            btnAidGameObject.SetActive(false);
        }

        string unitName = unitGameObject.name;
        slotImage.sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
        slotImage.enabled = true;

        int unitTier = GameInventory.Instance.GetUnitTier(unitName);
        slotImage.transform.parent.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/unit_frame_t" + unitTier);

        GameStats.Instance.TeamUnits[slotIndex] = unitName;
        disableUnitInfo();
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
        enableUnitInfo();
        print("loadUnitInfo -> selectedUnitName: " + _selectedUnitName);
        GameInventory gameInventory = GameInventory.Instance;

        MinMinUnit minMin = gameInventory.GetMinMinFromResources(_selectedUnitName);
        _slotInfoImage.sprite = minMin.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;

        int powerContentLenght = _powerGridContent.childCount;
        for (int i = 0; i < powerContentLenght; i++)
            _powerGridContent.GetChild(i).GetComponent<Image>().enabled = (i < minMin.Strength);

        int armorContentLenght = _armorGridContent.childCount;
        for (int i = 0; i < powerContentLenght; i++)
            _armorGridContent.GetChild(i).GetComponent<Image>().enabled = (i < minMin.Defense);

        _infoPopUp.transform.Find("UnitName").GetComponent<Text>().text = "Min-Min #" + _selectedUnitName;
        _infoPopUp.transform.Find("UnitType").GetComponent<Text>().text = minMin.Type.ToString();

        int unitExp = gameInventory.GetLocalUnitExp(_selectedUnitName);
        GameInventory.ExpData unitExpData = gameInventory.GetUnitExpData(unitExp);

        _infoPopUp.transform.Find("UnitLevel").GetComponent<Text>().text = "Level " + unitExpData.Level;
        _infoPopUp.transform.Find("UnitExp").GetComponent<Text>().text = "(" + unitExp + "/" + unitExpData.ExpForNextLevel + ")";
        _infoPopUp.transform.Find("ExpProgress").GetComponent<Slider>().value = (unitExp - unitExpData.ExpForPreviousLevel) /(unitExpData.ExpForNextLevel - unitExpData.ExpForPreviousLevel);

        for (int x = 1; x < 6; x++)
        {
            _infoPopUp.transform.Find("PowerStarsGrid/Viewport/Content/star" + x).gameObject.SetActive(true);
            _infoPopUp.transform.Find("ArmorStarsGrid/Viewport/Content/star" + x).gameObject.SetActive(true);

            if (minMin.Strength < x) _infoPopUp.transform.Find("PowerStarsGrid/Viewport/Content/star" + x).gameObject.SetActive(false);
            if (minMin.Defense < x) _infoPopUp.transform.Find("ArmorStarsGrid/Viewport/Content/star" + x).gameObject.SetActive(false);
        }
    }
}
