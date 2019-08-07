﻿using System.Collections;
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

    [SerializeField] private int _unitsAmount = 80;
    [SerializeField] private int _initialBronzeLootBoxes = 2;

    [SerializeField] private int _lootBoxSize = 3;
    [SerializeField] private int _lootBoxTiersAmount = 3;
    [SerializeField] private int _guaranteedUnitTierAmount = 1;

    [SerializeField] private char _parseSeparator = '|';
    [SerializeField] private int _expToAddOnDuplicateUnit = 10;


    private const string _LOOT_BOXES = "Lootboxes";
    private const string _STATS = "Stats";

    private const string _SINGLE_PLAYER_LEVEL = "SinglePlayerLevel";
    //private const string _

    private Hashtable _saveHashTable = new Hashtable();

    [SerializeField] private int _lastTierBronze_unitNumber = 40;
    [SerializeField] private int _lastTierSilver_unitNumber = 70;
    [SerializeField] private int _lastTierGold_unitNumber = 80;

    //[SerializeField]
    [SerializeField] private int _tierBronze_unitsAmount = 40;
    [SerializeField] private int _tierSilver_unitsAmount = 30;
    [SerializeField] private int _tierGold_unitsAmount = 10;

    [SerializeField] private float _tierBronze_GroupRarity = 0.5f;
    [SerializeField] private float _tierSilver_GroupRarity = 0.3f;
    [SerializeField] private float _tierGold_GroupRarity = 0.2f;

    private List<int> _tierSilver_units = new List<int>();
    private List<int> _tierGold_units = new List<int>();

    private List<UnitRarity> _unitSpecialRarities = new List<UnitRarity>();


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

        if (inventoryManager.CheckGroupExists(GameConstants.TeamNames.ALLIES, false))
        {
            foreach (string unitName in inventoryManager.GetGroupKeys(GameConstants.TeamNames.ALLIES))
                inventoryUnitIndexes.Add(unitName);
        }

        return inventoryUnitIndexes;
    }

    public void SetSinglePlayerLevel(int level)
    {
        InventoryManager.Instance.UpdateItem(_STATS, _SINGLE_PLAYER_LEVEL, level, true);
    }

    public int GetSinglePlayerLevel()
    {
        return InventoryManager.Instance.GetItem<int>(_STATS, _SINGLE_PLAYER_LEVEL);
    }

    public void AddExpToUnit(string unitName, int expToAdd)
    {
        int unitExp = InventoryManager.Instance.GetItem<int>(GameConstants.TeamNames.ALLIES, unitName);
        unitExp += expToAdd;
        InventoryManager.Instance.UpdateItem(GameConstants.TeamNames.ALLIES, unitName, unitExp);
    }

    public MinMinUnit GetMinMinFromResources(string unitName)
    {
        GameObject minMinPrefab = Resources.Load<GameObject>("Prefabs/MinMinUnits/" + unitName);
        MinMinUnit minMin = minMinPrefab.GetComponent<MinMinUnit>();
        return minMin;
    }

    public int GetAllyUnitExp(string unitName)
    {
        return InventoryManager.Instance.GetItem<int>(GameConstants.TeamNames.ALLIES, unitName);
    }

    public int GetLootBoxTierAmount(int tier)
    {
        return InventoryManager.Instance.GetItem<int>(_LOOT_BOXES, tier.ToString());
    }

    public void ChangeLootBoxAmount(int amount, int tier, bool isAddition, bool shouldSave)
    {
        int newtierLootBoxesAmount = InventoryManager.Instance.GetItem<int>(_LOOT_BOXES, tier.ToString());

        if (isAddition)
            newtierLootBoxesAmount += amount;
        else
            newtierLootBoxesAmount -= amount;

        InventoryManager.Instance.UpdateItem(_LOOT_BOXES, tier.ToString(), newtierLootBoxesAmount);

        if(shouldSave)
            saveLootBoxes();
    }
    
    public List<int> OpenLootBox(int boxTier)
    {
        Dictionary<int, double> specialRarities = new Dictionary<int, double>();
        foreach (UnitRarity rarity in _unitSpecialRarities)
            specialRarities.Add(rarity.UnitNumber, (double)rarity.Rarity);

        List<int> unitPicks = null;
        LootBoxManager lootBoxManager = LootBoxManager.Instance;
        if (boxTier == Tiers.BRONZE)
            unitPicks = lootBoxManager.PickRandomizedNumbers(_lootBoxSize, true, null, specialRarities);
        else
        {
            int specialRarityAmountToPick = _lootBoxSize - _guaranteedUnitTierAmount;
            unitPicks = lootBoxManager.PickRandomizedNumbers(specialRarityAmountToPick, true, null, specialRarities);
            List<int> guaranteedTierUnits = null;
            if (boxTier == Tiers.SILVER)
                guaranteedTierUnits = _tierSilver_units;
            else //tier 3 (GOLD)
                guaranteedTierUnits = _tierGold_units;

            foreach (int pick in unitPicks)
            {
                if (guaranteedTierUnits.Contains(pick))
                    guaranteedTierUnits.Remove(pick);  // Remove so guaranted unit pick cannot be already in the default rarity pick
            }

            List<int> guaranteedPicks = new List<int>();
            guaranteedPicks = lootBoxManager.PickRandomizedNumbers(_guaranteedUnitTierAmount, true, guaranteedTierUnits, null, true);

            foreach (int guaranteedUnitNumber in guaranteedPicks)
                unitPicks.Add(guaranteedUnitNumber);
        }
        //List<int> lootBoxIndexes = LootBoxManager.Instance.PickRandomizedIndexes(lootBoxSize, false, _defaultRarityIndexes, specialRarities); //test

        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (int lootBoxIndex in unitPicks)
        {
            print("LootBoxIndex got: " + lootBoxIndex);
            string unitName = lootBoxIndex.ToString();
            if (inventoryManager.HasItem(GameConstants.TeamNames.ALLIES, unitName, false))
            {
                AddExpToUnit(unitName, _expToAddOnDuplicateUnit);
                print("Added " + _expToAddOnDuplicateUnit + " exp to unit " + unitName);
            }
            else
                inventoryManager.AddItem(GameConstants.TeamNames.ALLIES, unitName, 0);
        }

        SaveUnits();
        ChangeLootBoxAmount(1, boxTier, false, true);

        return unitPicks;
    }

    public int GetUnitTier(int unitNumber)
    {
        int unitTier = Tiers.BRONZE;
        if (_tierSilver_units.Contains(unitNumber))
            unitTier = Tiers.SILVER;
        else if (_tierGold_units.Contains(unitNumber))
            unitTier = Tiers.GOLD;

        return unitTier;
    }

    public void SaveUnits()
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string unitName in inventoryManager.GetGroupKeys(GameConstants.TeamNames.ALLIES))
            saveUnit(GameConstants.TeamNames.ALLIES, unitName, false);

        saveHashTableToFile();
    }

    private void createUnitTierLists()
    {
        int firstTierSilver_number = _lastTierBronze_unitNumber + 1;
        for (int unitNumber = firstTierSilver_number; unitNumber <= _lastTierSilver_unitNumber; unitNumber++)
            _tierSilver_units.Add(unitNumber);

        int firstTierGold_number = _lastTierSilver_unitNumber + 1;
        for (int unitNumber = firstTierGold_number; unitNumber <= _lastTierGold_unitNumber; unitNumber++)
            _tierGold_units.Add(unitNumber);
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
        InventoryManager.Instance.AddItem(GameConstants.TeamNames.ALLIES, unitName, 0);
    }

    private void loadData()
    {
        _saveHashTable.Clear();
        _saveHashTable = FileManager.Instance.LoadData();

        InventoryManager inventoryManager = InventoryManager.Instance;
        inventoryManager.ClearAllGroups();

        //inventoryManager.AddItem(_STATS, RATING_KEY, 0);
        inventoryManager.AddItem(_STATS, _SINGLE_PLAYER_LEVEL, 1);

        for(int tier = 1; tier <= _lootBoxTiersAmount; tier++)
            inventoryManager.AddItem(_LOOT_BOXES, tier.ToString(), 0);

        bool isThereAnyUnit = false;
        bool isThereAnyLootBox = false;

        foreach (DictionaryEntry entry in _saveHashTable)
        {
            string[] terms = entry.Key.ToString().Split(_parseSeparator);
            string groupName = terms[0];

            if (groupName == GameConstants.TeamNames.ALLIES)
            {
                string unitName = terms[1];
                int unitExp = int.Parse(entry.Value.ToString());
                print("LoadData -> groupName: " + groupName + " -> unitName: " + unitName + " -> unit exp: " + unitExp.ToString());
                inventoryManager.AddItem(groupName, unitName, unitExp);

                isThereAnyUnit = true;
            }
            else if (groupName == _LOOT_BOXES)
            {
                int tier = int.Parse(terms[1]);
                int tierAmount = int.Parse((string)entry.Value);
                //print("LoadData -> box tier: " + tier + " amount: " + tierAmount);
                inventoryManager.UpdateItem(_LOOT_BOXES, tier.ToString(), tierAmount, false);

                isThereAnyLootBox = true;
            }
            else if (groupName == _STATS)
            {
                //    int rating = int.Parse(terms[1]);
                //    inventoryManager.UpdateItem(_STATS, RATING_KEY, rating);

                int singlePlayerLevel = int.Parse(terms[1]);
                inventoryManager.UpdateItem(_STATS, _SINGLE_PLAYER_LEVEL, singlePlayerLevel);
            }
        }

        if (!isThereAnyUnit && !isThereAnyLootBox)
        {
            inventoryManager.UpdateItem(_LOOT_BOXES, Tiers.BRONZE.ToString(), _initialBronzeLootBoxes, false);
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
        if (_saveHashTable.ContainsKey(_SINGLE_PLAYER_LEVEL))
            _saveHashTable.Remove(_SINGLE_PLAYER_LEVEL);

        _saveHashTable.Add(_SINGLE_PLAYER_LEVEL, GetSinglePlayerLevel());
        saveHashTableToFile();
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
        public int UnitNumber;
        public float Rarity;

        public UnitRarity(int unitNumber, float rarity)
        {
            UnitNumber = unitNumber;
            Rarity = rarity;
        }
    }
}
