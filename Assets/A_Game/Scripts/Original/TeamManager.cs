using UnityEngine;
using System.Collections;

public class TeamManager : MonoBehaviour {

	public int slots = 0;
	GameObject btn_next, manager;
	GameObject attack, gInfo, gTeam;
	string selection;
	Vector2[] waypoints;

	// Use this for initialization
	void Awake () {
		if(!GameObject.Find("/GameManager")) {
			manager = new GameObject("GameManager");
			manager.AddComponent<GameManager>();
		} else {
			manager = GameObject.Find("/GameManager");
		}
		
		btn_next = GameObject.Find("btn_next");
		btn_next.SetActive(false);

		if(Application.loadedLevelName.Equals("OriginalUnitSelect")) {
			gTeam = GameObject.Find("TeamGrid").gameObject;
			gInfo = GameObject.Find("Popup").gameObject;
		}
	}

	void Start () {
		if(Application.loadedLevelName.Equals("OriginalWarPrep")) {
			GameObject slot = null;

			for(int i = 1; i < 6; i++) {
				slot = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/Units/" + manager.GetComponent<GameManager>().team1[i-1].name));	
				slot.transform.parent = GameObject.Find("TeamGrid/slot"+ i).transform;
				slot.transform.localPosition = new Vector2(0,0);
				slot.transform.localScale = new Vector2(1,1);
				slot.transform.Find("Health").gameObject.SetActive(false);
				slot.transform.Find("Sprite").gameObject.AddComponent<UnitPrep>();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnGUI() {
		if(attack && gInfo.transform.localPosition.y == 0) {
			//Vector2 way = attack.GetComponent<SWS.BezierPathManager>().pathPoints[0]*12;
			Color c = new Color(30f/255f,152f/255f,0f);
			
			if(waypoints[2] == Vector2.zero) {
				DreamDrawing.DrawLine(	waypoints[0], waypoints[1], 100f, 6f, Color.black, waypoints[0], 0f);
				DreamDrawing.DrawLine(	waypoints[0], waypoints[1], 100f, 4f, c, waypoints[0], 0f);
			} else {
				DreamDrawing.DrawCurve(	waypoints[0], waypoints[1], waypoints[2], waypoints[3], 100f, 6f, Color.black, waypoints[0], 0f);
				DreamDrawing.DrawCurve(	waypoints[0], waypoints[1], waypoints[2], waypoints[3], 100f, 4f, c, waypoints[0], 0f);
			}
		}
		
	}

	public void TeamReady() {
		btn_next.SetActive(true);
	}

	public void Select(string obj) {
		ShowPanel(1);
		selection = obj;
		TeamSlot[] mins = GameObject.Find("TeamGrid").GetComponentsInChildren<TeamSlot>();
		
		foreach(TeamSlot val in mins) {
			val.ShowSelect();
		} 
	}

	public void FillSlot(Transform obj) {
		TeamSlot[] mins = GameObject.Find("TeamGrid").GetComponentsInChildren<TeamSlot>();
		
		foreach(TeamSlot val in mins) {
			val.HideSelect();
		} 

		GameObject slot = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/Units/" + selection));	
		slot.transform.parent = obj;
		slot.transform.localPosition = new Vector2(0,0);
		slot.transform.localScale = new Vector2(1,1);
		slot.transform.Find("Health").gameObject.SetActive(false);

		int i = int.Parse(obj.name.Replace("slot","")) - 1;
		manager.GetComponent<GameManager>().team1[i].name = selection;

		GameObject grid = GameObject.Find("UIWrap Content");
		GameObject orig = grid.transform.Find(selection).gameObject;
		DestroyImmediate(orig);
		grid.GetComponent<UIWrapContent>().SortBasedOnScrollMovement();
		grid.GetComponent<UIWrapContent>().WrapContent();

		selection = "";

		slots++;
		if(slots >= 5) TeamReady();
	}

	public void UnitInfo(Transform obj) {
		ShowPanel(2);

		GameObject temp = GameObject.Find("/UnitInfo");
		if(temp) {
			DestroyImmediate(temp);
			attack = null;
		}

		GameObject slotinfo = GameObject.Find("slotinfo");
		if(slotinfo.transform.childCount > 0) {
			DestroyImmediate(slotinfo.transform.GetChild(0).gameObject);
		}
		
		GameObject slot = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/Units/" + obj.name));	
		slot.transform.parent = slotinfo.transform;
		slot.transform.localPosition = new Vector2(0,0);
		slot.transform.localScale = new Vector2(1,1);
		slot.transform.Find("Health").gameObject.SetActive(false);

		// Stars
		GameObject strength = GameObject.Find("Power");
		NGUITools.SetActiveChildren(strength, true);

		for(int i = 0; i < 5; i++) {
			if(i >= obj.GetComponent<MinMin>().strength) {
				strength.transform.GetChild(i).gameObject.SetActive(false);
			}
		}

		GameObject defense = GameObject.Find("Armor");
		NGUITools.SetActiveChildren(defense, true);
		
		for(int i = 0; i < 5; i++) {
			if(i >= obj.GetComponent<MinMin>().defense) {
				defense.transform.GetChild(i).gameObject.SetActive(false);
			}
		}	


		// Attack Patterns
		GameObject container = new GameObject("UnitInfo");
		waypoints = new Vector2[4];
		int pattern = obj.GetComponent<MinMin>().attack[0];

		attack = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/Patterns/Attack"+pattern));
		attack.transform.parent = container.transform;
		attack.transform.localPosition = new Vector2(53.5f,33f);
		attack.GetComponent<SWS.BezierPathManager>().CalculatePath();

		Vector2 add = new Vector2(120,0);
		Vector2 add2 = new Vector2(60,0);
		
		for(int i = 0; i < 4; i++) {
			Transform t = null;
			
			if(attack.transform.childCount == 2) {
				t = attack.transform.Find("Waypoint "+i);
			} else {
				if(i == 0) t = attack.transform.Find("Waypoint "+i);
				if(i == 1) t = attack.transform.Find("Waypoint "+i+"/Left");
				if(i == 2) t = attack.transform.Find("Waypoint "+(i-1)+"/Right");
				if(i == 3) t = attack.transform.Find("Waypoint "+(i-1));
			}


			if(t) {
				waypoints[i] = t.position *12;
				if(waypoints[i].x == 642 || waypoints[i].x == 738 || waypoints[i].x == 582 || waypoints[i].x == 702) waypoints[i] -= add2;
				if(waypoints[i].x == 762) waypoints[i] -= add;
				
				waypoints[i].y += (414-waypoints[i].y)*2;
				Debug.Log(i +": "+waypoints[i].y);
			} else {
				waypoints[i] = Vector2.zero;
				//Debug.Log(i +": "+waypoints[i].x);
			}
		}
	}

	public void ShowPanel(int val) {
		bool pTeam = false;
		bool pInfo = false;
		
		if(val == 1) {
			pTeam = true;
			pInfo = false;
		}
		if(val == 2) {
			pTeam = false;
			pInfo = true;
		}

		if(pTeam) {
			gTeam.GetComponent<TweenPosition>().from = new Vector2(0,-1373.75f);
			gTeam.GetComponent<TweenPosition>().to = new Vector2(0,-873.75f);
			gTeam.GetComponent<TweenPosition>().delay = 0.5f;
			if(gTeam.transform.localPosition.y != -873.75f) gTeam.GetComponent<TweenPosition>().ResetToBeginning();
			gTeam.GetComponent<TweenPosition>().enabled = true;
		} else {
			gTeam.GetComponent<TweenPosition>().from = new Vector2(0,-873.75f);
			gTeam.GetComponent<TweenPosition>().to = new Vector2(0,-1373.75f);
			gTeam.GetComponent<TweenPosition>().delay = 0f;
			if(gTeam.transform.localPosition.y != -1373.75f) gTeam.GetComponent<TweenPosition>().ResetToBeginning();
			gTeam.GetComponent<TweenPosition>().enabled = true;
		}

		if(pInfo) {
			gInfo.GetComponent<TweenPosition>().from = new Vector2(0,-500);
			gInfo.GetComponent<TweenPosition>().to = new Vector2(0,0);
			gInfo.GetComponent<TweenPosition>().delay = 0.5f;
			if(gInfo.transform.localPosition.y != 0) gInfo.GetComponent<TweenPosition>().ResetToBeginning();
			gInfo.GetComponent<TweenPosition>().enabled = true;
		} else {
			gInfo.GetComponent<TweenPosition>().from = new Vector2(0,0);
			gInfo.GetComponent<TweenPosition>().to = new Vector2(0,-500);
			gInfo.GetComponent<TweenPosition>().delay = 0f;
			if(gInfo.transform.localPosition.y != -500) gInfo.GetComponent<TweenPosition>().ResetToBeginning();
			gInfo.GetComponent<TweenPosition>().enabled = true;
		}
	}

	public void SetTeam() {
		for(int i = 1; i < 6; i++) {
			
			GameObject obj = GameObject.Find("Team1/slot"+ i + "/Sprite");
			manager.GetComponent<GameManager>().team1[i-1].position = obj.transform.localPosition;
		}
	}
}
