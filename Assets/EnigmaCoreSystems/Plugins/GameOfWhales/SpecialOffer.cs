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
using System.Collections.Generic;
using UnityEngine;

public class SpecialOffer{
    public string id;
    public string product;
    public float countFactor;
    public float priceFactor;
    public DateTime finishedAt;
    public DateTime activatedAt;
    public string payload;
    public bool redeemable;
    public Dictionary<string, object> customValues = new Dictionary<string, object>();

    public bool IsExpired(){
        try
        {
            return finishedAt.Ticks < GameOfWhales.GetServerTime().Ticks;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
        return false;
    }

    public bool HasCountFactor(){
        return countFactor > 1.0;
    }

    public bool HasPriceFactor(){
        return priceFactor < 1.0;
    }

    public bool IsActivated() {
        try
        {
            return activatedAt.Ticks < GameOfWhales.GetServerTime().Ticks;
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
        return false;
    }
}