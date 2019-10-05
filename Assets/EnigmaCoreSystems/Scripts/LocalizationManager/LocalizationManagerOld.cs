using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enigma.CoreSystems;

public class LocalizationManagerOld : Manageable<LocalizationManagerOld>
{
	private string configFileLocation = "Localizations/config";
	public string languageToUse;

	protected override void Awake ()
	{
		base.Awake ();
		TextAsset config = Resources.Load (configFileLocation) as TextAsset;
		string languageLoaded = config.text.Replace ("\n", "");
		languageToUse = languageLoaded;

		string lang = PlayerPrefs.GetString ("language", "");

		if (lang == "") {
			if (Application.systemLanguage == SystemLanguage.ChineseSimplified)
				languageToUse = "ChineseSimplified";
			else if (Application.systemLanguage == SystemLanguage.ChineseTraditional)
				languageToUse = "ChineseTraditional";
			else if (Application.systemLanguage == SystemLanguage.Spanish)
				languageToUse = "Spanish";
			else
				languageToUse = "English";
			
		} else {
			if (lang == SystemLanguage.ChineseSimplified.ToString ())
				languageToUse = "ChineseSimplified";
			if (lang == SystemLanguage.ChineseTraditional.ToString ())
				languageToUse = "ChineseTraditional";
			if (lang == SystemLanguage.Spanish.ToString ())
				languageToUse = "Spanish";
			if (lang == "Hindi")
				languageToUse = "Hindi";
			if (lang == SystemLanguage.English.ToString ())
				languageToUse = "English";
		}
	}

	// Use this for initialization
	protected override void Start ()
	{
	
	}
	
	// Update is called once per frame
	protected override void Update ()
	{
	
	}

	public void ChangeLanguage (string language)
	{
		languageToUse = language;
		//UIManager.Instance.LocalizationChanged ();
	}
}
