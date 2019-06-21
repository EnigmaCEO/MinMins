using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SWS;

public class UnitSelectManager : MonoBehaviour
{
    [SerializeField] private float _uiMoveDistance = 5;
    [SerializeField] private float _uiMoveTime = 1;
    [SerializeField] private float _uiOutDelay = 0.1f;
    [SerializeField] private float _uiInDelay = 1.5f;
    [SerializeField] private iTween.EaseType _uiMoveEaseType = iTween.EaseType.linear;

    [SerializeField] private Transform _unitsGridContent;
    [SerializeField] private GameObject _teamGrid;
    [SerializeField] private GameObject _infoPopUp;

    [SerializeField] private Transform _powerGridContent;
    [SerializeField] private Transform _armorGridContent;

    [SerializeField] private Image _slotInfoImage;
    [SerializeField] private WaypointManager _waypointManager;

    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _backButton;
    
    private Transform _teamGridContent;
    private int _selectedUnitIndex;

    private int[] _teamUnitIndexes = new int[6];


    void Start()
    {
        Transform[] units = _unitsGridContent.GetComponentsInChildren<Transform>();
        int unitsLength = units.Length;

        for(int i = 0; i < unitsLength; i++)
        {
            Transform unit = units[i];
            unit.Find("Sprite").GetComponent<Image>().enabled = false;
            unit.Find("FightButton").GetComponent<Button>().onClick.AddListener(() => { onUnitFightButtonDown(i); });
            unit.Find("InfoButton").GetComponent<Button>().onClick.AddListener(() => { onUnitInfoButtonDown(i); });
        }

        Transform[] slots = _teamGridContent.GetComponentsInChildren<Transform>();
        int slotsLength = slots.Length;

        for (int i = 0; i < slotsLength; i++)
        {
            Transform slot = slots[i];
            Image slotImage = slot.Find("Sprite").GetComponent<Image>();
            slotImage.enabled = false;
            slot.Find("btn_aid").GetComponent<Button>().onClick.AddListener(() => { onTeamSlotButtonDown(i, slotImage); });

            if(i != 5)
                slot.Find("obj_item_locked").GetComponent<Image>().enabled = false;
        }

        _nextButton.onClick.AddListener(() => onNextButtonDown());
        _nextButton.gameObject.SetActive(false);

        _backButton.onClick.AddListener(() => onBackButtonDown());

        disableUnitInfo(true);
    }

    private void enableUnitInfo()
    {
        iTween.MoveBy(_infoPopUp, iTween.Hash("y", _uiMoveDistance, "easeType", _uiMoveEaseType.ToString(), "loopType", iTween.LoopType.none.ToString(), "delay", _uiInDelay, "time", _uiMoveTime));
    }

    private void disableUnitInfo(bool immediate)
    {
        float outTime = immediate? 0 : _uiMoveTime;
        float outDelay = immediate ? 0 : _uiOutDelay;
        iTween.MoveBy(_infoPopUp, iTween.Hash("y", -_uiMoveDistance, "easeType", _uiMoveEaseType.ToString(), "loopType", iTween.LoopType.none.ToString(), "delay", outDelay, "time", outTime));
    }

    private void enableTeamGrid()
    {
        iTween.MoveBy(_teamGrid, iTween.Hash("y", _uiMoveDistance, "easeType", _uiMoveEaseType.ToString(), "loopType", iTween.LoopType.none.ToString(), "delay", _uiInDelay, "time", _uiMoveTime));
    }

    private void disableTeamGrid()
    {
        iTween.MoveBy(_teamGrid, iTween.Hash("y", -_uiMoveDistance, "easeType", _uiMoveEaseType.ToString(), "loopType", iTween.LoopType.none.ToString(), "delay", _uiOutDelay, "time", _uiMoveTime));
    }

    private void onNextButtonDown()
    {

    }

    private void onBackButtonDown()
    {

    }

    private void onUnitFightButtonDown(int unitIndex)
    {
        _selectedUnitIndex = unitIndex;
    }

    private void onUnitInfoButtonDown(int unitIndex)
    {
        _selectedUnitIndex = unitIndex;
        enableUnitInfo();
    }

    private void onTeamSlotButtonDown(int slotIndex, Image slotImage)
    {
        _teamUnitIndexes[slotIndex] = _selectedUnitIndex;
    }
}
