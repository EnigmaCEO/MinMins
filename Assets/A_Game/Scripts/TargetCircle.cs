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

    private War _warRef;

    private Collider2D _collider;

    override protected void Awake()
    {
        base.Awake();

        _warRef = War.GetSceneInstance();

        _collider = GetComponent<Collider2D>();
        _collider.enabled = false;
    }

    public void SendSetupData(Vector3 position, MinMinUnit.Types unitType, string unitName, string virtualPlayerId, int networkPlayerId)
    {
        base.SendRpcToAll("receiveSetupData", position, unitType, unitName, virtualPlayerId, networkPlayerId);
    }

    [PunRPC]
    private void receiveSetupData(Vector3 position, MinMinUnit.Types unitType, string unitName, string virtualPlayerId, int networkPlayerId)
    {
        UnitType = unitType;
        UnitName = unitName;
        VirtualPlayerId = virtualPlayerId;
        NetworkPlayerId = networkPlayerId;

        transform.position = position;

        float scaleFactor = float.Parse(getUnitProperty(GameNetwork.UnitPlayerProperties.EFFECT_SCALE));
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scaleFactor * scale.x, scaleFactor * scale.y, scaleFactor * scale.z);

        string parentPath = "/TargetCircleContainers/" + UnitType.ToString();
        transform.SetParent(GameObject.Find(parentPath).transform);

        if (UnitType == MinMinUnit.Types.Bombers)
        {
            _collider.enabled = true;
        }
        else if (UnitType == MinMinUnit.Types.Healers)
        {
        }
        else if (UnitType == MinMinUnit.Types.Tanks)
        {
            _collider.enabled = true;
        }
        else if (UnitType == MinMinUnit.Types.Destroyers)
        {
        }
        else if (UnitType == MinMinUnit.Types.Scouts)
        {
            _collider.enabled = true;
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.Log("TargetCircle::OnTriggerEnter2D: " + coll.gameObject.name + " hit " + name);
        string targetUnitName = coll.transform.parent.name;
        GameNetwork gameNetwork = GameNetwork.Instance;

        if (UnitType == MinMinUnit.Types.Bombers)
        {
            int damage = int.Parse(getUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT));
            string oppositeTeam = GameNetwork.GetOppositeTeamName(VirtualPlayerId);

            print("TargetCircle virtual player Id: " + VirtualPlayerId);
            print("Target Unit virtual player Id: " + oppositeTeam);

            int targetUnitHealth = GameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.HEALTH, oppositeTeam, targetUnitName);
            targetUnitHealth -= damage;  //TODO: Check if damage formula is needed;
            if (targetUnitHealth < 0)
                targetUnitHealth = 0;
            _warRef.SetUnitHealth(oppositeTeam, targetUnitName, targetUnitHealth, true);
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
