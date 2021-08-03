using Enigma.CoreSystems;
using GameConstants;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchResultsPopUp : MonoBehaviour
{
    //public Button DismissButton;

    public Action<bool> DismissButtonDown;

    [SerializeField] private Text _winnerText;

    [SerializeField] private Text _damageDealtValue;
    [SerializeField] private Text _damageReceivedValue;
    [SerializeField] private Text _unitsKilledValue;
    [SerializeField] private Text _matchDurationValue;

    [SerializeField] private Transform _unitsAliveGridContent;
    [SerializeField] private Transform _rewardsGridContent;

    private War _warRef;
    private bool _questWasCompleted = false;

    private void Awake()
    {
        _warRef = War.GetSceneInstance();
    }

    public void OnDismissButtonDown()
    {
        if (DismissButtonDown != null)
        {
            DismissButtonDown(_questWasCompleted);
        }
    }

    public void Open(bool questWasCompleted)
    {
        _questWasCompleted = questWasCompleted;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void SetValues(War.MatchLocalData matchLocalData, TeamBoostItemGroup boostItemGroup)
    {
        string winnerNickname = NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.WINNER_NICKNAME);
        string localPlayerNickname = NetworkManager.GetLocalPlayerNickname();

        if (localPlayerNickname == winnerNickname)
        {
            _winnerText.text = "You win!";
            SoundManager.Play(GameConstants.SoundNames.WIN, SoundManager.AudioTypes.Sfx);
        }
        else
        {
            _winnerText.text = "You lose!";
            SoundManager.Play(GameConstants.SoundNames.LOSE, SoundManager.AudioTypes.Sfx);
        }

        LocalizationManager.LocalizeText(_winnerText);

        string localTeamName = _warRef.LocalPlayerTeam;

        _damageDealtValue.text = GameNetwork.GetTeamRoomProperty(GameNetwork.TeamRoomProperties.DAMAGE_DEALT, localTeamName);
        _damageReceivedValue.text = GameNetwork.GetTeamRoomProperty(GameNetwork.TeamRoomProperties.DAMAGE_RECEIVED, localTeamName);
        _unitsKilledValue.text = GameNetwork.GetTeamRoomProperty(GameNetwork.TeamRoomProperties.UNITS_KILLED, localTeamName);

        double matchDuration = double.Parse(NetworkManager.GetRoomCustomProperty(GameNetwork.RoomCustomProperties.MATCH_DURATION));
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(matchDuration);
        string matchDurationString = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        _matchDurationValue.text = matchDurationString;

        GameObject unitGridItemTemplate = _unitsAliveGridContent.GetChild(0).gameObject;

        string localTeam = _warRef.LocalPlayerTeam;
        string[] localTeamUnitNames = GameNetwork.GetTeamUnitNames(_warRef.LocalPlayerTeam);

        foreach(string unitName in localTeamUnitNames)
        {
            int unitHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.HEALTH, localTeam, unitName);
            bool unitIsAlive = (unitHealth > 0);

            if (!unitIsAlive)
            {
                GameObject unit = Instantiate<GameObject>(unitGridItemTemplate, _unitsAliveGridContent);
                unit.GetComponent<UnitAliveGridItem>().SetUp(unitName, unitIsAlive);
            }
        }

        unitGridItemTemplate.SetActive(false);

        Dictionary<int, int> boxTiersWithAmountRewards = matchLocalData.BoxTiersWithAmountsRewards;
        GameObject unitRewardGridItemTemplate = _rewardsGridContent.Find("UnitsRewardGridItem").gameObject;
        GameObject boostRewardGridItemTemplate = _rewardsGridContent.Find("BoostRewardGridItem").gameObject;

        GameInventory gameInventory = GameInventory.Instance;

        foreach (KeyValuePair<int, int> amountByTier in boxTiersWithAmountRewards)
        {
            int count = amountByTier.Value;
            for (int i = 0; i < count; i++)
            {
                createRewardGridItem(unitRewardGridItemTemplate, amountByTier.Key, false);
                gameInventory.ChangeLootBoxAmount(amountByTier.Value, amountByTier.Key, true, false);
            }
        }

        if (boostItemGroup != null)
        {
            GameObject boostReward = Instantiate<GameObject>(boostRewardGridItemTemplate, _rewardsGridContent);
            boostReward.GetComponent<BoostRewardGridItem>().SetUp(boostItemGroup.Category, boostItemGroup.Bonus, true);
        }

        gameInventory.SaveLootBoxes();

        if (matchLocalData.EnjinCollected)
        {
            createRewardGridItem(unitRewardGridItemTemplate, BoxTiers.GOLD, true);
        }

        unitRewardGridItemTemplate.SetActive(false);
        boostRewardGridItemTemplate.SetActive(false);
    }

    private void createRewardGridItem(GameObject rewardGridItemTemplate, int tier, bool isEnjin)
    {
        GameObject reward = Instantiate<GameObject>(rewardGridItemTemplate, _rewardsGridContent);
        reward.GetComponent<BoxRewardGridItem>().SetUp(tier, isEnjin);
    }
}
