using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enigma.CoreSystems;

public class WarPrepManager : MonoBehaviour
{
    [SerializeField] private Transform _slotsTeam1;
    [SerializeField] private Button _nextButton;
    private int _slotsInPlay = 0;
    private int _slotsReady = 0;

    void Start()
    {
        _nextButton.onClick.AddListener(() => { onNextButtonDown(); });
        _nextButton.gameObject.SetActive(false);

        Transform warPrepGrid = GameObject.Find("/WarPrepGrid").transform;
        int gridLength = warPrepGrid.childCount;

        for (int i = 0; i < gridLength; i++)
        {
            //int prepIndex = PlayerStats.Instance.TeamUnitIndexes[i] + 1;
            //if (prepIndex < 1)
            //    continue;

            string prepName = GameMatch.Instance.GetUnit(1, i).Name;
            if (prepName == "-1")
                continue;

            GameObject minMinObj = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/MinMinUnits/" + prepName));
            minMinObj.name = prepName;
            Transform minMinTransform = minMinObj.transform;
            minMinTransform.Find("Sprite").gameObject.AddComponent<PrepMinMin>().SetManager(this);

            minMinTransform.parent = warPrepGrid.Find("slot" + (i + 1));
            minMinTransform.localPosition = new Vector2(0, 0);

            _slotsInPlay++;
        }
    }

    public void AddToSlotsReady()
    {
        _slotsReady++;

        if (_slotsReady == _slotsInPlay)
            _nextButton.gameObject.SetActive(true);
    }

    private void onNextButtonDown()
    {
        GameMatch gameMatch = GameMatch.Instance;

        for (int i = 0; i < _slotsInPlay; i++)
        {
            Transform unitSpriteTransform = _slotsTeam1.Find("slot" + (i + 1) + "/Sprite");
            gameMatch.GetUnit(1, i).Position = unitSpriteTransform.localPosition;
        }

        SceneManager.LoadScene("War");
    }
}
