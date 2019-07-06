using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInventory : SingletonMonobehaviour<GameInventory>
{
    private const string _TEAM_1_GROUP_NAME = "Team1";

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
        AddLootBoxToInventory(20); //TODO: Remove test
    }

    public List<int> GetInventoryUnitIndexes()
    {
        List<int> inventoryUnitIndexes = new List<int>();

        foreach (object index in InventoryManager.Instance.GetGroupValues(_TEAM_1_GROUP_NAME))
            inventoryUnitIndexes.Add((int)index);

        return inventoryUnitIndexes;
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
            if (inventoryManager.HasItem(_TEAM_1_GROUP_NAME, lootBoxIndex.ToString()))
            {
                //TODO: Add exp to Unit
            }
            else
                inventoryManager.AddItem(_TEAM_1_GROUP_NAME, lootBoxIndex.ToString(), lootBoxIndex);
        }
    }

    private void initializeInventory()
    {
        InventoryManager inventoryManager = InventoryManager.Instance;

        inventoryManager.AddItem(_TEAM_1_GROUP_NAME, "1", 1);
        inventoryManager.AddItem(_TEAM_1_GROUP_NAME, "2", 2);
        inventoryManager.AddItem(_TEAM_1_GROUP_NAME, "3", 3);
        inventoryManager.AddItem(_TEAM_1_GROUP_NAME, "4", 4);
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
