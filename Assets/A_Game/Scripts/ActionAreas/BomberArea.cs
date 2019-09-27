using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomberArea : ActionArea
{
    override protected void OnTriggerEnter2D(Collider2D coll)
    {
        base.OnTriggerEnter2D(coll);

        checkFieldRewardBoxHit(coll);

        if (_warRef.GetIsHost())
        {
            MinMinUnit unit = getUnitFromCollider(coll);
            if(unit != null)
                dealDamage(unit.name);
        }
    }
}
