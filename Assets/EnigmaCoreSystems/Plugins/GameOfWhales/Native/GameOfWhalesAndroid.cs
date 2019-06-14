/*
 * Game Of Whales SDK
 *
 * https://www.gameofwhales.com/
 * 
 * Copyright © 2018 GameOfWhales. All rights reserved.
 *
 * Licence: https://github.com/Game-of-whales/GOW-SDK-UNITY/blob/master/LICENSE
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JsonUtils = GameOfWhalesJson.MiniJSON;

public class GameOfWhalesAndroid : GameOfWhales {
#if UNITY_ANDROID && !UNITY_EDITOR
	private static AndroidJavaObject gameofwhales = null;

	protected override void Initialize()
	{
        if (gameofwhales != null)
            return;
        
        Debug.Log("GameOfWhalesAndroid: Initialize");
		Debug.Log("GameOfWhalesAndroid: Store: " + GameOfWhales.store);

        
        GameOfWhalesSettings settings = GameOfWhalesSettings.instance;
        

        using(var pluginClass = new AndroidJavaClass("com.gameofwhales.unityplugin.GameOfWhalesProxy"))
			{
				gameofwhales = pluginClass.CallStatic<AndroidJavaObject>("instance");
                pluginClass.CallStatic("initialize", settings.gameID, GameOfWhales.store, settings.androidProjectID, this.gameObject.name, VERSION, settings.debugLogging);
			}
		
	}

    bool checkInstance(string from)
    {
        if (gameofwhales == null)
        {
            Debug.LogError("GameOfWhalesAndroid instance is null ( " + from + ")");
            return false;
        }

        return true;
    }

    public override void PushReacted(string camp)
    {   
        if (checkInstance("PushReacted"))
        {
            gameofwhales.Call("pushReacted", camp);
        }
    }



    public override void InAppPurchased(string sku, double price, string currency, string transactionID, string receipt) 
    {
        if (checkInstance("InAppPurchased"))
        {
            gameofwhales.Call("inAppPurchased", sku, price.ToString("#.00"), currency, transactionID, receipt);
        }
    }

    public override void UpdateToken(string token, string provider)
    {
        if (checkInstance("UpdateToken"))
        {
            gameofwhales.Call("updateToken", token, provider);
        }
    }

    public override void Converting(IDictionary<string, long> resources, string place)
    {
        if (checkInstance("Converting"))
        {
            gameofwhales.Call("converting", JsonUtils.Serialize(resources), place);
        }
    }

    public override void Profile(IDictionary<string, object> parameters)
    {
        if (checkInstance("Profile"))
        {
            gameofwhales.Call("profile", JsonUtils.Serialize(parameters));
        }
    }

    public override void Consume(string currency, long number, string sink, long amount, string place)
    {
        Debug.Log("Consume");
        if (checkInstance("Consume"))
        {
            gameofwhales.Call("consume", currency, number, sink, amount, place);
        }
    }

    public override void Acquire(string currency, long amount, string source, long number, string place)
    {
        if (checkInstance("Acquire"))
        {
            gameofwhales.Call("acquire", currency, amount, source, number, place);
        }
    }

    public override void ReportError(string message, string stacktrace)
    {
        if (checkInstance("ReportError"))
        {
            gameofwhales.Call("reportError", message, stacktrace);
        }
    }

    public override void SetPushNotificationsEnable(bool value)
    {
        if (checkInstance("SetPushNotificationsEnable"))
        {
            gameofwhales.Call("setPushNotificationsEnable", value ? "true" : "false");
        }
    }

    public override void RegisterForNotifications()
    {

    }

	public override System.DateTime GetServerTime()
	{
		if (checkInstance("GetServerTime"))
		{
			string st_str = gameofwhales.Call<string>("getServerTime");
			System.DateTime dt = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			long tt = Convert.ToInt64(st_str);
			return dt.AddMilliseconds( tt);
		}

		return System.DateTime.UtcNow;
	}
#endif
}
