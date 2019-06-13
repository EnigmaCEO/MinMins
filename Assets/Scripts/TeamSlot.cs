using UnityEngine;
using System.Collections;

public class TeamSlot : MonoBehaviour {
	GameObject unitgrid;

	// Use this for initialization
	void Start () {
		//unitgrid = GameObject.Find("/UI Root/UnitGrid");

		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/*void OnMouseOver() {
		if(Input.GetMouseButtonUp(0)) {
			Debug.Log(name);
			UnitSelect[] units = unitgrid.GetComponentsInChildren<UnitSelect>();
	
			foreach(UnitSelect val in units) {
				if(val.selected) {
					val.transform.parent = gameObject.transform;
				}
			} 
		}
	}*/

	public void ShowSelect() {
		if(transform.childCount == 0) {
			GameObject icon = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/UI/SlotSelect"));	
			icon.transform.parent = transform;
			icon.transform.localPosition = new Vector2(0,0);
		}
	}

	public void HideSelect() {
		Navigation nav = transform.GetComponentInChildren<Navigation>();
		
		if(nav) Destroy(nav.gameObject);
	}
}
