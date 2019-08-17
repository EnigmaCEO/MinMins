using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enigma.CoreSystems;

public class WarPrepManager : MonoBehaviour
{
    [SerializeField] private Transform _slotsTeam1;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _backButton;

    private int _slotsInPlay = 0;
    private int _slotsReady = 0;

    void Start()
    {
        _nextButton.onClick.AddListener(() => { onNextButtonDown(); });
        _nextButton.gameObject.SetActive(false);

        _backButton.onClick.AddListener(() => { onBackButtonDown(); });

        Transform warPrepGrid = GameObject.Find("/WarPrepGrid").transform;
        string[] unitNames = GameNetwork.Instance.GetLocalPlayerTeamUnits(GameConstants.VirtualPlayerIds.ALLIES);
        int unitsCount = unitNames.Length;

        for (int i = 0; i < unitsCount; i++)
        {
            string unitName = unitNames[i];
            if (unitName == "-1")
                continue;

            GameObject minMinObj = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/MinMinUnits/" + unitName));
            minMinObj.name = unitName;
            Transform minMinTransform = minMinObj.transform;
            PrepMinMinSprite prepMinMinSprite = minMinTransform.Find("Sprite").gameObject.AddComponent<PrepMinMinSprite>();
            prepMinMinSprite.SetManager(this);
            prepMinMinSprite.UnitName = unitName;

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
            Transform slotTransform = _slotsTeam1.Find("slot" + (i + 1));
            Transform unitSpriteTransform = slotTransform.Find("Sprite");
            PrepMinMinSprite prepMinMinSprite = unitSpriteTransform.GetComponent<PrepMinMinSprite>();

            Vector3 pos = unitSpriteTransform.localPosition;
            string positionString = pos.x.ToString() + NetworkManager.Separators.VALUES + pos.y.ToString();

            gameNetwork.SetLocalPlayerUnitProperty(GameNetwork.UnitPlayerProperties.POSITION, prepMinMinSprite.UnitName, positionString, GameConstants.VirtualPlayerIds.ALLIES);
        }

        GameStats.Modes gameMode = GameStats.Instance.Mode;
        if (gameMode == GameStats.Modes.SinglePlayer)
            SceneManager.LoadScene(GameConstants.Scenes.WAR);
        else if (gameMode == GameStats.Modes.Pvp)
            SceneManager.LoadScene(GameConstants.Scenes.LOBBY);
    }

    private void onBackButtonDown()
    {
        SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);
    }
}
