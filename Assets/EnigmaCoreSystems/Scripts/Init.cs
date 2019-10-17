using UnityEngine;
using System.Collections;
using Enigma.CoreSystems;
using System;

public class Init : EnigmaScene
{
    [SerializeField] string url = "https://min-mins.herokuapp.com/"; //"https://mykin-server.com";
    [SerializeField] string serverAddress = "GCHFGWHCU7GUF2E773D54XHFFE4662PP7BVPJZCDXC2ZNEJUVXXYEDVA"; //"GAFWSBEOGCCYVEC5YZUILDEDGHO27PODVTQJ45DSFBODKRXQ42MVLIZZ";

    [SerializeField] private string _nextSceneName = "Main";
    [SerializeField] private KinWrapper _kinWrapper;

	private float _startTime;

    //private const int SCHEDULED_NOTIFICATION_TIME = 1440; // 1440 minutes, thus 24 hours

    void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        //Screen.SetResolution(1920, 1080, true, 60);
        Screen.SetResolution(1280, 720, true, 60);
        //Screen.orientation = ScreenOrientation.Landscape;
#endif

        //IAPManager.WatchPlayers(QuizConfig.Instance.OneAudienceAppKey);

#if UNITY_EDITOR
        Resolution[] resolutions = Screen.resolutions;
        foreach (Resolution res in resolutions)
        {
            //print(res.width + "x" + res.height);
        }
        Screen.SetResolution(resolutions[resolutions.Length-1].width, resolutions[resolutions.Length - 1].height, true);
        //Screen.SetResolution(1680, 1050, false);
#endif

        try
        {
			//SoundManager.MusicVolume = PlayerPrefs.GetFloat(Settings.SAVED_MUSIC_PREF, 1.0f);
		}
		catch (PlayerPrefsException e)
		{
			//SoundManager.MusicVolume = 0.5f;
		}
		
		try
		{
			//SoundManager.SfxVolume = PlayerPrefs.GetFloat(Settings.SAVED_SOUND_EFFECTS_PREF, 1.0f); 
		}
		catch (PlayerPrefsException e)
		{
			//SoundManager.SfxVolume = 1.0f;
		}
    }
    // Use this for initialization
    void Start()
	{
		_startTime = Time.time;

        //handleNotifications();
        //SoundManager.Play("intro", SoundManager.AudioTypes.Sfx);
    }

    // Update is called once per frame
    void Update()
    {
        Misc.HandleScreenFade(_startTime, _nextSceneName);
    }

    /*
    private void handleNotifications()
    {
        CoreSystemManager.ClearAllLocalNotifications();

        DateTime dt = DateTime.Now;
        DayOfWeek day = dt.DayOfWeek;

        print("Day of the week: " + day.ToString());

        if (day == DayOfWeek.Monday)
        {
            setDailyNotification(0);
            setWeekleyNotification(6);
        }
        else if (day == DayOfWeek.Tuesday)
        {
            setDailyNotification(GameInventory.BonusDay3);
            setWeekleyNotification(5);
        }
        else if (day == DayOfWeek.Wednesday)
        {
            setDailyNotification(0);
            setWeekleyNotification(4);
        }
        else if (day == DayOfWeek.Thursday)
        {
            setDailyNotification(GameInventory.BonusDay5);
            setWeekleyNotification(3);
        }
        else if (day == DayOfWeek.Friday)
        {
            setDailyNotification(0);
            setWeekleyNotification(2);
        }
        else if (day == DayOfWeek.Saturday)
        {
            setDailyNotification(GameInventory.BonusDay7);
            setWeekleyNotification(1);
        }
        else if (day == DayOfWeek.Sunday)
        {
            setDailyNotification(GameInventory.BonusDay1);
            setWeekleyNotification(7);
        }
    }

    private void setDailyNotification(int coinPrize)
    {
        string message = I2.Loc.LocalizationManager.GetTermTranslation("NewQuizAvailable");
        if (coinPrize > 0)
            message = I2.Loc.LocalizationManager.GetTermTranslation("LoginNowForFreeCoins") + ": " + coinPrize.ToString();

        CoreSystemManager.ScheduleLocalNotification("Daily Quiz: " + QuizConfig.Instance.GameFullName, message, 0 , SCHEDULED_NOTIFICATION_TIME, "default_sound", true);
    }

    private void setWeekleyNotification(int daysToSunday)
    {
        CoreSystemManager.ScheduleLocalNotification("Daily Quiz: " + QuizConfig.Instance.GameFullName, I2.Loc.LocalizationManager.GetTermTranslation("NewWeek"), 1, daysToSunday * SCHEDULED_NOTIFICATION_TIME, "default_sound", true);
    }
    */
}
