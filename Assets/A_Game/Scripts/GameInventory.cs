using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInventory : SingletonMonobehaviour<GameInventory>
{
    [SerializeField] private char _parseSeparator = '|';
    [SerializeField] private int _expToAddOnDuplicateUnit = 50;

    private const string _TEAM_1_GROUP_NAME = "Team1";

    private Hashtable _saveHashTable = new Hashtable();

    [SerializeField]
    private List<UnitRarity> _unitSpecialRarities = new List<UnitRarity>()
    {
        new UnitRarity(1, 0.1f), new UnitRarity(2, 0.1f), new UnitRarity(3, 0.1f), new UnitRarity(4, 0.1f), new UnitRarity(5, 0.1f),
        new UnitRarity(6, 0.1f), new UnitRarity(7, 0.1f), new UnitRarity(8, 0.1f), new UnitRarity(9, 0.1f), new UnitRarity(10, 0.11f)
    };

    //[SerializeField] private List<int> _defaultRarityIndexes = new List<int>() { 1, 2/*, 3, 4, 5, 6, 7, 8, 9, 10*/ };  //test

    private void Awake()
    {
        initializeInventory();
        LoadData();
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

    public void AddLootBoxToInventory(int lootBoxSize)
    {
        Dictionary<int, double> specialRarities = new Dictionary<int, double>();
        foreach (UnitRarity rarity in _unitSpecialRarities)
            specialRarities.Add(rarity.Index, (double)rarity.Rarity);

        List<int> lootBoxIndexes = LootBoxManager.Instance.PickRandomizedIndexes(lootBoxSize, false, null, specialRarities);
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

        SaveUnits();
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

        AddLootBoxToInventory(5); //TODO: Remove hack
    }

    private void addMinMinUnit(string unitName)
    {
        GameObject minMinPrefab = Resources.Load<GameObject>("Prefabs/MinMinUnits/" + unitName);
        UnitStats stats = new UnitStats(minMinPrefab.GetComponent<MinMinUnit>().Stats);
        InventoryManager.Instance.AddItem(_TEAM_1_GROUP_NAME, unitName, stats);
    }

    public void LoadData()
    {
        _saveHashTable.Clear();
        _saveHashTable = FileManager.Instance.LoadData();

        InventoryManager inventoryManager = InventoryManager.Instance;
        inventoryManager.ClearAllGroups();
        foreach (DictionaryEntry entry in _saveHashTable)
        {
            string[] terms = entry.Key.ToString().Split(_parseSeparator);
            string groupName = terms[0];
            string unitName = terms[1];
            UnitStats stats = new UnitStats((string)entry.Value);
            print("LoadData -> groupName: " + groupName + " -> unitName: " + unitName + " -> stats: " + stats.Serialized());
            inventoryManager.AddItem(groupName, unitName, stats);
        }
    }

    public void SaveUnits()
    {
        SaveGroup(_TEAM_1_GROUP_NAME);
    }

    public void SaveGroup(string groupName)
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string unitName in inventoryManager.GetGroupKeys(groupName))
            SaveItem(groupName, unitName, false);

        FileManager.Instance.SaveData(_saveHashTable);
    }

    public void SaveItem(string groupName, string unitName, bool isStandAlone = true)
    {
        if (_saveHashTable.ContainsKey(unitName))
            _saveHashTable.Remove(unitName);

        UnitStats stats = InventoryManager.Instance.GetItem<UnitStats>(groupName, unitName);
        string hashKey = groupName + _parseSeparator + unitName;
        string statsSerialized = stats.Serialized();
        print("SaveItem -> hashKey: " + hashKey + " -> stats: " + statsSerialized);
        _saveHashTable.Add(hashKey, statsSerialized);

        if (isStandAlone)
            FileManager.Instance.SaveData(_saveHashTable);
    }

    [System.Serializable]
    public class UnitRarity
    {
        public int Index;
        public float Rarity;

        public UnitRarity(int index, float rarity)
        {
            Index = index;
            Rarity = rarity;
        }
    }
}
