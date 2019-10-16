﻿using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionArea : NetworkEntity
{
    public const string ACTION_AREAS_RESOURCES_FOLDER_PATH = "Prefabs/ActionAreas/";

    public const string ACTION_AREAS_PARENT_FIND_PATH = "/ActionAreaContainers/";
    public const string EFFECTS_RESOURCES_FOLDER_PATH = "Prefabs/EffectsNew/";

    public float LifeTime = -1; //Unlimited
    public float CollisionTime = -1;  //Unlimited

    public float ActionTime = 2; //Used by War script

    public float ScaleFactorToParticleSizeFactor = 1;


    [Header("Only for display. Set at runtime:")]
    public string OwnerUnitName;
    public string OwnerTeamName;
    public int OwnerNetworkPlayerId;
    public float Strenght;

    protected War _warRef;
    protected Collider2D _collider;
    protected GameObject _effect;


    override protected void Awake()
    {
        base.Awake();

        _warRef = War.GetSceneInstance();
        _collider = GetComponent<Collider2D>();

        object[] data = base.GetInstantiationData();
        setUpActionArea((string)data[0], (Vector3)data[1], (Vector3)data[2], (string)data[3], (MinMinUnit.EffectNames)data[4], (string)data[5], (int)data[6]);

        Strenght = float.Parse(getOwnerUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT));
    }

    override protected void Update()
    {
        base.Update();
        if (_warRef.GetIsHost())
        {
            if (CollisionTime > 0)
            {
                CollisionTime -= Time.deltaTime;
                if (CollisionTime < 0)
                {
                    CollisionTime = 0;
                    sendDisableCollider();
                }
            }

            if (LifeTime > 0)
            {
                LifeTime -= Time.deltaTime;
                if (LifeTime < 0)
                {
                    LifeTime = 0;
                    NetworkManager.NetworkDestroy(this.gameObject);
                }
            }
        }
    }

    virtual protected void setUpActionArea(string areaName, Vector3 position, Vector3 direction, string unitName, MinMinUnit.EffectNames effectName, string teamName, int networkPlayerId)
    {
        this.name = areaName;

        OwnerUnitName = unitName;
        OwnerTeamName = teamName;
        OwnerNetworkPlayerId = networkPlayerId;

        transform.position = position;

        float scaleFactor = float.Parse(getOwnerUnitProperty(GameNetwork.UnitPlayerProperties.EFFECT_SCALE));
        //scaleFactor = 5; //Hack power scale
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scaleFactor * scale.x, scaleFactor * scale.y, scaleFactor * scale.z);

        setEffect(effectName);
    }

    public static void DestroyActionAreaList(List<ActionArea> actionAreas)
    {
        while (actionAreas.Count > 0)
        {
            ActionArea area = actionAreas[0];
            //Debug.LogWarning("ActionArea::DestroyActionAreaList -> actionAreaToDestroy: " + area.name + " ownerTeamName: " + area.OwnerTeamName + " owerName: " + area.OwnerUnitName);
            actionAreas.RemoveAt(0);
            NetworkManager.NetworkDestroy(area.gameObject);
        }
    }

    private void sendDisableCollider()
    {
        base.SendRpcToAll("receiveDisableCollider");
    }

    [PunRPC]
    protected void receiveDisableCollider()
    {
        _collider.enabled = false;
    }

    virtual protected void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.LogWarning("ActionArea::OnTriggerEnter2D: " + coll.name + " collided with " + this.name);
    }

    protected void checkFieldRewardBoxHit(Collider2D coll)
    {
        FieldRewardBox fieldRewardBox = coll.GetComponent<FieldRewardBox>();
        if (fieldRewardBox != null)
            fieldRewardBox.Hit();
    }

    protected MinMinUnit getUnitFromCollider(Collider2D coll)
    {
        if (coll.transform.parent == null)
            return null;

        return coll.transform.parent.GetComponent<MinMinUnit>();
    }

    private void setEffect(MinMinUnit.EffectNames effectName)
    {
        string effectFullPath = EFFECTS_RESOURCES_FOLDER_PATH + (effectName.ToString());

        //Debug.LogWarning("ActionArea::setEffect -> effectFullPath: " + effectFullPath);

        _effect = Instantiate<GameObject>(Resources.Load<GameObject>(effectFullPath));
        _effect.transform.parent = this.transform;
        _effect.transform.localPosition = Vector3.zero;

        if (effectName == MinMinUnit.EffectNames.LightningProjectile)
        {
            foreach (ParticleSystem particles in _effect.GetComponentsInChildren<ParticleSystem>())
                particles.startSize = this.transform.localScale.x * ScaleFactorToParticleSizeFactor;
        }
    }

    protected void dealDamage (string targetUnitName)
    {
        string targetUnitTeam = GameNetwork.GetOppositeTeamName(OwnerTeamName);

        //print("ActionArea virtual player Id: " + VirtualPlayerId);
        //print("Target Unit virtual player Id: " + oppositeTeam);

        int targetUnitPlayerId = GameNetwork.GetTeamNetworkPlayerId(targetUnitTeam); 

        int targetUnitHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.HEALTH, targetUnitTeam, targetUnitName);

        float targetUnitDefense = float.Parse(GameNetwork.GetAnyPlayerUnitProperty(GameNetwork.UnitPlayerProperties.DEFENSE, targetUnitName, targetUnitTeam, targetUnitPlayerId));
        float targetUnitTankDefense = _warRef.GetUnitTankDefense(targetUnitTeam, targetUnitName);
        //int finalDefense = targetUnitDefense + targetUnitTankDefense;
        //int maxDefense = GameConfig.Instance.MaxUnitDefense;

        Debug.LogWarning("ActionArea::dealDamage -> Strenght: " + Strenght + " targetUnitName: " + targetUnitName + " targetUnitDefense: " + targetUnitDefense + " targetUnitSpecialDefense: " + targetUnitTankDefense/* + " finalDefense uncapped: " + finalDefense*/);

        //if (finalDefense > maxDefense)
        //    finalDefense = maxDefense;

        //int damage = Mathf.FloorToInt((float)ownerStrenght * (float)(1 - (finalDefense / 100.0f)));  //Defense is translated into damage reduction
        int damage = Mathf.RoundToInt((Strenght * 10.0f) * (1.0f - targetUnitDefense/10.0f) * (1 - targetUnitTankDefense/10.0f));

        //if (damage == 0)
        //    damage = 1;

        int damageDealt = GameNetwork.GetTeamRoomPropertyAsInt(GameNetwork.TeamRoomProperties.DAMAGE_DEALT, OwnerTeamName);
        GameNetwork.SetTeamRoomProperty(GameNetwork.TeamRoomProperties.DAMAGE_DEALT, OwnerTeamName, (damageDealt + damage).ToString());

        int damageReceived = GameNetwork.GetTeamRoomPropertyAsInt(GameNetwork.TeamRoomProperties.DAMAGE_RECEIVED, targetUnitTeam);
        GameNetwork.SetTeamRoomProperty(GameNetwork.TeamRoomProperties.DAMAGE_RECEIVED, targetUnitTeam, (damageReceived + damage).ToString());
        
        Debug.LogWarning("ActionArea::dealDamage -> damage: " + damage.ToString());

        targetUnitHealth -= damage;  
        if (targetUnitHealth < 0)
            targetUnitHealth = 0;

        if(_warRef.GetIsAiTurn())
            _warRef.HandleAiSuccessfulAttack(this);

        _warRef.SetUnitHealth(targetUnitTeam, targetUnitName, targetUnitHealth, true);
    }

    protected string getOwnerUnitProperty(string property)
    {
        return GameNetwork.GetAnyPlayerUnitProperty(property, OwnerUnitName, OwnerTeamName, OwnerNetworkPlayerId);
    }
}
