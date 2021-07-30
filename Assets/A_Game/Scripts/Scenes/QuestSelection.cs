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

    [SerializeField] GameObject _swoleCheeseQuestButton;
    [SerializeField] GameObject _swoleEmeraldQuestButton;
    [SerializeField] GameObject _swoleCrimsonQuestButton;
    [SerializeField] GameObject _swoleDiamondQuestButton;

    [SerializeField] GameObject _shalwendQuestButton;
    [SerializeField] GameObject _questProgressPanel;

    [SerializeField] private Image _questProgressFill;
    [SerializeField] private Text _questProgressText;
    [SerializeField] private Text _questLeadersText;

    [SerializeField] private QuestConfirmPopUp _questConfirmPopUp;

    private const int _POINTS_FOR_QUEST = 1000;

    private void Start()
    {
        NetworkManager.Transaction(Transactions.GET_QUEST_DATA, onGetQuestData);
        //GameNetwork.CheckEnjinTokenAvailable()

        _questConfirmPopUp.Close(false);

        _questProgressPanel.SetActive(false);
        _globalSystemQuestButton.SetActive(false);
        _shalwendQuestButton.SetActive(false);

        bool shalwendQuestHackEnabled = false;

        GameHacks gameHacks = GameHacks.Instance;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (gameHacks.SetLegendUnitServerQuest.Enabled)
        {
            if (gameHacks.SetLegendUnitServerQuest.GetValueAsEnum<LegendUnitQuests>() == LegendUnitQuests.Shalwend)
            {
                shalwendQuestHackEnabled = true;
            }
        }
#endif

        if (shalwendQuestHackEnabled || !GameInventory.Instance.GetQuestCompleted(nameof(LegendUnitQuests.Shalwend)))
        {
            if (shalwendQuestHackEnabled || GameNetwork.Instance.GetIsTokenAvailable(EnjinTokenKeys.QUEST_SHALWEND))
            {
                _shalwendQuestButton.SetActive(true);
            }
        }

        handleSwolesomeQuestVisibility(_swoleCheeseQuestButton, EnjinTokenKeys.BLUE_NARWHAL, ScoutQuests.SwoleCheese130);
        handleSwolesomeQuestVisibility(_swoleEmeraldQuestButton, EnjinTokenKeys.CHEESE_NARWHAL, ScoutQuests.SwoleEmerald131);
        handleSwolesomeQuestVisibility(_swoleCrimsonQuestButton, EnjinTokenKeys.EMERALD_NARWHAL, ScoutQuests.SwoleCrimson132);
        handleSwolesomeQuestVisibility(_swoleDiamondQuestButton, EnjinTokenKeys.CRIMSON_NARWHAL, ScoutQuests.SwoleDiamond133);
    }

    private void handleSwolesomeQuestVisibility(GameObject button, string tokenKey, ScoutQuests swolesomeQuest)
    {
        bool tokenAvailable = GameNetwork.Instance.GetIsTokenAvailable(tokenKey);
        bool questCompleted = GameInventory.Instance.GetQuestCompleted(swolesomeQuest.ToString());

        button.SetActive(tokenAvailable && !questCompleted);
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

        ScoutQuests globalSystemActiveQuest = gameInventory.GetGlobalSystemActiveQuest();
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

    private void handleSwolesomeButtonVisibility(bool hasCode)
    {
        if (!hasCode)
        {
            return;
        }

        GameInventory gameInventory = GameInventory.Instance;

        if (gameInventory.GetAllSwolesomeQuestLevelsCompleted())
        {
            return;
        }

        ScoutQuests swolesomeActiveQuest = gameInventory.GetSwolesomeActiveQuest();
        Sprite rewardSprite = gameInventory.GetQuestRewardSprite(swolesomeActiveQuest.ToString());

        _swoleCrimsonQuestButton.transform.Find("QuestName").GetComponent<Text>().text = gameInventory.GetSwolesomeActiveQuestName();

        if (rewardSprite != null)
        {
            _swoleCrimsonQuestButton.transform.Find("icon").GetComponent<Image>().sprite = rewardSprite;
        }

        _swoleCrimsonQuestButton.SetActive(true);
    }

    private void onGetQuestData(JSONNode response)
    {
        GameHacks gameHacks = GameHacks.Instance;
        ScoutQuests hackedQuest = ScoutQuests.None;
        bool questIsHacked = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (gameHacks.SetGlobalSystemServerQuest.Enabled)
        {
            hackedQuest = gameHacks.SetGlobalSystemServerQuest.GetValueAsEnum<ScoutQuests>();
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
                ScoutQuests serverActiveQuest = ScoutQuests.None;

                if (questIsHacked)
                {
                    serverActiveQuest = hackedQuest;
                }
                else
                {
                    serverActiveQuest = (ScoutQuests)questNode.AsInt;
                }

                ScoutQuests savedActiveQuest = gameInventory.GetGlobalSystemActiveQuest();
                if (serverActiveQuest != savedActiveQuest)
                {
                    if (savedActiveQuest != ScoutQuests.None)
                    {
                        gameInventory.SetScoutQuestEnemiesPositions(savedActiveQuest, new List<Vector3>());
                        gameInventory.ClearQuestLevelsCompleted(savedActiveQuest);
                        gameInventory.ClearQuestScoutProgress(savedActiveQuest);
                    }

                    gameInventory.SetGlobalSystemActiveQuest(serverActiveQuest);
                }
            }

            ScoutQuests globalSystemActiveQuest = gameInventory.GetGlobalSystemActiveQuest();
            if (globalSystemActiveQuest != ScoutQuests.None)
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
        _questConfirmPopUp.Open(GameInventory.Instance.GetGlobalSystemActiveQuestString(), GameConstants.Scenes.SCOUT_QUEST, QuestTypes.Scout);
    }

    public void ShalwendQuestButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();
        _questConfirmPopUp.Open(nameof(LegendUnitQuests.Shalwend), GameConstants.Scenes.LEVELS, QuestTypes.Levels);
    }

    public void SwoleCheeseQuestButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();
        _questConfirmPopUp.Open(ScoutQuests.SwoleCheese130.ToString(), GameConstants.Scenes.SCOUT_QUEST, QuestTypes.Scout); ;
    }

    public void SwoleEmeraldQuestButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();
        _questConfirmPopUp.Open(ScoutQuests.SwoleEmerald131.ToString(), GameConstants.Scenes.SCOUT_QUEST, QuestTypes.Scout);
    }

    public void SwoleCrimsonQuestButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();
        _questConfirmPopUp.Open(ScoutQuests.SwoleCrimson132.ToString(), GameConstants.Scenes.SCOUT_QUEST, QuestTypes.Scout);
    }

    public void SwoleDiamonQuestButtonDown()
    {
        GameSounds.Instance.PlayUiAdvanceSound();
        _questConfirmPopUp.Open(ScoutQuests.SwoleDiamond133.ToString(), GameConstants.Scenes.SCOUT_QUEST, QuestTypes.Scout);
    }
}
