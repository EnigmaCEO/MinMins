using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealerArea : ActionArea
{
    public int Healing = 0;

    override protected void Awake()
    {
        base.Awake();
        Healing = int.Parse(getOwnerUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT));
    }

    override protected void OnTriggerEnter2D(Collider2D coll)
    {
        base.OnTriggerEnter2D(coll);

        if (base._warRef.GetIsHost())
        {
            MinMinUnit unit = getUnitFromCollider(coll);

            if((unit != null) && (unit.TeamName == OwnerTeamName))
                _warRef.SetUnitForHealing(unit.name, this);
        }
    }
}
