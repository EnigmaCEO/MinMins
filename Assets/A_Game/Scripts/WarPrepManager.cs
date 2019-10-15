using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enigma.CoreSystems;

public class WarPrepManager : EnigmaScene
{
    [SerializeField] private Transform _slotsTeam1;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private GameObject _infoPopUp;

    private int _slotsInPlay = 0;
    private int _slotsReady = 0;

    void Start()
    {
        _nextButton.onClick.AddListener(() => { onNextButtonDown(); });
        _nextButton.gameObject.SetActive(false);

        _backButton.onClick.AddListener(() => { onBackButtonDown(); });

        Transform warPrepGrid = GameObject.Find("/Canvas/WarPrepGrid").transform;
        
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
            prepMinMinSprite.SetManager(this);
            prepMinMinSprite.UnitName = unitName;

            
            minMinObj.GetComponentInChildren<PolygonCollider2D>().isTrigger = true;

            minMinTransform.parent = warPrepGrid.Find("Viewport/Content/slot" + (i + 1));
            minMinTransform.localPosition = new Vector2(0, 0);

            int unitTier = GameInventory.Instance.GetUnitTier(unitName);
            minMinTransform.parent.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/unit_frame_t" + unitTier);
            _infoPopUp.SetActive(false);

            _slotsInPlay++;
        }

        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.positionCount = 4;
        lineRenderer.loop = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.cyan, 0.0f), new GradientColorKey(Color.cyan, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;

        lineRenderer.SetPosition(0, new Vector3(GameConfig.Instance.BattleFieldMinPos.x, GameConfig.Instance.BattleFieldMinPos.y, 0.0f));
        lineRenderer.SetPosition(1, new Vector3(GameConfig.Instance.BattleFieldMinPos.x, GameConfig.Instance.BattleFieldMaxPos.y, 0.0f));
        lineRenderer.SetPosition(2, new Vector3(GameConfig.Instance.BattleFieldMaxPos.x, GameConfig.Instance.BattleFieldMaxPos.y, 0.0f));
        lineRenderer.SetPosition(3, new Vector3(GameConfig.Instance.BattleFieldMaxPos.x, GameConfig.Instance.BattleFieldMinPos.y, 0.0f));
        
    }

    void Update()
    {
        
    }

    public void UpdateInfo(string unitName)
    {
        _infoPopUp.SetActive(true);
        _infoPopUp.transform.Find("UnitName").GetComponent<Text>().text = "Min-Min #" + unitName;
        MinMinUnit minMin = GameInventory.Instance.GetMinMinFromResources(unitName);

        _infoPopUp.transform.Find("UnitType").GetComponent<Text>().text = minMin.Type.ToString();

        _infoPopUp.GetComponent<UnitInfoPopUp>().UpdateExpInfo(unitName);
    }

    public void AddToSlotsReady()
    {
        _slotsReady++;
        _infoPopUp.SetActive(false);

        if (_slotsReady == _slotsInPlay)
        {
            _nextButton.gameObject.SetActive(true);
            GameObject.Find("/Team1").GetComponent<TweenPosition>().enabled = true;
            gameObject.GetComponent<LineRenderer>().enabled = false;
        }

    }

    private void onNextButtonDown()
    {
        GameNetwork gameNetwork = GameNetwork.Instance;
        GameStats gameStats = GameStats.Instance;

        gameStats.PreparationPositions.Clear();
        for (int i = 0; i < _slotsInPlay; i++)
        {
            Transform slotTransform = _slotsTeam1.Find("slot" + (i + 1));
            Transform unitSpriteTransform = slotTransform.Find("Sprite");
            PrepMinMinSprite prepMinMinSprite = unitSpriteTransform.GetComponent<PrepMinMinSprite>();

            Vector3 pos = unitSpriteTransform.localPosition;
            gameStats.PreparationPositions.Add(pos);
        }

        GameStats.Modes gameMode = GameStats.Instance.Mode;
        if (gameMode == GameStats.Modes.SinglePlayer)
        {
            NetworkManager.Connect(true);
            SceneManager.LoadScene(GameConstants.Scenes.WAR);
        }
        else if (gameMode == GameStats.Modes.Pvp)
        {
            NetworkManager.Connect(false);
            SceneManager.LoadScene(GameConstants.Scenes.LOBBY);
        }
    }

    private void onBackButtonDown()
    {
        SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);
    }
}
