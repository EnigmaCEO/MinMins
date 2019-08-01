using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMatch : SingletonMonobehaviour<GameMatch>
{
    public class TeamNumbers
    {
        public const int TEAM_1 = 1;
        public const int TEAM_2 = 2;
    }

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
                    unit.Name = (j + 1).ToString();  //default values

                team.Add(unit);
            }

            _teams.Add(team);
        }
    }

    public int GetTeamSize(int teamNumber)
    {
        return GetTeam(teamNumber).Count;
    }

    public UnitData GetUnit(int teamNumber, int unitIndex)
    {
        return GetTeam(teamNumber)[unitIndex];
    }

    public List<UnitData> GetTeam(int teamNumber)
    {
        return _teams[teamNumber - 1];
    }

    public class UnitData
    {
        public string Name = "-1";
        public Vector2 Position = new Vector2();
    }
}
