using Enigma.CoreSystems;
using GameConstants;
using GameEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestConfirmPopUp : MonoBehaviour
{
    [SerializeField] private Transform _rewardsGridContent;

    [SerializeField] private GameObject _unitRewardTemplate;
    [SerializeField] private GameObject _teamBoostRewardTemplate;
    [SerializeField] private GameObject _boxRewardTemplate;

    private string _sceneToLoad = "";
    private string _questStringToConfirm = nameof(ScoutQuests.None);
    private QuestTypes _questTypeToConfirm = QuestTypes.None;

    private void Start()
    {
        prepareGridTemplate(_unitRewardTemplate);
        prepareGridTemplate(_teamBoostRewardTemplate);
        prepareGridTemplate(_boxRewardTemplate);
    }

    private void prepareGridTemplate(GameObject gridTemplate)
    {
        gridTemplate.transform.SetParent(_rewardsGridContent.parent);
        gridTemplate.SetActive(false);
    }

    public void Open(string questStringToConfirm, string sceneToLoad, QuestTypes questTypeToConfirm /*, Dictionary<int, int> boxTiersWithAmountRewards, List<TeamBoostItemGroup> boostItemGroups*/)
    {
        _questStringToConfirm = questStringToConfirm;
        _sceneToLoad = sceneToLoad;
        _questTypeToConfirm = questTypeToConfirm;

        /*
        if (boxTiersWithAmountRewards != null)
        {
            foreach (KeyValuePair<int, int> amountByTier in boxTiersWithAmountRewards)
            {
                int count = amountByTier.Value;
                for (int i = 0; i < count; i++)
                {
                    GameObject boxReward = Instantiate<GameObject>(_boxRewardTemplate, _rewardsGridContent);
                    boxReward.SetActive(true);
                }
            }
        }

        if (boostItemGroups != null)
        {
            foreach (TeamBoostItemGroup boostItemGroup in boostItemGroups)
            {
                GameObject boostReward = Instantiate<GameObject>(_teamBoostRewardTemplate, _rewardsGridContent);
                boostReward.GetComponent<BoostRewardGridItem>().SetUp(boostItemGroup.Category, boostItemGroup.Bonus);
                boostReward.SetActive(true);
            }
        }
        */

        List<string> unitNames = getRewardUnitsNames(questStringToConfirm); 
        if (unitNames != null)
        {
            foreach (string unitName in unitNames)
            {
                GameObject unitReward = Instantiate<GameObject>(_unitRewardTemplate, _rewardsGridContent);
                unitReward.GetComponent<UnitRewardGridItem>().Setup(unitName);
                unitReward.SetActive(true);
            }
        }

        gameObject.SetActive(true);
    }

    public void Close(bool destroyChildren)
    {
        if (destroyChildren)
        {
            _rewardsGridContent.DestroyChildren();
        }

        gameObject.SetActive(false);
    }

    public void OnOkButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();

        GameStats gameStats = GameStats.Instance;

        gameStats.Mode = GameModes.Quest;
        gameStats.SelectedQuestString = _questStringToConfirm;
        gameStats.QuestType = _questTypeToConfirm;

        SceneManager.LoadScene(_sceneToLoad);
    }

    public void OnCancelButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        Close(true);
    }

    private List<string> getRewardUnitsNames(string questString)
    {
        List<string> rewardUnitsNames = new List<string>(); ;

        switch (questString)
        {
            case nameof(ScoutQuests.EnjinLegend122):
                rewardUnitsNames.Add("122");
                break;
            case nameof(ScoutQuests.EnjinLegend123):
                rewardUnitsNames.Add("123");
                break;
            case nameof(ScoutQuests.EnjinLegend124):
                rewardUnitsNames.Add("124");
                break;
            case nameof(ScoutQuests.EnjinLegend125):
                rewardUnitsNames.Add("125");
                break;
            case nameof(ScoutQuests.EnjinLegend126):
                rewardUnitsNames.Add("126");
                break;
            case nameof(LegendUnitQuests.Shalwend):
                rewardUnitsNames.Add("128");
                break;
            case nameof(ScoutQuests.SwoleCheese130):
                rewardUnitsNames.Add("130");
                break;
            case nameof(ScoutQuests.SwoleEmerald131):
                rewardUnitsNames.Add("131");
                break;
            case nameof(ScoutQuests.SwoleCrimson132):
                rewardUnitsNames.Add("132");
                break;
            case nameof(ScoutQuests.SwoleDiamond133):
                rewardUnitsNames.Add("133");
                break;
            default:
                rewardUnitsNames.Add("122");
                break;
        }

        return rewardUnitsNames;
    }
}
