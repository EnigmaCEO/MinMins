using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileManager : MonoBehaviour
{
    [SerializeField] private string _egi = "EGI_DAILY_QUIZ";
    [SerializeField] private string _secKey = "dataSec";
    [SerializeField] private string _fileName = "data.txt";
    [SerializeField] private char _entrySeparator = '/';
    [SerializeField] private char _keyValueSeparator = '=';

    [SerializeField] private bool _resetAtStart = false; //Hack for testing

    private string _filePath = "";

    public static FileManager Instance;


    private void Awake()
    {
        Instance = this;
        _filePath = Application.persistentDataPath + "/" + _fileName;

        if (_resetAtStart)
            FileManager.Instance.ClearData();
    }

    public Hashtable LoadData()
    {
        Hashtable data = new Hashtable();

        if (!System.IO.File.Exists(_filePath))
            return data;

        string dataString = System.IO.File.ReadAllText(_filePath);
        if (dataString == "")
            return data; 

        string fileSec = Enigma.CoreSystems.NetworkManager.md5(dataString + _egi);

        //Set default values to 0 if security code wasn't matched
        if (fileSec != PlayerPrefs.GetString(_secKey, ""))
            return data;  
        else
        {
            string[] entriesString = dataString.Split(_entrySeparator);
            foreach (string entryString in entriesString)
            {
                string[] pairString = entryString.Split(_keyValueSeparator);
                data.Add(pairString[0], pairString[1]);
            }

            return data;
        }
    }

    public void SaveData(Hashtable hashtable)
    {
        string dataToWrite = "";

        foreach (DictionaryEntry entry in hashtable)
            dataToWrite += entry.Key.ToString() + _keyValueSeparator + entry.Value.ToString() + _entrySeparator;

        dataToWrite = dataToWrite.Remove(dataToWrite.Length - 1);

        PlayerPrefs.SetString(_secKey, Enigma.CoreSystems.NetworkManager.md5(dataToWrite + _egi));
        System.IO.File.WriteAllText(_filePath, dataToWrite);
    }

    public void ClearData()
    {
        System.IO.File.WriteAllText(_filePath, "");
    }
}
