using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : SingletonPersistentPrefab<GameConfig>
{
    [SerializeField] public Vector2 BattleFieldMaxPos = new Vector2(6, 4.5f);
    [SerializeField] public Vector2 BattleFieldMinPos = new Vector2(-6, -1);
    [SerializeField] public float BoundsLineRendererOutwardOffset = 0; 
    [SerializeField] public float BoundsActionOutwardOffset = 5;
    //[SerializeField] public int MaxUnitDefense = 50;

    [SerializeField] public float ProjectilesSpeed = 4;

    [SerializeField] public Vector3 BronzeProjectileBaseDirection = new Vector3(1, 0, 0);
    [SerializeField] public Vector3 SilverProjectileBaseDirection = new Vector3(0.5f, 0.5f, 0);
    [SerializeField] public Vector3 GoldProjectileBaseDirection = new Vector3(0, 1, 0);

    [SerializeField] public float RewardChestDestructionDelay = 1;
    [SerializeField] public int FightsWithoutAdsMaxCount = 3;

    [SerializeField] public bool EnableServerBackup = false;
    [SerializeField] public bool SendToShopAutomaticallyOnUnitsNeeded = false;

    [SerializeField] public int MaxQuestLevel = 5;
    [SerializeField] public int QuestUnitExp = 150;

    public Vector2 GetRandomBattlefieldPosition()
    {
        return new Vector2(UnityEngine.Random.Range(BattleFieldMinPos.x + 0.5f, BattleFieldMaxPos.x - 0.5f), UnityEngine.Random.Range(BattleFieldMinPos.y + 0.5f, BattleFieldMaxPos.y - 0.5f));
    }
}
