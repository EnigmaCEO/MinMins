using UnityEngine;
using System.Collections;

public class UnitSelect : MonoBehaviour {

	float x, y;
	float orig_x, orig_y;
	public bool selected = false;
	GameObject unitgrid, manager;
	
	// Use this for initialization
	void Start () {
		unitgrid = GameObject.Find("/UI Root/UnitGrid");	
		manager = GameObject.Find("/Main Camera");
	}
	
	// Update is called once per frame
	void Update () {
		if(selected) {
			x = Input.mousePosition.x;
			y = Input.mousePosition.y;
		}
	}

	void OnMouseDown() {
		orig_x = transform.localPosition.x;
		orig_y = transform.localPosition.y;
		selected = true;
		Debug.Log(name + " selected");
	}

	void OnMouseDrag() {
		transform.position = Camera.main.ScreenToWorldPoint(new Vector3(x,y,10.0f));
	}

	void OnMouseUp() {
		selected = false;
		transform.localPosition = new Vector2(orig_x,orig_y);
		ScreenMouseRay();
	}

	public void ScreenMouseRay()	
	{
		
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.z = 5f;
		
		Vector2 v = Camera.main.ScreenToWorldPoint(mousePosition);
		Collider2D[] col = Physics2D.OverlapPointAll(v);
		
		if(col.Length > 0){
			
			foreach(Collider2D c in col)
			{
				//Debug.Log(c.collider2D.gameObject.name + " Collided");
				if(c.GetComponent<Collider2D>().gameObject.transform.childCount == 0 && c.GetComponent<Collider2D>().gameObject.name.Contains("slot")) {
					transform.parent = c.GetComponent<Collider2D>().gameObject.transform;
					transform.localPosition = new Vector2(0,0.1f);
					unitgrid.GetComponent<UIGrid>().Reposition();

					manager.GetComponent<TeamManager>().slots++;
					if(manager.GetComponent<TeamManager>().slots >= 5) manager.GetComponent<TeamManager>().TeamReady();
				}
			}
		}
	}
}
