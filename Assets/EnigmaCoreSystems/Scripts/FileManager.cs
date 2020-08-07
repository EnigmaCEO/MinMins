using CodeStage.AntiCheat.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileManager : MonoBehaviour
{
    [SerializeField] private string _secKey = "dataSec";
    [SerializeField] private string _egi = "EGI_DAILY_QUIZ";
    [SerializeField] private string _fileName = "data.txt";
    [SerializeField] private char _entrySeparator = '/';
    [SerializeField] private char _keyValueSeparator = '=';


    private string _filePath = "";

    public static FileManager Instance;


    private void Awake()
    {
        Instance = this;
        _filePath = Application.persistentDataPath + "/" + _fileName;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        if (EnigmaHacks.Instance.ResetFileManagerAtStart)
        {
            FileManager.Instance.ClearData();
        }
#endif
    }

    public bool CheckFileNull()
    {
        return !System.IO.File.Exists(_filePath);
    }

    public bool CheckFileNullOrEmpty()
    {
        if (CheckFileNull())
        {
            return true;
        }

        string dataString = System.IO.File.ReadAllText(_filePath);
        if (dataString == "")
        {
            return true;
        }

        return false;
    }

    public Hashtable LoadData()
    {
        Hashtable data = new Hashtable();

        if(CheckFileNull())
        {
            return data;
        }

        string dataString = System.IO.File.ReadAllText(_filePath);
        if (dataString == "")
        {
            return data;
        }

        //Set default values to 0 if security code wasn't matched
        if (CheckDataStringIsSecure(dataString))
        {
            Debug.LogWarning("LoadData::Security Breach.");
            return data;
        }
        else
        {
            return GetHashtableFromDataString(dataString);
        }
    }

    public Hashtable GetHashtableFromDataString(string dataString)
    {
        Hashtable data = new Hashtable();

        string[] entriesString = dataString.Split(_entrySeparator);
        foreach (string entryString in entriesString)
        {
            string[] pairString = entryString.Split(_keyValueSeparator);
            data.Add(pairString[0], pairString[1]);
        }

        return data;
    }

    public bool CheckDataStringIsSecure(string dataString)
    {
        string fileSec = Enigma.CoreSystems.NetworkManager.md5(dataString + _egi);
        bool invalidDataString = (fileSec != PlayerPrefs.GetString(_secKey, ""));
        return invalidDataString;
    }

    public void SaveDataWithSecurity(Hashtable hashtable)
    {
        string dataString = ConvertHashToDataStringAndSetSecurity(hashtable);
        SaveDataRaw(dataString);
    }

    public void SaveDataRaw(string dataString)
    {
        System.IO.File.WriteAllText(_filePath, dataString);
    }

    public string ConvertHashToDataStringAndSetSecurity(Hashtable hashtable)
    {
        string dataToWrite = "";

        foreach (DictionaryEntry entry in hashtable)
        {
            if (dataToWrite != "")
            {
                dataToWrite += _entrySeparator;
            }

            dataToWrite += entry.Key.ToString() + _keyValueSeparator + entry.Value.ToString();
        }

        PlayerPrefs.SetString(_secKey, Enigma.CoreSystems.NetworkManager.md5(dataToWrite + _egi));

        return dataToWrite;
    }

    public void ClearData()
    {
        System.IO.File.WriteAllText(_filePath, "");
    }
}
