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

public class SpecialOffer {
    public string id;
    public string product;
    public float countFactor;
    public float priceFactor;
    public DateTime finishedAt;
    public string payload;
	public bool redeemable;
	public Dictionary<string,object> customValues = new Dictionary<string,object>();

    public bool IsExpired()
    {
		return finishedAt.Ticks < GameOfWhales.Instance.GetServerTime().Ticks;
    }

    public bool HasCountFactor()
    {
        return countFactor > 1.0;
    }

    public bool HasPriceFactor()
    {
        return priceFactor < 1.0;
    }

}
