using Enigma.CoreSystems;
using GameConstants;
using GameEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Levels : EnigmaScene
{
    [SerializeField] private Transform _levelsGridContent;
    [SerializeField] GameObject _notEnoughUnitsPopUp;

    [SerializeField] private Text _title;

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

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        GameHacks gameHacks = GameHacks.Instance;

        if (gameHacks.Rating.Enabled)
        {
            gameStats.Rating = gameHacks.Rating.ValueAsInt;
        }
#endif

        GameObject levelGridItemTemplate = _levelsGridContent.GetChild(0).gameObject;
        GameModes mode = gameStats.Mode;

        int levelsLenght = 0;
        if (mode == GameModes.SinglePlayer)
        {
            levelsLenght = gameInventory.GetHigherSinglePlayerLevelCompleted() + 1;
            int arenaMaxLevel = gameInventory.GetSinglePlayerMaxLevel();

            if (levelsLenght > arenaMaxLevel)
            {
                levelsLenght = arenaMaxLevel;
            }

            _title.text = LocalizationManager.GetTermTranslation("LEVEL SELECTION");
        }
        else if (mode == GameModes.Pvp)
        {
            levelsLenght = GameNetwork.Instance.GetLocalPlayerPvpLevelNumber();
        }
        else if (mode == GameModes.Quest)
        {
            //if (gameStats.SelectedSerialQuest == SerialQuests.ShalwendWargod)
            {
                levelsLenght = gameInventory.GetHighestQuestLevelCompletedAmount() + 1;
                int questMaxLevel = gameInventory.GetSerialQuestMaxLevel(gameStats.SelectedSerialQuest);

                _title.text = gameInventory.GetQuestDisplayName(gameStats.SelectedQuestString);

                if (levelsLenght > questMaxLevel)
                {
                    levelsLenght = questMaxLevel;
                }
            }
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (gameHacks.UnlockSinglePlayerLevels.Enabled && (mode == (GameModes.SinglePlayer)))
        {
            levelsLenght = GameHacks.Instance.UnlockSinglePlayerLevels.ValueAsInt;
        }
#endif

        Debug.LogWarning(">Levels::Start -> Levels lenght: " + levelsLenght);

        string selectedQuestString = gameStats.SelectedQuestString;

int selectedQuestLastLevel = 0;

        if (mode == GameModes.Quest)
        {
            if (selectedQuestString == nameof(SerialQuests.ShalwendWargod))
            {
                selectedQuestLastLevel = gameInventory.GetSerialQuestMaxLevel(SerialQuests.ShalwendWargod);
            }
        }

        for (int i = 0; i < levelsLenght; i++)
        {
            GameObject levelGameObject = Instantiate<GameObject>(levelGridItemTemplate, _levelsGridContent);
            int levelNumber = i + 1;
            levelGameObject.name = "Level" + levelNumber;

            LevelGridItem levelGridItem = levelGameObject.GetComponent<LevelGridItem>();
            levelGridItem.SetLabel(levelNumber.ToString());
            levelGridItem.FightButton.onClick.AddListener(() => { onLevelFightButtonDown(levelNumber); });

            if (mode == GameModes.Quest)
            {
                if (selectedQuestString == nameof(SerialQuests.ShalwendWargod))
                {
                    if ((i + 1) == selectedQuestLastLevel) // if level number equals max level
                    {
                        string iconPath = "Images/Quests/" + selectedQuestString + " Icon";
                        Sprite questIcon = (Sprite)Resources.Load<Sprite>(iconPath);

                        if (questIcon != null)
                        {
                            levelGridItem.SetImageSprite(questIcon);
                        }
                        else
                        {
                            Debug.Log("Quest Icon image was not found at path: " + iconPath + " . Please check active quest is correct and image is in the right path.");
                        }
                    }
                }
            }

            if ((mode == GameModes.Quest) || (mode == GameModes.Pvp))
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
        GameSounds.Instance.PlayUiAdvanceSound();

        if (GameInventory.Instance.HasEnoughUnitsForBattle())
        {
            GameStats.Instance.SelectedLevelNumber = levelNumber;
            SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);
        }
        else
        {
            _notEnoughUnitsPopUp.SetActive(true);
        }
    }

    public void OnNotEnoughUnitsPopUpDismissButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();

        if (GameConfig.Instance.SendToShopAutomaticallyOnUnitsNeeded)
        {
            SceneManager.LoadScene(GameConstants.Scenes.STORE);
        }
        else
        {
            _notEnoughUnitsPopUp.gameObject.SetActive(false);
        }
    }

    public void OnBackButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();

        if (GameStats.Instance.Mode == GameModes.Quest)
        {
            SceneManager.LoadScene(GameConstants.Scenes.QUEST_SELECTION);
        }
        else
        {
            SceneManager.LoadScene(EnigmaConstants.Scenes.MAIN);
        }
    }
}
