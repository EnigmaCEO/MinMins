using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchResultsPopUp : MonoBehaviour
{
    public Button DismissButton;

    [SerializeField] private Text _damageDealtValue;
    [SerializeField] private Text _damageReceivedValue;
    [SerializeField] private Text _unitsKilledValue;
    [SerializeField] private Text _matchDurationValue;

    [SerializeField] private Transform _unitsAliveGridContent;
    [SerializeField] private Transform _rewardsGridContent;


    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void SetValues(War.MatchData matchData)
    {
        _damageDealtValue.text = matchData.DamageDealt.ToString();
        _damageReceivedValue.text = matchData.DamageReceived.ToString();
        _unitsKilledValue.text = matchData.UnitsKilled.ToString();
        _matchDurationValue.text = matchData.MatchDuration.ToString();

        Dictionary<string, bool> unitsAlive = matchData.UnitsAlive;
        GameObject unitGridItemTemplate = _unitsAliveGridContent.GetChild(0).gameObject;

        foreach(KeyValuePair<string, bool> unitAlive in unitsAlive)
        {
            GameObject unit = Instantiate<GameObject>(unitGridItemTemplate, _unitsAliveGridContent);
            unit.GetComponent<UnitAliveGridItem>().SetUp(unitAlive.Key, unitAlive.Value);
        }

        unitGridItemTemplate.SetActive(false);

        Dictionary<int, int> boxTiersWithAmountRewards = matchData.BoxTiersWithAmountsRewards;
        GameObject rewardGridItemTemplate = _rewardsGridContent.GetChild(0).gameObject;

        foreach (KeyValuePair<int, int> tiersWithAmount in boxTiersWithAmountRewards)
        {
            int count = tiersWithAmount.Value;
            for (int i = 0; i < count; i++)
            {
                GameObject reward = Instantiate<GameObject>(rewardGridItemTemplate, _rewardsGridContent);
                reward.GetComponent<RewardGridItem>().SetUp(tiersWithAmount.Key);
            }
        }

        rewardGridItemTemplate.SetActive(false);
    }
}
