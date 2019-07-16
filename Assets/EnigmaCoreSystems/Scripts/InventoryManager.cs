using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{
    private Dictionary<string, Dictionary<string, object>> _items = new Dictionary<string, Dictionary<string, object>>();
    //private Hashtable _saveHashTable = new Hashtable();

    public void AddItem(string groupName, string key, object value, bool shouldCheckGroupExists = true)
    {
        if (shouldCheckGroupExists && !CheckGroupExists(groupName, false))
            _items[groupName] = new Dictionary<string, object>();

        if (HasItem(groupName, key, true, false))
            Debug.LogError("Group " + groupName + " already has item: " + key);
        else
        {
            print("Added item: " + key + " to group: " + groupName);
            _items[groupName].Add(key, value);
        }
    }

    public void UpdateItem(string groupName, string key, object value, bool shouldCheckGroupExists = true)
    {
        if (shouldCheckGroupExists && !CheckGroupExists(groupName, false))
            _items[groupName] = new Dictionary<string, object>();

        if (!HasItem(groupName, key, true, false))
            Debug.LogError("Group " + groupName + " does not have item: " + key);
        else
            _items[groupName][key] = value;
    }

    public bool HasItem(string groupName, string key, bool displayGroupNotFoundError = true, bool shouldCheckGroupExists = true)
    {
        if (shouldCheckGroupExists && !CheckGroupExists(groupName, displayGroupNotFoundError))
            return false;

        return _items[groupName].ContainsKey(key);
    }

    public T GetItem<T>(string groupName, string key)
    {
        return (T)GetItem(groupName, key);
    }

    public object GetItem(string groupName, string key)
    {
        if (!CheckGroupExists(groupName))
            return null;

        if (!_items[groupName].ContainsKey(key))
        {
            Debug.LogError("InventoryManager does not contains key: " + key + " at group: " + groupName);
            return null;
        }

        return _items[groupName][key];
    }

    public int GetGroupSize(string groupName)
    {
        if (!CheckGroupExists(groupName))
            return -1;

        return _items[groupName].Count;
    }

    public void ClearGroup(string groupName)
    {
        if (CheckGroupExists(groupName))
            _items[groupName].Clear();
    }

    public void ClearAllGroups()
    {
        _items.Clear();
    }

    public bool CheckGroupExists(string groupName, bool displayError = true)
    {
        bool groupExists = true;

        if (!_items.ContainsKey(groupName))
        {
            if (displayError)
                Debug.LogError("InventoryManager does not contains group name: " + groupName);

            groupExists = false;
        }

        return groupExists;
    }

    public List<string> GetGroupKeys(string groupName)
    {
        if (!CheckGroupExists(groupName))
            return null;

        List<string> keys = new List<string>();
        foreach (string key in _items[groupName].Keys)
            keys.Add(key);

        return keys;
    }

    public List<object> GetGroupValues(string groupName)
    {
        if (!CheckGroupExists(groupName))
            return null;

        List<object> values = new List<object>();
        foreach (object value in _items[groupName].Values)
            values.Add(value);

        return values;
    }

    public Dictionary<string, object> GetSortedGroup(string groupName, bool isReverse = false)
    {
        List<string> keys = new List<string>();
        Dictionary<string, object> groupToSort = _items[groupName];
        foreach (string key in groupToSort.Keys)
            keys.Add(key);

        keys.Sort();
        if (isReverse)
            keys.Reverse();

        Dictionary<string, object> sortedGroup = new Dictionary<string, object>();
        foreach (string key in keys)
            sortedGroup.Add(key, groupToSort[key]);

        return sortedGroup;
    }

    public void SortGroup(string groupName, bool isReverse = false)
    {
        _items[groupName] = GetSortedGroup(groupName, isReverse);
    }
}
