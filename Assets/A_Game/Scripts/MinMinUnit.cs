using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMinUnit : NetworkEntity
{
    public enum Types
    {
        None,
        Healer,
        Bomber,
        Tank,
        Destroyer,
        Scout,
    }

    public enum EffectNames
    {
        None,
        FireExplosion = 1,
        LifeArea,
        LightningProjectile,
        ScoutLight,
        ShieldEffect,
        Six
    }

    public float Strength = 1;
    public float Defense = 1;

    public int MaxHealth = 5;

    public float EffectScale = 1;

    public int Effect;
    public Types Type;
    public int[] Attacks;

    public EffectNames EffectName;

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

    static public EffectNames GetEffectName(MinMinUnit unit)
    {
        EffectNames effect = unit.EffectName;

        if (GameHacks.Instance.AssignEffectByType)
        {
            if (unit.Type == Types.Bomber)
            {
                effect = EffectNames.FireExplosion;
            }
            else if (unit.Type == Types.Destroyer)
            {
                effect = EffectNames.LightningProjectile;
            }
            else if (unit.Type == Types.Healer)
            {
                effect = EffectNames.LifeArea;
            }
            else if (unit.Type == Types.Scout)
            {
                effect = EffectNames.ScoutLight;
            }
            else if (unit.Type == Types.Tank)
            {
                effect = EffectNames.ShieldEffect;
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

        bool isHost = (teamName == GameNetwork.TeamNames.HOST);

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

    public void SendDebugSettingsForWar(Types unitType)
    {
        SendRpcToAll(nameof(receiveDebugSettingsForWar), (int)unitType);
    }

    [PunRPC]
    private void receiveDebugSettingsForWar(int unitType)
    {
        Type = (Types)unitType;
    }
}
