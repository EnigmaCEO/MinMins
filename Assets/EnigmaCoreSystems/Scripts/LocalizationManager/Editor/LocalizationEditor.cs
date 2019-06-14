using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using I2.Loc;

public class LocalizationEditor : Editor
{ 
    [MenuItem("Enigma Games/Localization/RemoveLocalizeSetTerms")]
    public static void RemoveLocalizeSetTerm()
    {
        Text[] labels = GameObject.FindObjectsOfType<Text>();
        foreach (Text label in labels)
        {
            string term = label.text;

            Localize oldLocalize = label.gameObject.GetComponent<Localize>();
            if (oldLocalize != null)
            {
                term = oldLocalize.Term;
                DestroyImmediate(oldLocalize);

                label.text = term;
            }
        }
    }

    [MenuItem("Enigma Games/Localization/English")]
    public static void ChangeToEnglish()
    {
        ChangeLocalizationFile("English");
    }

    [MenuItem("Enigma Games/Localization/Simplified Chinese")]
    public static void ChangeToSimplifiedChinese()
    {
        ChangeLocalizationFile("ChineseSimplified");
    }

    [MenuItem("Enigma Games/Localization/Traditional Chinese")]
    public static void ChangeToTraditionalChinese()
    {
        ChangeLocalizationFile("ChineseTraditional");
    }

    private static void ChangeLocalizationFile(string localization)
    {
        string fileName = Application.dataPath + "/Resources/Localizations/config.txt";
        if( File.Exists(fileName ))
        {
            File.WriteAllText(fileName, localization);
            Debug.Log("Changing the localization to: " + localization);
            AssetDatabase.Refresh();
        }
    }
}
