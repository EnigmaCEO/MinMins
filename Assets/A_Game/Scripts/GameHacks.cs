using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;

public class GameHacks : SingletonMonobehaviour<GameHacks>
{
    public bool DecisionTimeFreeze;
    public bool RandomizeUnitTypes;
    public bool AssignEffectByType;

    public bool BuyAndroid;

    public bool HideClouds;

    public bool ForceEnjinRewardOnChest;
    public bool WarVictory;
    public bool ChestHit;

    public bool GuestCameraAsHost;

    public bool ForcePvpAi;
    public bool AllEnjinTeamBoostTokens;
    public bool CreateWarPrepLineRendererOnUpdate;

    public bool EnjinItemCollectedFailure;
    public bool CompletedQuestFailure;
    public bool CompleteQuestOffline;

    public ValueHack ForceQuest = new ValueHack("Torso");

    public ValueHack SinglePlayerLevel = new ValueHack("50");
    public ValueHack AnyQuestProgress = new ValueHack("3");

    public ValueHack ForceTeamBoostReward = new ValueHack("True");
    public ValueHack TeamBoostRewardCategory = new ValueHack("Size");
    public ValueHack SizeBonus = new ValueHack("10");
    public ValueHack SetTeamBoostBonuses = new ValueHack("5");

    public ValueHack BuyResult = new ValueHack("True");

    public ValueHack TimeWaitForRoomPvpAiSet = new ValueHack("10");

    public ValueHack TriggerPlayerDisconnectPopUp = new ValueHack("D");
    public ValueHack TriggerOpponentDisconnectPopUp = new ValueHack("O");

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
    public ValueHack Damage = new ValueHack("1000");
    public ValueHack FightsWithoutAdsMaxCount = new ValueHack("2");
    public ValueHack HasPurchased = new ValueHack("True");
    public ValueHack UnlockArenas = new ValueHack("5");
}
