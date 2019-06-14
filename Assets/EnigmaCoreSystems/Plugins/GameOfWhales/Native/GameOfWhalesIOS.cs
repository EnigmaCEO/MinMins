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


#if UNITY_IOS
    using NotificationServices = UnityEngine.iOS.NotificationServices;
    using NotificationType = UnityEngine.iOS.NotificationType;
#endif

public class GameOfWhalesIOS : GameOfWhales {
#if UNITY_IOS && !UNITY_EDITOR

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_initialize(string gameKey, string listener, string version, bool debug);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_pushReacted(string camp);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_inAppPurchased(string sku, double price, string currency, string transactionID, string receipt);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_updateToken(string token, string provider);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_profile(string changes);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_converting(string resources, string place);

    [System.Runtime.InteropServices.DllImport("__Internal")]
	extern static private void gw_consume(string currency, string number, string sink, string amount, string place);

    [System.Runtime.InteropServices.DllImport("__Internal")]
	extern static private void gw_acquire(string currency, string amount, string source, string number, string place);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_setPushNotificationsEnable(bool value);

	[System.Runtime.InteropServices.DllImport("__Internal")]
	extern static private string gw_getServerTime();


    protected override void Initialize()
    {
        Debug.Log("GameOfWhalesIOS: Initialize");

        GameOfWhalesSettings settings = GameOfWhalesSettings.instance;

        gw_initialize(settings.gameID, this.gameObject.name, VERSION, settings.debugLogging);
    }

    public override void PushReacted(string camp)
    { 
        Debug.Log("GameOfWhalesIOS: PushReacted " + camp);
        gw_pushReacted(camp);
    }

    public override void InAppPurchased(string sku, double price, string currency, string transactionID, string receipt) 
    {
        Debug.Log("GameOfWhalesIOS: InAppPurchased ");
        gw_inAppPurchased(sku, price, currency, transactionID, receipt);
    }

    public override void UpdateToken(string token, string provider)
    {
        Debug.Log("GameOfWhalesIOS: UpdateToken " + token + "  " + provider);
        gw_updateToken(token, provider);
    }

    public override void Converting(IDictionary<string, long> resources, string place)
    {
        string paramsStr = JsonUtils.Serialize(resources);
        Debug.Log("GameOfWhalesIOS: Converting " + paramsStr);
        gw_converting(paramsStr, place);
    }

    public override void Profile(IDictionary<string, object> parameters)
    {
        string paramsStr = JsonUtils.Serialize(parameters);
        Debug.Log("GameOfWhalesIOS: Profile " + paramsStr);
        gw_profile(paramsStr);
    }

    public override void Consume(string currency, long number, string sink, long amount, string place)
    {
		gw_consume(currency, number.ToString(), sink, amount.ToString(), place);
    }

    public override void Acquire(string currency, long amount, string source, long number, string place)
    {
		gw_acquire(currency, amount.ToString(), source, number.ToString(), place);
    }

    public override void SetPushNotificationsEnable(bool value)
    {
        Debug.Log("GameOfWhalesIOS: SetPushNotificationsEnable ");
        gw_setPushNotificationsEnable(value);
    }

	public override System.DateTime GetServerTime()
	{
		string st_str = gw_getServerTime();
		System.DateTime dt = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
		long tt = Convert.ToInt64(st_str);
		return dt.AddMilliseconds( tt);
	}

    public override void RegisterForNotifications()
    {
        Debug.Log("GameOfWhalesIOS RegisterForNotifications");
        NotificationServices.RegisterForNotifications(
            NotificationType.Alert |
            NotificationType.Badge |
            NotificationType.Sound);
    }
#endif
}
