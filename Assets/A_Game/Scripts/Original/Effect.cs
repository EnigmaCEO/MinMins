using UnityEngine;
using System.Collections;

public class Effect : MonoBehaviour {

	GameObject manager;
	public int power;
	
	// Use this for initialization
	void Start () {
		manager = GameObject.Find("/Main Camera");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D coll) {
		Debug.Log(coll.gameObject.name + " hit " + name);
		if(!manager.GetComponent<WarManager>().ready) {
			
			GameObject unit = coll.gameObject.GetComponent<Unit>().unit;
			UIProgressBar val = unit.GetComponentInChildren<UIProgressBar>();
			if(val) val.value -= 0.25f;
			int type = 1;
			if(val.value <= 0) {
				type = 2;
				Destroy(coll.gameObject);
			}

			GameObject hit = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/Hits/" + type));
			hit.transform.localPosition = transform.localPosition;
			hit.AddComponent<VFXSorter>().sortingOrder = 100;
			Destroy(gameObject);
			
			EndAttack();
		}
	}

	void EndAttack() {
		Debug.Log("Attack done");
		manager.GetComponent<WarManager>().Switch();
		Destroy(transform.gameObject);
		
	}
}
