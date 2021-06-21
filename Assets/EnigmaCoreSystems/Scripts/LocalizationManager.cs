using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LocalizationManager : MonoBehaviour
{
    static public LocalizationManager Instance;

    [SerializeField] private List<SystemLanguage> _supportedLanguages = new List<SystemLanguage> { SystemLanguage.English, SystemLanguage.Spanish, SystemLanguage.Korean,
                                                                                           SystemLanguage.Chinese, SystemLanguage.ChineseSimplified, SystemLanguage.ChineseTraditional  };

    private void Awake()
    {
        Instance = this;

        //load();

        if (EnigmaHacks.Instance.LanguageHack)
        {
            SystemLanguage hackedLanguage = EnigmaHacks.Instance.HackedLanguage;
            Debug.Log("Applying hacked language: " + hackedLanguage.ToString());
            ChangeLanguage(hackedLanguage);
        }
        else
        {
            load();
        }

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += onSceneLoaded;
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= onSceneLoaded;
    }

    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LocalizeAllTextsInScene();
    }

    static public string GetTranslationTerm(Text text)
    {
        string term = "";

        Localize localize = text.GetComponent<Localize>();
        if (localize != null)
            term = localize.Term;

        return term;
    }

    static public void LocalizeAllTextsInScene()
    {
        Text[] labels = GameObject.FindObjectsOfType<Text>();
        foreach (Text label in labels)
            LocalizeText(label);
    }

    //This overload will use label text as term if label has no Localize Component, and will do nothing if it already has it. 
    static public void LocalizeText(Text label)
    {
        //if(label.GetComponent<Localize>() == null)
            LocalizeText(label, label.text);
    }

    //This will use or add a localize component, and set term into the component so label text is translated.  An empty term will empty the string, and remove localize, as it will ignore an empty term.
    static public void LocalizeText(Text label, string term)
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

    static public void Save()
    {
        PlayerPrefs.SetString("Language", I2.Loc.LocalizationManager.CurrentLanguage);
    }

    static public string GetLanguage()
    {
        return I2.Loc.LocalizationManager.CurrentLanguage;
    }

    static public void ChangeLanguage(string language)
    {
        Debug.Log("Change Language: " + language);

        if (language == SystemLanguage.Chinese.ToString())
        {
            language = SystemLanguage.ChineseSimplified.ToString();
        }

        I2.Loc.LocalizationManager.CurrentLanguage = language;
    }

    static public void ChangeLanguage(SystemLanguage language)
    {
        ChangeLanguage(Instance.getSupportedLanguage(language).ToString());
    }

    static public string GetTermTranslation(string term)
    {
        string result = I2.Loc.LocalizationManager.GetTermTranslation(term);

        if (result == null)
        {
            result = term;
        }

        return result;
    }

   private void load()
    {
        string language;

        if (PlayerPrefs.HasKey("Language"))
        {
            language = PlayerPrefs.GetString("Language");
        }
        else
        {
            language = getSupportedLanguage(Application.systemLanguage).ToString();      
        }

        ChangeLanguage(language);
    }

    private SystemLanguage getSupportedLanguage(SystemLanguage language)
    {
        SystemLanguage resultLanguage = SystemLanguage.English;

        if (_supportedLanguages.Contains(language))
        {
            resultLanguage = language;
            Debug.Log("Language " + language.ToString() + " is supported.");
        }
        else
        {
            Debug.Log("Language " + language.ToString() + " was not supported. Returning default language: English");
        }

        return resultLanguage;
    }
}
