using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardChanceDisplay : MonoBehaviour
{
    [SerializeField] private GameObject _guaranteedUI;

    public void Set(bool guaranteed)
    {
        _guaranteedUI.SetActive(guaranteed);
    }
}
