﻿using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyerArea : ActionArea
{
    protected Vector3 _velocity = Vector3.zero;

    override protected void Update()
    {
        base.Update();
        transform.position += _velocity * Time.deltaTime;
    }

    override protected void setUpActionArea(string areaName, Vector3 position, Vector3 direction, string unitName, MinMinUnit.EffectNames effectName, string teamName, int networkPlayerId)
    {
        base.setUpActionArea(areaName, position, direction, unitName, effectName, teamName, networkPlayerId);
        _velocity = direction * GameConfig.Instance.ProjectilesSpeed;
    }

    override protected void OnTriggerEnter2D(Collider2D coll)
    {
        base.OnTriggerEnter2D(coll);

        checkFieldRewardBoxHit(coll);

        if (NetworkManager.GetIsMasterClient())
        {
            MinMinUnit targetUnit = getUnitFromCollider(coll);
            if (targetUnit != null)
            {
                string targetUnitName = targetUnit.name;

                string targetUnitTeam = GameNetwork.GetOppositeTeamName(OwnerTeamName);
                if (targetUnit.TeamName == targetUnitTeam)  //Just in case moving action circle collides with the wrong team
                {
                    if (!GameStats.Instance.UnitsDamagedInSingleDestroyerAction.Contains(targetUnitName))
                    {
                        dealDamage(targetUnitName);
                        GameStats.Instance.UnitsDamagedInSingleDestroyerAction.Add(targetUnitName);
                    }
                    //else
                    //    Debug.LogWarning("ActionAre::OnTriggerEnter2D -> Unit " + targetUnitName + " at team: " + targetUnitTeam + " already took damage this action.");
                }
            }
        }
    }
}