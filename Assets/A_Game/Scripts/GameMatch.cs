using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMatch : SingletonMonobehaviour<GameMatch>
{
    public int TeamsAmount = 2;
    public int TeamSize = 6;

    private List<List<UnitData>> _teams = new List<List<UnitData>>(); 

    void Awake()
    {
        for (int i = 0; i < TeamsAmount; i++)
        {
            List<UnitData> team = new List<UnitData>(TeamSize);

            for (int j = 0; j < TeamSize; j++)
            {
                UnitData unit = new UnitData();

                if (j < (TeamSize - 1)) //Last one is Locked
                    unit.name = (j + 1).ToString();  //default values

                team.Add(unit);
            }

            _teams.Add(team);
        }
    }

    public int GetTeamSize(int teamNumber)
    {
        return getTeam(teamNumber).Count;
    }

    public UnitData GetUnit(int teamNumber, int unitIndex)
    {
        return getTeam(teamNumber)[unitIndex];
    }

    private List<UnitData> getTeam(int teamNumber)
    {
        return _teams[teamNumber - 1];
    }

    public class UnitData
    {
        public string name = "-1";
        public Vector2 position = new Vector2();
    }
}
