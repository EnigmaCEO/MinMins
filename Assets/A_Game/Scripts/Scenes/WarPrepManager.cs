using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enigma.CoreSystems;
using UnityEngine.Video;
using GameConstants;
using GameEnums;

public class WarPrepManager : EnigmaScene
{
    [SerializeField] private Transform _slotsTeam1;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private GameObject _infoPopUp;
    [SerializeField] private Text _proceedHelpText;

    private int _slotsInPlay = 0;
    private int _slotsReady = 0;

    private LineRenderer _lineRenderer;
    private Transform _warPrepGrid;

    void Start()
    {
        //displayDragUnitsHelp();
        _proceedHelpText.gameObject.SetActive(false);

        _nextButton.onClick.AddListener(() => { onNextButtonDown(); });
        _nextButton.gameObject.SetActive(false);

        _backButton.onClick.AddListener(() => { onBackButtonDown(); });

        _warPrepGrid = GameObject.Find("/Canvas/WarPrepGrid").transform;
        
        List<string> unitNames = GameStats.Instance.TeamUnits;
        int unitsCount = unitNames.Count;

        for (int i = 0; i < unitsCount; i++)
        {
            string unitName = unitNames[i];
            if (unitName == "-1")
                continue;

            GameObject minMinObj = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/MinMinUnits/" + unitName));
            minMinObj.name = unitName;
            Transform minMinTransform = minMinObj.transform;
            PrepMinMinSprite prepMinMinSprite = minMinTransform.Find("Sprite").gameObject.AddComponent<PrepMinMinSprite>();

            Transform slot = _warPrepGrid.Find("Viewport/Content/slot" + (i + 1));
            minMinTransform.parent = slot;
            //minMinTransform.parent = warPrepGrid.Find("Viewport/Content/slot" + (i + 1));
            minMinTransform.localPosition = new Vector2(0, 0);

            WarPrepDragger dragger = slot.GetComponentInChildren<WarPrepDragger>();
            dragger.SetTarget(prepMinMinSprite);
            dragger.UnitName = unitName;
            dragger.SetManager(this);

            prepMinMinSprite.SetDragger(dragger);

            int unitTier = GameInventory.Instance.GetUnitTier(unitName);
            minMinTransform.parent.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/unit_frame_t" + unitTier);

            minMinTransform.parent.Find("level/txt_level").GetComponent<Text>().text = GameInventory.Instance.GetLocalUnitLevel(unitName).ToString();

            _infoPopUp.SetActive(false);

            _slotsInPlay++;
        }

        if (GameHacks.Instance.ClearSavedTeamDataOnScenesEnter)
        {
            PlayerPrefs.DeleteKey(getWarPrepKey());
        }

        _lineRenderer = gameObject.AddComponent<LineRenderer>();

        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.widthMultiplier = 0.1f;
        _lineRenderer.positionCount = 4;
        _lineRenderer.loop = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.cyan, 0.0f), new GradientColorKey(Color.cyan, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        _lineRenderer.colorGradient = gradient;

        updateBoundsLines();

        loadWarPrep();
    }

    void Update()
    {
        if (GameHacks.Instance.CreateWarPrepLineRendererOnUpdate)
        {
            updateBoundsLines();
        }
    }

    //private void displayDragUnitsHelp()
    //{
    //    _proceedHelpText.text = LocalizationManager.GetTermTranslation("Drag units to position.");
    //}

    private void displayProceedHelp()
    {
        _proceedHelpText.gameObject.SetActive(true);
    }

    private void saveWarPrep()
    {
        List<Vector3> prepPositions = GameStats.Instance.PreparationPositions;
        string warPrepString = "";

        foreach (Vector3 pos in prepPositions)
        {
            if (warPrepString != "")
            {
                warPrepString += "|";
            }

            warPrepString += pos.x + "," + pos.y + "," + pos.z;
        }

        PlayerPrefs.SetString(getWarPrepKey(), warPrepString);
        PlayerPrefs.Save();
    }

    private void loadWarPrep()
    {
        string warPrepString = PlayerPrefs.GetString(getWarPrepKey(), "");

        if (warPrepString == "")
        {
            return;
        }

        string[] warPrepTerms = warPrepString.Split('|');
        int termsCount = warPrepTerms.Length;

        for (int i = 0; i < termsCount; i++)
        {
            string term = warPrepTerms[i];
            string[] coords = term.Split(',');
            Vector3 spritePos = new Vector3(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));

            Transform slot = _warPrepGrid.Find("Viewport/Content/slot" + (i + 1));
            MinMinUnit unit = slot.GetComponentInChildren<MinMinUnit>();
            Transform draggerTransform = unit.transform.Find("WarPrepDragger");
            WarPrepDragger warPrepDragger = draggerTransform.GetComponent<WarPrepDragger>();
            warPrepDragger.MoveToTeamSlot();
            warPrepDragger.DropOnBattlefield();
            draggerTransform.localPosition = spritePos - warPrepDragger.Target.OriginalLocalPosition;

            //Vector3 pos = draggerTransform.localPosition + spriteTransform.localPosition;
        }
    }

    private string getWarPrepKey()
    {
        return (GameStats.Instance.SelectedSaveSlot + "_" + GameStats.Instance.Mode.ToString() + "_warPrep");
    }

    private void updateBoundsLines()
    {
        GameConfig gameConfig = GameConfig.Instance;

        _lineRenderer.SetPosition(0, new Vector3(gameConfig.BattleFieldMinPos.x - gameConfig.BoundsLineRendererOutwardOffset, gameConfig.BattleFieldMinPos.y - gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
        _lineRenderer.SetPosition(1, new Vector3(gameConfig.BattleFieldMinPos.x - gameConfig.BoundsLineRendererOutwardOffset, gameConfig.BattleFieldMaxPos.y + gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
        _lineRenderer.SetPosition(2, new Vector3(gameConfig.BattleFieldMaxPos.x + gameConfig.BoundsLineRendererOutwardOffset, gameConfig.BattleFieldMaxPos.y + gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
        _lineRenderer.SetPosition(3, new Vector3(gameConfig.BattleFieldMaxPos.x + gameConfig.BoundsLineRendererOutwardOffset, gameConfig.BattleFieldMinPos.y - gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
    }

    public void UpdateInfo(string unitName)
    {
        _infoPopUp.SetActive(true);
        _infoPopUp.transform.Find("UnitName").GetComponent<Text>().text = "Min-Min #" + unitName;
        MinMinUnit minMin = GameInventory.Instance.GetMinMinFromResources(unitName);

        _infoPopUp.transform.Find("UnitType").GetComponent<Text>().text = LocalizationManager.GetTermTranslation(minMin.Role.ToString());

        _infoPopUp.GetComponent<UnitInfoPopUp>().UpdateExpInfo(unitName);
    }

    public void CloseInfoPopUp()
    {
        _infoPopUp.SetActive(false);
    }

    public void AddToSlotsReady()
    {
        _slotsReady++;
        //_infoPopUp.SetActive(false);

        if (_slotsReady == _slotsInPlay)
        {
            displayProceedHelp();
            _nextButton.gameObject.SetActive(true);
            GameObject.Find("/Team1").GetComponent<TweenPosition>().enabled = true;
            //gameObject.GetComponent<LineRenderer>().enabled = false;
        }
    }

    private void onNextButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();

        GameNetwork gameNetwork = GameNetwork.Instance;
        GameStats gameStats = GameStats.Instance;

        savePositionsToGameStats();
        saveWarPrep();

        bool hasPurchased = GameStats.Instance.HasPurchased;
        int fightsWithoutAdsMaxCount = GameConfig.Instance.FightsWithoutAdsMaxCount;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        GameHacks gameHacks = GameHacks.Instance;
        if (gameHacks.HasPurchased.Enabled)
        {
            hasPurchased = gameHacks.HasPurchased.ValueAsBool;
        }

        if (gameHacks.FightsWithoutAdsMaxCount.Enabled)
        {
            fightsWithoutAdsMaxCount = gameHacks.FightsWithoutAdsMaxCount.ValueAsInt;
        }
#endif

        if (!hasPurchased)
        {
            gameStats.FightWithoutAdsCount++;

            if (gameStats.FightWithoutAdsCount == fightsWithoutAdsMaxCount)
            {
                AdsManager.Instance.ShowAd();
                GameStats.Instance.FightWithoutAdsCount = 0;
            }
        }

        GameModes gameMode = GameStats.Instance.Mode;
        if ((gameMode == GameModes.SinglePlayer) || (gameMode == GameModes.Quest))
        {
            NetworkManager.Connect(true);
            SceneManager.LoadScene(GameConstants.Scenes.WAR, true);
        }
        else if (gameMode == GameModes.Pvp)
        {
            NetworkManager.Connect(false);
            SceneManager.LoadScene(GameConstants.Scenes.LOBBY);
        }
    }

    private void savePositionsToGameStats()
    {
        GameStats gameStats = GameStats.Instance;
        gameStats.PreparationPositions.Clear();

        for (int i = 0; i < _slotsInPlay; i++)
        {
            Transform slotTransform = _slotsTeam1.Find("slot" + (i + 1));
            Transform draggerTransform = slotTransform.Find("WarPrepDragger");
            Transform spriteTransform = draggerTransform.Find("Sprite");

            Vector3 pos = draggerTransform.localPosition + spriteTransform.localPosition;
            gameStats.PreparationPositions.Add(pos);
        }
    }

    private void onBackButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        SceneManager.LoadScene(GameConstants.Scenes.TEAM_BOOST);
    }
}
