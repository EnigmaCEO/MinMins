using Enigma.CoreSystems;
using GameEnums;
using GameConstants;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestSelection : MonoBehaviour
{
    [SerializeField] GameObject _globalSystemQuestButton;
    [SerializeField] GameObject _shalwendQuestButton;
    [SerializeField] GameObject _questProgressPanel;

    [SerializeField] private Image _questProgressFill;
    [SerializeField] private Text _questProgressText;
    [SerializeField] private Text _questLeadersText;

    [SerializeField] private QuestConfirmPopUp _questConfirmPopUp;

    private const int _POINTS_FOR_QUEST = 1000;

    private void Start()
    {
        NetworkManager.Transaction(GameNetwork.Transactions.GET_QUEST_DATA, onGetQuestData);
        //GameNetwork.CheckEnjinTokenAvailable()

        _questConfirmPopUp.Close();

        _questProgressPanel.SetActive(false);
        _globalSystemQuestButton.SetActive(false);

        _shalwendQuestButton.SetActive(GameNetwork.Instance.GetIsTokenAvailable(EnjinTokenKeys.QUEST_SHALWEND));
    }

    public void OnBackButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        SceneManager.LoadScene(EnigmaConstants.Scenes.MAIN);
    }

    private void handleGlobalSystemQuestPanelAndButtonVisibility(int points)
    {
        GameInventory gameInventory = GameInventory.Instance;

        if (gameInventory.GetAllGlobalSystemQuestLevelsCompleted())
        {
            return;
        }

        GlobalSystemQuests globalSystemActiveQuest = gameInventory.GetGlobalSystemActiveQuest();
        Sprite rewardSprite = gameInventory.GetQuestRewardSprite(globalSystemActiveQuest.ToString());

        _questProgressText.text = points.ToString() + " / " + _POINTS_FOR_QUEST.ToString();
        _questProgressFill.fillAmount = ((float)points) / _POINTS_FOR_QUEST;

        if (points >= _POINTS_FOR_QUEST)
        {
            _globalSystemQuestButton.transform.Find("QuestName").GetComponent<Text>().text = gameInventory.GetGlobalSystemActiveQuestName();

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
        GlobalSystemQuests hackedQuest = GlobalSystemQuests.None;
        bool questIsHacked = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (gameHacks.SetServerQuest.Enabled)
        {
            hackedQuest = gameHacks.SetServerQuest.GetValueAsEnum<GlobalSystemQuests>();
            questIsHacked = true;
        }
#endif

        if (questIsHacked || !NetworkManager.CheckInvalidServerResponse(response, nameof(onGetQuestData)))
        {
            GameStats gameStats = GameStats.Instance;
            GameInventory gameInventory = GameInventory.Instance;

            JSONNode questNode = null;
            JSONNode leadersNode = null;

            if (!questIsHacked)
            {
                questNode = NetworkManager.CheckValidNode(response[0], GameNetwork.TransactionKeys.QUEST);
            }

            if (questIsHacked || (questNode != null))
            {
                GlobalSystemQuests serverActiveQuest = GlobalSystemQuests.None;

                if (questIsHacked)
                {
                    serverActiveQuest = hackedQuest;
                }
                else
                {
                    serverActiveQuest = (GlobalSystemQuests)questNode.AsInt;
                }

                GlobalSystemQuests savedActiveQuest = gameInventory.GetGlobalSystemActiveQuest();
                if (serverActiveQuest != savedActiveQuest)
                {
                    if (savedActiveQuest != GlobalSystemQuests.None)
                    {
                        gameInventory.SetGlobalSystemQuestEnemiesPositions(new List<Vector3>());
                        gameInventory.ClearQuestLevelsCompleted(savedActiveQuest.ToString());
                        gameInventory.ClearGlobalSystemScoutProgress();
                    }

                    gameInventory.SetGlobalSystemActiveQuest(serverActiveQuest);
                }
            }

            GlobalSystemQuests globalSystemActiveQuest = gameInventory.GetGlobalSystemActiveQuest();
            if (globalSystemActiveQuest != GlobalSystemQuests.None)
            {
                JSONNode progressNode = null;

                if (!questIsHacked)
                {
                    progressNode = NetworkManager.CheckValidNode(response[0], GameNetwork.TransactionKeys.PROGRESS);
                    leadersNode = NetworkManager.CheckValidNode(response[0], GameNetwork.TransactionKeys.LEADERS);
                }

                if (questIsHacked || (progressNode != null))
                {
                    int points = 0;

                    if (!questIsHacked)
                    {
                        points = progressNode.AsInt;
                    }

                    handleGlobalSystemQuestPanelAndButtonVisibility(points);
                }

                if (leadersNode != null)
                {
                    foreach (JSONNode leaders in leadersNode.AsArray)
                    {
                        if (leaders["name"].ToString().Trim('"') != "null")
                        {
                            _questLeadersText.text += leaders["name"].ToString().Trim('"') + " - " + leaders["points"].ToString().Trim('"') + "\n";
                        }
                    }
                }
            }
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.QuestsPoints.Enabled)
        {
            int points = GameHacks.Instance.QuestsPoints.ValueAsInt;
            handleGlobalSystemQuestPanelAndButtonVisibility(points);
        }
#endif
    }

    public void GlobalSystemButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();
        _questConfirmPopUp.Open(GameInventory.Instance.GetGlobalSystemActiveQuest().ToString(), GameConstants.Scenes.GLOBAL_SYSTEM_QUEST, null, null);
    }

    public void ShalwendQuestButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();
        _questConfirmPopUp.Open(nameof(LegendUnitQuests.Shalwend), GameConstants.Scenes.LEVELS, null, null);
    }
}
