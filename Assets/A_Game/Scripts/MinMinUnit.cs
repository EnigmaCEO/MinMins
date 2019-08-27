using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMinUnit : MonoBehaviour
{
    public enum Types
    {
        Scouts,
        Bombers,
        Tanks,
        Destroyers,
        Healers
    }

    public int Strength = 1;
    public int Defense = 1;
    public int MaxHealth = 5;

    public int EffectScale = 1;

    public int Effect;
    public Types Type;
    public int[] Attacks;

    public void SetForWar(string unitName, string teamName, int teamIndex)
    {
        name = unitName;
        string gridName = War.GetTeamGridName(teamName);
        int slotNumber = teamIndex + 1;
        transform.SetParent(GameObject.Find("Battlefield/" + gridName + "/slot" + slotNumber).transform);

        War.GetSceneInstance().GetTeamUnitsList(teamName).Add(this);

        transform.localPosition = Vector2.zero;
        Transform spriteTransform = transform.Find("Sprite");

        WarUnitSprite warUnitSprite = spriteTransform.gameObject.AddComponent<WarUnitSprite>();
        warUnitSprite.Unit = this.gameObject;

        GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
        shadow.transform.parent = spriteTransform;
        shadow.transform.localPosition = new Vector2(0, 0);
        shadow.transform.localScale = new Vector2(-1, 1);
    }
}
