using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyerProjectile : NetworkEntity
{
    private War _warRef;

    //Only master client uses this
    public void SetWarReference(War war)
    {
        _warRef = war;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {

    }
}
