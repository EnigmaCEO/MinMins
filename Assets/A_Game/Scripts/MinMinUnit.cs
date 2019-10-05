using Enigma.CoreSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMinUnit : NetworkEntity
{
    public enum Types
    {
        Healer,
        Bomber,
        Tank,
        Destroyer,
        Scout,
    }

    public enum EffectNames
    {
        One = 1,
        Two,
        Three,
        Four,
        Five,
        Six
    }

    public int Strength = 1;
    public int Defense = 1;
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

    protected override void Awake()
    {
        base.Awake();

        _spriteTransform = transform.Find("Sprite");

        WarUnitSprite warUnitSprite = _spriteTransform.gameObject.AddComponent<WarUnitSprite>();
        warUnitSprite.Unit = this.gameObject;

        GameObject shadow = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/UI/battle_shadow"));
        shadow.transform.parent = _spriteTransform;
        shadow.transform.localPosition = new Vector2(0, 0);
        shadow.transform.localScale = new Vector2(-1, 1);

        object[] data = base.GetInstantiationData();
        if (data != null)
            setUpUnitForWar((string)data[0], (string)data[1], (int)data[2], (float)data[3], (float)data[4]);
    }

    public Vector3 GetBattlefieldPosition()
    {
        return _spriteTransform.position;
    }

    private void setUpUnitForWar(string unitName, string teamName, int teamIndex, float posX, float posY)
    {
        //Debug.LogWarning("MinMinUnit::setUpUnitForWar -> unitName: " + unitName + " teamName: " + teamName + " teamIndex: " + teamIndex);

        this.name = unitName;
        _teamName = teamName;
        _teamIndex = teamIndex;

        bool isHost = (teamName == GameNetwork.TeamNames.HOST);

        if (!isHost)
        {
            Vector3 localScale = _spriteTransform.localScale;
            _spriteTransform.localScale = new Vector3(-localScale.x, localScale.y, localScale.z);
        }

        _spriteTransform.localPosition = new Vector3(posX, posY, _spriteTransform.localPosition.z);
    }

    public void SendDebugSettingsForWar(Types unitType)
    {
        SendRpcToAll("receiveDebugSettingsForWar", (int)unitType);
    }

    [PunRPC]
    private void receiveDebugSettingsForWar(int unitType)
    {
        Type = (Types)unitType;
    }
}
