using Enigma.CoreSystems;
using GameEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMinUnit : NetworkEntity
{
    public float Strength = 1;
    public float Defense = 1;

    public int MaxHealth = 5;

    public float EffectScale = 1;

    //public int Effect;
    public UnitRoles Role;
    public int[] Attacks;

    public AbilityEffects AbilityEffect;

    private string _teamName;
    private int _teamIndex;

    private Transform _spriteTransform;

    public string TeamName { get { return _teamName; } }
    public int TeamIndex { get { return _teamIndex; } }

    [HideInInspector] public int Tier = 1;  //Not supposed to be set by inspector but my GameInventory using index thresholds.

    protected override void Awake()
    {
        base.Awake();

        _spriteTransform = transform.Find("Sprite");

        WarUnitSprite warUnitSprite = _spriteTransform.gameObject.AddComponent<WarUnitSprite>();
        warUnitSprite.Unit = this.gameObject;

        /*GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
        shadow.transform.parent = _spriteTransform;
        shadow.transform.localPosition = new Vector2(0, 0);
        shadow.transform.localScale = new Vector2(-1, 1);*/

        object[] data = base.GetInstantiationData();
        if (data != null)
        {
            setUpUnitForWar((string)data[0], (string)data[1], (int)data[2], (float)data[3], (float)data[4], (int)data[5]);
        }

        Tier = GameInventory.Instance.GetUnitTier(gameObject.name);
    }

    static public AbilityEffects GetEffectName(MinMinUnit unit)
    {
        AbilityEffects effect = unit.AbilityEffect;

        if (GameHacks.Instance.AssignDefaultEffectByType)
        {
            if (unit.Role == UnitRoles.Bomber)
            {
                effect = AbilityEffects.FireExplosion;
            }
            else if (unit.Role == UnitRoles.Destroyer)
            {
                effect = AbilityEffects.LightningProjectile;
            }
            else if (unit.Role == UnitRoles.Healer)
            {
                effect = AbilityEffects.LifeArea;
            }
            else if (unit.Role == UnitRoles.Scout)
            {
                effect = AbilityEffects.ScoutLight;
            }
            else if (unit.Role == UnitRoles.Tank)
            {
                effect = AbilityEffects.ShieldEffect;
            }
        }

        return effect;
    }

    public Vector3 GetBattlefieldPosition()
    {
        return _spriteTransform.position;
    }

    private void setUpUnitForWar(string unitName, string teamName, int teamIndex, float posX, float posY, int sizeBonus)
    {
        //Debug.LogWarning("MinMinUnit::setUpUnitForWar -> unitName: " + unitName + " teamName: " + teamName + " teamIndex: " + teamIndex);

        this.name = unitName;
        _teamName = teamName;
        _teamIndex = teamIndex;

        bool isHost = (teamName == TeamNames.HOST);

        Vector3 localScale;

        if (!isHost)
        {
            localScale = _spriteTransform.localScale;
            _spriteTransform.localScale = new Vector3(-localScale.x, localScale.y, localScale.z);
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.SizeBonus.Enabled)
        {
            sizeBonus = GameHacks.Instance.SizeBonus.ValueAsInt;
        }
#endif

        float scaleFactor = (100.0f - (float)sizeBonus) / 100.0f;

        localScale = _spriteTransform.localScale;
        _spriteTransform.localScale = new Vector3(localScale.x * scaleFactor, localScale.y * scaleFactor, localScale.z * scaleFactor);

        _spriteTransform.localPosition = new Vector3(posX, posY, _spriteTransform.localPosition.z);
    }

    public void SendDebugSettingsForWar(UnitRoles unitRoles)
    {
        SendRpcToAll(nameof(receiveDebugSettingsForWar), (int)unitRoles);
    }

    [PunRPC]
    private void receiveDebugSettingsForWar(int unitType)
    {
        Role = (UnitRoles)unitType; 
    }
}
