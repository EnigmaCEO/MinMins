using UnityEngine;
using System.Collections;

public class MinMin : MonoBehaviour {
	public int strength, defense, health, effect, level;
	public int[] attack;

	// Use this for initialization
	void Awake ()
    {
		if(Application.loadedLevelName.Equals("OriginalUnitSelect"))
        {
			GameObject button = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/UI/Info"));
			button.transform.parent = transform;
			button.transform.localScale = new Vector2(0.01f,0.01f);
			button.transform.localPosition = new Vector2(-0.5f,-0.72f);
			button.name = "Info";
			button.AddComponent<VFXSorter>().sortingOrder = 100;
	
			button = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/UI/Select"));
			button.transform.parent = transform;
			button.transform.localScale = new Vector2(0.01f,0.01f);
			button.transform.localPosition = new Vector2(0.5f,-0.72f);
			button.name = "Select";
			button.AddComponent<VFXSorter>().sortingOrder = 100;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
