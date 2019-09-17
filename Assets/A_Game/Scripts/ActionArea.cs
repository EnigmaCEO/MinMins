using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionArea : NetworkEntity
{
    public const string ACTION_AREAS_PARENT_FIND_PATH = "/ActionAreasContainer";
    public const string SCOUT_SPRITE_MASKS_PARENT_FIND_PATH = "/ScoutSpriteMasksContainer";

    public const string EFFECTS_RESOURCES_FOLDER_PATH = "Prefabs/EffectsNew/";
    public const string SCOUTS_SPRITE_MASK_RESOURCE_PATH = "Prefabs/ScoutSpriteMask";

    public MinMinUnit.Types OwnerUnitType;
    public string OwnerUnitName;
    public string OwnerTeamName;
    public int OwnerNetworkPlayerId;

    private Vector3 _velocity;

    private War _warRef;
    //private Collider2D _collider;
    private Transform _transform;


    override protected void Awake()
    {
        base.Awake();

        _warRef = War.GetSceneInstance();
        //_collider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if(OwnerUnitType == MinMinUnit.Types.Destroyer)
            transform.position += _velocity * Time.deltaTime;
    }

    public void SendSetupData(Vector3 position, Vector3 velocity, MinMinUnit.Types unitType, string unitName, MinMinUnit.EffectNames effectName, string virtualPlayerId, int networkPlayerId)
    {
        base.SendRpcToAll("receiveSetupData", position, velocity, unitType, unitName, effectName, virtualPlayerId, networkPlayerId);
    }

    [PunRPC]
    private void receiveSetupData(Vector3 position, Vector3 direction, MinMinUnit.Types unitType, string unitName, MinMinUnit.EffectNames effectName, string teamName, int networkPlayerId)
    {
        OwnerUnitType = unitType;
        OwnerUnitName = unitName;
        OwnerTeamName = teamName;
        OwnerNetworkPlayerId = networkPlayerId;

        transform.position = position;
        _velocity = direction * GameConfig.Instance.ProjectilesSpeed;

        float scaleFactor = float.Parse(getOwnerUnitProperty(GameNetwork.UnitPlayerProperties.EFFECT_SCALE));
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scaleFactor * scale.x, scaleFactor * scale.y, scaleFactor * scale.z);
        transform.SetParent(GameObject.Find(ACTION_AREAS_PARENT_FIND_PATH).transform);

        setEffect(effectName);

        if (OwnerUnitType == MinMinUnit.Types.Bomber)
        {

        }
        else if (OwnerUnitType == MinMinUnit.Types.Healer)
        {

        }
        else if (OwnerUnitType == MinMinUnit.Types.Tank)
        {

        }
        else if (OwnerUnitType == MinMinUnit.Types.Destroyer)
        {
            scale = transform.localScale;
            float scaleModifier = GameConfig.Instance.DestroyerActionAreaScaleModifier;
            transform.localScale = new Vector3(scaleModifier * scale.x, scaleModifier * scale.y, scaleModifier * scale.z);
        }
        else if (OwnerUnitType == MinMinUnit.Types.Scout)
        {
            if (OwnerTeamName == _warRef.LocalPlayerTeam)
            {
                GameObject spriteMask = Instantiate<GameObject>(Resources.Load<GameObject>(SCOUTS_SPRITE_MASK_RESOURCE_PATH));
                Transform spriteMaskTransform = spriteMask.transform;
                spriteMaskTransform.SetParent(GameObject.Find(SCOUT_SPRITE_MASKS_PARENT_FIND_PATH).transform);
                spriteMaskTransform.position = this.transform.position;
                spriteMaskTransform.localScale = this.transform.localScale;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        //Debug.LogWarning("ActionArea::OnTriggerEnter2D: " + coll.gameObject.name + " hit " + name);
        MinMinUnit targetUnit = coll.transform.parent.GetComponent<MinMinUnit>();
        string targetUnitName = targetUnit.name;
        bool isHost = NetworkManager.GetIsMasterClient();

        if (OwnerUnitType == MinMinUnit.Types.Bomber)
        {
            if (isHost)
                dealDamage(targetUnitName);
        }
        else if (OwnerUnitType == MinMinUnit.Types.Healer)
        {
            if (isHost)
            {
                int healing = int.Parse(getOwnerUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT));
                _warRef.SetUnitForHealing(OwnerTeamName, targetUnitName, healing);
                //print("ActionArea virtual player Id: " + VirtualPlayerId);
            }
        }
        else if (OwnerUnitType == MinMinUnit.Types.Tank)
        {
            if (isHost)
            {
                int defense = int.Parse(getOwnerUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT));
                _warRef.SetUnitSpecialDefense(OwnerTeamName, targetUnitName, OwnerUnitName, defense);
                //print("ActionArea virtual player Id: " + VirtualPlayerId);
            }
        }
        else if (OwnerUnitType == MinMinUnit.Types.Destroyer)
        {
            if (isHost)
            {
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
        else if (OwnerUnitType == MinMinUnit.Types.Scout)
        {
            if (isHost)
            {
                string targetUnitTeam = GameNetwork.GetOppositeTeamName(OwnerTeamName);
                int strenght = int.Parse(getOwnerUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT));
                _warRef.HandleUnitScouted(targetUnitTeam, targetUnitName, strenght);
            }
        }
    }

    private void setEffect(MinMinUnit.EffectNames effectName)
    {
        string effectFullPath = EFFECTS_RESOURCES_FOLDER_PATH + ((int)effectName).ToString();

        //Debug.LogWarning("ActionArea::setEffect -> effectFullPath: " + effectFullPath);

        GameObject effectObject = Instantiate<GameObject>(Resources.Load<GameObject>(effectFullPath));
        effectObject.transform.parent = this.transform;
        effectObject.transform.localPosition = Vector3.zero;
    }

    private void dealDamage(string targetUnitName)
    {
        int damage = int.Parse(getOwnerUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT)); //TODO: Use damage formula if needed.
        string targetUnitTeam = GameNetwork.GetOppositeTeamName(OwnerTeamName);

        //print("ActionArea virtual player Id: " + VirtualPlayerId);
        //print("Target Unit virtual player Id: " + oppositeTeam);

        int targetUnitPlayerId = (targetUnitTeam == GameNetwork.VirtualPlayerIds.HOST) ? NetworkManager.GetLocalPlayerId() : GameNetwork.Instance.GuestPlayerId;

        int targetUnitHealth = GameNetwork.GetRoomUnitProperty(GameNetwork.UnitRoomProperties.HEALTH, targetUnitTeam, targetUnitName);
        int targetUnitDefense = GameNetwork.GetAnyPlayerUnitPropertyAsInt(GameNetwork.UnitPlayerProperties.DEFENSE, targetUnitName, targetUnitTeam, targetUnitPlayerId);
        int targetUnitSpecialDefense = _warRef.GetUnitSpecialDefense(targetUnitTeam, targetUnitName);
        int finalDefense = targetUnitDefense + targetUnitSpecialDefense;
        int maxDefense = GameConfig.Instance.MaxUnitDefense;

        //Debug.LogWarning("ActionArea::OnTriggerEnter2D -> targetUnitName: " + targetUnitName + " targetUnitDefense: " + targetUnitDefense + " targetUnitSpecialDefense: " + targetUnitSpecialDefense + " finalDefense uncapped: " + finalDefense);

        if (finalDefense > maxDefense)
            finalDefense = maxDefense;

        int finalDamage = Mathf.FloorToInt((float)damage * (float)(1 - (finalDefense / 100.0f)));  //Defense is translated into damage reduction

        //Debug.LogWarning("ActionArea::OnTriggerEnter2D -> finalDefense: " + finalDefense + " damage: " + damage + " finalDamage: " + finalDamage);

        targetUnitHealth -= finalDamage;  
        if (targetUnitHealth < 0)
            targetUnitHealth = 0;
        _warRef.SetUnitHealth(targetUnitTeam, targetUnitName, targetUnitHealth, true);
    }

    //private void enableCollider()
    //{
    //    _collider.enabled = true;
    //}

    private string getOwnerUnitProperty(string property)
    {
        return GameNetwork.GetAnyPlayerUnitProperty(property, OwnerUnitName, OwnerTeamName, OwnerNetworkPlayerId);
    }
}
