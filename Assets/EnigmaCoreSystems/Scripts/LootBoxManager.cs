using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LootBoxManager : SingletonMonobehaviour<LootBoxManager>
{
    [SerializeField] private int _decimalsForRaritySumChecks = 3;

    public List<string> PickRandomizedNames(int amountToPick, bool cannotRepeat, List<string> namesWithDefaultRarity, Dictionary<string, double> namesWithSpecialRarity = null, bool forbidDefaultRarityDuplicateOption = true)
    {
        List<string> randomizedNames = new List<string>();

        if (amountToPick <= 0)
        {
            return randomizedNames;
        }

        bool defaultListIsValid = (namesWithDefaultRarity != null) && (namesWithDefaultRarity.Count > 0);
        bool specialListValid = (namesWithSpecialRarity != null) && (namesWithSpecialRarity.Count > 0);

        if(forbidDefaultRarityDuplicateOption && defaultListIsValid)
        {
            List<string> duplicateCheckList = new List<string>();
            foreach (string itemName in namesWithDefaultRarity)
            {
                if (!duplicateCheckList.Contains(itemName))
                {
                    duplicateCheckList.Add(itemName);
                }
                else
                {
                    Debug.LogError("Default rarity list has a duplicate: " + itemName);
                    return null;
                }
            }
        }

        //Check if you have at least one non empty list to work with.
        if (!defaultListIsValid && !specialListValid)
        {
            Debug.LogError("At least one of default or rarity lists must be non null and non empty.");
            return null;
        }

        //Check if and index is in both lists. 
        if (defaultListIsValid && specialListValid)
        {
            foreach (string itemName in namesWithDefaultRarity)
            {
                if (namesWithSpecialRarity.ContainsKey(itemName))
                {
                    Debug.LogError("Index: " + itemName + " is both in default and special rarity lists. Please remove it from one of them and try again.");
                    return null;
                }
            }
        }

        if (cannotRepeat)
        {
            if (defaultListIsValid)
            {
                //Check if amount to pick is greater than default options. This cannot happen because special rarity pick might not happen and there can be no default options left to pick.
                if (amountToPick > namesWithDefaultRarity.Count)
                {
                    Debug.LogError("With valid default rarity list, amount to pick needs to at least be equal to default rarity options amount. Amount to pick: " + amountToPick + " special rarity amount: " + namesWithDefaultRarity.Count);
                    return null;
                }
            }
            //Check if amount to pick is greater or equal than special options when there are no default options. Equal makes no sense and greater is not possible.
            else if (amountToPick >= namesWithSpecialRarity.Count)
            {
                Debug.LogError("With not valid default rarity list, amount to pick should be less than the special rarity options amount. Amount to pick: " + amountToPick + " special rarity amount: " + namesWithSpecialRarity.Count);
                return null;
            }
        }

        if (specialListValid)
        {
            double specialRarityTotal = 0.000f;
            foreach (float specialRarity in namesWithSpecialRarity.Values)
            {
                specialRarityTotal += specialRarity;
            }

            specialRarityTotal = System.Math.Round(specialRarityTotal, _decimalsForRaritySumChecks);

            //Check if special rarity total ir greater than 1
            if (specialRarityTotal > 1.000f)
            {
                Debug.LogError("Special rarities together cannot be greater than 1. They sum: " + specialRarityTotal);
                return null;
            }

            //Check if special rarity total es bigger than 1, which is illogical. 
            if (defaultListIsValid)
            {
                //Check if special rarity total makes no room for default list pick.
                if (specialRarityTotal == 1.000f)
                {
                    Debug.LogError("Special rarities together cannot be equal to one if there are default rarity indexes. They sum: " + specialRarityTotal);
                    return null;
                }
            }
            else if (specialRarityTotal < 1.000f)
            {
                Debug.LogError("Special rarities together should be 1 if there are not default rarity indexes. They sum: " + specialRarityTotal);
                return null;
            }
        }

        List<string> defaultList = null;
        Dictionary<string, double> specialList = null;

        //Create copies so originals are not modified.
        if (defaultListIsValid)
        {
            defaultList = new List<string>(namesWithDefaultRarity);
        }

        if (specialListValid)
        {
            specialList = new Dictionary<string, double>(namesWithSpecialRarity);
        }

        for (int i = 0; i < amountToPick; i++)
        {
            bool nameWasSelected = false;

            if (specialListValid)
            {
                double specialRarityCheckSum = 0;
                double randomValue = (double)Random.Range(0.0f, 1.0f);
                int specialListCount = specialList.Count;
                int count = 0;

                foreach(string unitName in specialList.Keys)
                {
                    count++;

                    double rarity = specialList[unitName];
                    specialRarityCheckSum += rarity;
                    if ((randomValue <= specialRarityCheckSum) || (count == specialListCount))
                    {
                        randomizedNames.Add(unitName);

                        if (cannotRepeat)
                        {
                            specialList.Remove(unitName);
                        }

                        nameWasSelected = true;
                        break;
                    }
                }
            }

            if(defaultListIsValid && !nameWasSelected)
            {
                string nameToAdd = defaultList[Random.Range(0, defaultList.Count)];

                if (cannotRepeat)
                {
                    defaultList.Remove(nameToAdd);
                }

                randomizedNames.Add(nameToAdd);
            }
        }

        return randomizedNames;
    }

    public Dictionary<string, object> PickRandomObjects(Dictionary<string, object> possibilities)
    {
        Dictionary<string, object> randomObjects = new Dictionary<string, object>();

        List<string> possibilitiesKeys = new List<string>();
        foreach (string key in possibilities.Keys)
            possibilitiesKeys.Add(key);

        int iterations = possibilitiesKeys.Count;
        for (int i = 0; i < iterations; i++)
        {
            string keyToAdd = possibilitiesKeys[Random.Range(0, possibilitiesKeys.Count)];
            possibilitiesKeys.Remove(keyToAdd);

            randomObjects.Add(keyToAdd, possibilities[keyToAdd]);
        }

        return randomObjects;
    }
}
