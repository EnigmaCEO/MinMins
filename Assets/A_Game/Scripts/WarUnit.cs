using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarUnit : MonoBehaviour
{
    [HideInInspector] public GameObject Unit;
    [HideInInspector] public Image LifeFill;

    private War _war;

    public void SetWar(War war)
    {
        _war = war;
    }

    void OnMouseUp()
    {
        Debug.Log("WarUnit::OnMouseUp -> " + name + " selected");
        _war.Attack(transform.parent.parent.name + "/" + transform.parent.name);
    }
}
