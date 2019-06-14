using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreSystemManager : MonoBehaviour
{
    public static void ClearAllLocalNotifications()
    {
        NativeToolkit.ClearAllLocalNotifications();
    }

    public static void ScheduleLocalNotification(string title, string message, int id = 0, int delayInMinutes = 0, string sound = "default_sound",
                                         bool vibrate = false, string smallIcon = "ic_notification", string largeIcon = "ic_notification_large")
    {
        NativeToolkit.ScheduleLocalNotification(title, message, id, delayInMinutes, sound, vibrate, smallIcon, largeIcon);
    }

    public static void RateApp(string title = "Rate This App", string message = "Please take a moment to rate this App",
                           string positiveBtnText = "Rate Now", string neutralBtnText = "Later", string negativeBtnText = "No, Thanks",
                           string appleId = "", Action<string> externalCallback = null)
    {
        int sendRate = PlayerPrefs.GetInt(Application.productName + "_sendRate", 1);
        if(sendRate == 1)
            NativeToolkit.RateApp(title, message, positiveBtnText, neutralBtnText, negativeBtnText, appleId, (results) => { rateAppCallback(externalCallback, results); } );
    }

    private static void rateAppCallback(Action<string> externalCallback, string results)
    {
        if (externalCallback != null)
            externalCallback(results);

        if (results != "1") // 0 -> No thanks. 1 -> Later. 2 -> Rate now.
            PlayerPrefs.SetInt(Application.productName + "_sendRate", 0);
    }
}
