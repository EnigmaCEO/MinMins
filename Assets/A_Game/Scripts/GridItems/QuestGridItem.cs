using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestGridItem : MonoBehaviour
{
    [SerializeField] private GameObject _questCompletedUI;

    public void Set(bool completed)
    {
        _questCompletedUI.SetActive(completed);
    }
}
