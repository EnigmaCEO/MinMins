using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarUnit : MonoBehaviour
{
    [HideInInspector] public GameObject Unit;

    private War _war;

    public void SetWar(War war)
    {
        _war = war;
    }

    void OnMouseUp()
    {
        //Debug.Log(name + " selected");
        //_war.Attack(transform.parent.name + "/" + name);
        _war.Attack(name);
    }
}
