using Enigma.CoreSystems;
using GameEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalSystemQuest : MonoBehaviour
{
    public const string QUEST_PLAYER_UNIT_NAME = "quest_player_unit_name";
    public const string QUEST_PLAYER_TEAM_NAME = "quest_player_team_name";

    [SerializeField] private GameCamera _gameCamera;
    [SerializeField] private float _commonPosZ = -0.1f;
    [SerializeField] private string _scoutAreaPrefabName = "ScoutArea";

    [SerializeField] private Transform _scoutAreaContainer;
    [SerializeField] private Transform _unitsContainer;
    [SerializeField] private GameObject _questUnitPrefab;

    [SerializeField] private Transform _enemyUnitsGridContent;
    [SerializeField] private Text _questNameTitle;

    [SerializeField] GameObject _notEnoughUnitsPopUp;
    [SerializeField] GameObject _tutorialPopUp;

    private string _unitClicked = "";
    private List<string> _revealedUnitNames = new List<string>();

    private LineRenderer _lineRenderer1;
    private LineRenderer _lineRenderer2;

    private void Start()
    {
        GameInventory gameInventory = GameInventory.Instance;
        GameStats gameStats = GameStats.Instance;

        _notEnoughUnitsPopUp.SetActive(false);

        _questNameTitle.text = gameInventory.GetGlobalSystemActiveQuestName(); //gameStats.ActiveQuest.ToString();

        if (gameStats.QuestUnits.Count == 0)
        {
            determineQuestTeam();
        }

        refreshEnemyUnitsGrid();
        createQuestTeam();

        loadScouting();

        if (gameStats.QuestScoutPending)
        {
            performScouting(gameStats.LastScoutPosition);
            GameInventory.Instance.SetQuestNewScoutPosition(gameStats.LastScoutPosition);
            gameStats.QuestScoutPending = false;
        }

        _lineRenderer1 = _unitsContainer.gameObject.AddComponent<LineRenderer>();
        _lineRenderer1.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer1.widthMultiplier = 0.1f;
        _lineRenderer1.positionCount = 4;
        _lineRenderer1.loop = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.cyan, 0.0f), new GradientColorKey(Color.cyan, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        _lineRenderer1.colorGradient = gradient;

        GameConfig gameConfig = GameConfig.Instance;

        _lineRenderer1.SetPosition(0, new Vector3(gameConfig.BattleFieldMinPos.x - gameConfig.BoundsLineRendererOutwardOffset, gameConfig.BattleFieldMinPos.y - gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
        _lineRenderer1.SetPosition(1, new Vector3(gameConfig.BattleFieldMinPos.x - gameConfig.BoundsLineRendererOutwardOffset, gameConfig.BattleFieldMaxPos.y + gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
        _lineRenderer1.SetPosition(2, new Vector3(gameConfig.BattleFieldMaxPos.x + gameConfig.BoundsLineRendererOutwardOffset, gameConfig.BattleFieldMaxPos.y + gameConfig.BoundsLineRendererOutwardOffset, 0.0f));
        _lineRenderer1.SetPosition(3, new Vector3(gameConfig.BattleFieldMaxPos.x + gameConfig.BoundsLineRendererOutwardOffset, gameConfig.BattleFieldMinPos.y - gameConfig.BoundsLineRendererOutwardOffset, 0.0f));

        _lineRenderer1.GetComponent<Renderer>().sortingOrder = 303;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!_tutorialPopUp.GetActive())
            {
                StartCoroutine(handleMouseClick());
            }
        }
    }

    public void OnBackButtonDown()
    {
        SceneManager.LoadScene(EnigmaConstants.Scenes.MAIN);
    }

    private void loadScouting()
    {
        List<Vector3> positions = GameInventory.Instance.GetGlobalSystemQuestScoutProgress();

        foreach (Vector3 pos in positions)
        {
            performScouting(pos);
        }
    }

    public void OnNotEnoughUnitsPopUpDismissButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();

        if (GameConfig.Instance.SendToShopAutomaticallyOnUnitsNeeded)
        {
            SceneManager.LoadScene(GameConstants.Scenes.STORE);
        }
        else
        {
            _notEnoughUnitsPopUp.gameObject.SetActive(false);
        }
    }

    public void TutorialPopUpDismissButtonDown()
    {
        _tutorialPopUp.gameObject.SetActive(false);
    }

    private IEnumerator handleMouseClick()
    {
        yield return 0;  // wait one frame so MouseDown in units is called if clicked.

        bool IsEmptyUnitClick = (_unitClicked == "");
        GameStats gameStats = GameStats.Instance;

        if (IsEmptyUnitClick || !_revealedUnitNames.Contains(_unitClicked))
        {
            GameConfig gameConfig = GameConfig.Instance;
            Vector3 tapWorldPosition = _gameCamera.MyCamera.ScreenToWorldPoint(Input.mousePosition);

            float minPosX = gameConfig.BattleFieldMinPos.x;
            float maxPosX = gameConfig.BattleFieldMaxPos.x;

            if ((tapWorldPosition.y > (gameConfig.BattleFieldMinPos.y - gameConfig.BoundsActionOutwardOffset)) && (tapWorldPosition.y < (gameConfig.BattleFieldMaxPos.y + gameConfig.BoundsActionOutwardOffset))
            && (tapWorldPosition.x < (maxPosX + gameConfig.BoundsActionOutwardOffset)) && (tapWorldPosition.x > (minPosX - gameConfig.BoundsActionOutwardOffset)))
            {
                if (IsEmptyUnitClick)
                {
                    Debug.Log("Empty click -> Starting battle to Scout.");
                }
                else
                {
                    Debug.Log("Clicked not revealed unit: " + _unitClicked + " -> Starting battle to Scout.");
                }

                if (GameInventory.Instance.HasEnoughUnitsForBattle())
                {
                    gameStats.LastScoutPosition = tapWorldPosition;
                    gameStats.SelectedLevelNumber = 0;
                    SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);
                }
                else
                {
                    _notEnoughUnitsPopUp.SetActive(true);
                }
            }
            else
            {
                Debug.Log("Click it at out bounds at position: " + tapWorldPosition);
            }
        }
        else
        {
            Debug.Log("Attacked unit: " + _unitClicked);

            if (GameInventory.Instance.HasEnoughUnitsForBattle())
            {
                gameStats.SelectedLevelNumber = gameStats.QuestUnits.IndexOf(_unitClicked) + 1;  //And we use level 0 for random units when scout area is triggered
                gameStats.QuestSelectedUnitName = _unitClicked;
                Debug.Log("Selected level by quest unit: " + gameStats.SelectedLevelNumber);
                SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);
            }
            else
            {
                _notEnoughUnitsPopUp.SetActive(true);
            }

            _unitClicked = "";  //Only needed for testing
        }
    }

    private void performScouting(Vector3 actionAreaPos)
    {
        actionAreaPos.z = _commonPosZ;

        GameObject actionAreaObject = Instantiate<GameObject>(Resources.Load<GameObject>(ActionArea.ACTION_AREAS_RESOURCES_FOLDER_PATH + _scoutAreaPrefabName), Vector3.zero, Quaternion.identity, _scoutAreaContainer);
        ScoutArea scoutArea = actionAreaObject.GetComponent<ScoutArea>();
        scoutArea.GetComponent<CircleCollider2D>().enabled = true;
        scoutArea.SetUpActionArea(_scoutAreaPrefabName, actionAreaPos, Vector3.zero, QUEST_PLAYER_UNIT_NAME, MinMinUnit.EffectNames.ScoutLight, QUEST_PLAYER_TEAM_NAME, -1);
    }

    private void onUnitClicked(string unitName)
    {
        Debug.Log("Quest::Clicked on unit: " + name);
        _unitClicked = unitName;
    }

    private void createQuestTeam()
    {
        List<string> questUnits = GameStats.Instance.QuestUnits;
        GameConfig gameConfig = GameConfig.Instance;
        GameInventory gameInventory = GameInventory.Instance;

        int questUnitsCount = questUnits.Count;

        List<Vector3> enemiesPositions = gameInventory.GetGlobalSystemEnemiesPositions();
        if (enemiesPositions.Count == 0)
        {
            for (int i = 0; i < questUnitsCount; i++)
            {
                Vector3 randomPosition = gameConfig.GetRandomBattlefieldPosition();
                randomPosition.z = _commonPosZ;

                enemiesPositions.Add(randomPosition);
            }

            gameInventory.SetGlobalSystemQuestEnemiesPositions(enemiesPositions);
        }

        string activeGlobalSystemQuest = gameInventory.GetGlobalSystemActiveQuestString();
        for(int i = 0; i < questUnitsCount; i++)
        {
            if (gameInventory.GetQuestLevelCompleted(activeGlobalSystemQuest, i + 1))
            {
                continue;
            }

            Vector3 randomPosition = enemiesPositions[i];
            GameObject unitGameObject = Instantiate<GameObject>(_questUnitPrefab, randomPosition, Quaternion.identity, _unitsContainer);
            int levelAssociated = i + 1;
            unitGameObject.GetComponent<QuestUnit>().SetUp(questUnits[i], onUnitClicked, onUnitRevealed);
        }
    }

    private void onUnitRevealed(string unitName)
    {
        Debug.Log("Quest::onUnitRevealed -> Hit unit: " + unitName);
        if (!_revealedUnitNames.Contains(unitName))
        {
            Debug.Log("Quest::onUnitRevealed -> Unit added to revealed: " + unitName);
            _revealedUnitNames.Add(unitName);
        }
    }

    private void determineQuestTeam()
    {
        GameStats gameStats = GameStats.Instance;
        string selecteQuestString = gameStats.SelectedQuestString;

        if ((selecteQuestString == nameof(GlobalSystemQuests.EnjinLegend122)) || (selecteQuestString == nameof(GlobalSystemQuests.EnjinLegend123)) || (selecteQuestString == nameof(GlobalSystemQuests.EnjinLegend124))
            || (selecteQuestString == nameof(GlobalSystemQuests.EnjinLegend125)) || (selecteQuestString == nameof(GlobalSystemQuests.EnjinLegend126)))
        {
            gameStats.QuestUnits = new List<string>() { "122", "123", "124", "125", "126" };
        }
        else
        {
            Debug.LogError("There is no quest team for selected quest: " + selecteQuestString);
        }
    }

    private void refreshEnemyUnitsGrid()
    {
        GameInventory gameInventory = GameInventory.Instance;
        GameConfig gameConfig = GameConfig.Instance;

        List<string> unitNames = GameStats.Instance.QuestUnits;
        int unitsCount = unitNames.Count;

        string activeGlobalSystemQuestString = gameInventory.GetGlobalSystemActiveQuestString();

        for (int i = 0; i < unitsCount; i++)
        {
            Transform unitSlot = _enemyUnitsGridContent.Find("slot" + (i + 1));
            GameObject unitImageObject = unitSlot.Find("Sprite").gameObject;
            GameObject unitLevelObject = unitSlot.Find("level/txt_level").gameObject;

            if (gameInventory.GetQuestLevelCompleted(activeGlobalSystemQuestString, i + 1))
            {
                unitImageObject.SetActive(false);
                unitLevelObject.SetActive(false);
                continue;
            }

            string unitName = unitNames[i];

            int unitTier = gameInventory.GetUnitTier(unitName);
            unitSlot.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/unit_frame_t" + unitTier);

            unitImageObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
            unitLevelObject.GetComponent<Text>().text = gameInventory.GetUnitExpData(gameConfig.QuestUnitExp).Level.ToString();
        }
    }
}
