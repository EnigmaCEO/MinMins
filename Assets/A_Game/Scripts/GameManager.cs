using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public Players[] team1, team2;
	
	void Awake() {
		DontDestroyOnLoad(transform.gameObject);

		team1 = new Players[6];
		team2 = new Players[6];
		
		for(int i = 0; i < 6; i++) {
			team1[i] = new Players();
			team2[i] = new Players();
		}
	}

	// Use this for initialization
	void Start () {
		

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

public class Players {
	public string name;
	public Vector2 position;

	public Players() {
		name = "6";
		position = new Vector2();
	}
}
