using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;

public class GameHacks : SingletonPersistentPrefab<GameHacks>
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public bool EnableAllEnjinUnitTokens = false;
    public bool EnableAllEnjinCodeQuests = false;
    public bool DisableEnigmaTokenQuestUnlocks = false;
    public bool LockQuests = false;
    public bool ForceEnjinRewardOnChest = false;
    public bool AllEnjinTeamBoostTokens = false;
    public bool EnjinLinked = false;

    public bool AllowCompletedQuests = false;

    public bool HideClouds = false;
    public bool BuyAndroid = false;
    public bool DecisionTimeFreeze = false;
    public bool WarVictory = false;
    public bool ChestHit = false;
    public bool ForcePvpAi = false;
    public bool TreatBattleResultsAsPrivateMatch = false;

    public bool UseOfflineTestWithdrawnItems = false;
    public List<string> OfflineTestWithdrawnTokenKeys = new List<string>();

    public ValueHack EnjinTransactionResponse = new ValueHack("Success");  //Success, Pending or Error
    public float OfflineEnjinTransactionDelay = 2;
    public ValueHack StartingEnjBalance = new ValueHack("100");

    public ValueHack SinglePlayerLevel = new ValueHack("50");
    //public ValueHack AnyQuestProgress = new ValueHack("3");
    public ValueHack ForceTeamBoostReward = new ValueHack("True");
    public ValueHack TeamBoostRewardCategory = new ValueHack("Size");
    public ValueHack SetGlobalSystemServerQuest = new ValueHack("EnjinLegend122"); 
    public ValueHack SizeBonus = new ValueHack("10");
    public ValueHack SetTeamBoostBonuses = new ValueHack("5");
    public ValueHack BuyResult = new ValueHack("True");
    public ValueHack TimeWaitForRoomPvpAiSet = new ValueHack("10");
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
    public ValueHack UnlockSinglePlayerLevels = new ValueHack("5");
    public ValueHack QuestsPoints = new ValueHack("1000");
    public ValueHack Crystals = new ValueHack("1000");
#endif

    public bool RandomizeUnitTypes = false;
    public bool AssignDefaultEffectByType = false;

    public bool GuestCameraAsHost = false;

    public bool CreateWarPrepLineRendererOnUpdate = false;

    public bool EnjinItemCollectedFailure = false;
    public bool CompleteLevelQuestFail = false;
    public bool CompleteQuestOffline = false;

    public bool ClearSavedTeamDataOnScenesEnter = false;
    public bool NegatePrefsBackup = false;

    public ValueHack TriggerPlayerDisconnectPopUp = new ValueHack("D");
    public ValueHack TriggerOpponentDisconnectPopUp = new ValueHack("O");
}
