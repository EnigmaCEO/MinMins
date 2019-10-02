using UnityEngine;
using System.Collections;

public class ScaleToTransform : MonoBehaviour {

    public Transform target;

	public void Update() {
        float distance = Vector3.Distance(target.position, this.transform.position);
        Vector3 scale = this.transform.localScale;
        scale.z = distance;
        this.transform.localScale = scale;
	}
}
