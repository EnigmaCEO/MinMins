using Enigma.CoreSystems;
using GameEnums;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestSelection : MonoBehaviour
{
    [SerializeField] GameObject _globalSystemQuestButton;
    [SerializeField] GameObject _questProgressPanel;

    [SerializeField] private Image _questProgressFill;
    [SerializeField] private Text _questProgressText;


    private const int _POINTS_FOR_QUEST = 1000;

    private void Start()
    {
        NetworkManager.Transaction(GameNetwork.Transactions.GET_QUEST_DATA, onGetQuestData);

        _questProgressPanel.SetActive(false);
        _globalSystemQuestButton.SetActive(false);
    }

    private void handleQuestPanelOrButtonVisibility(int points)
    {
        GameInventory gameInventory = GameInventory.Instance;

        if (gameInventory.GetAllQuestLevelsCompleted())
        {
            return;
        }

        Quests activeQuest = gameInventory.GetActiveQuest();
        Sprite rewardSprite = gameInventory.GetQuestRewardSprite(activeQuest);

        _questProgressText.text = points.ToString() + " / " + _POINTS_FOR_QUEST.ToString();
        _questProgressFill.fillAmount = ((float)points) / _POINTS_FOR_QUEST;

        if (points >= _POINTS_FOR_QUEST)
        {
            _globalSystemQuestButton.transform.Find("QuestName").GetComponent<Text>().text = gameInventory.GetActiveQuestName();

            if (rewardSprite != null)
            {
                _globalSystemQuestButton.transform.Find("icon").GetComponent<Image>().sprite = rewardSprite;
            }

            _globalSystemQuestButton.SetActive(true);
            _questProgressPanel.SetActive(false);
        }
        else
        {
            if (rewardSprite != null)
            {
                _questProgressPanel.transform.Find("Prize").GetComponent<Image>().sprite = rewardSprite;
            }

            _questProgressPanel.SetActive(true);
        }
    }

    private void onGetQuestData(JSONNode response)
    {
        GameHacks gameHacks = GameHacks.Instance;
        Quests hackedQuest = Quests.None;
        bool questIsHacked = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (gameHacks.SetServerQuest.Enabled)
        {
            hackedQuest = gameHacks.SetServerQuest.GetValueAsEnum<Quests>();
            questIsHacked = true;
        }
#endif

        if (questIsHacked || !NetworkManager.CheckInvalidServerResponse(response, nameof(onGetQuestData)))
        {
            GameStats gameStats = GameStats.Instance;
            GameInventory gameInventory = GameInventory.Instance;

            JSONNode questNode = null;

            if (!questIsHacked)
            {
                questNode = NetworkManager.CheckValidNode(response[0], GameNetwork.TransactionKeys.QUEST);
            }

            if (questIsHacked || (questNode != null))
            {
                Quests serverActiveQuest = Quests.None;

                if (questIsHacked)
                {
                    serverActiveQuest = hackedQuest;
                }
                else
                {
                    serverActiveQuest = (Quests)questNode.AsInt;
                }

                Quests savedActiveQuest = gameInventory.GetActiveQuest();
                if (serverActiveQuest != savedActiveQuest)
                {
                    if (savedActiveQuest != Quests.None)
                    {
                        gameInventory.SetQuestEnemiesPositions(new List<Vector3>());
                        gameInventory.ClearQuestLevelsCompleted(savedActiveQuest.ToString());
                        gameInventory.ClearScoutProgress();
                    }

                    gameInventory.SetActiveQuest(serverActiveQuest);
                }
            }

            Quests activeQuest = gameInventory.GetActiveQuest();
            if (activeQuest != Quests.None)
            {
                JSONNode progressNode = null;

                if (!questIsHacked)
                {
                    progressNode = NetworkManager.CheckValidNode(response[0], GameNetwork.TransactionKeys.PROGRESS);
                }

                if (questIsHacked || (progressNode != null))
                {
                    int points = 0;

                    if (!questIsHacked)
                    {
                        points = progressNode.AsInt;
                    }

                    handleQuestPanelOrButtonVisibility(points);
                }
            }
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.QuestsPoints.Enabled)
        {
            int points = GameHacks.Instance.QuestsPoints.ValueAsInt;
            handleQuestPanelOrButtonVisibility(points);
        }
#endif
    }

    public void GlobalSystemButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);

        GameStats.Instance.Mode = GameStats.Modes.Quest;
        SceneManager.LoadScene(GameConstants.Scenes.GLOBAL_SYSTEM_QUEST);
    }
}
