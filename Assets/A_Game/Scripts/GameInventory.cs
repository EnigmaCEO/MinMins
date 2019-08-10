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
        public const string UNITS = "Units";
    }

    public class ItemKeys
    {
        public const string SINGLE_PLAYER_LEVEL = "SinglePlayerLevel";
    }

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

    [SerializeField] private float _tierBronze_GroupRarity = 0.5f;
    [SerializeField] private float _tierSilver_GroupRarity = 0.3f;
    [SerializeField] private float _tierGold_GroupRarity = 0.2f;

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

    public bool HasEnoughUnitsForBattle()
    {
        return (GetInventoryUnitNames().Count >= _unitsNecessaryToBattle);
    }

    public List<string> GetInventoryUnitNames()
    {
        List<string> inventoryUnitIndexes = new List<string>();

        InventoryManager inventoryManager = InventoryManager.Instance;

        if (inventoryManager.CheckGroupExists(GroupNames.UNITS, false))
        {
            foreach (string unitName in inventoryManager.GetGroupKeys(GroupNames.UNITS))
                inventoryUnitIndexes.Add(unitName);
        }

        return inventoryUnitIndexes;
    }

    public void SetSinglePlayerLevel(int level)
    {
        InventoryManager.Instance.UpdateItem(GroupNames.STATS, ItemKeys.SINGLE_PLAYER_LEVEL, level, true);
    }

    public int GetSinglePlayerLevel()
    {
        return InventoryManager.Instance.GetItem<int>(GroupNames.STATS, ItemKeys.SINGLE_PLAYER_LEVEL);
    }

    public void AddExpToUnit(string unitName, int expToAdd)
    {
        int unitExp = InventoryManager.Instance.GetItem<int>(GroupNames.UNITS, unitName);
        unitExp += expToAdd;

        int maxExp = GameNetwork.Instance.GetMaxUnitExperience();
        if (unitExp > maxExp)
            unitExp = maxExp;

        InventoryManager.Instance.UpdateItem(GroupNames.UNITS, unitName, unitExp);
    }

    public int GetAllyUnitExp(string unitName)
    {
        return InventoryManager.Instance.GetItem<int>(GroupNames.UNITS, unitName);
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

        if(shouldSave)
            saveLootBoxes();
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
            if (inventoryManager.HasItem(GroupNames.UNITS, unitName, false))
            {
                AddExpToUnit(unitName, _expToAddOnDuplicateUnit);
                print("Added " + _expToAddOnDuplicateUnit + " exp to unit " + unitName);
            }
            else
                inventoryManager.AddItem(GroupNames.UNITS, unitName, 0);
        }

        SaveUnits();
        ChangeLootBoxAmount(1, boxTier, false, true);

        return unitPicks;
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
        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string unitName in inventoryManager.GetGroupKeys(GroupNames.UNITS))
            saveUnit(GroupNames.UNITS, unitName, false);

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
        InventoryManager.Instance.AddItem(GroupNames.UNITS, unitName, 0);
    }

    private void loadData()
    {
        _saveHashTable.Clear();
        _saveHashTable = FileManager.Instance.LoadData();

        InventoryManager inventoryManager = InventoryManager.Instance;
        inventoryManager.ClearAllGroups();

        inventoryManager.AddItem(GroupNames.STATS, ItemKeys.SINGLE_PLAYER_LEVEL, 1);

        for(int tier = 1; tier <= _lootBoxTiersAmount; tier++)
            inventoryManager.AddItem(GroupNames.LOOT_BOXES, tier.ToString(), 0);

        bool isThereAnyUnit = false;
        bool isThereAnyLootBox = false;

        foreach (DictionaryEntry entry in _saveHashTable)
        {
            string[] terms = entry.Key.ToString().Split(_parseSeparator);
            string groupName = terms[0];

            if (groupName == GroupNames.UNITS)
            {
                string unitName = terms[1];
                int unitExp = int.Parse(entry.Value.ToString());
                //print("LoadData -> groupName: " + groupName + " -> unitName: " + unitName + " -> unit exp: " + unitExp.ToString());
                inventoryManager.AddItem(groupName, unitName, unitExp);

                isThereAnyUnit = true;
            }
            else if (groupName == GroupNames.LOOT_BOXES)
            {
                int tier = int.Parse(terms[1]);
                int tierAmount = int.Parse((string)entry.Value);
                //print("LoadData -> box tier: " + tier + " amount: " + tierAmount);
                inventoryManager.UpdateItem(GroupNames.LOOT_BOXES, tier.ToString(), tierAmount, false);

                isThereAnyLootBox = true;
            }
            else if (groupName == GroupNames.STATS)
            {
                int singlePlayerLevel = int.Parse(terms[1]);
                inventoryManager.UpdateItem(GroupNames.STATS, ItemKeys.SINGLE_PLAYER_LEVEL, singlePlayerLevel);
            }
        }

        if (!isThereAnyUnit && !isThereAnyLootBox)
        {
            inventoryManager.UpdateItem(GroupNames.LOOT_BOXES, Tiers.BRONZE.ToString(), _initialBronzeLootBoxes, false);
            saveLootBoxes();
        }
    }

    //private void saveRating()
    //{
    //    if (_saveHashTable.ContainsKey(RATING_KEY))
    //        _saveHashTable.Remove(RATING_KEY);

    //    _saveHashTable.Add(RATING_KEY, GetRating());
    //    saveHashTableToFile();
    //}

    private void saveSinglePlayerLevelNumber()
    {
        if (_saveHashTable.ContainsKey(ItemKeys.SINGLE_PLAYER_LEVEL))
            _saveHashTable.Remove(ItemKeys.SINGLE_PLAYER_LEVEL);

        _saveHashTable.Add(ItemKeys.SINGLE_PLAYER_LEVEL, GetSinglePlayerLevel());
        saveHashTableToFile();
    }

    private void saveLootBoxes()
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string tier in inventoryManager.GetGroupKeys(GroupNames.LOOT_BOXES))
        {
            int tierAmount = InventoryManager.Instance.GetItem<int>(GroupNames.LOOT_BOXES, tier);
            string hashKey = GroupNames.LOOT_BOXES + _parseSeparator + tier;

            print("SaveLootBoxes -> hashKey: " + hashKey + " -> tierAmount: " + tierAmount.ToString());

            if (_saveHashTable.ContainsKey(hashKey))
                _saveHashTable.Remove(hashKey);

            _saveHashTable.Add(hashKey, tierAmount);
        }

        saveHashTableToFile();
    }

    private void saveUnit(string groupName, string unitName, bool isStandAlone = true)
    {
        int unitExp = InventoryManager.Instance.GetItem<int>(groupName, unitName);
        string hashKey = groupName + _parseSeparator + unitName;
        print("SaveUnit -> hashKey: " + hashKey + " -> unitExp: " + unitExp);

        if (_saveHashTable.ContainsKey(hashKey))
            _saveHashTable.Remove(hashKey);

        _saveHashTable.Add(hashKey, unitExp);

        if (isStandAlone)
            saveHashTableToFile();
    }

    private void saveHashTableToFile()
    {
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
}
