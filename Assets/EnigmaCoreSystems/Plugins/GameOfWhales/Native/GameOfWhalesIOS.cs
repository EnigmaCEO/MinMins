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

public class GameOfWhalesIOS : GameOfWhales{
#if UNITY_IOS && !UNITY_EDITOR
    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_initialize(string gameKey, string listener, string version, bool debug, bool nonPersonal);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_pushReacted(string camp);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_inAppPurchased(string sku, double price, string currency, string transactionID, string receipt);

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_purchase(string product, string currency, double price);
    
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

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private string gw_getProperties();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private bool gw_isAdLoaded();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_loadAd();

    [System.Runtime.InteropServices.DllImport("__Internal")]
    extern static private void gw_showAd();


    protected override void _Initialize()
    {
        try
        {
            Debug.Log("GameOfWhalesIOS: Initialize");   
            GameOfWhalesSettings settings = GameOfWhalesSettings.instance;
            gw_initialize(settings.gameID, this.gameObject.name, VERSION, settings.debugLogging, GameOfWhales.nonPersonal);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    protected override void _PushReacted(string camp)
    { 
        try
        {
            Debug.Log("GameOfWhalesIOS: PushReacted " + camp);
            gw_pushReacted(camp);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }
    
    protected override void _Purchase(string product, string currency, double price) 
    {
        try
        {
            Debug.Log("GameOfWhalesIOS: _Purchase ");
            gw_purchase(product, currency, price);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }
    
    protected override void _Purchase(string product, string currency, double price, string transactionID, string receipt, bool verify) 
    {
        try
        {
            Debug.LogError("Not implemented: Use InAppPurchased method");
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    protected override void _InAppPurchased(string sku, double price, string currency, string transactionID, string receipt) 
    {
        try
        {
            Debug.Log("GameOfWhalesIOS: InAppPurchased ");
            gw_inAppPurchased(sku, price, currency, transactionID, receipt);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    protected override void _UpdateToken(string token, string provider)
    {
        try
        {
            Debug.Log("GameOfWhalesIOS: UpdateToken " + token + "  " + provider);
            gw_updateToken(token, provider);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    protected override void _Converting(IDictionary<string, long> resources, string place)
    {
        try
        {
            string paramsStr = JsonUtils.Serialize(resources);
            Debug.Log("GameOfWhalesIOS: Converting " + paramsStr);
            gw_converting(paramsStr, place);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    protected override void _Profile(IDictionary<string, object> parameters)
    {
        try
        {
            string paramsStr = JsonUtils.Serialize(parameters);
            Debug.Log("GameOfWhalesIOS: Profile " + paramsStr);
            gw_profile(paramsStr);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    protected override void _Consume(string currency, long number, string sink, long amount, string place)
    {
        try
        {
    		gw_consume(currency, number.ToString(), sink, amount.ToString(), place);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    protected override void _Acquire(string currency, long amount, string source, long number, string place)
    {
        try
        {
    		gw_acquire(currency, amount.ToString(), source, number.ToString(), place);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    protected override void _ShowAd()
    {
        try
        {
            gw_showAd();
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    protected override bool _IsAdLoaded()
    {
        try
        {
            return gw_isAdLoaded();
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }

        return false;
    }

    protected override void _LoadAd()
    {
        try
        {
            gw_loadAd();
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    protected override void _SetPushNotificationsEnable(bool value)
    {
        try
        {
            Debug.Log("GameOfWhalesIOS: SetPushNotificationsEnable ");
            gw_setPushNotificationsEnable(value);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

	protected override System.DateTime _GetServerTime()
	{
        try
        {
    		string st_str = gw_getServerTime();
    
            if (st_str != null)
            {
        		System.DateTime dt = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        		long tt = Convert.ToInt64(st_str);
                dt = dt.AddMilliseconds(tt);
        		return dt;
            }

            return System.DateTime.Now;
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }

        return System.DateTime.Now;
	}

    protected override Dictionary<string, object> _GetProperties()
    {
        try
        {
            string st_properties = gw_getProperties();

            if (st_properties != null)
            {
                var data = JsonUtils.Deserialize(st_properties) as Dictionary<string, object>;
                return data;
            }
            else
            {
                return _emptyProperties;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
        
        return _emptyProperties;
    }

    protected override void _RegisterForNotifications()
    {
        try
        {
            Debug.Log("GameOfWhalesIOS RegisterForNotifications");
            NotificationServices.RegisterForNotifications(
                NotificationType.Alert |
                NotificationType.Badge |
                NotificationType.Sound);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }
#endif
}