using SWS;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enigma.CoreSystems;
using GameConstants;
using GameEnums;
using System.Linq;

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
    [SerializeField] private Button _clearTeamButton;
    [SerializeField] private Button _expTeamButton;

    [SerializeField] private Toggle _savedTeamToggle1;
    [SerializeField] private Toggle _savedTeamToggle2;

    //[SerializeField] private Transform _patternBG;

    [SerializeField] private Text _helpText;
    [SerializeField] GameObject _expPopUp;

    private Transform _teamGridContent;
    private string _selectedUnitName;

    private bool _infoEnabled = false;

    private Vector2[] _waypoints;
    private GameObject _attack;


   
    void Start()
    {
        SoundManager.FadeCurrentSong(1f, () => 
        {
            int arena = Random.Range(1, 4);
            SoundManager.Stop();
            SoundManager.Play("arena" + arena, SoundManager.AudioTypes.Music, "", true);
        });

        AdsManager.OnAdRewardGranted += handleAdReward;
        CloseExpPopup();

        GameInventory gameInventory = GameInventory.Instance;

        List<string> inventoryUnitNames = gameInventory.GetInventoryUnitNames();  //TODO: Check if this needs to return stats
        List<string> descendingUnitNames = inventoryUnitNames.OrderByDescending(x => int.Parse(x)).ToList();

        int unitsLength = descendingUnitNames.Count;
        GameObject unitGridItemTemplate = _unitsGridContent.GetChild(0).gameObject;

        for (int i = 0; i < unitsLength; i++)
        {
            Transform unitTransform = Instantiate<GameObject>(unitGridItemTemplate, _unitsGridContent).transform;
            string unitName = descendingUnitNames[i];
            unitTransform.name = unitName;
            unitTransform.Find("Sprite").GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
            unitTransform.GetComponent<Button>().onClick.AddListener(() => { onUnitFightButtonDown(unitName); });
            unitTransform.Find("InfoButton").GetComponent<Button>().onClick.AddListener(() => { onUnitInfoButtonDown(unitName); });

            int unitTier = gameInventory.GetUnitTier(unitName);
            unitTransform.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/unit_frame_t" + unitTier);

            for (int x = 1; x < 4; x++)
            {
                if (unitTier < x)
                {
                    unitTransform.Find("StarsGrid/Viewport/Content/star" + x).gameObject.SetActive(false);
                }
            }

            unitTransform.Find("level/txt_level").GetComponent<Text>().text = gameInventory.GetLocalUnitLevel(unitName).ToString();
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

            if (i != 5) //Lock last one
            {
                slot.Find("obj_item_locked").GetComponent<Image>().enabled = false;
            }
        }

        _nextButton.onClick.AddListener(() => onNextButtonDown());
        _nextButton.gameObject.SetActive(false);

        _infoBackButton.onClick.AddListener(() => onInfoBackButtonDown());
        _sceneBackButton.onClick.AddListener(() => onSceneBackButtonDown());

        _clearTeamButton.onClick.AddListener(() => onClearTeamButtonDown());
        _clearTeamButton.gameObject.SetActive(false);

        _expTeamButton.onClick.AddListener(() => OpenExpPopup());

        disableUnitInfo(true);

        createDefaultSelectedUnits(slotsLength);

        if (GameHacks.Instance.ClearSavedTeamDataOnScenesEnter)
        {
            PlayerPrefs.DeleteKey(getSavedTeamSlotKey());
            PlayerPrefs.DeleteKey(getTeamUnitsKey());
        }

        GameStats gameStats = GameStats.Instance;
        gameStats.SelectedSaveSlot = PlayerPrefs.GetInt(getSavedTeamSlotKey(), 1);
        if (gameStats.SelectedSaveSlot == 1)
        {
            _savedTeamToggle1.isOn = true;
            _savedTeamToggle2.isOn = false;
        }
        else
        {
            _savedTeamToggle1.isOn = false;
            _savedTeamToggle2.isOn = true;
        }

        loadUnitSelection();

        _savedTeamToggle1.onValueChanged.AddListener(delegate { onSavedTeamToggleChanged(_savedTeamToggle1, 1); });
        _savedTeamToggle2.onValueChanged.AddListener(delegate { onSavedTeamToggleChanged(_savedTeamToggle2, 2); });
    }

    private void OnDestroy()
    {
        AdsManager.OnAdRewardGranted -= handleAdReward;
    }

    private void onSavedTeamToggleChanged(Toggle toggle, int saveSlotSelected)
    {
        if (toggle.isOn)
        {
            PlayerPrefs.SetInt(getSavedTeamSlotKey(), saveSlotSelected);
            SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);  //Reload
        }
    }

    private void displaySelectUnitHelp()
    {
        _helpText.text = LocalizationManager.GetTermTranslation("Select a unit.");    
    }

    private void displayAssignUnitHelp()
    {
        _helpText.text = LocalizationManager.GetTermTranslation("Assign the selected unit to a team position.");
    }

    private void displayProceedHelp()
    {
        _helpText.text = LocalizationManager.GetTermTranslation("Press right arrow button to proceed.");
    }

    private void createDefaultSelectedUnits(int amount)
    {
        GameStats gameStats = GameStats.Instance;
        gameStats.TeamUnits.Clear();

        for (int i = 0; i < amount; i++)
        {
            gameStats.TeamUnits.Add("-1");
        }
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
        GameSounds.Instance.PlayUiAdvanceSound();
        saveUnitSelection();

        if ((GameStats.Instance.TeamBoostTokensOwnedByName.Count > 0) || GameInventory.Instance.IsThereAnyOreSingleItem())
        {
            SceneManager.LoadScene(GameConstants.Scenes.TEAM_BOOST);
        }
        else
        {
            SceneManager.LoadScene(GameConstants.Scenes.WAR_PREP);
        }
    }

    private void onInfoBackButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        disableUnitInfo();
        enableTeamGrid();
    }

    private void onSceneBackButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        GameStats gameStats = GameStats.Instance;

        if (GameStats.Instance.Mode == GameModes.Quest)
        {
            if (gameStats.QuestType == QuestTypes.Scout)
            {
                SceneManager.LoadScene(Scenes.SCOUT_QUEST);
            }
            else
            {
                SceneManager.LoadScene(Scenes.LEVELS);
            }
        }
        else
        {
            SceneManager.LoadScene(Scenes.LEVELS);
        }
    }

    private void onClearTeamButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        PlayerPrefs.DeleteKey(getTeamUnitsKey());
        SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);  //Reload
    }

    private void onUnitFightButtonDown(string unitName)
    {
        GameSounds.Instance.PlayUiAdvanceSound();
        _selectedUnitName = unitName;

        displayAssignUnitHelp();

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
        GameSounds.Instance.PlayUiAdvanceSound();
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
        GameSounds.Instance.PlayUiBackSound();
        GameObject unitGameObject = _unitsGridContent.Find(_selectedUnitName).gameObject;

        selectUnit(slotIndex, slotImage, unitGameObject);

        checkTeamReady();
    }

    private void selectUnit(int slotIndex, Image slotImage, GameObject unitGameObject)
    {
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

        slotImage.transform.parent.Find("level/txt_level").GetComponent<Text>().text = GameInventory.Instance.GetLocalUnitLevel(unitName).ToString();

        GameStats.Instance.TeamUnits[slotIndex] = unitName;

        disableUnitInfo();
        activateClearButtonIfInactive();
    }

    private void checkTeamReady()
    {
        bool teamReady = true;

        int slotsLength = _teamGridContent.childCount;
        for (int i = 0; i < slotsLength; i++)
        {
            Transform slot = _teamGridContent.GetChild(i);
            if (!slot.Find("Sprite").GetComponent<Image>().enabled && !slot.Find("obj_item_locked").GetComponent<Image>().enabled)
            {
                teamReady = false;
            }
        }

        if (teamReady)
        {
            displayProceedHelp();
            _nextButton.gameObject.SetActive(true);
        }
        else
        {
            displaySelectUnitHelp();
        }
    }

    private void activateClearButtonIfInactive()
    {
        if (!_clearTeamButton.gameObject.activeSelf)
        {
            _clearTeamButton.gameObject.SetActive(true);
        }
    }

    //private void handleClearTeamButtonVisible()
    //{
    //    bool isThereUnit = false;
    //    foreach (string unitName in GameStats.Instance.TeamUnits)
    //    {
    //        if (unitName != "-1")
    //        {
    //            isThereUnit = true;
    //        }
    //    }

    //    _clearTeamButton.gameObject.SetActive(isThereUnit);
    //}

    private void loadUnitInfo()
    {
        enableUnitInfo();
        print("loadUnitInfo -> selectedUnitName: " + _selectedUnitName);
        GameInventory gameInventory = GameInventory.Instance;

        MinMinUnit minMin = gameInventory.GetMinMinFromResources(_selectedUnitName);
        _slotInfoImage.sprite = minMin.transform.Find("Sprite").GetComponent<SpriteRenderer>().sprite;

        int powerContentLenght = _powerGridContent.childCount;
        for (int i = 0; i < powerContentLenght; i++)
        {
            _powerGridContent.GetChild(i).GetComponent<Image>().enabled = (i < minMin.Strength);
        }

        int armorContentLenght = _armorGridContent.childCount;
        for (int i = 0; i < powerContentLenght; i++)
        {
            _armorGridContent.GetChild(i).GetComponent<Image>().enabled = (i < minMin.Defense);
        }

        _infoPopUp.transform.Find("UnitName").GetComponent<Text>().text = "Min-Min #" + _selectedUnitName;
        _infoPopUp.transform.Find("UnitType").GetComponent<Text>().text = LocalizationManager.GetTermTranslation(minMin.Role.ToString());

        _infoPopUp.GetComponent<UnitInfoPopUp>().UpdateExpInfo(_selectedUnitName);

        for (int x = 1; x < 6; x++)
        {
            _infoPopUp.transform.Find("PowerStarsGrid/Viewport/Content/star" + x).gameObject.SetActive(true);
            _infoPopUp.transform.Find("ArmorStarsGrid/Viewport/Content/star" + x).gameObject.SetActive(true);

            if (minMin.Strength < x)
            {
                _infoPopUp.transform.Find("PowerStarsGrid/Viewport/Content/star" + x).gameObject.SetActive(false);
            }

            if (minMin.Defense < x)
            {
                _infoPopUp.transform.Find("ArmorStarsGrid/Viewport/Content/star" + x).gameObject.SetActive(false);
            }
        }
    }

    private void saveUnitSelection()
    {
        GameStats gameStats = GameStats.Instance;
        List<string> teamUnits = gameStats.TeamUnits;

        string unitsString = "";
        foreach (string unitName in teamUnits)
        {
            if (unitsString != "")
            {
                unitsString += "|";
            }

            unitsString += unitName;
        }

        PlayerPrefs.SetString(getTeamUnitsKey(), unitsString);
        PlayerPrefs.Save();
    }

    private void loadUnitSelection()
    {
        string unitsString = PlayerPrefs.GetString(getTeamUnitsKey(), "");

        if (unitsString == "")
        {
            displaySelectUnitHelp();
            return;
        }

        string[] unitNames = unitsString.Split('|');
        int unitsCount = unitNames.Length;

        bool unitWasFound = false;

        for(int i = 0; i < unitsCount; i++)
        {
            Transform unitTransform = _unitsGridContent.Find(unitNames[i]);

            if (unitTransform != null)
            {
                Transform slot = _teamGridContent.GetChild(i);
                Image slotImage = slot.Find("Sprite").GetComponent<Image>();

                selectUnit(i, slotImage, unitTransform.gameObject);
                unitWasFound = true;
            }
        }

        if (unitWasFound)
        {
            activateClearButtonIfInactive();
        }

        checkTeamReady();
    }

    private string getSavedTeamSlotKey()
    {
        return (GameStats.Instance.Mode.ToString() + "_selectedSaveSlot"); 
    }

    private string getTeamUnitsKey()
    {
        return (GameStats.Instance.SelectedSaveSlot + "_" + GameStats.Instance.Mode.ToString() + "_teamUnits");
    }

    public void OpenExpPopup() 
    {
        _expPopUp.SetActive(true);
    }

    public void CloseExpPopup()
    {
        _expPopUp.SetActive(false);
    }

    public void ShowAd() {
        GameSounds.Instance.PlayUiAdvanceSound();
        AdsManager.Instance.ShowRewardAd();
    }

    public void handleAdReward(string zoneId, bool success, string name, int amount)
    {
        Debug.Log("exp gained");

        GameInventory gameInventory = GameInventory.Instance;
        GameStats gameStats = GameStats.Instance;
        List<string> teamUnits = gameStats.TeamUnits;

        foreach (string unitName in teamUnits)
        {
            if (unitName != "-1")
            {
                Debug.Log("exp to unit: " + unitName);
                gameInventory.AddExpToUnit(unitName, 10);
            }
        }

        gameInventory.SaveUnits();
        
        saveUnitSelection();
        SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);
    }
}
