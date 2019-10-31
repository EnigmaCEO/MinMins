using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankArea : ActionArea
{
    public override void SetWarRef(War warRef)
    {
        base.SetWarRef(warRef);

        if (_warRef.GetIsHost())
            _warRef.AddTankArea(this);
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
