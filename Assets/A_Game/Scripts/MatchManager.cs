using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : SingletonMonobehaviour<MatchManager>
{
    public int TeamSize = 6;

    public UnitData[] Team1; 
    public UnitData[] Team2;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);

        Team1 = new UnitData[TeamSize];
        Team2 = new UnitData[TeamSize];

        for (int i = 0; i < TeamSize; i++)
        {
            Team1[i] = new UnitData();
            Team2[i] = new UnitData();
        }
    }

    public class UnitData
    {
        public string name;
        public Vector2 position;

        public UnitData()
        {
            name = "1";
            position = new Vector2();
        }
    }
}
