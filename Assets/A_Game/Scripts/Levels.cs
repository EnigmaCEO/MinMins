﻿using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Levels : MonoBehaviour
{
    [SerializeField] private Transform _levelsGridContent;
    [SerializeField] GameObject _notEnoughUnitsPopUp;

    void Start()
    {
        _notEnoughUnitsPopUp.SetActive(false);

        //GameNetwork.SetLocalPlayerRating(150, GameNetwork.VirtualPlayerIds.HOST); 
        GameStats.Instance.Rating = 150; //TODO: Remove hack
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
        SceneManager.LoadScene(GameConstants.Scenes.STORE);
    }

    public void OnBackButtonDown()
    {
        SceneManager.LoadScene(GameConstants.Scenes.MAIN);
    }
}
