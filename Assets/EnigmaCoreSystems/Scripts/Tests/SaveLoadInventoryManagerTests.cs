using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadInventoryManagerTests : MonoBehaviour
{
    public Vector2 testVector1 = new Vector2(2, 4);
    public Vector3 testVector2 = new Vector3(4, 6, 8);
    public Transform myTestGO;

    void Start()
    {
        //InventoryManager inventoryManager = InventoryManager.Instance;

        //inventoryManager.AddItem("Group1", "myKey1", testVector1);
        //inventoryManager.AddItem("Group2", "TheKey", testVector2);
        //inventoryManager.AddItem("Group3", "MyGO", myTestGO);

        //inventoryManager.AddItem("Group1", "myKey1", "Lucas");
        //inventoryManager.AddItem("Group2", "TheKey", 2);
        //inventoryManager.AddItem("Group3", "MyGO", "3");

        //inventoryManager.SaveGroup("Group1");
        //inventoryManager.SaveGroup("Group2");
        //inventoryManager.SaveGroup("Group3");

        //inventoryManager.LoadData();

        //Vector2 vectorBack = (Vector2)inventoryManager.GetItem("Group1", "myKey1");

        //foreach (string key in inventoryManager.GetGroupKeys("Group1"))
        //    print("Key: " + key + " value: " + inventoryManager.GetItem("Group1", key));

        //foreach (string key in inventoryManager.GetGroupKeys("Group2"))
        //    print("Key: " + key + " value: " + inventoryManager.GetItem("Group2", key));

        //foreach (string key in inventoryManager.GetGroupKeys("Group3"))
        //    print("Key: " + key + " value: " + inventoryManager.GetItem("Group3", key));
    }
}
