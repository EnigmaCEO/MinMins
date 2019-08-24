using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarUnitSprite : MonoBehaviour
{
    [HideInInspector] public GameObject Unit;
    [HideInInspector] public Image LifeFill;

    private War _war;
    private MinMinUnit _minMinUnit;

    private void Awake()
    {
        _minMinUnit = transform.parent.GetComponent<MinMinUnit>();
        _war = War.GetSceneInstance();
    }

    void OnMouseUp()
    {
        Debug.Log("WarUnitSprite::OnMouseUp -> " + name + " selected");
        //_war.Attack(transform.parent.name + "/" + transform.parent.name);
        _war.Attack(_minMinUnit);
    }
}
