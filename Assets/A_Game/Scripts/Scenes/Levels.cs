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
 
        if(GameHacks.Instance.Rating.Enabled)
            GameStats.Instance.Rating = GameHacks.Instance.Rating.ValueAsInt; 

        GameObject levelGridItemTemplate = _levelsGridContent.GetChild(0).gameObject;

        int levelsLenght = 0;
        if (GameStats.Instance.Mode == GameStats.Modes.SinglePlayer)
        {
            levelsLenght = GameInventory.Instance.GetSinglePlayerLevel();
        }
        else if (GameStats.Instance.Mode == GameStats.Modes.Pvp)
        {
            levelsLenght = GameNetwork.Instance.GetLocalPlayerPvpLevelNumber();
        }

        if (GameHacks.Instance.UnlockArenas.Enabled)
        {
            levelsLenght = GameHacks.Instance.UnlockArenas.ValueAsInt;
        }

        Debug.LogWarning(">Levels::Start -> Levels lenght: " + levelsLenght);

        for (int i = 0; i < levelsLenght; i++)
        {
            GameObject levelGameObject = Instantiate<GameObject>(levelGridItemTemplate, _levelsGridContent);
            int levelNumber = i + 1;
            levelGameObject.name = "Level" + levelNumber;

            LevelGridItem levelGridItem = levelGameObject.GetComponent<LevelGridItem>();
            levelGridItem.SetLabel(levelNumber.ToString());
            levelGridItem.FightButton.onClick.AddListener(() => { onLevelFightButtonDown(levelNumber); });
            if (GameNetwork.Instance.rewardedLevels[i] == 1)
            {
                Debug.Log("Rewarded Enjin " + i);
                levelGridItem.enjinReward.SetActive(true);
            }
            else {
                levelGridItem.enjinReward.SetActive(false);
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
