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
    [SerializeField] private GameObject _boostRewardTemplate;
    [SerializeField] private GameObject _boxRewardTemplate;

    private string _sceneToLoad = "";
    private string _questStringToConfirm = nameof(ScoutQuests.None);
    private QuestTypes _questTypeToConfirm = QuestTypes.None;

    private void Start()
    {
        prepareGridTemplate(_unitRewardTemplate);
        prepareGridTemplate(_boostRewardTemplate);
        prepareGridTemplate(_boxRewardTemplate);
    }

    private void prepareGridTemplate(GameObject gridTemplate)
    {
        gridTemplate.transform.SetParent(_rewardsGridContent.parent);
        gridTemplate.SetActive(false);
    }

    public void Open(string questStringToConfirm, string sceneToLoad, QuestTypes questTypeToConfirm)
    {
        _questStringToConfirm = questStringToConfirm;
        _sceneToLoad = sceneToLoad;
        _questTypeToConfirm = questTypeToConfirm;

        string unitName = getQuestUnitRewardName(questStringToConfirm); 
        if (unitName != "")
        {
            GameObject unitReward = Instantiate<GameObject>(_unitRewardTemplate, _rewardsGridContent);
            unitReward.GetComponent<UnitRewardGridItem>().Setup(unitName);
            setGuaranteed(unitReward, true);
            unitReward.SetActive(true);
        }

        addBoxReward(BoxTiers.GOLD, true);
        addBoxReward(BoxTiers.BRONZE, false);

        GameInventory gameInventory = GameInventory.Instance;
        int maxLevel = gameInventory.GetQuestMaxLevel(questStringToConfirm);
        int maxOreBonus = gameInventory.GetLevelMaxBonus(maxLevel);

        string[] boostCategories = gameInventory.BoostCategories;

        int count = boostCategories.Length;
        for (int i = 0; i < count; i++)
        {
            string boostCategory = boostCategories[i];
            if (maxOreBonus == OreBonuses.PERFECT_ORE)
            {
                addBoostReward(boostCategory, OreTiers.PERFECT, OreBonuses.PERFECT_ORE, false);
            }

            if (maxOreBonus >= OreBonuses.POLISHED_ORE_MIN)
            {
                addBoostReward(boostCategory, OreTiers.POLISHED, OreBonuses.POLISHED_ORE_MIN, false);
            }

            addBoostReward(boostCategory, OreTiers.RAW, OreBonuses.RAW_ORE_MIN, false);
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

    private void addBoxReward(int boxTier, bool guaranteed)
    {
        GameObject reward = Instantiate<GameObject>(_boxRewardTemplate, _rewardsGridContent);
        reward.GetComponent<BoxRewardGridItem>().SetUp(boxTier, false);
        setGuaranteed(reward, guaranteed);
        reward.SetActive(true);
    }

    private void addBoostReward(string category, string oreTier, int bonus, bool guaranteed)
    {
        GameObject boostReward = Instantiate<GameObject>(_boostRewardTemplate, _rewardsGridContent);
        BoostRewardGridItem boostRewardScript = boostReward.GetComponent<BoostRewardGridItem>();
        boostRewardScript.SetUp(category, bonus, false);
        boostRewardScript.SetTextForQuestReward(oreTier, category);
        setGuaranteed(boostReward, guaranteed);
        boostReward.SetActive(true);
    }

    private void setGuaranteed(GameObject reward, bool guaranteed)
    {
        reward.GetComponent<RewardChanceDisplay>().Set(guaranteed);
    }

    private string getQuestUnitRewardName(string questString)
    {
        string rewardUnitName = ""; 

        switch (questString)
        {
            case nameof(ScoutQuests.EnjinLegend122):
                rewardUnitName = "122";
                break;
            case nameof(ScoutQuests.EnjinLegend123):
                rewardUnitName = "123";
                break;
            case nameof(ScoutQuests.EnjinLegend124):
                rewardUnitName = "124";
                break;
            case nameof(ScoutQuests.EnjinLegend125):
                rewardUnitName = "125";
                break;
            case nameof(ScoutQuests.EnjinLegend126):
                rewardUnitName = "126";
                break;
            case nameof(SerialQuests.ShalwendWargod):
                rewardUnitName = "128";
                break;
            case nameof(SerialQuests.ShalwendDeadlyKnight):
                rewardUnitName = "134";
                break;
            case nameof(ScoutQuests.NarwhalBlue):
                rewardUnitName = "130";
                break;
            case nameof(ScoutQuests.NarwhalCheese):
                rewardUnitName = "131";
                break;
            case nameof(ScoutQuests.NarwhalEmerald):
                rewardUnitName = "132";
                break;
            case nameof(ScoutQuests.NarwhalCrimson):
                rewardUnitName = "133";
                break;
            default:
                rewardUnitName = "122";
                break;
        }

        return rewardUnitName;
    }
}
