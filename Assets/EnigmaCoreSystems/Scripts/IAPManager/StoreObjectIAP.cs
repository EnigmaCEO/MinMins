using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class StoreObjectIAP : StoreObject 
{
	public string iapID = "";
	public decimal iapCost = 0;
	
	public StoreObjectIAP()
	{
		this.iapID = "";
		this.iapCost = 0;
	}
	
	public override void CreateFromJSON (JSONNode json)
	{
		base.CreateFromJSON (json);
		#if SAMSUNG
		this.iapID = json["samsung_iap_id"].ToString ().Trim ('"');
		#else
		this.iapID = json["item_iap_id"].ToString ().Trim ('"');
		#endif
		
		this.iapCost = decimal.Parse(json["item_cost"].ToString ().Trim ('"'));
	}
}

