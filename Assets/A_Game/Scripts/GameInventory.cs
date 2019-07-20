using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInventory : SingletonMonobehaviour<GameInventory>
{
    [SerializeField] private int _unitsAmount = 80;
    [SerializeField] private int _initialBronzeLootBoxes = 2;

    [SerializeField] private int _lootBoxSize = 3;
    [SerializeField] private int _lootBoxTiersAmount = 3;
    [SerializeField] private int _guaranteedUnitTierAmount = 1;

    [SerializeField] private char _parseSeparator = '|';
    [SerializeField] private int _expToAddOnDuplicateUnit = 50;

    public const string RATING_KEY = "Rating";

    private const string _TEAM_1_GROUP_NAME = "Team1";
    private const string _LOOT_BOXES = "Lootboxes";
    private const string _STATS = "Stats";

    private const int _BRONZE_TIER = 1;
    private const int _SILVER_TIER = 2;
    private const int _GOLD_TIER = 3;

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

        if (inventoryManager.CheckGroupExists(_TEAM_1_GROUP_NAME, false))
        {
            foreach (string unitName in inventoryManager.GetGroupKeys(_TEAM_1_GROUP_NAME))
                inventoryUnitIndexes.Add(unitName);
        }

        return inventoryUnitIndexes;
    }

    public int GetRating()
    {
        return InventoryManager.Instance.GetItem<int>(_STATS, RATING_KEY);
    }

    public void ChangeRatingAmount(int amount, bool isAddition, bool shouldSave)
    {
        int newRatingAmount = GetRating();

        if (isAddition)
            newRatingAmount += amount;
        else
            newRatingAmount -= amount;

        InventoryManager.Instance.UpdateItem(_STATS, RATING_KEY, newRatingAmount);

        if (shouldSave)
            saveRating();
    }

    public UnitStats GetUnitStats(string unitName)
    {
        return InventoryManager.Instance.GetItem<UnitStats>(_TEAM_1_GROUP_NAME, unitName);
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
        if (boxTier == _BRONZE_TIER)
            unitPicks = lootBoxManager.PickRandomizedNumbers(_lootBoxSize, true, null, specialRarities);
        else
        {
            int specialRarityAmountToPick = _lootBoxSize - _guaranteedUnitTierAmount;
            unitPicks = lootBoxManager.PickRandomizedNumbers(specialRarityAmountToPick, true, null, specialRarities);
            List<int> guaranteedTierUnits = null;
            if (boxTier == _SILVER_TIER)
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
            if (inventoryManager.HasItem(_TEAM_1_GROUP_NAME, unitName, false))
            {
                UnitStats stats = inventoryManager.GetItem<UnitStats>(_TEAM_1_GROUP_NAME, unitName);
                stats.Exp += _expToAddOnDuplicateUnit;
                print("Added " + _expToAddOnDuplicateUnit + " exp to unit " + unitName + " for new exp of: " + stats.Exp);
            }
            else
            {
                GameObject minMinPrefab = Resources.Load<GameObject>("Prefabs/MinMinUnits/" + unitName);
                UnitStats stats = new UnitStats(minMinPrefab.GetComponent<MinMinUnit>().Stats);
                inventoryManager.AddItem(_TEAM_1_GROUP_NAME, unitName, stats);
            }
        }

        saveUnits();
        ChangeLootBoxAmount(1, boxTier, false, true);

        return unitPicks;
    }

    public int GetUnitTier(int unitNumber)
    {
        int unitTier = _BRONZE_TIER;
        if (_tierSilver_units.Contains(unitNumber))
            unitTier = _SILVER_TIER;
        else if (_tierGold_units.Contains(unitNumber))
            unitTier = _GOLD_TIER;

        return unitTier;
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

        inventoryManager.AddItem(_STATS, RATING_KEY, 0);

        for(int tier = 1; tier <= _lootBoxTiersAmount; tier++)
            inventoryManager.AddItem(_LOOT_BOXES, tier.ToString(), 0);

        bool isThereAnyUnit = false;
        bool isThereAnyLootBox = false;

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

                isThereAnyUnit = true;
            }
            else if (groupName == _LOOT_BOXES)
            {
                int tier = int.Parse(terms[1]);
                int tierAmount = int.Parse((string)entry.Value);
                print("LoadData -> box tier: " + tier + " amount: " + tierAmount);
                inventoryManager.UpdateItem(_LOOT_BOXES, tier.ToString(), tierAmount, false);

                isThereAnyLootBox = true;
            }
            else if (groupName == _STATS)
            {
                int rating = int.Parse(terms[1]);
                inventoryManager.UpdateItem(_STATS, RATING_KEY, rating);
            }
        }

        if (!isThereAnyUnit && !isThereAnyLootBox)
        {
            inventoryManager.UpdateItem(_LOOT_BOXES, _BRONZE_TIER.ToString(), _initialBronzeLootBoxes, false);
            saveLootBoxes();
        }
    }

    private void saveRating()
    {
        if (_saveHashTable.ContainsKey(RATING_KEY))
            _saveHashTable.Remove(RATING_KEY);

        _saveHashTable.Add(RATING_KEY, GetRating());
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

    private void saveUnits()
    {
        InventoryManager inventoryManager = InventoryManager.Instance;
        foreach (string unitName in inventoryManager.GetGroupKeys(_TEAM_1_GROUP_NAME))
            saveUnit(_TEAM_1_GROUP_NAME, unitName, false);

        saveHashTableToFile();
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
