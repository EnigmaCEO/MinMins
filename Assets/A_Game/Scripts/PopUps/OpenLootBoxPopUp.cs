using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenLootBoxPopUp : MonoBehaviour
{
    [SerializeField] private List<LootGridItem> _lootGridItems; 

    public void Feed(Dictionary<string, int> _unitsWithTier)
    {
        foreach (LootGridItem gridItem in _lootGridItems)
        {
            gridItem.gameObject.SetActive(false);
        }

        int lootIndex = 0;
        foreach (KeyValuePair<string, int> unitWithTier in _unitsWithTier)
        {
            LootGridItem lootItem = _lootGridItems[lootIndex];
            lootItem.SetUnitName(unitWithTier.Key);
            lootItem.SetStars(unitWithTier.Value);
            lootItem.gameObject.SetActive(true);
            lootIndex++;
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
