using UnityEngine;
using System.Collections;

public class Xample_TestLayoutRotator : MonoBehaviour {

	public float rotation = 30;

	// Update is called once per frame
	void Update() {
		transform.Rotate(0, rotation * Time.deltaTime, 0);	
	}
}
