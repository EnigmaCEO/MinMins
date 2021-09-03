using CodeStage.AntiCheat.Storage;
using Enigma.CoreSystems;
using EnigmaConstants;
using GameConstants;
using GameEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInventory : SingletonPersistentPrefab<GameInventory>
{
    public class GroupNames
    {
        public const string LOOT_BOXES = "Lootboxes";
        public const string STATS = "Stats";
        public const string ENJIN_TOKENS_WITHDRAWAL = "EnjinTokensWithdrawal";
        public const string UNITS_EXP = "Units";
        public const string ORE = "Ore";
        public const string QUESTS_LEVEL_PROGRESS = "QuestsLevelsProgress";
        public const string QUESTS_SCOUT_PROGRESS = "QuestsScoutProgress";
        public const string QUESTS_ENEMIES_POSITIONS = "QuestsEnemiesPositions";
    }

    public class ItemKeys
    {
        public const string SINGLE_PLAYER_LEVEL = "SinglePlayerLevel";
        public const string ENJIN_ATTEMPTS = "EnjinAttempts";
        public const string WITHDRAWN_TOKENS = "WithdrawnTokens";
        public const string GLOBAL_SYSTEM_ACTIVE_QUEST = "ActiveQuest";
        public const string SWOLESOME_ACTIVE_QUEST = "SwolesomeActiveQuest";
        public const string CRYSTALS = "Crystals";
    }

    [SerializeField] List<int> _experienceNeededPerUnitLevel = new List<int>() { 0, 10, 30, 70, 150, 310 };  //Rule: Doubles each level

    [SerializeField] private int _maxArenaLevel = 50;

    [SerializeField] private int _unitsNecessaryToBattle = 5;

    [SerializeField] private int _unitsAmount = 80;
    [SerializeField] private int _initialBronzeLootBoxes = 2;

    [SerializeField] private int _lootBoxSize = 3;
    [SerializeField] private int _lootBoxTiersAmount = 3;
    [SerializeField] private int _guaranteedUnitTierAmount = 1;

    [SerializeField] private int _demonBoxUnitsAmount = 3;
    [SerializeField] private int _legendBoxUnitAmount = 3;

    [SerializeField] private int _expToAddOnDuplicateUnit = 10;

    [SerializeField] private int _lastTierBronze_unitNumber = 40;
    [SerializeField] private int _lastTierSilver_unitNumber = 70;
    [SerializeField] private int _lastTierGold_unitNumber = 80;

    [SerializeField] private int _tierBronze_unitsAmount = 40;
    [SerializeField] private int _tierSilver_unitsAmount = 30;
    [SerializeField] private int _tierGold_unitsAmount = 10;

    [SerializeField] private int _legend_firstUnitNumber = 100;
    [SerializeField] private int _legend_lastUnitNumer = 134;

    [SerializeField] private int _demonFirstUnitNumber = 110;
    [SerializeField] private int _demonLastUnitNumber = 114;

    [SerializeField] private int _narwhalFirstUnitNumber = 129;
    [SerializeField] private int _narwhalLastUnitNumber = 133;

    [SerializeField] private float _tierBronze_GroupRarity = 0.5f;
    [SerializeField] private float _tierSilver_GroupRarity = 0.3f;
    [SerializeField] private float _tierGold_GroupRarity = 0.2f;

    [SerializeField] private int _enjinUnitStartingLevel = 3;
    [SerializeField] private int _oreMaxBonus = 10;

    [SerializeField] private List<int> _packTiers = new List<int> { 1, 2, 3, 3, 3, 3 };

    private Hashtable _saveHashTable = new Hashtable();

    private List<string> _tierBronze_units = new List<string>();
    private List<string> _tierSilver_units = new List<string>();
    private List<string> _tierGold_units = new List<string>();

    private List<string> _demon_units = new List<string>();
    private List<string> _swolesome_units = new List<string>();
    private List<string> _legend_units = new List<string>();

    //private List<UnitRarity> _unitSpecialRarities = new List<UnitRarity>();
    private Dictionary<SerialQuests, int> _maxLevelBySerialQuest = new Dictionary<SerialQuests, int>();

    private Dictionary<string, double> _rarityByUnit = new Dictionary<string, double>();
    private Dictionary<string, string> _unitNameByToken = new Dictionary<string, string>();
    private Dictionary<string, string> _tokenByUnitName = new Dictionary<string, string>();
    private List<string> _oreTokens = new List<string>();

    private const char _GROUP_WITH_KEY_SEPARATOR = '|';
    private const char QUEST_WITH_LEVEL_SEPARATOR = ':';
    private const char POSITIONS_SEPARATOR = ';';
    private const char COORDS_SEPARATOR = ',';

    private string[] _boostCategories = { GameConstants.BoostCategory.DAMAGE, GameConstants.BoostCategory.DEFENSE,
                                    GameConstants.BoostCategory.HEALTH, GameConstants.BoostCategory.POWER,
                                    GameConstants.BoostCategory.SIZE };

    private string[] _boostBaseNames = { GameConstants.BoostEnjinOreItems.DAMAGE, GameConstants.BoostEnjinOreItems.DEFENSE,
                                   GameConstants.BoostEnjinOreItems.HEALTH, GameConstants.BoostEnjinOreItems.POWER,
                                   GameConstants.BoostEnjinOreItems.SIZE };

    public string[] BoostCategories
    {
        get
        {
            return _boostCategories;
        }
    }

    public string[] BoostBaseNames
    {
        get
        {
            return _boostBaseNames;
        }
    }

    override protected void Awake()
    {
        base.Awake();

        initializeMaxLevelsByLevelUnitQuest();

        //initializeOreTokens();
        initializeUnitNameByToken();
        createUnitTierLists();
        createRarity();
        loadDataFromFile();
        initializeInventory();
    }

    public List<string> GetLegendUnitNames()
    {
        return new List<string>(_legend_units);
    }

    public List<string> GetSwolesomeUnitNames()
    {
        return new List<string>(_swolesome_units);
    }

    public string GetTokenUnitName(string tokenName)
    {
        return _unitNameByToken[tokenName];
    }

    public string GetUnitNameToken(string unitName)
    {
        return _tokenByUnitName[unitName];
    }

    public bool IsThereFileSaved()
    {
        return (_saveHashTable.Count > 0);
    }

    public int GetPackTier(int boxIndex)
    {
        return _packTiers[boxIndex];
    }

    public int GetRandomTier()
    {
        return UnityEngine.Random.Range(BoxTiers.BRONZE, BoxTiers.GOLD + 1);
    }

    public bool HasEnoughUnitsForBattle()
    {
        return (GetInventoryUnitNames().Count >= _unitsNecessaryToBattle);
    }

    public List<string> GetInventoryUnitNames()
    {
        List<string> inventoryUnitIndexes = new List<string>();

        InventoryManager inventoryManager = InventoryManager.Instance;

        if (inventoryManager.CheckGroupExists(GroupNames.UNITS_EXP, false))
        {
            foreach (string unitName in inventoryManager.GetGroupKeys(GroupNames.UNITS_EXP))
            {
                inventoryUnitIndexes.Add(unitName);
            }
        }

        return inventoryUnitIndexes;
    }

    public TeamBoostItemGroup GetOreItemGroup(string itemName)
    {
        return InventoryManager.Instance.GetItem<TeamBoostItemGroup>(GroupNames.ORE, itemName);
    }

    public bool IsThereAnyOreSingleItem()
    {
        List<TeamBoostItemGroup> teamBoostItems = GetOreItemsOwned();

        foreach (TeamBoostItemGroup teamBoost in teamBoostItems)
        {
            if (teamBoost.Amount > 0)
            {
                return true;
            }
        }

        return false;
    }

    public List<TeamBoostItemGroup> GetOreItemsOwned()
    {
        List<TeamBoostItemGroup> oreItems = new List<TeamBoostItemGroup>();

        InventoryManager inventoryManager = InventoryManager.Instance;

        if (inventoryManager.CheckGroupExists(GroupNames.ORE, false))
        {
            foreach (string itemName in inventoryManager.GetGroupKeys(GroupNames.ORE))
            {
                TeamBoostItemGroup item = GetOreItemGroup(itemName);
                oreItems.Add(item);
            }
        }

        return oreItems;
    }

    public void SetHigherSinglePlayerLevelCompleted(int newLevel)
    {
        Debug.LogWarning("GameInventory::SetSinglePlayerLevel -> level: " + newLevel);
        int higherLevelCompleted = GetHigherSinglePlayerLevelCompleted();
        if (newLevel > higherLevelCompleted)
        {
            if (newLevel <= _maxArenaLevel)
            {
                InventoryManager.Instance.UpdateItem(GroupNames.STATS, ItemKeys.SINGLE_PLAYER_LEVEL, newLevel, true);
                saveInventoryItemToFile<int>(GroupNames.STATS, ItemKeys.SINGLE_PLAYER_LEVEL);
            }
        }
    }

    public int GetHigherSinglePlayerLevelCompleted()
    {
        int value = InventoryManager.Instance.GetItem<int>(GroupNames.STATS, ItemKeys.SINGLE_PLAYER_LEVEL);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (GameHacks.Instance.SinglePlayerLevel.Enabled)
        {
            value = GameHacks.Instance.SinglePlayerLevel.ValueAsInt;
        }
#endif

        return value;
    }

    public int GetCrystalsAmount()
    {
        int value = InventoryManager.Instance.GetItem<int>(GroupNames.STATS, ItemKeys.CRYSTALS);
        return value;
    }

    public void ChangeCrystalsAmount(int diff)
    {
        int currentValue = GetCrystalsAmount();
        currentValue += diff;
        InventoryManager.Instance.UpdateItem(GroupNames.STATS, ItemKeys.CRYSTALS, currentValue);
        saveInventoryItemToFile<int>(GroupNames.STATS, ItemKeys.CRYSTALS);
    }

    public void SetGlobalSystemActiveQuest(ScoutQuests activeQuest)
    {
        InventoryManager.Instance.UpdateItem(GroupNames.STATS, ItemKeys.GLOBAL_SYSTEM_ACTIVE_QUEST, activeQuest.ToString());
        saveInventoryItemToFile<string>(GroupNames.STATS, ItemKeys.GLOBAL_SYSTEM_ACTIVE_QUEST);
    }

    public ScoutQuests GetGlobalSystemActiveQuest()
    {
        return (ScoutQuests)Enum.Parse(typeof(ScoutQuests), GetGlobalSystemActiveQuestString());
    }

    public string GetGlobalSystemActiveQuestString()
    {
        return InventoryManager.Instance.GetItem<string>(GroupNames.STATS, ItemKeys.GLOBAL_SYSTEM_ACTIVE_QUEST);
    }

    public string GetGlobalSystemActiveQuestName()
    {
        return GetQuestDisplayName(GetGlobalSystemActiveQuestString());
    }

    public void SetSwolesomeActiveQuest(ScoutQuests activeQuest)
    {
        InventoryManager.Instance.UpdateItem(GroupNames.STATS, ItemKeys.SWOLESOME_ACTIVE_QUEST, activeQuest.ToString());
        saveInventoryItemToFile<string>(GroupNames.STATS, ItemKeys.SWOLESOME_ACTIVE_QUEST);
    }

    public ScoutQuests GetSwolesomeActiveQuest()
    {
        return (ScoutQuests)Enum.Parse(typeof(ScoutQuests), GetSwolesomeActiveQuestString());
    }

    public string GetSwolesomeActiveQuestString()
    {
        return InventoryManager.Instance.GetItem<string>(GroupNames.STATS, ItemKeys.SWOLESOME_ACTIVE_QUEST);
    }

    public string GetSwolesomeActiveQuestDisplayName()
    {
        return GetQuestDisplayName(GetSwolesomeActiveQuestString());
    }

    public string GetSelectedQuestDisplayName()
    {
        return GetQuestDisplayName(GameStats.Instance.SelectedQuestString);
    }

    public string GetQuestDisplayName(string questString)
    {
        string enjinLegendLocalized = LocalizationManager.GetTermTranslation("Enjin Legend:");
        string questName = enjinLegendLocalized + " ";

        switch(questString)
        {
            case nameof(ScoutQuests.EnjinLegend122):
                questName += "#122";
                break;
            case nameof(ScoutQuests.EnjinLegend123):
                questName += "#123";
                break;
            case nameof(ScoutQuests.EnjinLegend124):
                questName += "#124";
                break;
            case nameof(ScoutQuests.EnjinLegend125):
                questName += "#125";
                break;

            case nameof(ScoutQuests.EnjinLegend126):
                questName += "#126";
                break;

            case nameof(ScoutQuests.NarwhalBlue):
                questName = "Blue Narwhal";
                break;
            case nameof(ScoutQuests.NarwhalCheese):
                questName = "Cheese Narwhal";
                break;
            case nameof(ScoutQuests.NarwhalEmerald):
                questName = "Emerald Narwhal";
                break;
            case nameof(ScoutQuests.NarwhalCrimson):
                questName = "Crimson Narwhal";
                break;

            case nameof(SerialQuests.ShalwendWargod):
                questName = "Wargod Shalwend";
                break;
            case nameof(SerialQuests.ShalwendDeadlyKnight):
                questName = "Deadly Knight Shalwend";
                break;
        }

        return questName;
    }

    public string GetEnjinKeyDisplayName(string enjinKey, bool addTokenSuffixIfNecessary)
    {
        string enjinKeyName = enjinKey;

        switch (enjinKey)
        {
            case EnigmaConstants.EnjinTokenKeys.ENIGMA_TOKEN:
                enjinKeyName = "Enigma Token";
                break;
            case EnigmaConstants.EnjinTokenKeys.ENJIN_MFT:
                enjinKeyName = "Enjin MFT";
                break;

            case GameConstants.EnjinTokenKeys.MINMINS_TOKEN:
                enjinKeyName = "Min Mins Token";
                break;

            case GameConstants.EnjinTokenKeys.QUEST_WARGOD_SHALWEND:
                enjinKeyName = "Rank Wargod";
                break;
            case GameConstants.EnjinTokenKeys.QUEST_DEADLY_KNIGHT_SHALWEND:
                enjinKeyName = "Black Token";
                break;

            case GameConstants.EnjinTokenKeys.SHALWEND_WARGOD:
                enjinKeyName = "Wargod Shalwend";
                break;
            case GameConstants.EnjinTokenKeys.SHALWEND_DEADLY_KNIGHT:
                enjinKeyName = "Deadly Knight Shalwend";
                break;

            case GameConstants.EnjinTokenKeys.SWISSBORG_CYBORG:
                enjinKeyName = "Swissborg Cyborg";
                break;

            case EnjinCodeKeys.QUEST_BLUE_NARWHAL:
            case GameConstants.EnjinTokenKeys.NARWHAL_BLUE:
                enjinKeyName = "Blue Narwhal";
                break;
            case EnjinCodeKeys.QUEST_CHEESE_NARWHAL:
            case GameConstants.EnjinTokenKeys.NARWHAL_CHEESE:
                enjinKeyName = "Cheese Narwhal";
                break;
            case EnjinCodeKeys.QUEST_EMERALD_NARWHAL:
            case GameConstants.EnjinTokenKeys.NARWHAL_EMERALD:
                enjinKeyName = "Emerald Narwhal";
                break;
            case EnjinCodeKeys.QUEST_CRIMSON_NARWHAL:
            case GameConstants.EnjinTokenKeys.NARWHAL_CRIMSON:
                enjinKeyName = "Crimson Narwhal";
                break;
            case GameConstants.EnjinTokenKeys.NARWHAL_DIAMOND:
                enjinKeyName = "Diamond Narwhal";
                break;

            case GameConstants.EnjinTokenKeys.ENJIN_MAXIM:
                enjinKeyName = "Maxim Legend";
                break;
            case GameConstants.EnjinTokenKeys.ENJIN_WITEK:
                enjinKeyName = "Witek Legend";
                break;
            case GameConstants.EnjinTokenKeys.ENJIN_BRYANA:
                enjinKeyName = "Bryana Legend";
                break;
            case GameConstants.EnjinTokenKeys.ENJIN_TASSIO:
                enjinKeyName = "Tassio Legend";
                break;
            case GameConstants.EnjinTokenKeys.ENJIN_SIMON:
                enjinKeyName = "Simon Legend";
                break;

            case GameConstants.EnjinTokenKeys.ENJIN_ESTHER:
                enjinKeyName = "Esther Legend";
                break;
            case GameConstants.EnjinTokenKeys.ENJIN_ALEX:
                enjinKeyName = "Alex Legend";
                break;
            case GameConstants.EnjinTokenKeys.ENJIN_EVAN:
                enjinKeyName = "Evan Legend";
                break;
            case GameConstants.EnjinTokenKeys.ENJIN_LIZZ:
                enjinKeyName = "Lizz Legend";
                break;
            case GameConstants.EnjinTokenKeys.ENJIN_BRAD:
                enjinKeyName = "Brad Legend";
                break;

            case GameConstants.EnjinTokenKeys.KNIGHT_HEALER:
                enjinKeyName = "Deadly Knight Healer";
                break;
            case GameConstants.EnjinTokenKeys.KNIGHT_BOMBER:
                enjinKeyName = "Deadly Knight Bomber";
                break;
            case GameConstants.EnjinTokenKeys.KNIGHT_DESTROYER:
                enjinKeyName = "Deadly Knight Destroyer";
                break;
            case GameConstants.EnjinTokenKeys.KNIGHT_SCOUT:
                enjinKeyName = "Deadly Knight Scout";
                break;
            case GameConstants.EnjinTokenKeys.KNIGHT_TANK:
                enjinKeyName = "Deadly Knight Tank";
                break;

            case GameConstants.EnjinTokenKeys.DEMON_HEALER:
                enjinKeyName = "Demon King Healer";
                break;
            case GameConstants.EnjinTokenKeys.DEMON_BOMBER:
                enjinKeyName = "Demon King Bomber";
                break;
            case GameConstants.EnjinTokenKeys.DEMON_DESTROYER:
                enjinKeyName = "Demon King Destroyer";
                break;
            case GameConstants.EnjinTokenKeys.DEMON_SCOUT:
                enjinKeyName = "Demon King Scout";
                break;
            case GameConstants.EnjinTokenKeys.DEMON_TANK:
                enjinKeyName = "Demon King Tank";
                break;

            case GameConstants.EnjinTokenKeys.GOD_HEALER:
                enjinKeyName = "God Healer";
                break;
            case GameConstants.EnjinTokenKeys.GOD_BOMBER:
                enjinKeyName = "God Bomber";
                break;
            case GameConstants.EnjinTokenKeys.GOD_DESTROYER_1:
                enjinKeyName = "God Destroyer 1";
                break;
            case GameConstants.EnjinTokenKeys.GOD_DESTROYER_2:
                enjinKeyName = "God Destroyer 2";
                break;
            case GameConstants.EnjinTokenKeys.GOD_SCOUT:
                enjinKeyName = "God Scout";
                break;
            case GameConstants.EnjinTokenKeys.GOD_TANK_1:
                enjinKeyName = "God Tank 1";
                break;
            case GameConstants.EnjinTokenKeys.GOD_TANK_2:
                enjinKeyName = "God Tank 2";
                break;
        }

        if (addTokenSuffixIfNecessary)
        {
            if (!enjinKeyName.Contains(LocalizationTerms.TOKEN))
            {
                enjinKeyName += " " + LocalizationManager.GetTermTranslation(LocalizationTerms.TOKEN);
            }
        }

        return enjinKeyName;
    }

    public Sprite GetQuestRewardSprite(string questString)
    {
        string rewardImagePath = "";

        switch (questString)
        {
            case nameof(ScoutQuests.EnjinLegend122):
                rewardImagePath = ResourcePaths.UNIT_IMAGES + "122";
                break;
            case nameof(ScoutQuests.EnjinLegend123):
                rewardImagePath = ResourcePaths.UNIT_IMAGES + "123";
                break;
            case nameof(ScoutQuests.EnjinLegend124):
                rewardImagePath = ResourcePaths.UNIT_IMAGES + "124";
                break;
            case nameof(ScoutQuests.EnjinLegend125):
                rewardImagePath = ResourcePaths.UNIT_IMAGES + "125";
                break;
            case nameof(ScoutQuests.EnjinLegend126):
                rewardImagePath = ResourcePaths.UNIT_IMAGES + "126";
                break;
            case nameof(SerialQuests.ShalwendWargod):
                rewardImagePath = ResourcePaths.UNIT_IMAGES + "128";
                break;
            case nameof(SerialQuests.ShalwendDeadlyKnight):
                rewardImagePath = ResourcePaths.UNIT_IMAGES + "134";
                break;
            case nameof(ScoutQuests.NarwhalBlue):
                rewardImagePath = ResourcePaths.UNIT_IMAGES + "130";
                break;
            case nameof(ScoutQuests.NarwhalCheese):
                rewardImagePath = ResourcePaths.UNIT_IMAGES + "131";
                break;
            case nameof(ScoutQuests.NarwhalEmerald):
                rewardImagePath = ResourcePaths.UNIT_IMAGES + "132";
                break;
            case nameof(ScoutQuests.NarwhalCrimson):
                rewardImagePath = ResourcePaths.UNIT_IMAGES + "133";
                break;
            default:
                Debug.LogError("There is not image path for quest: " + questString);
                return null;
        }

        Sprite questRewardSprite = (Sprite)Resources.Load<Sprite>(rewardImagePath);

        if (questRewardSprite != null)
        {
            return questRewardSprite;
        }
        else
        {
            Debug.Log("Quest reward image was not found at path: " + rewardImagePath + " . Please check active quest is correct and image is in the right path.");
            return null;
        }
    }

    public void SetQuestLevelCompleted(int newLevel)
    {
        Debug.LogWarning("GameInventory::SetQuestLevelCompleted -> level: " + newLevel);

        string selectedQuestString = GameStats.Instance.SelectedQuestString;
        string questLevelKey = selectedQuestString + QUEST_WITH_LEVEL_SEPARATOR + newLevel;

        InventoryManager.Instance.UpdateItem(GroupNames.QUESTS_LEVEL_PROGRESS, questLevelKey, true, true);
        saveInventoryItemToFile<bool>(GroupNames.QUESTS_LEVEL_PROGRESS, questLevelKey);
        //saveQuestLevelProgress();
    }

    public bool GetQuestLevelCompleted(string questString, int level)
    {
        bool levelCompleted = InventoryManager.Instance.GetItem<bool>(GroupNames.QUESTS_LEVEL_PROGRESS,
                                                                      questString + QUEST_WITH_LEVEL_SEPARATOR + level);

        return levelCompleted;
    }

    public bool GetAllGlobalSystemQuestLevelsCompleted()
    {
        return getQuestLevelsCompleted(GameConfig.Instance.MaxQuestLevel, GameInventory.Instance.GetGlobalSystemActiveQuestString());
    }

    public bool GetAllSwolesomeQuestLevelsCompleted()
    {
        return getQuestLevelsCompleted(GameConfig.Instance.NarwhalMaxQuestLevel, GameInventory.Instance.GetSwolesomeActiveQuestString());
    }

    private bool getQuestLevelsCompleted(int maxQuestLevel, string questString)
    {
        string swoleSomeActiveQuestString = GameInventory.Instance.GetSwolesomeActiveQuestString();

        for (int i = 1; i <= maxQuestLevel; i++)
        {
            if (!GetQuestLevelCompleted(questString, i))
            {
                return false;
            }
        }

        return true;
    }

    //public void SetQuestLevelProgress(int newLevel)
    //{
    //    Debug.LogWarning("GameInventory::SetQuestLevelProgress -> level: " + newLevel);

    //    int higherLevelCompleted = GetHighestQuestLevelCompleted();
    //    if (newLevel > higherLevelCompleted)
    //    {
    //        Quests activeQuest = GameStats.Instance.ActiveQuest;
    //        if (newLevel <= _maxLevelByQuest[activeQuest])
    //        {
    //            InventoryManager.Instance.UpdateItem(GroupNames.QUESTS_PROGRESS, activeQuest.ToString(), newLevel, true);
    //            saveQuestLevelProgress();
    //        }
    //    }
    //}

    public bool GetSelectedQuestCompleted()
    {
        return GetQuestCompleted(GameStats.Instance.SelectedQuestString);
    }

    public bool GetQuestCompleted(string questString)
    {
        bool questCompleted = true;

        if (questString == Constants.NONE)
        {
            Debug.LogError("Attempting to get progress for an active quest of None.");
            questCompleted = false;
        }
        else
        {
            int maxQuestLevel = GetQuestMaxLevel(questString);

            for (int i = 1; i <= maxQuestLevel; i++)
            {
                string questLevelString = questString + QUEST_WITH_LEVEL_SEPARATOR + i;
                bool levelIsCompleted = InventoryManager.Instance.GetItem<bool>(GroupNames.QUESTS_LEVEL_PROGRESS, questLevelString);

                if (!levelIsCompleted)
                {
                    questCompleted = false;
                    break;
                }
            }
        }

        return questCompleted;
    }

    public int GetLevelMinBonus(int levelNumber)
    {
        return levelNumber;
    }

    public int GetLevelMaxBonus(int levelNumber)
    {
        return levelNumber * 2;
    }

    public int GetQuestMaxLevel(string questString)
    {
        GameConfig gameConfig = GameConfig.Instance;
        int maxQuestLevel = gameConfig.MaxQuestLevel;

        if (questString == nameof(SerialQuests.ShalwendWargod))
        {
            maxQuestLevel = GetSerialQuestMaxLevel(SerialQuests.ShalwendWargod);
        }
        else if (questString == nameof(SerialQuests.ShalwendDeadlyKnight))
        {
            maxQuestLevel = GetSerialQuestMaxLevel(SerialQuests.ShalwendDeadlyKnight);
        }
        else if (questString.Contains("Narwhal"))
        {
            maxQuestLevel = gameConfig.NarwhalMaxQuestLevel;
        }

        return maxQuestLevel;
    }

    private void initializeMaxLevelsByLevelUnitQuest()
    {
        //_maxLevelByQuest.Add(Quests.Swissborg, 4);
        _maxLevelBySerialQuest.Add(SerialQuests.ShalwendWargod, 4);
        _maxLevelBySerialQuest.Add(SerialQuests.ShalwendDeadlyKnight, 4);
    }

    public int GetSerialQuestMaxLevel(SerialQuests serialQuest)
    {
        if (_maxLevelBySerialQuest.ContainsKey(serialQuest))
        {
            return _maxLevelBySerialQuest[serialQuest];
        }
        else
        {
            Debug.LogError("");
            return 0;
        }
    }

    public int GetHighestQuestLevelCompletedAmount()
    {
        if ((GameStats.Instance.SelectedQuestString == Constants.NONE))
        {
            Debug.LogError("Attempting to get progress for an active quest of None.");
            return -1;
        }
        else
        {
            int amount = 0;
            InventoryManager inventoryManager = InventoryManager.Instance;

            for (int i = 1; i <= GameConfig.Instance.MaxQuestLevel; i++)
            {
                if (GetQuestLevelCompleted(GameStats.Instance.SelectedQuestString, i))
                {
                    amount++;
                }
            }

            return amount;
        }
    }

    //    public int GetHighestQuestLevelCompleted()
    //    {
    //        Quests activeQuest = GameStats.Instance.ActiveQuest;

    //        if (activeQuest == Quests.None)
    //        {
    //            Debug.LogError("Attempting to get progress for an active quest of None.");
    //            return -1;
    //        }
    //        else
    //        {
    //            int value = InventoryManager.Instance.GetItem<int>(GroupNames.QUESTS_PROGRESS, activeQuest.ToString());

    //#if DEVELOPMENT_BUILD || UNITY_EDITOR
    //            if (GameHacks.Instance.AnyQuestProgress.Enabled)
    //            {
    //                value = GameHacks.Instance.AnyQuestProgress.ValueAsInt;
    //            }
    //#endif

    //            return value;
    //        }
    //    }

    public void SetScoutQuestEnemiesPositions(ScoutQuests quest, List<Vector3> positions)
    {
        string enemiesPositionsString = "";

        foreach (Vector3 pos in positions)
        {
            if (enemiesPositionsString != "")
            {
                enemiesPositionsString += POSITIONS_SEPARATOR;
            }

            enemiesPositionsString += pos.x.ToString() + COORDS_SEPARATOR + pos.y.ToString() + COORDS_SEPARATOR + pos.z.ToString();
        }

        string questString = quest.ToString();
        InventoryManager.Instance.UpdateItem(GroupNames.QUESTS_ENEMIES_POSITIONS, questString, enemiesPositionsString);
        saveInventoryItemToFile<string>(GroupNames.QUESTS_ENEMIES_POSITIONS, questString);
    }

    public List<Vector3> GetScoutQuestEnemiesPositions(ScoutQuests quest)
    {
        string enemiesPositionsString = InventoryManager.Instance.GetItem<string>(GroupNames.QUESTS_ENEMIES_POSITIONS, quest.ToString());
        return getPositionsFromString(enemiesPositionsString);
    }

    public void ClearQuestScoutProgress(ScoutQuests quest)
    {
        string questString = quest.ToString();
        InventoryManager.Instance.UpdateItem(GroupNames.QUESTS_SCOUT_PROGRESS, questString, "");
        saveInventoryItemToFile<string>(GroupNames.QUESTS_SCOUT_PROGRESS, questString);
    }

    public void SetQuestNewScoutPosition(ScoutQuests quest, Vector3 newPos)
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        string questString = quest.ToString();

        string scoutProgressString = inventoryManager.GetItem<string>(GroupNames.QUESTS_SCOUT_PROGRESS, questString);

        if (scoutProgressString != "")
        {
            scoutProgressString += POSITIONS_SEPARATOR;
        }

        scoutProgressString += newPos.x.ToString() + COORDS_SEPARATOR + newPos.y.ToString() + COORDS_SEPARATOR + newPos.z.ToString();

        inventoryManager.UpdateItem(GroupNames.QUESTS_SCOUT_PROGRESS, questString, scoutProgressString);
        saveInventoryItemToFile<string>(GroupNames.QUESTS_SCOUT_PROGRESS, questString);
    }

    public List<Vector3> GetQuestScoutProgress(ScoutQuests quest)
    {
        string scoutProgressString = InventoryManager.Instance.GetItem<string>(GroupNames.QUESTS_SCOUT_PROGRESS, quest.ToString());
        return getPositionsFromString(scoutProgressString);
    }

    public int GetEnjinAttempts()
    {
        return InventoryManager.Instance.GetItem<int>(GroupNames.STATS, ItemKeys.ENJIN_ATTEMPTS);
    }

    public void AddExpToUnit(string unitName, int expToAdd)
    {
        if (GameNetwork.Instance.GetIsEnjinKeyAvailable(GameConstants.EnjinTokenKeys.MINMINS_TOKEN))
        {
            expToAdd *= 2; // Min Min Token perk
        }

        int unitExp = InventoryManager.Instance.GetItem<int>(GroupNames.UNITS_EXP, unitName);
        unitExp += expToAdd;

        int maxExp = getMaxUnitExperience();
        if (unitExp > maxExp)
        {
            unitExp = maxExp;
        }

        InventoryManager.Instance.UpdateItem(GroupNames.UNITS_EXP, unitName, unitExp);
    }

    public int GetLocalUnitExp(string unitName)
    {
        return InventoryManager.Instance.GetItem<int>(GroupNames.UNITS_EXP, unitName);
    }

    public int GetLocalUnitLevel(string unitName)
    {
        return GetLocalUnitExpData(unitName).Level;
    }

    public MinMinUnit GetMinMinFromResources(string unitName)
    {
        GameObject minMinPrefab = Resources.Load<GameObject>("Prefabs/MinMinUnits/" + unitName);
        MinMinUnit minMin = minMinPrefab.GetComponent<MinMinUnit>();
        return minMin;
    }

    public int GetLootBoxIndexAmount(int boxIndex)
    {
        return InventoryManager.Instance.GetItem<int>(GroupNames.LOOT_BOXES, boxIndex.ToString());
    }

    public bool HasAnyLootBox()
    {
        return (GetLootBoxesTotalAmount() > 0);
    }

    public int GetLootBoxesTotalAmount()
    {
        return (GetLootBoxIndexAmount(BoxIndexes.STARTER) + GetLootBoxIndexAmount(BoxIndexes.MASTER) + GetLootBoxIndexAmount(BoxIndexes.PREMIUM)
                + GetLootBoxIndexAmount(BoxIndexes.SPECIAL) + GetLootBoxIndexAmount(BoxIndexes.DEMON) + GetLootBoxIndexAmount(BoxIndexes.LEGEND));
    } 

    public void ChangeLootBoxAmount(int amount, int boxIndex, bool isAddition, bool shouldSave)
    {
        int newIndexLootBoxesAmount = InventoryManager.Instance.GetItem<int>(GroupNames.LOOT_BOXES, boxIndex.ToString());

        if (isAddition)
        {
            newIndexLootBoxesAmount += amount;
        }
        else
        {
            newIndexLootBoxesAmount -= amount;
        }

        InventoryManager.Instance.UpdateItem(GroupNames.LOOT_BOXES, boxIndex.ToString(), newIndexLootBoxesAmount);

        if (shouldSave)
        {
            SaveLootBoxes();
        }
    }

    public List<string> OpenLootBox(int boxIndex)
    {
        LootBoxManager lootBoxManager = LootBoxManager.Instance;

        int boxTier = GetPackTier(boxIndex);

        List<string> unitPicks = null;

        if (boxIndex == BoxIndexes.DEMON)
        {
            unitPicks = lootBoxManager.PickRandomizedNames(_demonBoxUnitsAmount, true, _demon_units, null, true);
        }
        else if (boxIndex == BoxIndexes.LEGEND)
        {
            unitPicks = lootBoxManager.PickRandomizedNames(_legendBoxUnitAmount, true, _legend_units, null, true);
        }
        else if (boxTier == BoxTiers.BRONZE)
        {
            unitPicks = lootBoxManager.PickRandomizedNames(_lootBoxSize, true, null, _rarityByUnit);
        }
        else
        {
            int specialRarityAmountToPick = _lootBoxSize - _guaranteedUnitTierAmount;
            unitPicks = lootBoxManager.PickRandomizedNames(specialRarityAmountToPick, true, null, _rarityByUnit);
            List<string> guaranteedTierUnits = null;
            if (boxTier == BoxTiers.SILVER)
            {
                guaranteedTierUnits = _tierSilver_units;
            }
            else //tier 3 (GOLD)
            {
                guaranteedTierUnits = _tierGold_units;
            }

            foreach (string pick in unitPicks)
            {
                if (guaranteedTierUnits.Contains(pick))
                {
                    guaranteedTierUnits.Remove(pick);  // Remove so guaranted unit pick cannot be already in the default rarity pick
                }
            }

            List<string> guaranteedPicks = new List<string>();
            guaranteedPicks = lootBoxManager.PickRandomizedNames(_guaranteedUnitTierAmount, true, guaranteedTierUnits, null, true);

            foreach (string guaranteedUnitName in guaranteedPicks)
            {
                unitPicks.Add(guaranteedUnitName);
            }
        }

        //List<int> lootBoxIndexes = LootBoxManager.Instance.PickRandomizedIndexes(lootBoxSize, false, _defaultRarityIndexes, specialRarities); //test

        foreach (string lootBoxUnitName in unitPicks)
        {
            print("LootBoxUnitName got: " + lootBoxUnitName);
            string unitName = lootBoxUnitName.ToString();
            HandleAddUnitOrExp(unitName);
        }

        SaveUnits();
        ChangeLootBoxAmount(1, boxIndex, false, true);

        return unitPicks;
    }

    public void HandleAddUnitOrExp(string unitName)
    {
        if (InventoryManager.Instance.HasItem(GroupNames.UNITS_EXP, unitName, false))
        {
            AddExpToUnit(unitName, _expToAddOnDuplicateUnit);
            print("Added " + _expToAddOnDuplicateUnit + " exp to unit " + unitName);
        }
        else
        {
            AddUnit(unitName, 0);
        }
    }

    public void AddUnit(string unitName, int exp, bool save = true)
    {
        InventoryManager.Instance.AddItem(GroupNames.UNITS_EXP, unitName, exp);

        if (save)
        {
            SaveUnits();
        }
    }

    public bool HasUnit(string unitName)
    {
        return InventoryManager.Instance.HasItem(GroupNames.UNITS_EXP, unitName, false);
    }

    public void HandleUnitDeath(string unitName)
    {
        InventoryManager.Instance.RemoveItem(GroupNames.UNITS_EXP, unitName);
        SaveUnits();
    }

    public int GetUnitTier(string unitName)
    {
        int unitTier = BoxTiers.BRONZE;
        if (_tierSilver_units.Contains(unitName))
        {
            unitTier = BoxTiers.SILVER;
        }
        else if (_tierGold_units.Contains(unitName))
        {
            unitTier = BoxTiers.GOLD;
        }

        return unitTier;
    }

    public void SaveUnits()
    {
        removeGroupFromSaveHashTable(GroupNames.UNITS_EXP);

        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string unitName in inventoryManager.GetGroupKeys(GroupNames.UNITS_EXP))
        {
            string groupName = GroupNames.UNITS_EXP;
            int unitExp = InventoryManager.Instance.GetItem<int>(groupName, unitName);
            string hashKey = groupName + _GROUP_WITH_KEY_SEPARATOR + unitName;
            //Debug.LogWarning("GameInventory::saveUnit -> hashKey: " + hashKey + " -> unitExp: " + unitExp);

            saveHashKey(hashKey, unitExp, false);
        }

        saveHashTableToFileAndBackup();
    }

    public int GetSinglePlayerMaxLevel()
    {
        return _maxArenaLevel;
    }

    public void SaveLootBoxes()
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string tier in inventoryManager.GetGroupKeys(GroupNames.LOOT_BOXES))
        {
            int tierAmount = InventoryManager.Instance.GetItem<int>(GroupNames.LOOT_BOXES, tier);
            string hashKey = GroupNames.LOOT_BOXES + _GROUP_WITH_KEY_SEPARATOR + tier;

            print("SaveLootBoxes -> hashKey: " + hashKey + " -> tierAmount: " + tierAmount.ToString());
            saveHashKey(hashKey, tierAmount, false);
        }

        saveHashTableToFileAndBackup();
    }

    public List<string> GetRandomUnitsFromTier(int amount, int tier)
    {
        List<string> tierUnits = _tierBronze_units;
        if (tier == BoxTiers.SILVER)
        {
            tierUnits = _tierSilver_units;
        }
        else if (tier == BoxTiers.GOLD)
        {
            tierUnits = _tierGold_units;
        }

        List<string> unitNames = LootBoxManager.Instance.PickRandomizedNames(amount, true, tierUnits);

        return unitNames;
    }

    public ExpData GetLocalUnitExpData(string unitName)
    {
        int unitExp = GetLocalUnitExp(unitName);
        return GetUnitExpData(unitExp);
    }

    public ExpData GetUnitExpData(int unitExp)
    {
        ExpData expData = new ExpData();

        int thresholdAmountToCheck = _experienceNeededPerUnitLevel.Count - 1;
        for (int i = 0; i < thresholdAmountToCheck; i++)
        {
            int threshold = _experienceNeededPerUnitLevel[i];
            if (unitExp >= threshold)
            {
                expData.ExpForPreviousLevel = threshold;
                expData.Level = i + 1;
                expData.ExpForNextLevel = _experienceNeededPerUnitLevel[i + 1];
            }
        }

        return expData;
    }

    public void AddMissingEnjinUnits()
    {
        if (GetEnjinAttempts() < 1)
        {
            return;
        }

        saveEnjinAttempts(GetEnjinAttempts() - 1);
        int enjinUnitStartingExperience = _experienceNeededPerUnitLevel[_enjinUnitStartingLevel - 1];

        for (int i = _legend_firstUnitNumber; i <= _legend_lastUnitNumer; i++)
        {
            string unitName = i.ToString();
            if (!HasUnit(unitName))
            {
                AddUnit(unitName, 0);
            }
        }

        SaveUnits();
    }

    public void AddMinMinEnjinUnits()
    {
        GameInventory gameInventory = GameInventory.Instance;
        GameNetwork gameNetwork = GameNetwork.Instance;

        for (int i = _legend_firstUnitNumber; i <= _legend_lastUnitNumer; i++)
        {
            string unitName = i.ToString();
            if (gameNetwork.GetIsEnjinKeyAvailable(gameInventory.GetUnitNameToken(unitName)) && !HasUnit(unitName))
            {
                AddUnit(unitName, 0);
            }
        }

        SaveUnits();
    }


    public bool HasAllEnjinUnits()
    {
        bool hasAllEnjinUnits = true;

        for (int i = _legend_firstUnitNumber; i <= _legend_lastUnitNumer; i++)
        {
            string unitName = i.ToString();

            if (!HasUnit(unitName))
            {
                hasAllEnjinUnits = false;
                break;
            }
        }
        return hasAllEnjinUnits;
    }

    public bool CheckCanBeWithdrawn(string unitName)
    {
        bool canBeWithdrawn = true; 
        int unitNumber = int.Parse(unitName);

        if (unitNumber < _legend_firstUnitNumber)
        {
            canBeWithdrawn = false;
        }
        else if (unitName == GetTokenUnitName(GameConstants.EnjinTokenKeys.SWISSBORG_CYBORG))
        {
            canBeWithdrawn = false;
        }
        //else if (checkUnitBetweenTokens(unitNumber, GameConstants.EnjinTokenKeys.ENJIN_MAXIM, GameConstants.EnjinTokenKeys.ENJIN_SIMON))
        //{
        //    canBeWithdrawn = false;
        //}
        //else if (checkUnitBetweenTokens(unitNumber, GameConstants.EnjinTokenKeys.ENJIN_ALEX, GameConstants.EnjinTokenKeys.ENJIN_LIZZ))
        //{
        //    canBeWithdrawn = false;
        //}

        return canBeWithdrawn;
    }

    private bool checkUnitBetweenTokens(int unitNumber, string firstUnitTokenKey, string lastUnitTokenKey)
    {
        int firstUnitNumber = int.Parse(GetTokenUnitName(firstUnitTokenKey));
        int lastUnitNumber = int.Parse(GetTokenUnitName(lastUnitTokenKey));

        return ((unitNumber >= firstUnitNumber) && (unitNumber <= lastUnitNumber));

    }

    public List<string> GetOreTokens()
    {
        return new List<string>(_oreTokens);
    }

    //private void initializeOreTokens()
    //{
    //    addOreToken(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_1);
    //    addOreToken(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_2);
    //    addOreToken(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_3);
    //    addOreToken(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_4);
    //    addOreToken(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_5);
    //    addOreToken(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_6);
    //    addOreToken(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_7);
    //    addOreToken(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_8);
    //    addOreToken(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_9);
    //    addOreToken(EnjinTokenKeys.ENJIN_DAMAGE_ORE_ITEM_10);

    //    addOreToken(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_1);
    //    addOreToken(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_2);
    //    addOreToken(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_3);
    //    addOreToken(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_4);
    //    addOreToken(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_5);
    //    addOreToken(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_6);
    //    addOreToken(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_7);
    //    addOreToken(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_8);
    //    addOreToken(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_9);
    //    addOreToken(EnjinTokenKeys.ENJIN_DEFENSE_ORE_ITEM_10);

    //    addOreToken(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_1);
    //    addOreToken(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_2);
    //    addOreToken(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_3);
    //    addOreToken(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_4);
    //    addOreToken(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_5);
    //    addOreToken(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_6);
    //    addOreToken(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_7);
    //    addOreToken(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_8);
    //    addOreToken(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_9);
    //    addOreToken(EnjinTokenKeys.ENJIN_HEALTH_ORE_ITEM_10);

    //    addOreToken(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_1);
    //    addOreToken(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_2);
    //    addOreToken(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_3);
    //    addOreToken(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_4);
    //    addOreToken(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_5);
    //    addOreToken(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_6);
    //    addOreToken(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_7);
    //    addOreToken(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_8);
    //    addOreToken(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_9);
    //    addOreToken(EnjinTokenKeys.ENJIN_POWER_ORE_ITEM_10);

    //    addOreToken(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_1);
    //    addOreToken(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_2);
    //    addOreToken(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_3);
    //    addOreToken(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_4);
    //    addOreToken(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_5);
    //    addOreToken(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_6);
    //    addOreToken(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_7);
    //    addOreToken(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_8);
    //    addOreToken(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_9);
    //    addOreToken(EnjinTokenKeys.ENJIN_SIZE_ORE_ITEM_10);
    //}

    //private void addOreToken(string tokenKey)
    //{
    //    _oreTokens.Add(tokenKey);
    //}

    private void initializeUnitNameByToken()
    {
        linkUnitAndToken("100", GameConstants.EnjinTokenKeys.ENJIN_MAXIM);
        linkUnitAndToken("101", GameConstants.EnjinTokenKeys.ENJIN_WITEK);
        linkUnitAndToken("102", GameConstants.EnjinTokenKeys.ENJIN_BRYANA);
        linkUnitAndToken("103", GameConstants.EnjinTokenKeys.ENJIN_TASSIO);
        linkUnitAndToken("104", GameConstants.EnjinTokenKeys.ENJIN_SIMON);

        linkUnitAndToken("105", GameConstants.EnjinTokenKeys.KNIGHT_TANK);
        linkUnitAndToken("106", GameConstants.EnjinTokenKeys.KNIGHT_HEALER);
        linkUnitAndToken("107", GameConstants.EnjinTokenKeys.KNIGHT_SCOUT);
        linkUnitAndToken("108", GameConstants.EnjinTokenKeys.KNIGHT_DESTROYER);
        linkUnitAndToken("109", GameConstants.EnjinTokenKeys.KNIGHT_BOMBER);

        linkUnitAndToken("110", GameConstants.EnjinTokenKeys.DEMON_BOMBER);
        linkUnitAndToken("111", GameConstants.EnjinTokenKeys.DEMON_SCOUT);
        linkUnitAndToken("112", GameConstants.EnjinTokenKeys.DEMON_DESTROYER);
        linkUnitAndToken("113", GameConstants.EnjinTokenKeys.DEMON_TANK);
        linkUnitAndToken("114", GameConstants.EnjinTokenKeys.DEMON_HEALER);

        linkUnitAndToken("115", GameConstants.EnjinTokenKeys.GOD_TANK_1);
        linkUnitAndToken("116", GameConstants.EnjinTokenKeys.GOD_TANK_2);
        linkUnitAndToken("117", GameConstants.EnjinTokenKeys.GOD_DESTROYER_1);
        linkUnitAndToken("118", GameConstants.EnjinTokenKeys.GOD_BOMBER);
        linkUnitAndToken("119", GameConstants.EnjinTokenKeys.GOD_HEALER);
        linkUnitAndToken("120", GameConstants.EnjinTokenKeys.GOD_DESTROYER_2);
        linkUnitAndToken("121", GameConstants.EnjinTokenKeys.GOD_SCOUT);

        linkUnitAndToken("122", GameConstants.EnjinTokenKeys.ENJIN_ALEX);
        linkUnitAndToken("123", GameConstants.EnjinTokenKeys.ENJIN_EVAN);
        linkUnitAndToken("124", GameConstants.EnjinTokenKeys.ENJIN_ESTHER);
        linkUnitAndToken("125", GameConstants.EnjinTokenKeys.ENJIN_BRAD);
        linkUnitAndToken("126", GameConstants.EnjinTokenKeys.ENJIN_LIZZ);

        linkUnitAndToken("129", GameConstants.EnjinTokenKeys.NARWHAL_BLUE);
        linkUnitAndToken("130", GameConstants.EnjinTokenKeys.NARWHAL_CHEESE);
        linkUnitAndToken("131", GameConstants.EnjinTokenKeys.NARWHAL_EMERALD);
        linkUnitAndToken("132", GameConstants.EnjinTokenKeys.NARWHAL_CRIMSON);
        linkUnitAndToken("133", GameConstants.EnjinTokenKeys.NARWHAL_DIAMOND);

        linkUnitAndToken("127", GameConstants.EnjinTokenKeys.SWISSBORG_CYBORG);
        linkUnitAndToken("128", GameConstants.EnjinTokenKeys.SHALWEND_WARGOD);
        linkUnitAndToken("134", GameConstants.EnjinTokenKeys.SHALWEND_DEADLY_KNIGHT);
    }

    private void linkUnitAndToken(string unitName, string tokenName)
    {
        _unitNameByToken.Add(tokenName, unitName);
        _tokenByUnitName.Add(unitName, tokenName);
    }

    private List<Vector3> getPositionsFromString(string positionsString)
    {
        List<Vector3> positionsList = new List<Vector3>();

        if (positionsString != "")
        {
            string[] positions = positionsString.Split(POSITIONS_SEPARATOR);

            foreach (string positionString in positions)
            {
                string[] coords = positionString.Split(COORDS_SEPARATOR);

                Vector3 position = new Vector3(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));
                positionsList.Add(position);
            }
        }

        return positionsList;
    }

    private void removeGroupFromSaveHashTable(string groupName)
    {
        List<object> keysToRemove = new List<object>();

        foreach (DictionaryEntry entry in _saveHashTable)
        {
            string[] terms = entry.Key.ToString().Split(_GROUP_WITH_KEY_SEPARATOR);

            if (groupName == terms[0])
            {
                keysToRemove.Add(entry.Key);
            }
        }

        foreach (object key in keysToRemove)
        {
            _saveHashTable.Remove(key);
        }
    }


    private int getMaxUnitExperience()
    {
        int maxLevelIndexToCheck = _experienceNeededPerUnitLevel.Count - 2;
        int maxExp = _experienceNeededPerUnitLevel[maxLevelIndexToCheck + 1];
        return maxExp;
    }

    private void createUnitTierLists()
    {
        int firstTierBronze_number = 1;
        populateUnitList(_tierBronze_units, firstTierBronze_number, _lastTierBronze_unitNumber);
        
        int firstTierSilver_number = _lastTierBronze_unitNumber + 1;
        populateUnitList(_tierSilver_units, firstTierSilver_number, _lastTierSilver_unitNumber);

        int firstTierGold_number = _lastTierSilver_unitNumber + 1;
        populateUnitList(_tierGold_units, firstTierGold_number, _lastTierGold_unitNumber);

        populateUnitList(_tierGold_units, _legend_firstUnitNumber, _legend_lastUnitNumer);
        populateUnitList(_legend_units, _legend_firstUnitNumber, _legend_lastUnitNumer);
        populateUnitList(_demon_units, _demonFirstUnitNumber, _demonLastUnitNumber);
        populateUnitList(_swolesome_units, _narwhalFirstUnitNumber, _narwhalLastUnitNumber);
    }

    public void populateUnitList(List<string> unitList, int firstUnitNumber, int lastUnitNumber)
    {
        for (int i = firstUnitNumber; i <= lastUnitNumber; i++)
        {
            unitList.Add(i.ToString());
        }
    }

    private void createRarity()
    {
        float totalRarity = _tierBronze_GroupRarity + _tierSilver_GroupRarity + _tierGold_GroupRarity;
        if (totalRarity != 1)
        {
            Debug.LogError("Total tier rarity needs to sum 1. It is at: " + totalRarity);
            return;
        }

        float tierBronze_rarity = _tierBronze_GroupRarity / _tierBronze_unitsAmount;
        float tierSilver_rarity = _tierSilver_GroupRarity / _tierSilver_unitsAmount;
        float tierGold_rarity = _tierGold_GroupRarity / _tierGold_unitsAmount;

        int unitsLenght = _unitsAmount;
        for (int i = 1; i <= unitsLenght; i++)
        {
            float rarity = tierBronze_rarity;
            if (i > _lastTierSilver_unitNumber)
            {
                rarity = tierGold_rarity;
            }
            else if (i > _lastTierBronze_unitNumber)
            {
                rarity = tierSilver_rarity;
            }

            //_unitSpecialRarities.Add(new UnitRarity(i.ToString(), rarity));
            _rarityByUnit.Add(i.ToString(), rarity);
        }
    }

    private void initializeInventory()
    {
        /*
        addMinMinUnit("2");
        addMinMinUnit("1");
        addMinMinUnit("5");
        addMinMinUnit("8");
        addMinMinUnit("4");
        SaveUnits();
        */

        //OpenLootBox(); //TODO: Remove hack
    }

    private void addMinMinUnit(string unitName)
    {
        InventoryManager.Instance.AddItem(GroupNames.UNITS_EXP, unitName, 0);
    }

    private void loadDataFromFile()
    {
        _saveHashTable.Clear();
        _saveHashTable = FileManager.Instance.LoadData();
        loadDataFromHashTable(_saveHashTable);
    }

    private void loadDataFromString(string dataString)
    {
        loadDataFromHashTable(FileManager.Instance.GetHashtableFromDataString(dataString));
    }

    private void loadDataFromHashTable(Hashtable hashtable)
    {
        Debug.LogWarning("GameInventory::loadDataFromHashTable");

        GameConfig gameConfig = GameConfig.Instance;

        InventoryManager inventoryManager = InventoryManager.Instance;
        inventoryManager.ClearAllGroups();

        //Set default values ========================================================================
        inventoryManager.AddItem(GroupNames.STATS, ItemKeys.SINGLE_PLAYER_LEVEL, 0);
        inventoryManager.AddItem(GroupNames.STATS, ItemKeys.ENJIN_ATTEMPTS, 5);
        inventoryManager.AddItem(GroupNames.STATS, ItemKeys.GLOBAL_SYSTEM_ACTIVE_QUEST, ScoutQuests.None.ToString());
        inventoryManager.AddItem(GroupNames.STATS, ItemKeys.SWOLESOME_ACTIVE_QUEST, ScoutQuests.None.ToString());
        inventoryManager.AddItem(GroupNames.STATS, ItemKeys.CRYSTALS, 50);

        foreach (ScoutQuests quest in Enum.GetValues(typeof(ScoutQuests)))
        {
            if (quest != ScoutQuests.None)
            {
                string questString = quest.ToString();
                inventoryManager.AddItem(GroupNames.QUESTS_SCOUT_PROGRESS, questString, "");
                inventoryManager.AddItem(GroupNames.QUESTS_ENEMIES_POSITIONS, questString, "");

                int maxQuestLevel = GameConfig.Instance.MaxQuestLevel;
                for (int i = 1; i <= maxQuestLevel; i++)
                {
                    inventoryManager.AddItem(GroupNames.QUESTS_LEVEL_PROGRESS, questString + QUEST_WITH_LEVEL_SEPARATOR + i, false);
                }
            }
        }

        foreach (SerialQuests quest in Enum.GetValues(typeof(SerialQuests)))
        {
            if (quest != SerialQuests.None)
            {
                string questString = quest.ToString();

                int maxQuestLevel = GameConfig.Instance.MaxQuestLevel;
                for (int i = 1; i <= maxQuestLevel; i++)
                {
                    inventoryManager.AddItem(GroupNames.QUESTS_LEVEL_PROGRESS, questString + QUEST_WITH_LEVEL_SEPARATOR + i, false);
                }
            }
        }

        for (int boxIndex = 0; boxIndex <= BoxIndexes.LEGEND; boxIndex++)
        {
            inventoryManager.AddItem(GroupNames.LOOT_BOXES, boxIndex.ToString(), 0);
        }

        addDefaultOreItemGroups(GameConstants.BoostEnjinOreItems.DAMAGE, GameConstants.BoostCategory.DAMAGE);
        addDefaultOreItemGroups(GameConstants.BoostEnjinOreItems.DEFENSE, GameConstants.BoostCategory.DEFENSE);
        addDefaultOreItemGroups(GameConstants.BoostEnjinOreItems.HEALTH, GameConstants.BoostCategory.HEALTH);
        addDefaultOreItemGroups(GameConstants.BoostEnjinOreItems.POWER, GameConstants.BoostCategory.POWER);
        addDefaultOreItemGroups(GameConstants.BoostEnjinOreItems.SIZE, GameConstants.BoostCategory.SIZE);

        //addTokenWithdrawalDefaultValues();

        //SaveOre();
        //===========================================================================================


        foreach (DictionaryEntry entry in hashtable)
        {
            string[] keyTerms = entry.Key.ToString().Split(_GROUP_WITH_KEY_SEPARATOR);
            string groupName = keyTerms[0];

            if (keyTerms.Length < 2)
            {
                Debug.LogError("Key with one term: " + entry.Key);
            }

            string keyString = keyTerms[1];
            string valueString = (string)entry.Value;

            if (groupName == GroupNames.UNITS_EXP)
            {
                inventoryManager.AddItem(groupName, keyString, int.Parse(valueString));
            }
            else if (groupName == GroupNames.LOOT_BOXES)
            {
                inventoryManager.UpdateItem(groupName, keyString, int.Parse(valueString));
            }
            else if (groupName == GroupNames.STATS)
            {
                if ((keyString == GameInventory.ItemKeys.GLOBAL_SYSTEM_ACTIVE_QUEST) || (keyString == GameInventory.ItemKeys.SWOLESOME_ACTIVE_QUEST))
                {
                    inventoryManager.UpdateItem(groupName, keyString, valueString);
                }
                else if ((keyString == GameInventory.ItemKeys.SINGLE_PLAYER_LEVEL) || (keyString == GameInventory.ItemKeys.ENJIN_ATTEMPTS))
                {
                    inventoryManager.UpdateItem(groupName, keyString, int.Parse(valueString));
                }
                else if (keyString == GameInventory.ItemKeys.CRYSTALS)
                {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    if (GameHacks.Instance.Crystals.Enabled)
                    {
                        valueString = GameHacks.Instance.Crystals.Value;
                    }
#endif
                    inventoryManager.UpdateItem(groupName, keyString, int.Parse(valueString));
                }
                else
                {
                    Debug.LogError("GameInventory::loadData -> Unknow stats key loaded: " + keyString);
                }
            }
            else if (groupName == GroupNames.QUESTS_ENEMIES_POSITIONS)
            {
                inventoryManager.UpdateItem(groupName, keyString, valueString);
            }
            else if (groupName == GroupNames.QUESTS_LEVEL_PROGRESS)
            {
                inventoryManager.UpdateItem(groupName, keyString, bool.Parse(valueString));
            }
            else if (groupName == GroupNames.QUESTS_SCOUT_PROGRESS)
            {
                inventoryManager.UpdateItem(groupName, keyString, valueString);
            }
            else if (groupName == GroupNames.ORE)
            {
                string oreItemName = keyTerms[1];

                string[] valueTerms = entry.Value.ToString().Split(_GROUP_WITH_KEY_SEPARATOR);
                //string value = boostItem.Category + _parseSeparator + boostItem.Bonus + _parseSeparator + boostItem.Amount;

                string category = valueTerms[0];
                int bonus = int.Parse(valueTerms[1]);
                int amount = int.Parse(valueTerms[2]);

                UpdateTeamBoostOreItem(new TeamBoostItemGroup(oreItemName, amount, bonus, category, false), false);
            }
        }

        //if (!isThereAnyUnit && !isThereAnyLootBox)
        //{
        //    inventoryManager.UpdateItem(GroupNames.LOOT_BOXES, BoxTiers.BRONZE.ToString(), _initialBronzeLootBoxes, false);
        //    saveLootBoxes();
        //}
    }

    //private void addTokenWithdrawalDefaultValues()
    //{
    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_MAXIM);
    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_WITEK);
    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_BRYANA);
    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_TASSIO);
    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_SIMON);

    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_ESTHER);
    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_ALEX);
    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_LIZZ);
    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_EVAN);
    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_BRAD);

    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_SWORD);
    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_ARMOR);
    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_SHADOWSONG);
    //    addTokenWithdrawalDefaultValue(TokenKeys.ENJIN_BULL);

    //    addTokenWithdrawalDefaultValue(TokenKeys.KNIGHT_HEALER);
    //    addTokenWithdrawalDefaultValue(TokenKeys.KNIGHT_BOMBER);
    //    addTokenWithdrawalDefaultValue(TokenKeys.KNIGHT_SCOUT);
    //    addTokenWithdrawalDefaultValue(TokenKeys.KNIGHT_DESTROYER);
    //    addTokenWithdrawalDefaultValue(TokenKeys.KNIGHT_TANK);

    //    addTokenWithdrawalDefaultValue(TokenKeys.DEMON_HEALER);
    //    addTokenWithdrawalDefaultValue(TokenKeys.DEMON_BOMBER);
    //    addTokenWithdrawalDefaultValue(TokenKeys.DEMON_SCOUT);
    //    addTokenWithdrawalDefaultValue(TokenKeys.DEMON_DESTROYER);
    //    addTokenWithdrawalDefaultValue(TokenKeys.DEMON_TANK);

    //    addTokenWithdrawalDefaultValue(TokenKeys.SWISSBORG_CYBORG);
    //}

    //private void addTokenWithdrawalDefaultValue(string tokenKey)
    //{
    //    InventoryManager.Instance.AddItem(GroupNames.ENJIN_TOKENS_WITHDRAWAL, tokenKey, false);
    //}

    public void ClearQuestLevelsCompleted(ScoutQuests quest)
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        int maxQuestLevel = GameConfig.Instance.MaxQuestLevel;
        string questString = quest.ToString();

        for (int i = 1; i <= maxQuestLevel; i++)
        {
            string groupName = GroupNames.QUESTS_LEVEL_PROGRESS;
            string key = questString + QUEST_WITH_LEVEL_SEPARATOR + i;
            string hashKey = groupName + _GROUP_WITH_KEY_SEPARATOR + key;
            bool value = false;
            inventoryManager.UpdateItem(groupName, key, value);
            saveHashKey(hashKey, value, false);
        }

        saveHashTableToFileAndBackup();
    }

    //private void saveRating()
    //{
    //    if (_saveHashTable.ContainsKey(RATING_KEY))
    //        _saveHashTable.Remove(RATING_KEY);

    //    _saveHashTable.Add(RATING_KEY, GetRating());
    //    saveHashTableToFile();
    //}

    private void addDefaultOreItemGroups(string baseOreItemName, string category)
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        for (int i = 1; i <= _oreMaxBonus; i++)
        {
            string oreFullName = baseOreItemName + " " + i;
            AddTeamBoostOreItemGroup(new TeamBoostItemGroup(oreFullName, 0, i, category, false), false);
        }
    }

    private void saveInventoryItemToFile<ItemType>(string groupName, string key)
    {
        ItemType item = InventoryManager.Instance.GetItem<ItemType>(groupName, key);
        string hashKey = groupName + _GROUP_WITH_KEY_SEPARATOR + key;
        Debug.LogWarning("GameInventory::saveInventoryItemToFile -> groupName: " + groupName + " key: " + key + item.ToString());
        saveHashKey(hashKey, item);
    }

    private void saveEnjinAttempts(int number)
    {
        Debug.Log("Attempts Remaining: " + number);
        string hashKey = GroupNames.STATS + _GROUP_WITH_KEY_SEPARATOR + ItemKeys.ENJIN_ATTEMPTS;
        InventoryManager.Instance.UpdateItem(GroupNames.STATS, ItemKeys.ENJIN_ATTEMPTS, number, true);
        saveHashKey(hashKey, number);
    }

    public void UpdateTeamBoostOreItem(TeamBoostItemGroup teamBoostItem, bool saveToFile)
    {
        InventoryManager.Instance.UpdateItem(GroupNames.ORE, teamBoostItem.Name, teamBoostItem);

        if (saveToFile)
        {
            SaveOre();
        }
    }

    public void AddTeamBoostOreItemGroup(TeamBoostItemGroup teamBoostItem, bool saveToFile)
    {
        InventoryManager.Instance.AddItem(GroupNames.ORE, teamBoostItem.Name, teamBoostItem);

        if (saveToFile)
        {
            SaveOre();
        }
    }

    public void SaveOre()
    {
        removeGroupFromSaveHashTable(GroupNames.ORE);

        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string oreItemName in inventoryManager.GetGroupKeys(GroupNames.ORE))
        {
            TeamBoostItemGroup boostItem = inventoryManager.GetItem<TeamBoostItemGroup>(GroupNames.ORE, oreItemName);
            string hashKey = GroupNames.ORE + _GROUP_WITH_KEY_SEPARATOR + boostItem.Name;
            string value = boostItem.Category + _GROUP_WITH_KEY_SEPARATOR + boostItem.Bonus + _GROUP_WITH_KEY_SEPARATOR + boostItem.Amount;

            saveHashKey(hashKey, value, false);
        }

        saveHashTableToFileAndBackup();
    }

    private void saveHashKey(string hashKey, object value, bool saveToFile = true)
    {
        if (_saveHashTable.ContainsKey(hashKey))
        {
            _saveHashTable.Remove(hashKey);
        }

        _saveHashTable.Add(hashKey, value);

        if (saveToFile)
        {
            saveHashTableToFileAndBackup();
        }
    }

    private void saveHashTableToFileAndBackup()
    {
        Debug.LogWarning("saveHashTableToFile -> _saveHashTable: " + _saveHashTable.ToStringFull());
        FileManager fileManager = FileManager.Instance;

        string dataString = fileManager.ConvertHashToDataStringAndSetSec(_saveHashTable);

        //Backup saves for logged and not logged users =========================
        if (NetworkManager.LoggedIn && GameConfig.Instance.EnableServerBackup)
        {
            string fileSec = fileManager.GetDataStringSec(dataString);

            Hashtable hashTable = new Hashtable();
            hashTable.Add(GameNetwork.TransactionKeys.DATA, dataString);
            hashTable.Add(GameNetwork.TransactionKeys.SEC_CODE, fileSec);

            NetworkManager.Transaction(GameConstants.Transactions.SAVE_FILE_TO_SERVER, hashTable, onSaveFileToServer);
        }

        ObscuredPrefs.SetString(GameNetwork.TransactionKeys.DATA, dataString);
        //======================================================================

        FileManager.Instance.SaveDataRaw(dataString);
    }

    private void onSaveFileToServer(SimpleJSON.JSONNode data)
    {
        NetworkManager.CheckInvalidServerResponse(data, nameof(onSaveFileToServer));
    }

    public void LoadBackupSave()
    {
        if (NetworkManager.LoggedIn && GameConfig.Instance.EnableServerBackup && GameStats.Instance.IsThereServerBackup)
        {
            NetworkManager.Transaction(GameConstants.Transactions.LOAD_FILE_FROM_SERVER, onLoadFileFromServer);
        }
        else
        {
            FileManager fileManager = FileManager.Instance;

            string dataString = ObscuredPrefs.GetString(GameNetwork.TransactionKeys.DATA, "");

            if (dataString != "")
            {
                if (fileManager.CheckDataStringAgainstPrefSec(dataString))
                {
                    Debug.LogWarning("LoadBackupSave::Security Breach.");
                    dataString = "";
                    ObscuredPrefs.SetString(GameNetwork.TransactionKeys.DATA, dataString);
                }

                fileManager.SaveDataRaw(dataString);
                loadDataFromString(dataString);  //To avoid loading from a file that was saved in the same frame. 
            }
        }
    }

    private void onLoadFileFromServer(SimpleJSON.JSONNode response)
    {
        if (NetworkManager.CheckInvalidServerResponse(response, nameof(onLoadFileFromServer)))
        {
            return;
        }

        SimpleJSON.JSONNode response_hash = response[0];

        string dataString = response_hash[GameNetwork.TransactionKeys.DATA];
        string sec = response_hash[GameNetwork.TransactionKeys.SEC_CODE];

        if (!string.IsNullOrEmpty(dataString))
        {
            FileManager fileManager = FileManager.Instance;
            if (fileManager.CheckDataStringAgainstGivenSec(dataString, sec))
            {
                Debug.LogWarning("onLoadFileFromServer::Security Breach.");
            }
            else
            {
                FileManager.Instance.SaveDataRaw(dataString);
                loadDataFromString(dataString);  //To avoid loading from a file that was saved in the same frame. 
            }
        }
    }

    public bool IsTherePrefsBackupSave()
    {
        string key = GameNetwork.TransactionKeys.DATA;
        bool isThereBackupSave = (ObscuredPrefs.HasKey(key) && (ObscuredPrefs.GetString(key, "") != ""));

        if (GameHacks.Instance.NegatePrefsBackup)
        {
            isThereBackupSave = false;
        }

        return isThereBackupSave;
    }

    [System.Serializable]
    public class UnitRarity
    {
        public string UnitName;
        public float Rarity;

        public UnitRarity(string unitName, float rarity)
        {
            UnitName = unitName;
            Rarity = rarity;
        }
    }

    public class ExpData
    {
        public int Level;
        public int ExpForPreviousLevel;
        public int ExpForNextLevel;
    }
}
