using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankArea : ActionArea
{
    public int Defense = 0;

    protected override void Awake()
    {
        base.Awake();
        Defense = int.Parse(getOwnerUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT));
    }

    override protected void OnTriggerEnter2D(Collider2D coll)
    {
        base.OnTriggerEnter2D(coll);

        if (base._warRef.GetIsHost())
        {
            MinMinUnit unit = getUnitFromCollider(coll);
            if ((unit != null) && (unit.TeamName == OwnerTeamName))
            {
                string targetUnitName = unit.name;
                _warRef.SetUnitSpecialDefense(targetUnitName, this);
            }
        }
    }
}
