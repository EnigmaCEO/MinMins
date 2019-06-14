using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationManager : MonoBehaviour
{
    static public LocalizationManager Instance;


    public enum Languages
    {
        English = 0,
        Spanish
    }

    private void Awake()
    {
        Instance = this;

        load();
        TranslateAllTextsInScene();
    }

    public string GetTranslationTerm(Text text)
    {
        string term = "";

        Localize localize = text.GetComponent<Localize>();
        if (localize != null)
            term = localize.Term;

        return term;
    }

    public void TranslateAllTextsInScene()
    {
        Text[] labels = GameObject.FindObjectsOfType<Text>();
        foreach (Text label in labels)
            TranslateText(label);
    }

    //This overload will use label text as term if label has no Localize Component, and will do nothing if it already has it. 
    public void TranslateText(Text label)
    {
        if(label.GetComponent<Localize>() == null)
            TranslateText(label, label.text);
    }

    //This will use or add a localize component, and set term into the component so label text is translated.  An empty term will empty the string, and remove localize, as it will ignore an empty term.
    public void TranslateText(Text label, string term)
    {
        Localize localize = label.GetComponent<Localize>();

        if (term == "")
        {
            if (localize != null)
                Destroy(localize);

            label.text = "";
        }
        else
        {
            if (localize == null)
                localize = label.gameObject.AddComponent<Localize>();

            localize.Term = term;
        }
    }

    public void Save()
    {
        PlayerPrefs.SetString("Language", I2.Loc.LocalizationManager.CurrentLanguage);
    }

    public string GetLanguage()
    {
        return I2.Loc.LocalizationManager.CurrentLanguage;
    }

    public void ChangeLanguage(string language)
    {
        I2.Loc.LocalizationManager.CurrentLanguage = language;
    }

    public void ChangeLanguage(Languages language)
    {
        ChangeLanguage(language.ToString());
    }

    private void load()
    {
        string language = Languages.English.ToString();

        if (PlayerPrefs.HasKey("Language"))
            language = PlayerPrefs.GetString("Language");
        else
        {
            if (Application.systemLanguage == SystemLanguage.Spanish)
                language = Languages.Spanish.ToString();
        }

        ChangeLanguage(language);
    }
}
