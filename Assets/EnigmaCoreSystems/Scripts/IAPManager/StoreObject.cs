using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class StoreObject 
{
	// container class for objects in the store
	public int item_id, item_curr_type_id, item_category_id;
	public float item_cost;
	public string item_name, item_desc, item_image_name;
	public DateTime item_start_date, item_end_date;
	
	public StoreObject()
	{
		this.item_id = 0;
		this.item_curr_type_id = 0;
		this.item_category_id = 0;
		this.item_cost = 0;
		this.item_desc = "";
		this.item_name = "";
		this.item_image_name = "";
		this.item_start_date = DateTime.Now;
		this.item_end_date = DateTime.Now;
	}
	
	public virtual void CreateFromJSON( JSONNode json )
	{
		int item_id = json["item_id"].AsInt;
		int item_curr_id = json["item_curr_type_id"].AsInt;
		int item_cat_id = json["item_category_id"].AsInt;
		float item_cost = json["item_cost"].AsFloat;
		string item_desc = json["item_desc"].ToString().Trim ('"');
		string item_name = json["item_name"].ToString ().Trim ('"');
		string item_img_name = json["item_image_name"].ToString ().Trim ('"');
		string item_start_date = json["item_start_date"].ToString ().Trim ('"');
		string item_end_date = json["item_end_date"].ToString ().Trim ('"');
		
		this.item_id = item_id;
		this.item_curr_type_id = item_curr_id;
		this.item_category_id = item_cat_id;
		this.item_name = item_name;
		this.item_cost = item_cost;
		this.item_desc = item_desc;
		this.item_image_name = item_img_name;
		
		//Debug.Log ( item_start_date );
		
		if( item_start_date != null && item_start_date != "null" )
			this.item_start_date = DateTime.Parse (item_start_date);
		
		if( item_end_date != null && item_end_date != "null" )
			this.item_end_date = DateTime.Parse( item_end_date );
	}
}
