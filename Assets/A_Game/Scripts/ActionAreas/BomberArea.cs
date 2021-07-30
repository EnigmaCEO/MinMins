using Enigma.CoreSystems;
using GameEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberArea : ActionArea
{
    private GameObject _colliderEffect;
    private GameObject _chargeEffect;


    override protected void OnTriggerEnter2D(Collider2D coll)
    {
        base.OnTriggerEnter2D(coll);

        checkFieldRewardBoxHit(coll);

        if (_warRef.GetIsHost())
        {
            MinMinUnit unit = getUnitFromCollider(coll);
            if ((unit != null) && (unit.TeamName != OwnerTeamName))
            {
                dealDamage(unit.name);
            }
        }
    }

    override protected void setEffect(AbilityEffects effectName)
    {
        base.setEffect(effectName);

        //if (effectName == MinMinUnit.EffectNames.FireExplosion)
        //{
            _colliderEffect = _effect.transform.Find("Burst").gameObject;
            _colliderEffect.SetActive(false);

            _chargeEffect = _effect.transform.Find("Charge").gameObject;
        //}

        if (_colliderEffect != null)
        {
            _colliderEffect.SetActive(false);
        }
    }

    protected override void enableCollider(bool enabled)
    {
        base.enableCollider(enabled);

        SoundManager.Play(GameConstants.SoundNames.BOMB, SoundManager.AudioTypes.Sfx);

        if (_colliderEffect != null)
        {
            _colliderEffect.SetActive(true);
        }

        if (_chargeEffect != null)
        {
            _chargeEffect.SetActive(false);
        }
    }
}
