using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager>
{   
    private Dictionary<string, Dictionary<string, object>> _items = new Dictionary<string, Dictionary<string, object>>();

    public void AddItem(string groupName, string key, object value, bool shouldCheckGroupExists = true)
    {
        if (shouldCheckGroupExists && !checkGroupExists(groupName, false))
            _items[groupName] = new Dictionary<string, object>();

        _items[groupName].Add(key, value);
    }

    public bool HasItem(string groupName, string key)
    {
        if (!checkGroupExists(groupName))
            return false;

        return _items[groupName].ContainsKey(key);
    }

    public object GetItem(string groupName, string key)
    {
        if (!checkGroupExists(groupName))
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
        if (!checkGroupExists(groupName))
            return -1;

        return _items[groupName].Count;
    }

    public void ClearGroup(string groupName)
    {
        if (checkGroupExists(groupName))
            _items[groupName].Clear();
    }

    private bool checkGroupExists(string groupName, bool displayError = true)
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
        if (!checkGroupExists(groupName))
            return null;

        List<string> keys = new List<string>();
        foreach (string key in _items[groupName].Keys)
            keys.Add(key);

        return keys;
    }

    public List<object> GetGroupValues(string groupName)
    {
        if (!checkGroupExists(groupName))
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

    /*
    private string serialize(object obj)
    {
        print("serialize: -> obj: " + obj.ToString());
        string str = JsonUtility.ToJson(obj);
        print("serialize: -> str: " + str);
        return str;
    }

    private object deserialize(string str)
    {
        return deserialize<object>(str);
    }

    private T deserialize<T>(string str)
    {
        return JsonUtility.FromJson<T>(str);
    }
    */
}
