using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonSerializationTests : MonoBehaviour
{
    public UnitStats Stats;

    void Start()
    {
        string statsSerialized = Stats.Serialized();
        UnitStats newStats = new UnitStats(statsSerialized);

        //InventoryManager.Serialize("2");
        //InventoryManager.Serialize(2.0f);
        //InventoryManager.Deserialize<string>("\"2\"");
        //InventoryManager.Deserialize<int>("2");
        //InventoryManager.Serialize("Word");
        //InventoryManager.Deserialize<string>(InventoryManager.Serialize("Word"));
        //string serializedString = InventoryManager.Serialize("Word");
        //InventoryManager.Deserialize(serializedString);
    }
}
