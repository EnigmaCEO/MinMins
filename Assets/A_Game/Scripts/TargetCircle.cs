using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCircle : NetworkEntity
{
    public MinMinUnit.Types UnitType;
    public string UnitName;
    public string VirtualPlayerId;
    public int NetworkPlayerId;

    private Collider2D _collider;

    override protected void Awake()
    {
        base.Awake();

        _collider = GetComponent<Collider2D>();
        _collider.enabled = false;
    }

    public void sendSetupData(MinMinUnit.Types unitType, string unitName, string virtualPlayerId, int networkPlayerId)
    {
        base.SendRpcToAll("receiveSetupData", unitType, UnitName, virtualPlayerId, networkPlayerId);
    }

    [PunRPC]
    private void receiveSetupData(MinMinUnit.Types unitType, string unitName, string virtualPlayerId, int networkPlayerId)
    {
        UnitType = unitType;
        UnitName = unitName;
        VirtualPlayerId = virtualPlayerId;
        NetworkPlayerId = networkPlayerId;

        float scaleFactor = float.Parse(getUnitProperty(GameNetwork.UnitPlayerProperties.EFFECT_SCALE));
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scaleFactor * scale.x, scaleFactor * scale.y, scaleFactor * scale.z);

        string parentPath = "/TargetCircleContainers/";

        if (UnitType == MinMinUnit.Types.Bombers)
        {
            _collider.enabled = true;
            parentPath += "Offense";
        }
        else if (UnitType == MinMinUnit.Types.Healers)
        {
            parentPath += "Healing";
        }
        else if (UnitType == MinMinUnit.Types.Tanks)
        {
            _collider.enabled = true;
            parentPath += "Defense";
        }
        else if (UnitType == MinMinUnit.Types.Destroyers)
        {
            parentPath += "Offense";
        }
        else if (UnitType == MinMinUnit.Types.Scouts)
        {
            _collider.enabled = true;
            parentPath += "Scout";
        }

        transform.SetParent(GameObject.Find(parentPath).transform);
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.Log("TargetCircle::OnTriggerEnter2D: " + coll.gameObject.name + " hit " + name);
        string unitName = coll.transform.parent.name;
        GameNetwork gameNetwork = GameNetwork.Instance;

        if (UnitType == MinMinUnit.Types.Bombers)
        {
            int damage = int.Parse(getUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT));
            string oppositeTeam = GameNetwork.GetOppositeTeamName(VirtualPlayerId);
            int unitHealth = GameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.HEALTH, oppositeTeam, UnitName);
            //TODO: Reduce health
        }
        else if (UnitType == MinMinUnit.Types.Healers)
        {

        }
        else if (UnitType == MinMinUnit.Types.Tanks)
        {

        }
        else if (UnitType == MinMinUnit.Types.Destroyers)
        {

        }
        else if (UnitType == MinMinUnit.Types.Scouts)
        {

        }
    }

    private string getUnitProperty(string property)
    {
        return GameNetwork.GetAnyPlayerUnitProperty(property, UnitName, VirtualPlayerId, NetworkPlayerId);
    }
}
