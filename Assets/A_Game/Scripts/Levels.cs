using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Levels : MonoBehaviour
{
    [SerializeField] private Transform _levelsGridContent;

    void Start()
    {
        GameNetwork.Instance.SetLocalPlayerRating(150, GameNetwork.VirtualPlayerIds.HOST); //TODO: Remove hack
        GameObject levelGridItemTemplate = _levelsGridContent.GetChild(0).gameObject;

        int levelsLenght = 0;
        if (GameStats.Instance.Mode == GameStats.Modes.SinglePlayer)
            levelsLenght = GameInventory.Instance.GetSinglePlayerLevel();
        else if (GameStats.Instance.Mode == GameStats.Modes.Pvp)
            levelsLenght = GameNetwork.Instance.GetLocalPlayerPvpLevelNumber();
            
        for (int i = 0; i < levelsLenght; i++)
        {
            GameObject levelGameObject = Instantiate<GameObject>(levelGridItemTemplate, _levelsGridContent);
            int levelNumber = i + 1;
            levelGameObject.name = "Level" + levelNumber;

            LevelGridItem levelGridItem = levelGameObject.GetComponent<LevelGridItem>();
            levelGridItem.SetLabel(levelNumber.ToString());
            levelGridItem.FightButton.onClick.AddListener(() => { onLevelFightButtonDown(levelNumber); });
        }

        levelGridItemTemplate.SetActive(false);
    }

    private void onLevelFightButtonDown(int levelNumber)
    {
        GameStats.Instance.SelecteLevelNumber = levelNumber;
        SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);
    }

    public void OnBackButtonDown()
    {
        SceneManager.LoadScene(GameConstants.Scenes.MAIN);
    }
}
