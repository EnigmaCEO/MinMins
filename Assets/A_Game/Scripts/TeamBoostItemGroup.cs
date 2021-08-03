using GameConstants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamBoostItemGroup
{
    public string Name = "Unknown";
    public int Amount = 0;
    public int Bonus = 0;
    public string Category = "Unknown";
    public bool IsToken = false;

    public TeamBoostItemGroup(string name, int amount, int bonus, string category, bool isToken)
    {
        Name = name;
        Amount = amount;
        Bonus = bonus;
        Category = category;
        IsToken = isToken;
    }

    public static string GetOreImagePath(string category, int bonus)
    {
        string oreTierSuffix = OreTiers.RAW;

        if (bonus == OreBonuses.PERFECT_ORE)
        {
            oreTierSuffix = OreTiers.PERFECT;
        }
        else if (bonus >= OreBonuses.POLISHED_ORE_MIN)
        {
            oreTierSuffix = OreTiers.POLISHED;
        }

        return ("Images/Ore/" + category + "Ore" + oreTierSuffix);
    }
}
