using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardChanceDisplay : MonoBehaviour
{
    [SerializeField] private Text _guaranteedText;

    public void Set(int odds)
    {
        _guaranteedText.text = odds.ToString() + "%";
    }
}
