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

    public static string GetOreTier(int bonus)
    {
        string oreTier = OreTiers.RAW;

        if (bonus == OreBonuses.PERFECT_ORE_MIN)
        {
            oreTier = OreTiers.PERFECT;
        }
        else if (bonus >= OreBonuses.POLISHED_ORE_MIN)
        {
            oreTier = OreTiers.POLISHED;
        }

        return oreTier;
    }

    public static string GetOreImagePath(string category, int bonus)
    {
        string oreTierSuffix = GetOreTier(bonus);

        return ("Images/Ore/" + category + "Ore" + oreTierSuffix);
    }
}
