using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchResultsPopUp : MonoBehaviour
{
    [SerializeField] private Text _damageDealtValue;
    [SerializeField] private Text _damageReceivedValue;
    [SerializeField] private Text _unitsKilledValue;
    [SerializeField] private Text _matchDurationValue;

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
    }
}
