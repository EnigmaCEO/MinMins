using UnityEngine;
using System.Collections;

public class Navigation : MonoBehaviour {

	GameObject manager;
	// Use this for initialization
	void Start () {
		//Debug.Log(name + " Started");
		manager = Camera.main.gameObject;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown() {
		if(name == "btn_next") {
			manager.GetComponent<TeamManager>().SetTeam();
			if(Application.loadedLevelName.Equals("OriginalWarPrep")) Application.LoadLevel("OriginalWar");
		}
	}

	void OnPress(bool isDown) {
		if(!isDown) return;

		if(name == "Fight") {
			Application.LoadLevel("OriginalUnitSelect");
			
		}

		if(name == "btn_next") {
			if(Application.loadedLevelName.Equals("OriginalUnitSelect"))
                Application.LoadLevel("OriginalWarPrep");
        }

		if(name == "Info") {
			Debug.Log(transform.parent.name + " Info");
			manager.GetComponent<TeamManager>().UnitInfo(transform.parent);
		}

		if(name == "Select") {
			Debug.Log(transform.parent.name + " Selection");
			manager.GetComponent<TeamManager>().Select(transform.parent.name);
		}

		if(name.Contains("SlotSelect")) {
			Debug.Log(transform.parent.name + " Slot");
			manager.GetComponent<TeamManager>().FillSlot(transform.parent);
		}

		if(name == "btn_back") {
			manager.GetComponent<TeamManager>().ShowPanel(1);
		}
	}


	
}
