using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamBoostSelection : EnigmaScene
{
    [SerializeField] private Transform _teamBoostGridContent;
    [SerializeField] private Transform _teamUnitsGridContent;

    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _backButton;

    [SerializeField] private BoostInfoPopUp _boostInfoPopUp;

    private TeamBoostGridItem _selectedGridItem;
    private TeamBoostItem _selectedTeamBoost; 

    void Start()
    {
        SoundManager.FadeCurrentSong(1f, () => {
            int level = Random.Range(1, 6);
            SoundManager.Stop();
            SoundManager.Play("level" + level, SoundManager.AudioTypes.Music, "", true);
        });

        _nextButton.onClick.AddListener(() => { onNextButtonDown(); });
        _backButton.onClick.AddListener(() => { onBackButtonDown(); });

        refreshTeamBoostItemsGrid();
        refreshTeamUnitsGrid();

        GameStats.Instance.TeamBoostSelected = null;

        if (GameHacks.Instance.ClearSavedTeamDataOnScenesEnter)
        {
            PlayerPrefs.DeleteKey(getTeamBoostKey());
        }

        loadTeamBoostSelection();
    }

    private void loadTeamBoostSelection()
    {
        string teamBoostString = PlayerPrefs.GetString(getTeamBoostKey(), "");

        if (teamBoostString == "")
        {
            return;
        }

        string[] teamBoostTerms = teamBoostString.Split('|');

        string boostName = teamBoostTerms[0];
        string category = teamBoostTerms[1];
        string bonus = teamBoostTerms[2];

        foreach (Transform child in _teamBoostGridContent)
        {
            if (!child.gameObject.activeSelf)
            {
                continue;
            }

            TeamBoostGridItem gridItem = child.GetComponent<TeamBoostGridItem>();
            TeamBoostItem boostItem = gridItem.BoostItem;

            if ((boostItem.Name == boostName) && (boostItem.Category == category) && (boostItem.Bonus.ToString() == bonus))
            {
                selectTeamBoost(gridItem);
                break;
            }
        }
    }

    private void saveTeamBoostSelection()
    {
        if (_selectedGridItem != null)
        {
            string teamBoostString = _selectedGridItem.name + "|" + _selectedGridItem.BoostItem.Category + "|" + _selectedGridItem.BoostItem.Bonus;
            PlayerPrefs.SetString(getTeamBoostKey(), teamBoostString);
            PlayerPrefs.Save();
        }
    }

    private string getTeamBoostKey()
    {
        return (GameStats.Instance.SelectedSaveSlot + "_" + GameStats.Instance.Mode.ToString() + "_teamBoost");
    }

    public void refreshTeamBoostItemsGrid()
    {
        GameObject teamBoostItemTemplate = _teamBoostGridContent.GetChild(0).gameObject;
        teamBoostItemTemplate.SetActive(true);

        foreach (Transform child in _teamBoostGridContent)
        {
            if (child.gameObject != teamBoostItemTemplate)
                Destroy(child.gameObject);
        }

        GameStats gameStats = GameStats.Instance;

        //gameStats.TeamBoostOreItemsOwnedByName.Clear();
        //gameStats.TeamBoostOreItemsOwnedByName.Add(GameConstants.TeamBoostEnjinOreItems.DAMAGE + "1", new TeamBoostItem(GameConstants.TeamBoostEnjinOreItems.DAMAGE + "1", 3, 8, GameConstants.TeamBoostCategory.DEFENSE));
        //gameStats.TeamBoostOreItemsOwnedByName.Add(GameConstants.TeamBoostEnjinOreItems.POWER + "1", new TeamBoostItem(GameConstants.TeamBoostEnjinOreItems.DAMAGE + "2", 2, 4, GameConstants.TeamBoostCategory.POWER));

        foreach (string teamBoostName in gameStats.TeamBoostTokensOwnedByName.Keys)
        {
            TeamBoostItem tokenItem = gameStats.TeamBoostTokensOwnedByName[teamBoostName];
            addBoostItem(teamBoostItemTemplate, tokenItem, false);
        }

        List<TeamBoostItem> oreItems = GameInventory.Instance.GetOreItemsOwned();
        foreach (TeamBoostItem boostItem in oreItems)
        {
            addBoostItem(teamBoostItemTemplate, boostItem, true);
        }

        teamBoostItemTemplate.SetActive(false);
    }

    private void addBoostItem(GameObject teamBoostItemTemplate, TeamBoostItem boostItem, bool isOre)
    {
        for (int i = 0; i < boostItem.Amount; i++)
        {
            GameObject itemObject = Instantiate<GameObject>(teamBoostItemTemplate, _teamBoostGridContent);
            Transform itemTransform = itemObject.transform;

            TeamBoostGridItem gridItem = itemObject.GetComponent<TeamBoostGridItem>();
            gridItem.SetUp(boostItem, isOre);

            gridItem.GetComponent<Button>().onClick.AddListener(() => onTeamBoostItemClicked(gridItem));

            break; //To display only the first one of each type
        }
    }

    private void onTeamBoostItemClicked(TeamBoostGridItem clickedGridItem)
    {
        foreach (Transform child in _teamBoostGridContent)
        {
            TeamBoostGridItem gridItem = child.GetComponent<TeamBoostGridItem>();

            if (gridItem == clickedGridItem)
            {
                if (_selectedGridItem == clickedGridItem)
                {
                    SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
                    gridItem.Deselect();
                    _boostInfoPopUp.Close();

                    GameStats.Instance.TeamBoostSelected = null;
                    _selectedGridItem = null;
                }
                else
                {
                    SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);
                    selectTeamBoost(clickedGridItem);
                }
            }
            else
            {
                gridItem.Deselect();
            }
        }


    }

    private void selectTeamBoost(TeamBoostGridItem gridItem)
    {
        gridItem.Select();
        TeamBoostItem boostItem = gridItem.BoostItem;

        _boostInfoPopUp.Open(boostItem.Name, boostItem.Category, boostItem.Bonus);

        GameStats.Instance.TeamBoostSelected = gridItem.BoostItem;
        _selectedGridItem = gridItem;
    }

    private void refreshTeamUnitsGrid()
    {
        List<string> unitNames = GameStats.Instance.TeamUnits;
        int unitsCount = unitNames.Count;

        for (int i = 0; i < unitsCount; i++)
        {
            string unitName = unitNames[i];
            if (unitName == "-1")
                continue;

            //minMinObj.GetComponentInChildren<PolygonCollider2D>().isTrigger = true;

            Transform unitSlot = _teamUnitsGridContent.Find("slot" + (i + 1));

            int unitTier = GameInventory.Instance.GetUnitTier(unitName);
            unitSlot.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/unit_frame_t" + unitTier);

            unitSlot.Find("Sprite").GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
            unitSlot.Find("level/txt_level").GetComponent<Text>().text = GameInventory.Instance.GetLocalUnitLevel(unitName).ToString();
        }
    }

    private void onNextButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);
        saveTeamBoostSelection();
        SceneManager.LoadScene(GameConstants.Scenes.WAR_PREP);
    }

    private void onBackButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);
    }
}
