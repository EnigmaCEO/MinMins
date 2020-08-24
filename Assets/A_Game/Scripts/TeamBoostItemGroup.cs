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
        string oreTierSuffix = GameConstants.OreTier.RAW;

        if (bonus == 10)
        {
            oreTierSuffix = GameConstants.OreTier.POLISHED;
        }
        else if (bonus >= 6)
        {
            oreTierSuffix = GameConstants.OreTier.PERFECT;
        }

        return ("Images/Ore/" + category + "Ore" + oreTierSuffix);
    }
}
