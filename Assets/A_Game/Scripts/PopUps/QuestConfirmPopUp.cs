using CodeStage.AntiCheat.Storage;
using Enigma.CoreSystems;
using GameConstants;
using GameEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestConfirmPopUp : MonoBehaviour
{
    [SerializeField] private Transform _rewardsGridContent;

    [SerializeField] private GameObject _unitRewardTemplate;
    [SerializeField] private GameObject _boostRewardTemplate;
    [SerializeField] private GameObject _boxRewardTemplate;

    [SerializeField] private Text _messageText;
    [SerializeField] private GameObject _confirmButton;

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

    public void Open(bool questAvailable, string questStringToConfirm, string sceneToLoad, QuestTypes questTypeToConfirm, bool isGlobalSystem, bool questCompleted)
    {
        _questStringToConfirm = questStringToConfirm;
        _sceneToLoad = sceneToLoad;
        _questTypeToConfirm = questTypeToConfirm;

        int guaranteedOdds = RewardsChances.GUARANTEED_ODDS;

        string unitName = getQuestUnitRewardName(questStringToConfirm); 
        if (unitName != "")
        {
            GameObject unitReward = Instantiate<GameObject>(_unitRewardTemplate, _rewardsGridContent);
            unitReward.GetComponent<UnitRewardGridItem>().Setup(unitName);
            setOdds(unitReward, guaranteedOdds);
            unitReward.SetActive(true);
        }

        addBoxReward(BoxTiers.GOLD, guaranteedOdds);
        addBoxReward(BoxTiers.BRONZE, guaranteedOdds);

        GameInventory gameInventory = GameInventory.Instance;

        int maxLevel = gameInventory.GetQuestMaxLevel(questStringToConfirm);

        float maxOreBonusAtMaxLevel = (float)gameInventory.GetLevelMaxBonus(maxLevel);
        float minOreBonusAtMaxLevel = (float)gameInventory.GetLevelMinBonus(maxLevel);
        
        float oreProbability = (float)RewardsChances.ORE_ODDS/(float)RewardsChances.GUARANTEED_ODDS;

        string[] boostCategories = gameInventory.BoostCategories;

        float perfectOreProbability = getOreTierProbability(maxOreBonusAtMaxLevel, minOreBonusAtMaxLevel, (float)OreBonuses.PERFECT_ORE_MIN);
        float finalPerfectOreProbability = oreProbability * perfectOreProbability;

        float polishedOreProbability = getOreTierProbability(maxOreBonusAtMaxLevel, minOreBonusAtMaxLevel, (float)OreBonuses.POLISHED_ORE_MIN);
        float finalPolishedOreProbability = oreProbability * polishedOreProbability;

        int minLevel = 1;
        float maxOreBonusAtMinLevel = (float)gameInventory.GetLevelMaxBonus(minLevel);
        float minOreBonusAtMinLevel = (float)gameInventory.GetLevelMinBonus(minLevel);
        float rawOreProbability = getOreTierProbability(maxOreBonusAtMinLevel, minOreBonusAtMinLevel, (float)OreBonuses.RAW_ORE_MIN);
        float finalRawOreProbability = rawOreProbability * oreProbability;

        int count = boostCategories.Length;
        for (int i = 0; i < count; i++)
        {
            string boostCategory = boostCategories[i];
            if (finalPerfectOreProbability > 0)
            {
                addBoostReward(boostCategory, OreTiers.PERFECT, OreBonuses.PERFECT_ORE_MIN, getOddFromProbability(finalPerfectOreProbability));
            }

            if (finalPolishedOreProbability > 0)
            {
                addBoostReward(boostCategory, OreTiers.POLISHED, OreBonuses.POLISHED_ORE_MIN, getOddFromProbability(finalPolishedOreProbability));
            }

            addBoostReward(boostCategory, OreTiers.RAW, OreBonuses.RAW_ORE_MIN, getOddFromProbability(finalRawOreProbability));
        }

        if (questCompleted)
        {
            _messageText.text = LocalizationManager.GetTermTranslation(LocalizationTerms.QUEST_COMPLETED);
        }
        else if (!questAvailable)
        {
            if (isGlobalSystem)
            {
                _messageText.text = LocalizationManager.GetTermTranslation(LocalizationTerms.REQUIRES_ENOUGH_GLOBAL_SYSTEM_QUEST_POINTS);
            }
            else
            {
                _messageText.text = LocalizationManager.GetTermTranslation(LocalizationTerms.REQUIRES_TOKEN) + ": " + gameInventory.GetQuestName(questStringToConfirm);
            }
        }

        _messageText.gameObject.SetActive(!questAvailable || questCompleted);
        _confirmButton.SetActive(questAvailable && !questCompleted);
        gameObject.SetActive(true);
    }

    private int getOddFromProbability(float probability)
    {
        return Mathf.FloorToInt(probability * (float)RewardsChances.GUARANTEED_ODDS);
    }

    private float getOreTierProbability(float maxOreBonus, float minOreBonus, float minRequiredBonus)
    {
        float oreTierProbabilityNumerator = maxOreBonus - minRequiredBonus + 1.0f;
        if (oreTierProbabilityNumerator < 0)
        {
            oreTierProbabilityNumerator = 0;
        }

        float oreTierProbabilityDenominator = maxOreBonus - minOreBonus + 1.0f;

        float oreTierProbability = oreTierProbabilityNumerator / oreTierProbabilityDenominator;
        
        return oreTierProbability;
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

    private void addBoxReward(int boxTier, int odds)
    {
        GameObject reward = Instantiate<GameObject>(_boxRewardTemplate, _rewardsGridContent);
        reward.GetComponent<BoxRewardGridItem>().SetUp(boxTier, false);
        setOdds(reward, odds);
        reward.SetActive(true);
    }

    private void addBoostReward(string category, string oreTier, int bonus, int odds)
    {
        GameObject boostReward = Instantiate<GameObject>(_boostRewardTemplate, _rewardsGridContent);
        BoostRewardGridItem boostRewardScript = boostReward.GetComponent<BoostRewardGridItem>();
        boostRewardScript.SetUp(category, bonus, false);
        boostRewardScript.SetTextForQuestReward(oreTier, category);
        setOdds(boostReward, odds);
        boostReward.SetActive(true);
    }

    private void setOdds(GameObject reward, int odds)
    {
        reward.GetComponent<RewardChanceDisplay>().Set(odds);
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
