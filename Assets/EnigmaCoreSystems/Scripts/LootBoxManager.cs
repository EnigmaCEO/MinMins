using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootBoxManager : SingletonMonobehaviour<LootBoxManager>
{
    [SerializeField] private int _decimalsForRaritySumChecks = 3;

    public List<int> PickRandomizedIndexes(int amountToPick, bool cannotRepeat, List<int> indexesWithDefaultRarity, Dictionary<int, double> indexesWithSpecialRarity = null, bool forbidDefaultRarityDuplicates = true)
    {
        bool defaultListIsValid = (indexesWithDefaultRarity != null) && (indexesWithDefaultRarity.Count > 0);
        bool specialListValid = (indexesWithSpecialRarity != null) && (indexesWithSpecialRarity.Count > 0);

        if(forbidDefaultRarityDuplicates && defaultListIsValid)
        {
            List<int> duplicateCheckList = new List<int>();
            foreach (int index in indexesWithDefaultRarity)
            { 
                if (!duplicateCheckList.Contains(index))
                    duplicateCheckList.Add(index);
                else
                {
                    Debug.LogError("Default rarity list has a duplicate: " + index);
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
            foreach (int index in indexesWithDefaultRarity)
            {
                if (indexesWithSpecialRarity.ContainsKey(index))
                {
                    Debug.LogError("Index: " + index + " is both in default and special rarity lists. Please remove it from one of them and try again.");
                    return null;
                }
            }
        }

        if (cannotRepeat)
        {
            if (defaultListIsValid)
            {
                //Check if amount to pick is greater than default options. This cannot happen because special rarity pick might not happen and there can be no default options left to pick.
                if (amountToPick > indexesWithDefaultRarity.Count)
                {
                    Debug.LogError("With valid default rarity list, amount to pick needs to at least be equal to default rarity options amount. Amount to pick: " + amountToPick + " special rarity amount: " + indexesWithDefaultRarity.Count);
                    return null;
                }
            }
            //Check if amount to pick is greater or equal than special options when there are no default options. Equal makes no sense and greater is not possible.
            else if (amountToPick >= indexesWithSpecialRarity.Count)
            {
                Debug.LogError("With not valid default rarity list, amount to pick should be less than the special rarity options amount. Amount to pick: " + amountToPick + " special rarity amount: " + indexesWithSpecialRarity.Count);
                return null;
            }
        }

        if (specialListValid)
        {
            double specialRarityTotal = 0.000f;
            foreach (float specialRarity in indexesWithSpecialRarity.Values)
                specialRarityTotal += specialRarity;

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

        List<int> defaultList = null;
        Dictionary<int, double> specialList = null;

        //Create copies so originals are not modified.
        if (defaultListIsValid)
            defaultList = new List<int>(indexesWithDefaultRarity);

        if(specialListValid)
            specialList = new Dictionary<int, double>(indexesWithSpecialRarity);

        List<int> randomizedIndexes = new List<int>();
        for (int i = 0; i < amountToPick; i++)
        {
            bool indexWasSelected = false;

            if (specialListValid)
            {
                double specialRarityCheckSum = 0;
                double randomValue = (double)Random.Range(0.0f, 1.0f);

                foreach (KeyValuePair<int, double> entry in specialList)
                {
                    specialRarityCheckSum += entry.Value;
                    if (randomValue <= specialRarityCheckSum)
                    {
                        randomizedIndexes.Add(entry.Key);
                        if (cannotRepeat)
                            specialList.Remove(entry.Key);

                        indexWasSelected = true;
                        break;
                    }
                }
            }

            if(defaultListIsValid && !indexWasSelected)
            {
                int iterations = defaultList.Count;
                for (int j = 0; j < iterations; j++)
                {
                    int indexToAdd = defaultList[Random.Range(0, defaultList.Count)];

                    if(cannotRepeat)
                        defaultList.Remove(indexToAdd);

                    randomizedIndexes.Add(indexToAdd);
                }
            }
        }

        return randomizedIndexes;
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
