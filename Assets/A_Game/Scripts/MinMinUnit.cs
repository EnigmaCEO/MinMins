﻿using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMinUnit : NetworkEntity
{
    public enum Types
    {
        Healers,
        Bombers,
        Tanks,
        Destroyers,
        Scouts,
    }

    public int Strength = 1;
    public int Defense = 1;
    public int MaxHealth = 5;

    public float EffectScale = 1;

    public int Effect;
    public Types Type;
    public int[] Attacks;

    protected override void Awake()
    {
        base.Awake();
    }

    public void SendSettingsForWar(string unitName, string teamName, int teamIndex, float posX, float posY)
    {
        SendRpcToAll("receiveSettingsForWar", unitName, teamName, teamIndex, posX, posY);
    }

    [PunRPC]
    private void receiveSettingsForWar(string unitName, string teamName, int teamIndex, float posX, float posY)
    {
        name = unitName;
        string gridName = War.GetTeamGridName(teamName);
        int slotNumber = teamIndex + 1;
        transform.SetParent(GameObject.Find("Battlefield/" + gridName + "/slot" + slotNumber).transform);

        War.GetSceneInstance().GetTeamUnitsDictionary(teamName).Add(unitName, this);

        transform.localPosition = Vector2.zero;
        Transform spriteTransform = transform.Find("Sprite");

        WarUnitSprite warUnitSprite = spriteTransform.gameObject.AddComponent<WarUnitSprite>();
        warUnitSprite.Unit = this.gameObject;

        GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
        shadow.transform.parent = spriteTransform;
        shadow.transform.localPosition = new Vector2(0, 0);
        shadow.transform.localScale = new Vector2(-1, 1);


        bool isAllies = (teamName == GameNetwork.VirtualPlayerIds.HOST);

        if (!isAllies)
        {
            Vector3 localScale = spriteTransform.localScale;
            spriteTransform.localScale = new Vector3(-localScale.x, localScale.y, localScale.z);
        }

        spriteTransform.localPosition = new Vector3(posX, posY, spriteTransform.localPosition.z);
    }
}
