using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarUnit : MonoBehaviour
{
    private string _unitName;
    private string _teamName;
    private int _teamIndex;

    private MinMinUnit _minMinUnit;

    private void Awake()
    {
        _minMinUnit = GetComponent<MinMinUnit>();
        GetComponent<PhotonView>().ObservedComponents.Add(this);
        print("WarUnit::Awake -> name: " + name);
    }

    void Start()
    {
        name = _unitName;
        string gridName = War.GetTeamGridName(_teamName);
        int slotNumber = _teamIndex + 1;
        transform.SetParent(GameObject.Find("Battlefield/" + gridName + "/slot" + slotNumber).transform);
 
        War.GetSceneInstance().GetTeamUnitsList(_teamName).Add(_minMinUnit);

        transform.localPosition = Vector2.zero;
        Transform spriteTransform = transform.Find("Sprite");

        WarUnitSprite warUnitSprite = spriteTransform.gameObject.AddComponent<WarUnitSprite>();
        warUnitSprite.Unit = this.gameObject;

        GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
        shadow.transform.parent = spriteTransform;
        shadow.transform.localPosition = new Vector2(0, 0);
        shadow.transform.localScale = new Vector2(-1, 1);

        print("WarUnit::Start -> name: " + name);
    }

    public void Set(string unitName, string teamName, int teamIndex)
    {
        _unitName = unitName;
        _teamName = teamName;
        _teamIndex = teamIndex;

        print("WarUnit::Set -> name: " + name);
    }
}
