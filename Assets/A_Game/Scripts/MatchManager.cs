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

            if (i < (TeamSize - 1))
            {
                Team1[i].name = (i + 1).ToString();  //default values
                Team2[i].name = (i + 1).ToString(); // default values
            }
        }
    }

    public class UnitData
    {
        public string name = "-1";
        public Vector2 position = new Vector2();
    }
}
