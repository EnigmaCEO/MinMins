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
        _nextButton.onClick.AddListener(() => { onNextButtonDown(); });
        _backButton.onClick.AddListener(() => { onBackButtonDown(); });

        refreshTeamBoostItemsGrid();
        refreshTeamUnitsGrid();

        GameStats.Instance.TeamBoostSelected = null;
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

        //addTeamBoostDictionaryToGrid(teamBoostItemTemplate, gameStats.TeamBoostTokensOwnedByName);
        //addTeamBoostDictionaryToGrid(teamBoostItemTemplate, gameStats.TeamBoostOreItemsOwnedByName);

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
            gridItem.SetUp(boostItem.Name, boostItem.Category, boostItem.Bonus, isOre);

            gridItem.GetComponent<Button>().onClick.AddListener(() => onTeamBoostItemClicked(boostItem, gridItem));

            break; //To display only the first one of each type
        }
    }

    //private void addTeamBoostDictionaryToGrid(GameObject teamBoostItemTemplate, Dictionary<string, TeamBoostItem> teamBoostItemsByName)
    //{
    //    foreach (string teamBoostName in teamBoostItemsByName.Keys)
    //    {
    //        TeamBoostItem itemToken = teamBoostItemsByName[teamBoostName];

    //        for (int i = 0; i < itemToken.Amount; i++)
    //        {
    //            GameObject itemObject = Instantiate<GameObject>(teamBoostItemTemplate, _teamBoostGridContent);
    //            Transform itemTransform = itemObject.transform;

    //            TeamBoostGridItem gridItem = itemObject.GetComponent<TeamBoostGridItem>();
    //            gridItem.SetUp(teamBoostName, itemToken.Category, itemToken.Bonus);

    //            gridItem.GetComponent<Button>().onClick.AddListener(() => onTeamBoostItemClicked(itemToken, gridItem));
    //        }
    //    }
    //}

    private void onTeamBoostItemClicked(TeamBoostItem selectedItem, TeamBoostGridItem clickedGridItem)
    {
        foreach (Transform child in _teamBoostGridContent)
        {
            TeamBoostGridItem gridItem = child.GetComponent<TeamBoostGridItem>();

            if (gridItem == clickedGridItem)
            {
                if (_selectedGridItem == clickedGridItem)
                {
                    gridItem.Deselect();
                    _boostInfoPopUp.Close();

                    GameStats.Instance.TeamBoostSelected = null;
                    _selectedGridItem = null;
                }
                else
                {
                    gridItem.Select();
                    _boostInfoPopUp.Open(selectedItem.Name, selectedItem.Category, selectedItem.Bonus);

                    GameStats.Instance.TeamBoostSelected = selectedItem;
                    _selectedGridItem = clickedGridItem;
                }
            }
            else
            {
                gridItem.Deselect();
            }
        }


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
        SceneManager.LoadScene(GameConstants.Scenes.WAR_PREP);
    }

    private void onBackButtonDown()
    {
        SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);
    }
}
