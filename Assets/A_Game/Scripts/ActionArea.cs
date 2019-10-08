using Enigma.CoreSystems;
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


    [Header("Only for display. Set at runtime:")]
    public string OwnerUnitName;
    public string OwnerTeamName;
    public int OwnerNetworkPlayerId;

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
        string effectFullPath = EFFECTS_RESOURCES_FOLDER_PATH + ((int)effectName).ToString();

        //Debug.LogWarning("ActionArea::setEffect -> effectFullPath: " + effectFullPath);

        _effect = Instantiate<GameObject>(Resources.Load<GameObject>(effectFullPath));
        _effect.transform.parent = this.transform;
        _effect.transform.localPosition = Vector3.zero;
    }

    protected void dealDamage (string targetUnitName)
    {
        int damage = int.Parse(getOwnerUnitProperty(GameNetwork.UnitPlayerProperties.STRENGHT)); //TODO: Use damage formula if needed.
        //damage = 50; // damage hack
        string targetUnitTeam = GameNetwork.GetOppositeTeamName(OwnerTeamName);

        //print("ActionArea virtual player Id: " + VirtualPlayerId);
        //print("Target Unit virtual player Id: " + oppositeTeam);

        int targetUnitPlayerId = GameNetwork.GetTeamNetworkPlayerId(targetUnitTeam); 

        int targetUnitHealth = GameNetwork.GetUnitRoomPropertyAsInt(GameNetwork.UnitRoomProperties.HEALTH, targetUnitTeam, targetUnitName);
        int targetUnitDefense = GameNetwork.GetAnyPlayerUnitPropertyAsInt(GameNetwork.UnitPlayerProperties.DEFENSE, targetUnitName, targetUnitTeam, targetUnitPlayerId);
        int targetUnitSpecialDefense = _warRef.GetUnitSpecialDefense(targetUnitTeam, targetUnitName);
        int finalDefense = targetUnitDefense + targetUnitSpecialDefense;
        int maxDefense = GameConfig.Instance.MaxUnitDefense;

        Debug.LogWarning("ActionArea::dealDamage -> targetUnitName: " + targetUnitName + " targetUnitDefense: " + targetUnitDefense + " targetUnitSpecialDefense: " + targetUnitSpecialDefense + " finalDefense uncapped: " + finalDefense);

        if (finalDefense > maxDefense)
            finalDefense = maxDefense;

        int finalDamage = Mathf.FloorToInt((float)damage * (float)(1 - (finalDefense / 100.0f)));  //Defense is translated into damage reduction
        if (finalDamage == 0)
            finalDamage = 1;

        int damageDealt = GameNetwork.GetTeamRoomPropertyAsInt(GameNetwork.TeamRoomProperties.DAMAGE_DEALT, OwnerTeamName);
        GameNetwork.SetTeamRoomProperty(GameNetwork.TeamRoomProperties.DAMAGE_DEALT, OwnerTeamName, (damageDealt + finalDamage).ToString());

        int damageReceived = GameNetwork.GetTeamRoomPropertyAsInt(GameNetwork.TeamRoomProperties.DAMAGE_RECEIVED, targetUnitTeam);
        GameNetwork.SetTeamRoomProperty(GameNetwork.TeamRoomProperties.DAMAGE_RECEIVED, targetUnitTeam, (damageReceived + finalDamage).ToString());
        
        Debug.LogWarning("ActionArea::dealDamage -> finalDefense: " + finalDefense + " damage: " + damage + " finalDamage: " + finalDamage.ToString());

        targetUnitHealth -= finalDamage;  
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
