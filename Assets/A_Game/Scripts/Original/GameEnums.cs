
namespace GameEnums
{
    public enum GameModes
    {
        None,

        SinglePlayer,
        Pvp,
        Quest
    }

    public enum QuestTypes
    {
        None,
        Scout,
        Serial
    }

    public enum ScoutQuests
    {
        None = 0,
        
        //Global System
        EnjinLegend122, 
        EnjinLegend123,
        EnjinLegend124,
        EnjinLegend125,
        EnjinLegend126, 

        NarwhalBlue,
        NarwhalCheese,
        NarwhalEmerald,
        NarwhalCrimson
    }

    public enum SerialQuests
    {
        None = 0,

        ShalwendWargod,
        ShalwendDeadlyKnight
        //Swissborg,
    }

    public enum UnitRoles
    {
        None,

        Healer,
        Bomber,
        Tank,
        Destroyer,
        Scout
    }

    public enum AbilityEffects
    {
        None,

        FireExplosion = 1,
        LifeArea,
        LightningProjectile,
        ScoutLight,
        ShieldEffect,

        FireExplosionDemon,
        LifeAreaDemon,
        LightningProjectileDemon,
        ScoutLightDemon,
        ShieldEffectDemon,
    }

    public enum Powers
    {
        Earth,  //Slows time
        Water,  //Freezes clock
        Wind,   //Switch question
        Fire    //Burns 2 wrong answers
    }
}
