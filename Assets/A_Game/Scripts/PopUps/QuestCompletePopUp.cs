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

    private Dictionary<Quests, string> _messagesTermPerQuest = new Dictionary<Quests, string>();
    private MatchResultsPopUp _resultsPopUp;

    private void Start()
    {
        Close();
    }

    public void OnOkButtonDown()
    {
        _resultsPopUp.Open();
        Close();
    }

    public void Open(MatchResultsPopUp resultsPopUp)
    {
        _resultsPopUp = resultsPopUp;

        GameInventory gameInventory = GameInventory.Instance;
        Quests activeQuest = gameInventory.GetActiveQuest(); 

        _questName.text = gameInventory.GetQuestName(activeQuest);

        fillQuestRewardMessage(activeQuest);
        handleRewardImage(activeQuest);

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void fillQuestRewardMessage(Quests quest)
    {
        //_messagesTermPerQuest.Add(Quests.Swissborg, "Swissborg Reward");

        if ((quest == Quests.EnjinLegend122) || (quest == Quests.EnjinLegend123) || (quest == Quests.EnjinLegend124)
        || (quest == Quests.EnjinLegend125) || (quest == Quests.EnjinLegend126))
        {
            _messageReward.text = LocalizationManager.GetTermTranslation("You got this new unit in your inventory or experience bonus for it.");
        }
        else
        {
            Debug.LogError("There is not message set for Quest: " + quest.ToString());
        }
    }

    private void handleRewardImage(Quests activeQuest)
    {
        Sprite questSprite = GameInventory.Instance.GetQuestRewardSprite(activeQuest);

        if (questSprite != null)
        {
            _questIcon.sprite = questSprite;
        }
    }
}
