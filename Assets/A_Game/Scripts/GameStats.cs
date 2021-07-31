using GameConstants;
using GameEnums;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStats : SingletonPersistentPrefab<GameStats>
{
    [Header("Only for display. Set at runtime:")]
    public GameModes Mode = GameModes.None;
    public QuestTypes QuestType = QuestTypes.None;

    public int SelectedLevelNumber = 0;
    public string QuestSelectedUnitName = "";

    public bool UsesAiForPvp = false;
    public bool HasPurchased = false;
    public int FightWithoutAdsCount = 0;

    public string SelectedQuestString = nameof(ScoutQuests.None);

    public List<Vector3> PreparationPositions = new List<Vector3>();
    public List<string> TeamUnits = new List<string>();

    public int Rating = 0;

    public List<string> UnitsDamagedInSingleDestroyerAction = new List<string>();

    public Dictionary<string, TeamBoostItemGroup> TeamBoostTokensOwnedByName = new Dictionary<string, TeamBoostItemGroup>();
    //public Dictionary<string, TeamBoostItem> TeamBoostOreItemsOwnedByName = new Dictionary<string, TeamBoostItem>();

    public TeamBoostItemGroup TeamBoostGroupSelected;

    public int SelectedSaveSlot = 1; // 1 or 2
    public bool IsThereServerBackup;

    public bool QuestScoutPending = false;
    public Vector3 LastScoutPosition = Vector3.zero;
    public List<string> QuestUnits = new List<string>();

    public float EnjBalance = 0;

    public ScoutQuests SelectedScoutQuest
    {
        get
        {
            return (ScoutQuests)Enum.Parse(typeof(ScoutQuests), SelectedQuestString);
        }

        set
        {
            SelectedQuestString = value.ToString();
        }
    }

    public SerialQuests SelectedSerialQuest
    {
        get
        {
            return (SerialQuests)Enum.Parse(typeof(SerialQuests), SelectedQuestString);
        }

        set
        {
            SelectedQuestString = value.ToString();
        }
    }
}
