using UnityEngine;
using System.Collections;

public class UnitPrep : MonoBehaviour {

	float x, y;
//	float orig_x, orig_y;
	public bool selected = false;
	GameObject team, slot, manager;
	Vector3 screenPoint, offset;
	
	// Use this for initialization
	void Start () {
		team = GameObject.Find("/Team1");
		slot = transform.parent.gameObject;
		manager = Camera.main.gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		if(selected) {
			x = Input.mousePosition.x;
			y = Input.mousePosition.y;
		} else {
			PosLimit();
		}	
	}
	
	void OnMouseDown() {
//		orig_x = transform.localPosition.x;
//		orig_y = transform.localPosition.y;

		screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
		
		offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
		
		selected = true;
		//Debug.Log(name + " selected");
		if(transform.parent.parent.name.Contains("slot")) {
			transform.parent = team.transform.FindChild(transform.parent.parent.name);
		}
	}
	
	void OnMouseDrag() {
		if(selected) {
			//transform.position = Camera.main.ScreenToWorldPoint(new Vector3(x,y,10.0f));
			
			transform.position = Camera.main.ScreenToWorldPoint(new Vector3(x, y, screenPoint.z)) + offset;
		}
	}
	
	void OnMouseUp() {
		selected = false;

		PosLimit();
		
		if(slot.transform.FindChild("confirm_ok(Clone)") == null) {
			GameObject confirm = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/UI/confirm_ok"));
			confirm.transform.parent = slot.transform;
			confirm.transform.localPosition = new Vector2(0,0);
			confirm.transform.localScale = new Vector2(2,2);

			GameObject shadow = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/UI/battle_shadow"));
			shadow.transform.parent = transform;
			shadow.transform.localPosition = new Vector2(0,0);
			shadow.transform.localScale = new Vector2(-1,1);

			manager.GetComponent<TeamManager>().slots++;
			if(manager.GetComponent<TeamManager>().slots >= 5) manager.GetComponent<TeamManager>().TeamReady();
		}
	}

	void PosLimit() {
		if(transform.localPosition.y < -1) transform.localPosition = new Vector2(transform.localPosition.x, -1);
		if(transform.localPosition.y > 4.5f) transform.localPosition = new Vector2(transform.localPosition.x, 4.5f);
		
		if(transform.localPosition.x > 7.4f) transform.localPosition = new Vector2(7.4f,transform.localPosition.y);
		if(transform.localPosition.x < -7.4f) transform.localPosition = new Vector2(-7.4f,transform.localPosition.y);
	}
	
	void OnCollisionEnter2D(Collision2D coll) {
		//Debug.Log(coll.gameObject.transform.parent.name + " collision");
		OnMouseUp();
	}

	void OnCollisionStay2D(Collision2D coll) {
		float x1,y1;

		if(transform.localPosition.x >= coll.transform.localPosition.x) 
			x1 = 0.1f;
		else
			x1 = -0.1f;

		if(transform.localPosition.y >= coll.transform.localPosition.y) 
			y1 = 0.1f;
		else
			y1 = -0.1f;

		transform.Translate(x1,y1,0);
	}

}
