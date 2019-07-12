using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInventory : SingletonMonobehaviour<GameInventory>
{
    [SerializeField] private int _lootBoxSize = 3;
    [SerializeField] private int _lootBoxTiersAmount = 3;
    [SerializeField] private int _guaranteedUnitTierAmount = 1;

    [SerializeField] private char _parseSeparator = '|';
    [SerializeField] private int _expToAddOnDuplicateUnit = 50;

    private const string _TEAM_1_GROUP_NAME = "Team1";
    private const string _LOOT_BOXES = "Lootboxes";

    private Hashtable _saveHashTable = new Hashtable();

    [SerializeField] private int _lastTier1_unitNumber = 60;
    [SerializeField] private int _lastTier2_unitNumber = 100;
    [SerializeField] private int _lastTier3_unitNumber = 110;

    //[SerializeField]
    [SerializeField] private int _tier1_unitsAmount = 60;
    [SerializeField] private int _tier2_unitsAmount = 40;
    [SerializeField] private int _tier3_unitsAmount = 10;

    [SerializeField] private float _tier1_GroupRarity = 0.5f;
    [SerializeField] private float _tier2_GroupRarity = 0.3f;
    [SerializeField] private float _tier3_GroupRarity = 0.2f;

    private List<int> _tier2_units = new List<int>();
    private List<int> _tier3_units = new List<int>();

    private List<UnitRarity> _unitSpecialRarities = new List<UnitRarity>();
    //{
    //    new UnitRarity(1, 0.1f), new UnitRarity(2, 0.1f), new UnitRarity(3, 0.1f), new UnitRarity(4, 0.1f), new UnitRarity(5, 0.1f),
    //};

    //[SerializeField] private List<int> _defaultRarityIndexes = new List<int>() { 1, 2/*, 3, 4, 5, 6, 7, 8, 9, 10*/ };  //test

    private void Awake()
    {
        createUnitTierLists();
        createRarity();
        loadData();
        initializeInventory();
    }

    public List<string> GetInventoryUnitNames()
    {
        List<string> inventoryUnitIndexes = new List<string>();

        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string unitName in inventoryManager.GetGroupKeys(_TEAM_1_GROUP_NAME))
            inventoryUnitIndexes.Add(unitName);

        return inventoryUnitIndexes;
    }

    public UnitStats GetUnitStats(string unitName)
    {
        return InventoryManager.Instance.GetItem<UnitStats>(_TEAM_1_GROUP_NAME, unitName);
    }

    public int GetLootBoxTierAmount(int tier)
    {
        return InventoryManager.Instance.GetItem<int>(_LOOT_BOXES, tier.ToString());
    }

    public void ChangeLootBoxAmount(int tier, bool isAddition, bool shouldSave)
    {
        int newtierLootBoxesAmount = InventoryManager.Instance.GetItem<int>(_LOOT_BOXES, tier.ToString());

        if (isAddition)
            newtierLootBoxesAmount += 1;
        else
            newtierLootBoxesAmount -= 1;

        InventoryManager.Instance.UpdateItem(_LOOT_BOXES, tier.ToString(), newtierLootBoxesAmount);

        if(shouldSave)
            saveLootBoxes();
    }
    
    public List<int> OpenLootBox(int boxTier)
    {
        Dictionary<int, double> specialRarities = new Dictionary<int, double>();
        foreach (UnitRarity rarity in _unitSpecialRarities)
            specialRarities.Add(rarity.UnitNumber, (double)rarity.Rarity);

        List<int> lootBoxIndexes = null;
        LootBoxManager lootBoxManager = LootBoxManager.Instance;
        if (boxTier == 1)
            lootBoxIndexes = lootBoxManager.PickRandomizedNumbers(_lootBoxSize, false, null, specialRarities);
        else
        {
            int randomAmounToPick = _lootBoxSize - _guaranteedUnitTierAmount;
            lootBoxIndexes = lootBoxManager.PickRandomizedNumbers(randomAmounToPick, false, null, specialRarities);

            List<int> guaranteedPicks = new List<int>();
            if (boxTier == 2)
                guaranteedPicks = lootBoxManager.PickRandomizedNumbers(_guaranteedUnitTierAmount, false, _tier2_units, null);
            else //tier 3
                guaranteedPicks = lootBoxManager.PickRandomizedNumbers(_guaranteedUnitTierAmount, false, _tier3_units, null);

            foreach (int guaranteedUnitNumber in guaranteedPicks)
                lootBoxIndexes.Add(guaranteedUnitNumber);
        }
        //List<int> lootBoxIndexes = LootBoxManager.Instance.PickRandomizedIndexes(lootBoxSize, false, _defaultRarityIndexes, specialRarities); //test

        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (int lootBoxIndex in lootBoxIndexes)
        {
            print("LootBoxIndex: " + lootBoxIndex);
            string unitName = lootBoxIndex.ToString();
            if (inventoryManager.HasItem(_TEAM_1_GROUP_NAME, unitName, false))
            {
                UnitStats stats = inventoryManager.GetItem<UnitStats>(_TEAM_1_GROUP_NAME, unitName);
                stats.Exp += _expToAddOnDuplicateUnit;
            }
            else
            {
                GameObject minMinPrefab = Resources.Load<GameObject>("Prefabs/MinMinUnits/" + unitName);
                UnitStats stats = new UnitStats(minMinPrefab.GetComponent<MinMinUnit>().Stats);
                inventoryManager.AddItem(_TEAM_1_GROUP_NAME, unitName, stats);
            }
        }

        saveUnits();

        return lootBoxIndexes;
    }

    public int GetUnitTier(int unitNumber)
    {
        int unitTier = 1;
        if (_tier2_units.Contains(unitNumber))
            unitTier = 2;
        else if (_tier3_units.Contains(unitNumber))
            unitTier = 3;

        return unitTier;
    }

    private void createUnitTierLists()
    {
        int firstTier2_number = _lastTier1_unitNumber + 1;
        for (int unitNumber = firstTier2_number; unitNumber <= _lastTier2_unitNumber; unitNumber++)
            _tier2_units.Add(unitNumber);

        int firstTier3_number = _lastTier2_unitNumber + 1;
        for (int unitNumber = firstTier3_number; unitNumber <= _lastTier3_unitNumber; unitNumber++)
            _tier3_units.Add(unitNumber);
    }

    private void createRarity()
    {
        float totalRarity = _tier1_GroupRarity + _tier2_GroupRarity + _tier3_GroupRarity;
        if (totalRarity != 1)
        {
            Debug.LogError("Total tier rarity needs to sum 1. It is at: " + totalRarity);
            return;
        }

        float tier1_rarity = _tier1_GroupRarity / _tier1_unitsAmount;
        float tier2_rarity = _tier2_GroupRarity / _tier2_unitsAmount;
        float tier3_rarity = _tier2_GroupRarity / _tier3_unitsAmount;

        int unitsLenght = _unitSpecialRarities.Capacity;
        for (int i = 1; i <= unitsLenght; i++)
        {
            float rarity = tier1_rarity;
            if (i > _lastTier2_unitNumber)
                rarity = tier3_rarity;
            else if (i > _lastTier1_unitNumber)
                rarity = tier2_rarity;

            _unitSpecialRarities.Add(new UnitRarity(i, rarity));
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
        GameObject minMinPrefab = Resources.Load<GameObject>("Prefabs/MinMinUnits/" + unitName);
        UnitStats stats = new UnitStats(minMinPrefab.GetComponent<MinMinUnit>().Stats);
        InventoryManager.Instance.AddItem(_TEAM_1_GROUP_NAME, unitName, stats);
    }

    private void loadData()
    {
        _saveHashTable.Clear();
        _saveHashTable = FileManager.Instance.LoadData();

        InventoryManager inventoryManager = InventoryManager.Instance;
        inventoryManager.ClearAllGroups();

        for(int tier = 1; tier <= _lootBoxTiersAmount; tier++)
            inventoryManager.AddItem(_LOOT_BOXES, tier.ToString(), 0);

        foreach (DictionaryEntry entry in _saveHashTable)
        {
            string[] terms = entry.Key.ToString().Split(_parseSeparator);
            string groupName = terms[0];

            if (groupName == _TEAM_1_GROUP_NAME)
            {
                string unitName = terms[1];
                UnitStats stats = new UnitStats((string)entry.Value);
                print("LoadData -> groupName: " + groupName + " -> unitName: " + unitName + " -> stats: " + stats.Serialized());
                inventoryManager.AddItem(groupName, unitName, stats);
            }
            else if (groupName == _LOOT_BOXES)
            {
                int tier = int.Parse(terms[1]);
                int tierAmount = int.Parse((string)entry.Value);
                for (int i = 0; i < tierAmount; i++)
                    ChangeLootBoxAmount(tier, true, false);
            }
        }
    }

    private void saveLootBoxes()
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string tier in inventoryManager.GetGroupKeys(_LOOT_BOXES))
        {
            int tierAmount = InventoryManager.Instance.GetItem<int>(_LOOT_BOXES, tier);
            string hashKey = _LOOT_BOXES + _parseSeparator + tier;

            print("SaveLootBoxes -> hashKey: " + hashKey + " -> tierAmount: " + tierAmount.ToString());

            if (_saveHashTable.ContainsKey(hashKey))
                _saveHashTable.Remove(hashKey);

            _saveHashTable.Add(hashKey, tierAmount);

            FileManager.Instance.SaveData(_saveHashTable);
        }

        FileManager.Instance.SaveData(_saveHashTable);
    }

    private void saveUnits()
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string unitName in inventoryManager.GetGroupKeys(_TEAM_1_GROUP_NAME))
            saveUnit(_TEAM_1_GROUP_NAME, unitName, false);

        FileManager.Instance.SaveData(_saveHashTable);
    }

    private void saveUnit(string groupName, string unitName, bool isStandAlone = true)
    {
        UnitStats stats = InventoryManager.Instance.GetItem<UnitStats>(groupName, unitName);
        string hashKey = groupName + _parseSeparator + unitName;
        string statsSerialized = stats.Serialized();
        print("SaveUnit -> hashKey: " + hashKey + " -> stats: " + statsSerialized);

        if (_saveHashTable.ContainsKey(hashKey))
            _saveHashTable.Remove(hashKey);

        _saveHashTable.Add(hashKey, statsSerialized);

        if (isStandAlone)
            FileManager.Instance.SaveData(_saveHashTable);
    }

    [System.Serializable]
    public class UnitRarity
    {
        public int UnitNumber;
        public float Rarity;

        public UnitRarity(int unitNumber, float rarity)
        {
            UnitNumber = unitNumber;
            Rarity = rarity;
        }
    }
}
