using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHacks : SingletonMonobehaviour<GameHacks>
{
    public bool DecisionTimeFreeze;
    public bool RandomizeUnitTypes;
    public bool AssignEffectByType;

    public bool BuyResultFalse;
    public bool BuyAndroid;

    public ValueHack UnitTier = new ValueHack("1");
    public ValueHack ActionTimeHack = new ValueHack("60");
    public ValueHack SetHostUnitType = new ValueHack("Bomber");
    public ValueHack SetGuestUnitType = new ValueHack("Tank");
    public ValueHack SetAllUnitsType = new ValueHack("Destroyer");
    public ValueHack PowerScale = new ValueHack("5");
    public ValueHack UnitLevel = new ValueHack("5");
    public ValueHack RandomizeUnitLevelWithMaxLevel = new ValueHack("20");
    public ValueHack FractionOfStartingHealth = new ValueHack("2");
    public ValueHack Rating = new ValueHack("150");
    public ValueHack RoundCount = new ValueHack("2");
}
