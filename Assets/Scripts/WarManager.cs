using UnityEngine;
using System.Collections;

public class WarManager : MonoBehaviour {

	public bool ready = true;
	int side = 0;
	GameObject field, grid, enemies, manager, slot;	
	string aUnit = "";
	float timer, playTime;
	
	// Use this for initialization
	void Awake () {	
		if(!GameObject.Find("/GameManager")) {
			manager = new GameObject("GameManager");
			manager.AddComponent<GameManager>();
		} else {
			manager = GameObject.Find("/GameManager");
		}

		field = GameObject.Find("Battlefield");
		grid = GameObject.Find("TeamGrid");
		enemies = GameObject.Find("EnemyGrid");
		
		for(int i = 1; i < 6; i++) {
			slot = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/Units/" + manager.GetComponent<GameManager>().team1[i-1].name));	
			slot.transform.parent = grid.transform.FindChild("slot" + i);
			slot.name = manager.GetComponent<GameManager>().team1[i-1].name;
			slot.transform.localScale = new Vector2(240,240);
			slot.AddComponent<MinMin>();

			GameObject obj = (GameObject)Instantiate(slot.transform.FindChild("Sprite").gameObject);
			obj.name = manager.GetComponent<GameManager>().team1[i-1].name;
			obj.transform.parent = field.transform.FindChild("Team1/slot" + i);
			obj.transform.localPosition = new Vector2(manager.GetComponent<GameManager>().team1[i-1].position.x, manager.GetComponent<GameManager>().team1[i-1].position.y);
			obj.AddComponent<Unit>();			
			obj.GetComponent<Unit>().unit = slot;

			GameObject shadow = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/UI/battle_shadow"));
			shadow.transform.parent = obj.transform;
			shadow.transform.localPosition = new Vector2(0,0);
			shadow.transform.localScale = new Vector2(-1,1);
		}

		for(int i = 1; i < 6; i++) {
			slot = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/Units/" + manager.GetComponent<GameManager>().team2[i-1].name));	
			slot.transform.parent = enemies.transform.FindChild("slot" + i);
			slot.name = manager.GetComponent<GameManager>().team2[i-1].name;
			slot.transform.localScale = new Vector2(240,240);
			slot.transform.localPosition = new Vector2(0,0);
			
			GameObject obj = (GameObject)Instantiate(slot.transform.FindChild("Sprite").gameObject);
			obj.name = manager.GetComponent<GameManager>().team2[i-1].name;
			obj.transform.parent = field.transform.FindChild("Team2/slot" + i);
			obj.transform.localPosition = new Vector2(manager.GetComponent<GameManager>().team2[i-1].position.x, manager.GetComponent<GameManager>().team1[i-1].position.y);
			obj.AddComponent<Unit>();
			obj.transform.localScale = new Vector2(1,1);
			obj.GetComponent<Unit>().unit = slot;
			obj.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1f/255f);
			
		}
		
		SetAttacks(grid);
		SetAttacks(enemies);
	}


	
	// Update is called once per frame
	void Update () {
		playTime += Time.deltaTime;
		
		if(side == 0) timer = playTime;

		if(playTime - timer >= 2 && ready) {
			for(int i = 0; i < 6; i++) {
				Transform eSlot = enemies.transform.GetChild(i);
				if(eSlot.transform.childCount == 0) continue;
				
				Attack(eSlot.name + "/" + eSlot.GetChild(0).name);
				break;
			}
		}
	}

	public void Attack(string unit) {
		if(!ready) return;
		Transform target = null;

		if(side == 0) {
			target = grid.transform.FindChild(unit);
		} else {
			target = enemies.transform.FindChild(unit);
		}

		if(target == null) return;
		if(target.FindChild("Effect").transform.childCount == 0) return;
		
		ready = false;
		field.GetComponent<TweenPosition>().ResetToBeginning();
		if(side == 0) {
			field.GetComponent<TweenPosition>().from = new Vector2(0,0);
			field.GetComponent<TweenPosition>().to = new Vector2(-5000,0);
		} else {
			field.GetComponent<TweenPosition>().to = new Vector2(0,0);
			field.GetComponent<TweenPosition>().from = new Vector2(-5000,0);
		}

		field.GetComponent<TweenPosition>().enabled = true;
		aUnit = unit;
	}

	public void AttackReady(UITweener tween) {
		Debug.Log ("Attack ready!");
		string effect_name = "";
		GameObject attack = GameObject.Find("Waypoint Manager/" + aUnit.Split('/')[1] + "/Attack").transform.GetChild(0).gameObject;
		
		if(side == 0) {
			effect_name = grid.transform.FindChild(aUnit + "/Effect").transform.GetChild(0).name;
			
			attack.transform.localEulerAngles = new Vector3(0,0,0);
		} else {
			effect_name = enemies.transform.FindChild(aUnit + "/Effect").transform.GetChild(0).name;
			
			attack.transform.localEulerAngles = new Vector3(0,180,0);
		}
	
		attack.transform.localPosition = new Vector2(0,0);
		attack.GetComponent<SWS.BezierPathManager>().CalculatePath();

		GameObject effect = (GameObject)Instantiate(Resources.Load <GameObject> ("Prefabs/Attacks/" + effect_name));
		effect.GetComponent<SWS.splineMove>().pathContainer = attack.GetComponent<SWS.BezierPathManager>();
		effect.GetComponent<SWS.splineMove>().enabled = true;
		effect.GetComponent<SWS.splineMove>().StartMove();
		effect.AddComponent<VFXSorter>().sortingOrder = 100;

		/* SWS.MessageOptions opt = effect.GetComponent<SWS.splineMove>().messages.GetMessageOption(2); - needs to be replaced
		opt.message [0] = "EndAttack";
		opt.pos = 1;*/
	}

	public void Switch() {
		ready = true;
		if(side == 0) 
			side = 1;
		else
			side = 0;
			
	}

	void SetAttacks(GameObject val) {
		for(int i = 0; i < val.transform.childCount; i++) {
			
			GameObject temp = val.transform.GetChild(i).gameObject;
			
			if(temp.transform.childCount > 0) {
				Transform attack = temp.transform.GetChild(0).FindChild("Attack");
				if(attack != null) {
					GameObject item = new GameObject();
					item.name = temp.transform.GetChild(0).name;
					item.transform.parent = GameObject.Find("Waypoint Manager").transform;
					attack.parent = item.transform;
					attack.transform.localPosition = new Vector2(0,0);
				}
			}
		}

	}

	
}
