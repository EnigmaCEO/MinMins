using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Levels : EnigmaScene
{
    [SerializeField] private Transform _levelsGridContent;
    [SerializeField] GameObject _notEnoughUnitsPopUp;


    void Start()
    {
        SoundManager.FadeCurrentSong(1f, () => {
            int level = Random.Range(1, 6);
            SoundManager.Stop();
            SoundManager.Play("level" + level, SoundManager.AudioTypes.Music, "", true);
        });
        
        _notEnoughUnitsPopUp.SetActive(false);

        GameStats gameStats = GameStats.Instance;
        GameInventory gameInventory = GameInventory.Instance;
        GameHacks gameHacks = GameHacks.Instance;

        if (gameHacks.Rating.Enabled)
        {
            gameStats.Rating = GameHacks.Instance.Rating.ValueAsInt;
        }

        GameObject levelGridItemTemplate = _levelsGridContent.GetChild(0).gameObject;
        GameStats.Modes mode = gameStats.Mode;

        int levelsLenght = 0;
        if (mode == GameStats.Modes.SinglePlayer)
        {
            levelsLenght = gameInventory.GetHigherSinglePlayerLevelCompleted() + 1;
            int arenaMaxLevel = gameInventory.GetSinglePlayerMaxLevel();

            if (levelsLenght > arenaMaxLevel)
            {
                levelsLenght = arenaMaxLevel;
            }
        }
        else if (mode == GameStats.Modes.Pvp)
        {
            levelsLenght = GameNetwork.Instance.GetLocalPlayerPvpLevelNumber();
        }
        else if (mode == GameStats.Modes.Quest)
        {
            levelsLenght = GameInventory.Instance.GetHighestQuestLevelCompleted() + 1;
            int questMaxLevel = gameInventory.GetActiveQuestMaxLevel();

            if (levelsLenght > questMaxLevel)
            {
                levelsLenght = questMaxLevel;
            }
        }

        if (gameHacks.UnlockArenas.Enabled)
        {
            levelsLenght = GameHacks.Instance.UnlockArenas.ValueAsInt;
        }

        Debug.LogWarning(">Levels::Start -> Levels lenght: " + levelsLenght);
        GameEnums.Quests activeQuest = GameStats.Instance.ActiveQuest;
        
        int activeQuestLastLevel = GameInventory.Instance.GetActiveQuestMaxLevel();

        for (int i = 0; i < levelsLenght; i++)
        {
            GameObject levelGameObject = Instantiate<GameObject>(levelGridItemTemplate, _levelsGridContent);
            int levelNumber = i + 1;
            levelGameObject.name = "Level" + levelNumber;

            LevelGridItem levelGridItem = levelGameObject.GetComponent<LevelGridItem>();
            levelGridItem.SetLabel(levelNumber.ToString());
            levelGridItem.FightButton.onClick.AddListener(() => { onLevelFightButtonDown(levelNumber); });

            if ((i + 1) == activeQuestLastLevel) // if level number equals max level
            {
                if (mode == GameStats.Modes.Quest)
                {
                    Sprite questIcon = (Sprite)Resources.Load<Sprite>("Images/QuestIcons/" + activeQuest.ToString());

                    if (questIcon != null)
                    {
                        levelGridItem.SetImageSprite(questIcon);
                    }
                }
            }

            if (mode == GameStats.Modes.Quest)
            {
                levelGridItem.enjinReward.SetActive(false);
            }
            else
            {
                if (GameNetwork.Instance.rewardedTrainingLevels[i] == 1)
                {
                    Debug.Log("Rewarded Training Enjin " + i);
                    levelGridItem.enjinReward.SetActive(true);
                }
                else
                {
                    levelGridItem.enjinReward.SetActive(false);
                }
            }
        }

        levelGridItemTemplate.SetActive(false);
        GameObject.Find("LevelsGrid").GetComponent<ScrollRect>().horizontalNormalizedPosition = 0;
    }

    private void onLevelFightButtonDown(int levelNumber)
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);

        if (GameInventory.Instance.HasEnoughUnitsForBattle())
        {
            GameStats.Instance.SelectedLevelNumber = levelNumber;
            SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);
        }
        else
            _notEnoughUnitsPopUp.SetActive(true);
    }

    public void OnNotEnoughUnitsPopUpDismissButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        SceneManager.LoadScene(GameConstants.Scenes.STORE);
    }

    public void OnBackButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        SceneManager.LoadScene(EnigmaConstants.Scenes.MAIN);
    }
}
