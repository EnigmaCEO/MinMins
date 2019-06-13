using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

	GameObject manager;
	public GameObject unit;

	// Use this for initialization
	void Awake () {
		manager = GameObject.Find("/Main Camera");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	

	void OnMouseUp() {
		//Debug.Log(name + " selected");
		manager.GetComponent<WarManager>().Attack(transform.parent.name + "/" + name);
	}
}
