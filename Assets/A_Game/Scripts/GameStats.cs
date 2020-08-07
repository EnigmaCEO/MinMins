using GameEnums;
using I2.Loc.SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStats : SingletonMonobehaviour<GameStats>
{
    public enum Modes
    {
        None,
        SinglePlayer,
        Pvp, 
        Quest
    }

    [Header("Only for display. Set at runtime:")]
    public Modes Mode = Modes.None;
    public int SelectedLevelNumber = 0;

    public bool UsesAiForPvp = false;
    public bool HasPurchased = false;
    public int FightWithoutAdsCount = 0;

    public Quests ActiveQuest = Quests.None;

    public List<Vector3> PreparationPositions = new List<Vector3>();
    public List<string> TeamUnits = new List<string>();

    public int Rating = 0;

    public List<string> UnitsDamagedInSingleDestroyerAction = new List<string>();

    public Dictionary<string, TeamBoostItem> TeamBoostTokensOwnedByName = new Dictionary<string, TeamBoostItem>();
    //public Dictionary<string, TeamBoostItem> TeamBoostOreItemsOwnedByName = new Dictionary<string, TeamBoostItem>();

    public TeamBoostItem TeamBoostSelected;

    public int SelectedSaveSlot = 1; // 1 or 2
    public bool IsThereServerBackup;
}
