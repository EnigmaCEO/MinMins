using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : SingletonMonobehaviour<GameConfig>
{
    [SerializeField] public Vector2 BattleFieldMaxPos = new Vector2(6, 4.5f);
    [SerializeField] public Vector2 BattleFieldMinPos = new Vector2(-6, -1);
    [SerializeField] public int MaxUnitDefense = 50;

    [SerializeField] public float ProjectilesSpeed = 4;

    [SerializeField] public Vector3 BronzeProjectileBaseDirection = new Vector3(1, 0, 0);
    [SerializeField] public Vector3 SilverProjectileBaseDirection = new Vector3(0.5f, 0.5f, 0);
    [SerializeField] public Vector3 GoldProjectileBaseDirection = new Vector3(0, 1, 0);

    [SerializeField] public float DestroyerActionAreaScaleModifier = 0.33f;
}
