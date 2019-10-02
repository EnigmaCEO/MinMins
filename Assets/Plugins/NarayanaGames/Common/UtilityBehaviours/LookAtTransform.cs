using UnityEngine;
using System.Collections;

public class LookAtTransform : MonoBehaviour {

    public Transform target;

	public void Update() {
        this.transform.LookAt(target, Vector3.up);
	}
}
