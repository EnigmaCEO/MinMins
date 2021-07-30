using Enigma.CoreSystems;
using GameEnums;
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

    override public void SetUpActionArea(string areaName, Vector3 position, Vector3 direction, string unitName, AbilityEffects effectName, string teamName, int networkPlayerId)
    {
        base.SetUpActionArea(areaName, position, direction, unitName, effectName, teamName, networkPlayerId);
        _velocity = direction * GameConfig.Instance.ProjectilesSpeed;

        //if (effectName == MinMinUnit.EffectNames.LightningProjectile)
        {
            foreach (ParticleSystem particles in _effect.GetComponentsInChildren<ParticleSystem>())
            {
                particles.startSize *= this.transform.localScale.x * ScaleFactorToParticleSizeFactor;
            }
        }

        SoundManager.Play(GameConstants.SoundNames.DESTROY, SoundManager.AudioTypes.Sfx);
    }

    override protected void OnTriggerEnter2D(Collider2D coll)
    {
        base.OnTriggerEnter2D(coll);

        checkFieldRewardBoxHit(coll);

        if (base._warRef.GetIsHost())
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
