using Enigma.CoreSystems;
using GameEnums;
using GameConstants;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class QuestSelection : MonoBehaviour
{
    [SerializeField] GameObject _globalSystemQuestButton;

    [SerializeField] GameObject _narwhalBlueQuestButton;
    [SerializeField] GameObject _narwhalCheeseQuestButton;
    [SerializeField] GameObject _narwwhalEmeraldQuestButton;
    [SerializeField] GameObject _narwhalCrimsonQuestButton;

    [SerializeField] GameObject _shalwendWargodQuestButton; 
    [SerializeField] GameObject _shalwendDeadlyKnightQuestButton;

    [SerializeField] GameObject _questProgressPanel;

    [SerializeField] private Image _questProgressFill;
    [SerializeField] private Text _questProgressText;
    [SerializeField] private Text _questLeadersText;

    [SerializeField] private QuestConfirmPopUp _questConfirmPopUp;
    [SerializeField] BasicPopUp _loginForPvpPopUp;

    private const int _POINTS_FOR_QUEST = 1000;

    private void Start()
    {
        NetworkManager.Transaction(Transactions.GET_QUEST_DATA, onGetQuestData);
        //GameNetwork.CheckEnjinTokenAvailable()

        _questConfirmPopUp.Close(false);

        _questProgressPanel.SetActive(false);
        _globalSystemQuestButton.SetActive(false);

        setNonGlobalQuestButton(_shalwendWargodQuestButton, new List<string> { EnjinTokenKeys.QUEST_WARGOD_SHALWEND }, nameof(SerialQuests.ShalwendWargod), QuestTypes.Serial);
        setNonGlobalQuestButton(_shalwendDeadlyKnightQuestButton, new List<string> { EnjinTokenKeys.QUEST_DEADLY_KNIGHT_SHALWEND }, nameof(SerialQuests.ShalwendDeadlyKnight), QuestTypes.Serial);

        setNonGlobalQuestButton(_narwhalBlueQuestButton, new List<string> { EnjinCodeKeys.QUEST_BLUE_NARWHAL }, nameof(ScoutQuests.NarwhalBlue), QuestTypes.Scout);
        setNonGlobalQuestButton(_narwhalCheeseQuestButton, new List<string> { EnjinCodeKeys.QUEST_CHEESE_NARWHAL }, nameof(ScoutQuests.NarwhalCheese), QuestTypes.Scout);
        setNonGlobalQuestButton(_narwwhalEmeraldQuestButton, new List<string> { EnjinCodeKeys.QUEST_EMERALD_NARWHAL }, nameof(ScoutQuests.NarwhalEmerald), QuestTypes.Scout);
        setNonGlobalQuestButton(_narwhalCrimsonQuestButton, new List<string> { EnjinCodeKeys.QUEST_CRIMSON_NARWHAL }, nameof(ScoutQuests.NarwhalCrimson), QuestTypes.Scout);
    }

    public void OnSinglePlayerButtonDown()
    {
        print("OnSinglePlayerButtonDown");
        GameSounds.Instance.PlayUiAdvanceSound();
        GameStats.Instance.Mode = GameModes.SinglePlayer;
        goToLevels();
    }

    public void OnPvpButtonDown()
    {
        print("OnPvpButtonDown");
        GameSounds.Instance.PlayUiAdvanceSound();

        if (NetworkManager.LoggedIn)
        {
            GameStats.Instance.Mode = GameModes.Pvp;
            goToLevels();
        }
        else
        {
            _loginForPvpPopUp.Open();
        }
    }

    public void OnStoreButtonDown()
    {
        print("OnStoreButtonDown");
        GameSounds.Instance.PlayUiAdvanceSound();
        SceneManager.LoadScene(GameConstants.Scenes.STORE);
    }

    private void goToLevels()
    {
        SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
    }

    private void setNonGlobalQuestButton(GameObject button, List<string> unlockEnjinKeys, string questString, QuestTypes questType)
    {
        bool allEnjinQuestCodesAvailable = false;

        bool allowEnigmaTokenUnlock = true;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        GameHacks gameHacks = GameHacks.Instance;

        if (gameHacks.EnableAllEnjinCodeQuests)
        {
            allEnjinQuestCodesAvailable = true;
        }

        if (gameHacks.DisableEnigmaTokenQuestUnlocks)
        {
            allowEnigmaTokenUnlock = false;
        }
#endif

        if (allowEnigmaTokenUnlock)
        {
            unlockEnjinKeys.Add(EnigmaConstants.EnjinTokenKeys.ENIGMA_TOKEN); //Unlocks all non global quests
        }

        bool questUnlocked = false;
        foreach (string enjinKey in unlockEnjinKeys)
        {
            if (GameNetwork.Instance.GetIsEnjinKeyAvailable(enjinKey) == true)
            {
                questUnlocked = true;
                break;
            }
        }

        bool questCompleted = GameInventory.Instance.GetQuestCompleted(questString);

        button.GetComponent<Button>().onClick.AddListener(() => { onQuestButtonDown(allEnjinQuestCodesAvailable || questUnlocked, unlockEnjinKeys, questString, questType, false, questCompleted); });
        button.GetComponent<QuestGridItem>().Set(questCompleted);
    }

    private void onQuestButtonDown(bool questUnlocked, List<string> unlockEnjinKeys, string questString, QuestTypes questType, bool isGlobalSystem, bool isCompleted)
    {
        GameSounds.Instance.PlayUiAdvanceSound();

        string sceneToLoad = Scenes.LEVELS;

        if (questType == QuestTypes.Scout)
        {
            sceneToLoad = Scenes.SCOUT_QUEST;
        }

        _questConfirmPopUp.Open(questUnlocked, unlockEnjinKeys, questString, sceneToLoad, questType, isGlobalSystem, isCompleted);
    }

    public void OnBackButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        SceneManager.LoadScene(EnigmaConstants.Scenes.MAIN);
    }

    private void handleGlobalSystemQuestPanelAndButtonStates(int points)
    {
        GameInventory gameInventory = GameInventory.Instance;

        ScoutQuests globalSystemActiveQuest = gameInventory.GetGlobalSystemActiveQuest();
        string questString = globalSystemActiveQuest.ToString();
        Sprite rewardSprite = gameInventory.GetQuestRewardSprite(questString);

        _questProgressText.text = points.ToString() + " / " + _POINTS_FOR_QUEST.ToString();
        _questProgressFill.fillAmount = ((float)points) / _POINTS_FOR_QUEST;

        bool questAvailable = false;

        if (points >= _POINTS_FOR_QUEST)
        {
            questAvailable = true;
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

        _globalSystemQuestButton.transform.Find("QuestName").GetComponent<Text>().text = gameInventory.GetGlobalSystemActiveQuestName();

        if (rewardSprite != null)
        {
            _globalSystemQuestButton.transform.Find("icon").GetComponent<Image>().sprite = rewardSprite;
        }

        bool questCompleted = GameInventory.Instance.GetQuestCompleted(questString);
        _globalSystemQuestButton.GetComponent<Button>().onClick.AddListener(() => { onQuestButtonDown(questAvailable, null, GameInventory.Instance.GetGlobalSystemActiveQuestString(), QuestTypes.Scout, true, questCompleted); });
        _globalSystemQuestButton.GetComponent<QuestGridItem>().Set(questCompleted);
        _globalSystemQuestButton.SetActive(true);
    }

    //private void handleSwolesomeAvailability(bool hasCode)
    //{
    //    if (!hasCode)
    //    {
    //        return;
    //    }

    //    GameInventory gameInventory = GameInventory.Instance;

    //    //if (gameInventory.GetAllSwolesomeQuestLevelsCompleted())
    //    //{
    //    //    return;
    //    //}

    //    ScoutQuests swolesomeActiveQuest = gameInventory.GetSwolesomeActiveQuest();
    //    Sprite rewardSprite = gameInventory.GetQuestRewardSprite(swolesomeActiveQuest.ToString());

    //    _narwwhalEmeraldQuestButton.transform.Find("QuestName").GetComponent<Text>().text = gameInventory.GetSwolesomeActiveQuestName();

    //    if (rewardSprite != null)
    //    {
    //        _narwwhalEmeraldQuestButton.transform.Find("icon").GetComponent<Image>().sprite = rewardSprite;
    //    }

    //    _narwwhalEmeraldQuestButton.SetActive(true);
    //}

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

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    if (GameHacks.Instance.QuestsPoints.Enabled)
                    {
                        points = GameHacks.Instance.QuestsPoints.ValueAsInt;
                    }
#endif

                    handleGlobalSystemQuestPanelAndButtonStates(points);
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
    }
}
