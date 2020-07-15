using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInventory : SingletonMonobehaviour<GameInventory>
{
    public class Tiers
    {
        public const int BRONZE = 1;
        public const int SILVER = 2;
        public const int GOLD = 3;
    }

    public class GroupNames
    {
        public const string LOOT_BOXES = "Lootboxes";
        public const string STATS = "Stats";
        public const string UNITS_EXP = "Units";
        public const string ORE = "Ore";
    }

    public class ItemKeys
    {
        public const string SINGLE_PLAYER_LEVEL = "SinglePlayerLevel";
        public const string ENJIN_ATTEMPTS = "EnjinAttempts";
    }

    [SerializeField] List<int> _experienceNeededPerUnitLevel = new List<int>() { 0, 10, 30, 70, 150, 310 };  //Rule: Doubles each level

    [SerializeField] private int _maxArenaLevel = 50;

    [SerializeField] private int _unitsNecessaryToBattle = 5;

    [SerializeField] private int _unitsAmount = 80;
    [SerializeField] private int _initialBronzeLootBoxes = 2;

    [SerializeField] private int _lootBoxSize = 3;
    [SerializeField] private int _lootBoxTiersAmount = 3;
    [SerializeField] private int _guaranteedUnitTierAmount = 1;

    [SerializeField] private char _parseSeparator = '|';
    [SerializeField] private int _expToAddOnDuplicateUnit = 10;

    [SerializeField] private int _lastTierBronze_unitNumber = 40;
    [SerializeField] private int _lastTierSilver_unitNumber = 70;
    [SerializeField] private int _lastTierGold_unitNumber = 80;

    [SerializeField] private int _tierBronze_unitsAmount = 40;
    [SerializeField] private int _tierSilver_unitsAmount = 30;
    [SerializeField] private int _tierGold_unitsAmount = 10;

    [SerializeField] private int _enjin_firstUnitNumber = 100;
    [SerializeField] private int _enjin_lastUnitNumer = 121;

    [SerializeField] private float _tierBronze_GroupRarity = 0.5f;
    [SerializeField] private float _tierSilver_GroupRarity = 0.3f;
    [SerializeField] private float _tierGold_GroupRarity = 0.2f;

    [SerializeField] private int _enjinUnitStartingLevel = 3;
    [SerializeField] private int _oreMaxBonus = 10;

    private Hashtable _saveHashTable = new Hashtable();

    private List<string> _tierBronze_units = new List<string>();
    private List<string> _tierSilver_units = new List<string>();
    private List<string> _tierGold_units = new List<string>();

    private List<UnitRarity> _unitSpecialRarities = new List<UnitRarity>();


    private void Awake()
    {
        createUnitTierLists();
        createRarity();
        loadData();
        initializeInventory();
    }

    public int GetRandomTier()
    {
        return Random.Range(Tiers.BRONZE, Tiers.GOLD + 1);
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
                inventoryUnitIndexes.Add(unitName);
        }

        return inventoryUnitIndexes;
    }

    public TeamBoostItem GetOreItem(string itemName)
    {
        return InventoryManager.Instance.GetItem<TeamBoostItem>(GroupNames.ORE, itemName);
    }

    public List<TeamBoostItem> GetOreItemsOwned()
    {
        List<TeamBoostItem> oreItems = new List<TeamBoostItem>();

        InventoryManager inventoryManager = InventoryManager.Instance;

        if (inventoryManager.CheckGroupExists(GroupNames.ORE, false))
        {
            foreach (string itemName in inventoryManager.GetGroupKeys(GroupNames.ORE))
            {
                TeamBoostItem item = GetOreItem(itemName);
                oreItems.Add(item);
            }
        }

        return oreItems;
    }

    public void SetSinglePlayerLevel(int level)
    {
        Debug.LogWarning("GameInventory::SetSinglePlayerLevel -> level: " + level);
        int currentLevel = GetSinglePlayerLevel();
        if (level > currentLevel)
        {
            if (level > _maxArenaLevel)
                level = _maxArenaLevel;

            InventoryManager.Instance.UpdateItem(GroupNames.STATS, ItemKeys.SINGLE_PLAYER_LEVEL, level, true);
            saveSinglePlayerLevelNumber();
        }
    }

    public int GetSinglePlayerLevel()
    {
        return InventoryManager.Instance.GetItem<int>(GroupNames.STATS, ItemKeys.SINGLE_PLAYER_LEVEL);
    }

    public int GetEnjinAttempts()
    {
        return InventoryManager.Instance.GetItem<int>(GroupNames.STATS, ItemKeys.ENJIN_ATTEMPTS);
    }

    public void AddExpToUnit(string unitName, int expToAdd)
    {
        if (GameNetwork.Instance.HasEnjinMinMinsToken) expToAdd *= 2; // Min Min Token perk

        int unitExp = InventoryManager.Instance.GetItem<int>(GroupNames.UNITS_EXP, unitName);
        unitExp += expToAdd;

        int maxExp = getMaxUnitExperience();
        if (unitExp > maxExp)
            unitExp = maxExp;

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

    public int GetLootBoxTierAmount(int tier)
    {
        return InventoryManager.Instance.GetItem<int>(GroupNames.LOOT_BOXES, tier.ToString());
    }

    public bool HasAnyLootBox()
    {
        return (GetLootBoxesTotalAmount() > 0);
    }

    public int GetLootBoxesTotalAmount()
    {
        return (GetLootBoxTierAmount(Tiers.BRONZE) + GetLootBoxTierAmount(Tiers.SILVER) + GetLootBoxTierAmount(Tiers.GOLD));
    }

    public void ChangeLootBoxAmount(int amount, int tier, bool isAddition, bool shouldSave)
    {
        int newtierLootBoxesAmount = InventoryManager.Instance.GetItem<int>(GroupNames.LOOT_BOXES, tier.ToString());

        if (isAddition)
            newtierLootBoxesAmount += amount;
        else
            newtierLootBoxesAmount -= amount;

        InventoryManager.Instance.UpdateItem(GroupNames.LOOT_BOXES, tier.ToString(), newtierLootBoxesAmount);

        if (shouldSave)
        {
            SaveLootBoxes();
        }
    }

    public List<string> OpenLootBox(int boxTier)
    {
        Dictionary<string, double> specialRarities = new Dictionary<string, double>();
        foreach (UnitRarity rarity in _unitSpecialRarities)
            specialRarities.Add(rarity.UnitName, (double)rarity.Rarity);

        List<string> unitPicks = null;
        LootBoxManager lootBoxManager = LootBoxManager.Instance;
        if (boxTier == Tiers.BRONZE)
            unitPicks = lootBoxManager.PickRandomizedNames(_lootBoxSize, true, null, specialRarities);
        else
        {
            int specialRarityAmountToPick = _lootBoxSize - _guaranteedUnitTierAmount;
            unitPicks = lootBoxManager.PickRandomizedNames(specialRarityAmountToPick, true, null, specialRarities);
            List<string> guaranteedTierUnits = null;
            if (boxTier == Tiers.SILVER)
                guaranteedTierUnits = _tierSilver_units;
            else //tier 3 (GOLD)
                guaranteedTierUnits = _tierGold_units;

            foreach (string pick in unitPicks)
            {
                if (guaranteedTierUnits.Contains(pick))
                    guaranteedTierUnits.Remove(pick);  // Remove so guaranted unit pick cannot be already in the default rarity pick
            }

            List<string> guaranteedPicks = new List<string>();
            guaranteedPicks = lootBoxManager.PickRandomizedNames(_guaranteedUnitTierAmount, true, guaranteedTierUnits, null, true);

            foreach (string guaranteedUnitName in guaranteedPicks)
                unitPicks.Add(guaranteedUnitName);
        }
        //List<int> lootBoxIndexes = LootBoxManager.Instance.PickRandomizedIndexes(lootBoxSize, false, _defaultRarityIndexes, specialRarities); //test

        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string lootBoxUnitName in unitPicks)
        {
            print("LootBoxUnitName got: " + lootBoxUnitName);
            string unitName = lootBoxUnitName.ToString();
            if (inventoryManager.HasItem(GroupNames.UNITS_EXP, unitName, false))
            {
                AddExpToUnit(unitName, _expToAddOnDuplicateUnit);
                print("Added " + _expToAddOnDuplicateUnit + " exp to unit " + unitName);
            }
            else
                AddUnit(unitName, 0);
        }

        SaveUnits();
        ChangeLootBoxAmount(1, boxTier, false, true);

        return unitPicks;
    }

    public void AddUnit(string unitName, int exp, bool save = true)
    {
        InventoryManager.Instance.AddItem(GroupNames.UNITS_EXP, unitName, exp);

        if (save)
            SaveUnits();
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
        int unitTier = Tiers.BRONZE;
        if (_tierSilver_units.Contains(unitName))
            unitTier = Tiers.SILVER;
        else if (_tierGold_units.Contains(unitName))
            unitTier = Tiers.GOLD;

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
            string hashKey = groupName + _parseSeparator + unitName;
            //Debug.LogWarning("GameInventory::saveUnit -> hashKey: " + hashKey + " -> unitExp: " + unitExp);

            saveHashKey(hashKey, unitExp, false);
        }

        saveHashTableToFile();
    }

    private void removeGroupFromSaveHashTable(string groupName)
    {
        List<object> keysToRemove = new List<object>();

        foreach (DictionaryEntry entry in _saveHashTable)
        {
            string[] terms = entry.Key.ToString().Split(_parseSeparator);

            if (groupName == terms[0])
                keysToRemove.Add(entry.Key);
        }

        foreach(object key in keysToRemove)
            _saveHashTable.Remove(key);
    }

    public void SaveLootBoxes()
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string tier in inventoryManager.GetGroupKeys(GroupNames.LOOT_BOXES))
        {
            int tierAmount = InventoryManager.Instance.GetItem<int>(GroupNames.LOOT_BOXES, tier);
            string hashKey = GroupNames.LOOT_BOXES + _parseSeparator + tier;

            print("SaveLootBoxes -> hashKey: " + hashKey + " -> tierAmount: " + tierAmount.ToString());
            saveHashKey(hashKey, tierAmount, false);
        }

        saveHashTableToFile();
    }

    public List<string> GetRandomUnitsFromTier(int amount, int tier)
    {
        List<string> tierUnits = _tierBronze_units;
        if (tier == Tiers.SILVER)
            tierUnits = _tierSilver_units;
        else if (tier == Tiers.GOLD)
            tierUnits = _tierGold_units;

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
        if (GetEnjinAttempts() < 1) return;

        saveEnjinAttempts(GetEnjinAttempts() - 1);
        int enjinUnitStartingExperience = _experienceNeededPerUnitLevel[_enjinUnitStartingLevel - 1];

        for (int i = _enjin_firstUnitNumber; i <= _enjin_lastUnitNumer; i++)
        {
            string unitName = i.ToString();
            if (!HasUnit(unitName))
                AddUnit(unitName, 0);
        }

        SaveUnits();
    }
    private bool checkAddUnit(string unitName, string checkName, bool flag)
    {
        return ((unitName == checkName) && flag && !HasUnit(unitName));
    }

    public void AddMinMinEnjinUnits()
    {
        for (int i = _enjin_firstUnitNumber; i <= _enjin_lastUnitNumer; i++)
        {
            GameNetwork gameNetwork = GameNetwork.Instance;

            string unitName = i.ToString();
            if (
                    checkAddUnit(unitName, "100", gameNetwork.HasEnjinMaxim) ||
                    checkAddUnit(unitName, "101", gameNetwork.HasEnjinWitek) ||
                    checkAddUnit(unitName, "102", gameNetwork.HasEnjinBryana) ||
                    checkAddUnit(unitName, "103", gameNetwork.HasEnjinTassio) ||
                    checkAddUnit(unitName, "104", gameNetwork.HasEnjinSimon) ||
                    checkAddUnit(unitName, "105",gameNetwork.HasKnightHealer) ||
                    checkAddUnit(unitName, "106", gameNetwork.HasKnightBomber) ||
                    checkAddUnit(unitName, "107", gameNetwork.HasKnightDestroyer) ||
                    checkAddUnit(unitName, "108", gameNetwork.HasKnightScout) ||
                    checkAddUnit(unitName, "109", gameNetwork.HasKnightTank) ||
                    checkAddUnit(unitName, "110", gameNetwork.HasDemonHealer) ||
                    checkAddUnit(unitName, "111", gameNetwork.HasDemonBomber) ||
                    checkAddUnit(unitName, "112", gameNetwork.HasDemonDestroyer) ||
                    checkAddUnit(unitName, "113", gameNetwork.HasDemonScout) ||
                    checkAddUnit(unitName, "114", gameNetwork.HasDemonTank) ||
                    checkAddUnit(unitName, "122", gameNetwork.HasEnjinAlex) ||
                    checkAddUnit(unitName, "123", gameNetwork.HasEnjinEvan) ||
                    checkAddUnit(unitName, "124", gameNetwork.HasEnjinEsther) ||
                    checkAddUnit(unitName, "125", gameNetwork.HasEnjinBrad) ||
                    checkAddUnit(unitName, "126", gameNetwork.HasEnjinLizz)
                )
            {
                AddUnit(unitName, 0);
            }
        }

        /*
         *         public const string ENJIN_ESTHER = "enjin_esther"; //fairy 124
        public const string ENJIN_ALEX = "enjin_alex";  //black 122
        public const string ENJIN_LIZZ = "enjin_lizz";  //fire 126
        public const string ENJIN_EVAN = "enjin_evan";  //wizard 123
        public const string ENJIN_BRAD = "enjin_brad";  //book 125
         * */

        SaveUnits();
    }


    public bool HasAllEnjinUnits()
    {
        bool hasAllEnjinUnits = true;

        for (int i = _enjin_firstUnitNumber; i <= _enjin_lastUnitNumer; i++)
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


    private int getMaxUnitExperience()
    {
        int maxLevelIndexToCheck = _experienceNeededPerUnitLevel.Count - 2;
        int maxExp = _experienceNeededPerUnitLevel[maxLevelIndexToCheck + 1];
        return maxExp;
    }

    private void createUnitTierLists()
    {
        int firstTierBronze_number = 1;
        for (int unitNumber = firstTierBronze_number; unitNumber <= _lastTierBronze_unitNumber; unitNumber++)
            _tierBronze_units.Add(unitNumber.ToString());

        int firstTierSilver_number = _lastTierBronze_unitNumber + 1;
        for (int unitNumber = firstTierSilver_number; unitNumber <= _lastTierSilver_unitNumber; unitNumber++)
            _tierSilver_units.Add(unitNumber.ToString());

        int firstTierGold_number = _lastTierSilver_unitNumber + 1;
        for (int unitNumber = firstTierGold_number; unitNumber <= _lastTierGold_unitNumber; unitNumber++)
            _tierGold_units.Add(unitNumber.ToString());

        addEnjinUnitsToTierGoldList();
    }

    private void addEnjinUnitsToTierGoldList()
    {
        for(int i = _enjin_firstUnitNumber; i <=  _enjin_lastUnitNumer; i++)
            _tierGold_units.Add(i.ToString());
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
                rarity = tierGold_rarity;
            else if (i > _lastTierBronze_unitNumber)
                rarity = tierSilver_rarity;

            _unitSpecialRarities.Add(new UnitRarity(i.ToString(), rarity));
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

    private void loadData()
    {
        Debug.LogWarning("GameInventory::loadData");
        _saveHashTable.Clear();
        _saveHashTable = FileManager.Instance.LoadData();

        InventoryManager inventoryManager = InventoryManager.Instance;
        inventoryManager.ClearAllGroups();

        //Set default values ========================================================================
        inventoryManager.AddItem(GroupNames.STATS, ItemKeys.SINGLE_PLAYER_LEVEL, 1);
        inventoryManager.AddItem(GroupNames.STATS, ItemKeys.ENJIN_ATTEMPTS, 5);

        for (int tier = 1; tier <= _lootBoxTiersAmount; tier++)
        {
            inventoryManager.AddItem(GroupNames.LOOT_BOXES, tier.ToString(), 0);
        }

        addDefaultOreItems(GameConstants.TeamBoostEnjinOreItems.DAMAGE, GameConstants.TeamBoostCategory.DAMAGE);
        addDefaultOreItems(GameConstants.TeamBoostEnjinOreItems.DEFENSE, GameConstants.TeamBoostCategory.DEFENSE);
        addDefaultOreItems(GameConstants.TeamBoostEnjinOreItems.HEALTH, GameConstants.TeamBoostCategory.HEALTH);
        addDefaultOreItems(GameConstants.TeamBoostEnjinOreItems.POWER, GameConstants.TeamBoostCategory.POWER);
        addDefaultOreItems(GameConstants.TeamBoostEnjinOreItems.SIZE, GameConstants.TeamBoostCategory.SIZE);

        //SaveOre();
        //===========================================================================================

        //bool isThereAnyUnit = false;
        //bool isThereAnyLootBox = false;


        foreach (DictionaryEntry entry in _saveHashTable)
        {
            string[] keyTerms = entry.Key.ToString().Split(_parseSeparator);
            string groupName = keyTerms[0];

            if (groupName == GroupNames.UNITS_EXP)
            {
                string unitName = keyTerms[1];
                int unitExp = int.Parse(entry.Value.ToString());
                //print("LoadData -> groupName: " + groupName + " -> unitName: " + unitName + " -> unit exp: " + unitExp.ToString());
                inventoryManager.AddItem(groupName, unitName, unitExp);

                //isThereAnyUnit = true;
            }
            else if (groupName == GroupNames.LOOT_BOXES)
            {
                int tier = int.Parse(keyTerms[1]);
                int tierAmount = int.Parse((string)entry.Value);
                //print("LoadData -> box tier: " + tier + " amount: " + tierAmount);
                inventoryManager.UpdateItem(GroupNames.LOOT_BOXES, tier.ToString(), tierAmount, false);

                //isThereAnyLootBox = true;
            }
            else if (groupName == GroupNames.STATS)
            {
                string key = keyTerms[1];

                if ((key == GameInventory.ItemKeys.SINGLE_PLAYER_LEVEL) || (key == GameInventory.ItemKeys.ENJIN_ATTEMPTS))
                {
                    int value = int.Parse((string)entry.Value);
                    Debug.LogWarning("GameInventory::loadData -> key loaded: " + key + " and value: " + value.ToString());
                    inventoryManager.UpdateItem(GroupNames.STATS, key, value);
                }
                else
                {
                    Debug.LogError("GameInventory::loadData -> Unknow stats key loaded: " + key);
                }
            }
            else if (groupName == GroupNames.ORE)
            {
                string oreItemName = keyTerms[1];

                string[] valueTerms = entry.Value.ToString().Split(_parseSeparator);
                //string value = boostItem.Category + _parseSeparator + boostItem.Bonus + _parseSeparator + boostItem.Amount;

                string category = valueTerms[0];
                int bonus = int.Parse(valueTerms[1]);
                int amount = int.Parse(valueTerms[2]);

                UpdateTeamBoostOreItem(new TeamBoostItem(oreItemName, amount, bonus, category), false);
            }
        }

        //if (!isThereAnyUnit && !isThereAnyLootBox)
        //{
        //    inventoryManager.UpdateItem(GroupNames.LOOT_BOXES, Tiers.BRONZE.ToString(), _initialBronzeLootBoxes, false);
        //    saveLootBoxes();
        //}
    }

    //private void saveRating()
    //{
    //    if (_saveHashTable.ContainsKey(RATING_KEY))
    //        _saveHashTable.Remove(RATING_KEY);

    //    _saveHashTable.Add(RATING_KEY, GetRating());
    //    saveHashTableToFile();
    //}

    private void addDefaultOreItems(string baseOreItemName, string category)
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        for (int i = 1; i <= _oreMaxBonus; i++)
        {
            string oreFullName = baseOreItemName + " " + i;
            AddTeamBoostOreItem(new TeamBoostItem(oreFullName, 0, i, category), false);
        }
    }

    private void saveSinglePlayerLevelNumber()
    {
        int level = InventoryManager.Instance.GetItem<int>(GroupNames.STATS, ItemKeys.SINGLE_PLAYER_LEVEL);
        string hashKey = GroupNames.STATS + _parseSeparator + ItemKeys.SINGLE_PLAYER_LEVEL;
        Debug.LogWarning("GameInventory::saveSinglePlayerLevelNumber: " + level);
        saveHashKey(hashKey, level);
    }

    private void saveEnjinAttempts(int number)
    {
        Debug.Log("Attempts Remaining: " + number);
        string hashKey = GroupNames.STATS + _parseSeparator + ItemKeys.ENJIN_ATTEMPTS;
        InventoryManager.Instance.UpdateItem(GroupNames.STATS, ItemKeys.ENJIN_ATTEMPTS, number, true);
        saveHashKey(hashKey, number);
    }

    public void UpdateTeamBoostOreItem(TeamBoostItem teamBoostItem, bool saveToFile)
    {
        InventoryManager.Instance.UpdateItem(GroupNames.ORE, teamBoostItem.Name, teamBoostItem);

        if (saveToFile)
        {
            SaveOre();
        }
    }

    public void AddTeamBoostOreItem(TeamBoostItem teamBoostItem, bool saveToFile)
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
            TeamBoostItem boostItem = inventoryManager.GetItem<TeamBoostItem>(GroupNames.ORE, oreItemName);
            string hashKey = GroupNames.ORE + _parseSeparator + boostItem.Name;
            string value = boostItem.Category + _parseSeparator + boostItem.Bonus + _parseSeparator + boostItem.Amount;

            saveHashKey(hashKey, value, false);
        }

        saveHashTableToFile();
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
            saveHashTableToFile();
        }
    }

    private void saveHashTableToFile()
    {
        Debug.LogWarning("saveHashTableToFile -> _saveHashTable: " + _saveHashTable.ToStringFull());
        FileManager.Instance.SaveData(_saveHashTable);
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
