using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Levels : MonoBehaviour
{
    [SerializeField] private Transform _levelsGridContent;
    [SerializeField] List<int> _ratingsForArena = new List<int>() { 500, 600, 700, 800, 900, 1000 };

    void Start()
    {
        GameObject levelGridItemTemplate = _levelsGridContent.GetChild(0).gameObject;

        int levelsLenght = 0;
        if (GameStats.Instance.Mode == GameStats.Modes.SinglePlayer)
            levelsLenght = GameInventory.Instance.GetSinglePlayerLevel();
        else if (GameStats.Instance.Mode == GameStats.Modes.Pvp)
            levelsLenght = getPvpLevelNumber();
            
        for (int i = 0; i < levelsLenght; i++)
        {
            Transform unitTransform = Instantiate<GameObject>(levelGridItemTemplate, _levelsGridContent).transform;
            int levelNumber = i + 1;
            unitTransform.name = "Level" + levelNumber;
            unitTransform.Find("FightButton").GetComponent<Button>().onClick.AddListener(() => { onLevelFightButtonDown(levelNumber); });
        }
    }

    private void onLevelFightButtonDown(int levelNumber)
    {
        GameStats.Instance.SelecteLevelNumber = levelNumber;
        SceneManager.LoadScene("UnitSelect");
    }

    private int getPvpLevelNumber()
    {
        int levelNumber = 0;
        //int rating = (int)NetworkManager.GetCustomProperty(GameEnums.PlayerCustomProperties.Rating.ToString());

        //int arenasLenght = _ratingsForArena.Count;
        //for (int i = (arenasLenght - 1); i == 0; i--)
        //{
        //    if (rating >= _ratingsForArena[i])
        //    {
        //        levelNumber = i + 1;
        //        break;
        //    }
        //}

        return levelNumber;
    }
}
