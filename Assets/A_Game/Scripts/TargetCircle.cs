using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCircle : NetworkEntity
{
    public const string PARENT_PATH = "/TargetCircleContainer";

    public MinMinUnit.Types OwnerUnitType;
    public string OwnerUnitName;
    public string OwnerTeamName;
    public int OwnerNetworkPlayerId;

    private War _warRef;

    //private Collider2D _collider;

    override protected void Awake()
    {
        base.Awake();

        _warRef = War.GetSceneInstance();

        //_collider = GetComponent<Collider2D>();
    }

    //public void EnableCollider()
    //{
    //    _collider.enabled = true;
    //}

    public void SendSetupData(Vector3 position, MinMinUnit.Types unitType, string unitName, string virtualPlayerId, int networkPlayerId)
    {
        base.SendRpcToAll("receiveSetupData", position, unitType, unitName, virtualPlayerId, networkPlayerId);
    }

    [PunRPC]
    private void receiveSetupData(Vector3 position, MinMinUnit.Types unitType, string unitName, string teamName, int networkPlayerId)
    {
        OwnerUnitType = unitType;
        OwnerUnitName = unitName;
        OwnerTeamName = teamName;
        OwnerNetworkPlayerId = networkPlayerId;

        transform.position = position;

        float scaleFactor = float.Parse(getUnitProperty(GameNetwork.UnitPlayerProperties.EFFECT_SCALE));
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scaleFactor * scale.x, scaleFactor * scale.y, scaleFactor * scale.z);
        transform.SetParent(GameObject.Find(PARENT_PATH).transform);

        if (OwnerUnitType == MinMinUnit.Types.Bombers)
        {

        }
        else if (OwnerUnitType == MinMinUnit.Types.Healers)
        {

        }
        else if (OwnerUnitType == MinMinUnit.Types.Tanks)
        {

        }
        else if (OwnerUnitType == MinMinUnit.Types.Destroyers)
        {

        }
        else if (OwnerUnitType == MinMinUnit.Types.Scouts)
        {

        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.Log("TargetCircle::OnTriggerEnter2D: " + coll.gameObject.name + " hit " + name);
        string targetUnitName = coll.transform.parent.name;
        GameNetwork gameNetwork = GameNetwork.Instance;
        bool isHost = NetworkManager.GetIsMasterClient();

        if (OwnerUnitType == MinMinUnit.Types.Bombers)
        {
            if (isHost)
            {
                int damage = int.Parse(getUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT));
                string oppositeTeam = GameNetwork.GetOppositeTeamName(OwnerTeamName);

                //print("TargetCircle virtual player Id: " + VirtualPlayerId);
                //print("Target Unit virtual player Id: " + oppositeTeam);

                int targetUnitHealth = GameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.HEALTH, oppositeTeam, targetUnitName);
                targetUnitHealth -= damage;  //TODO: Check if damage formula is needed;
                if (targetUnitHealth < 0)
                    targetUnitHealth = 0;
                _warRef.SetUnitHealth(oppositeTeam, targetUnitName, targetUnitHealth, true);
            }
        }
        else if (OwnerUnitType == MinMinUnit.Types.Healers)
        {
            if (isHost)
            {
                int healing = int.Parse(getUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT));
                _warRef.SetUnitForHealing(OwnerTeamName, targetUnitName, healing);
                //print("TargetCircle virtual player Id: " + VirtualPlayerId);
            }
        }
        else if (OwnerUnitType == MinMinUnit.Types.Tanks)
        {

        }
        else if (OwnerUnitType == MinMinUnit.Types.Destroyers)
        {

        }
        else if (OwnerUnitType == MinMinUnit.Types.Scouts)
        {

        }
    }

    private string getUnitProperty(string property)
    {
        return GameNetwork.GetAnyPlayerUnitProperty(property, OwnerUnitName, OwnerTeamName, OwnerNetworkPlayerId);
    }
}
