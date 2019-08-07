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
        string[] unitNames = GameNetwork.Instance.GetLocalPlayerTeamUnits(GameConstants.TeamNames.ALLIES);
        int unitsCount = unitNames.Length;

        for (int i = 0; i < unitsCount; i++)
        {
            string unitName = unitNames[i];
            if (unitName == "-1")
                continue;

            GameObject minMinObj = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/MinMinUnits/" + unitName));
            minMinObj.name = unitName;
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
        GameNetwork gameNetwork = GameNetwork.Instance;

        for (int i = 0; i < _slotsInPlay; i++)
        {
            Transform unitTransform = _slotsTeam1.Find("slot" + (i + 1));
            Transform unitSpriteTransform = unitTransform.Find("Sprite");

            Vector3 pos = unitSpriteTransform.localPosition;
            string positionString = pos.x.ToString() + Constants.Separators.First + pos.y.ToString();

            gameNetwork.SetLocalPlayerUnitProperty(unitTransform.name, GameNetwork.UnitPlayerProperties.POSITION, positionString);
        }

        SceneManager.LoadScene(GameConstants.Scenes.WAR);
    }
}
