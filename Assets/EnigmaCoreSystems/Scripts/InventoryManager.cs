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
        if (shouldCheckGroupExists && !checkGroupExists(groupName, false))
            _items[groupName] = new Dictionary<string, object>();

        if (HasItem(groupName, key, true, false))
            Debug.LogError("Group " + groupName + " already has item: " + key);
        else 
            _items[groupName].Add(key, value);
    }

    public void UpdateItem(string groupName, string key, object value, bool shouldCheckGroupExists = true)
    {
        if (shouldCheckGroupExists && !checkGroupExists(groupName, false))
            _items[groupName] = new Dictionary<string, object>();

        if (!HasItem(groupName, key, true, false))
            Debug.LogError("Group " + groupName + " does not have item: " + key);
        else
            _items[groupName][key] = value;
    }

    public bool HasItem(string groupName, string key, bool displayGroupNotFoundError = true, bool shouldCheckGroupExists = true)
    {
        if (shouldCheckGroupExists && !checkGroupExists(groupName, displayGroupNotFoundError))
            return false;

        return _items[groupName].ContainsKey(key);
    }

    public T GetItem<T>(string groupName, string key)
    {
        return (T)GetItem(groupName, key);
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

    public void ClearAllGroups()
    {
        _items.Clear();
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

    //public void LoadData()
    //{
    //    _saveHashTable.Clear();
    //    _saveHashTable = FileManager.Instance.LoadData();

    //    _items.Clear();
    //    foreach (DictionaryEntry entry in _saveHashTable)
    //    {
    //        string[] terms = entry.Key.ToString().Split(_parseSeparator);
    //        string groupName = terms[0];
    //        string itemKey = terms[1];
    //        object item = Deserialize((string)entry.Value);
    //        print("LoadData -> groupName: " + groupName + " -> itemKey: " + itemKey + " -> item: " + item.ToString());
    //        AddItem(groupName, itemKey, item);
    //    }
    //}

    //public void SaveGroup(string groupName)
    //{
    //    foreach (string key in GetGroupKeys(groupName))
    //        SaveItem(groupName, key, false);

    //    FileManager.Instance.SaveData(_saveHashTable);
    //}

    //public void SaveItem(string groupName, string key, bool isStandAlone = true)
    //{
    //    if (_saveHashTable.ContainsKey(key))
    //        _saveHashTable.Remove(key);

    //    object item = GetItem(groupName, key);
    //    string serializedItem = Serialize(item);
    //    string hashKey = groupName + _parseSeparator + key;
    //    print("SaveItem -> hashKey: " + hashKey + " -> serializedItem: " + serializedItem);
    //    _saveHashTable.Add(hashKey, serializedItem);

    //    if (isStandAlone)
    //        FileManager.Instance.SaveData(_saveHashTable);
    //}

    //static public string Serialize(object obj)
    //{
    //    print("serialize input: -> obj: " + obj.ToString());
    //    Newtonsoft.Json.JsonSerializerSettings settings = new Newtonsoft.Json.JsonSerializerSettings();
    //    settings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    //    string str = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented, settings);
    //    print("serialize output: -> str: " + str);
    //    return str;
    //}

    //static public object Deserialize(string str)
    //{
    //    return Deserialize<object>(str);
    //}

    //static public T Deserialize<T>(string str)
    //{
    //    print("deserialize<T> input: -> str: " + str);
    //    T result = (T)Newtonsoft.Json.JsonConvert.DeserializeObject(str);
    //    print("deserialize<T> output: -> T: " + typeof(T).ToString() + " -> result: "  + result.ToString());
    //    return result;
    //}

    //static public object Deserialize(string str)
    //{
    //    print("deserialize input: -> str: " + str);
    //    object result = Newtonsoft.Json.JsonConvert.DeserializeObject(str);
    //    print("deserialize output: " + result);
    //    return result;
    //}
}
