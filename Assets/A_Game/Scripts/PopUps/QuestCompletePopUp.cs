using GameEnums;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestCompletePopUp : MonoBehaviour
{
    [SerializeField] private Text _questName;
    [SerializeField] private Text _messageReward;
    [SerializeField] private Image _questIcon;

    private MatchResultsPopUp _resultsPopUp;

    private void Start()
    {
        Close();
    }

    public void OnOkButtonDown()
    {
        _resultsPopUp.Open(true);
        Close();
    }

    public void Open(MatchResultsPopUp resultsPopUp)
    {
        _resultsPopUp = resultsPopUp;

        string selectedQuestString = GameStats.Instance.SelectedQuestString;

        _questName.text = GameInventory.Instance.GetQuestDisplayName(selectedQuestString);

        fillQuestRewardMessage(selectedQuestString);
        handleRewardImage(selectedQuestString);

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void fillQuestRewardMessage(string questString)
    {
        //if ((questString == nameof(ScoutQuests.EnjinLegend122)) || (questString == nameof(ScoutQuests.EnjinLegend123)) || (questString == nameof(ScoutQuests.EnjinLegend124))
        //|| (questString == nameof(ScoutQuests.EnjinLegend125)) || (questString == nameof(ScoutQuests.EnjinLegend126)) || (questString == nameof(LegendUnitQuests.Shalwend)))
        {
            _messageReward.text = LocalizationManager.GetTermTranslation("You got this new unit in your inventory or experience bonus for it.");
        }
        //else
        //{
        //    Debug.LogError("There is not message set for Quest: " + questString.ToString());
        //}
    }

    private void handleRewardImage(string questString)
    {
        Sprite questSprite = GameInventory.Instance.GetQuestRewardSprite(questString);

        if (questSprite != null)
        {
            _questIcon.sprite = questSprite;
        }
    }
}
