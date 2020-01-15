using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamBoostItem
{
    public string Name = "Unknown";
    public int Amount = 0;
    public int Bonus = 0;
    public string Category = "Unknown";

    public TeamBoostItem(string name, int amount, int bonus, string category)
    {
        Name = name;
        Amount = amount;
        Bonus = bonus;
        Category = category;
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
