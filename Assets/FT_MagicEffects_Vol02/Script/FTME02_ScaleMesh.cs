using UnityEngine;
using System.Collections;

public class FTME02_ScaleMesh : MonoBehaviour {

	Transform myMesh;
	Vector3 origMeshScale;
	public float scaleMeshSize = 1f;

	public void ScaleMeshOnEnable () {
		myMesh = GetComponent<Transform>();
		origMeshScale = myMesh.localScale;
	}

	public void ScaleMeshOnDisable () {
		myMesh.localScale = origMeshScale;
	}

	public void ScaleMesh () {
		myMesh.localScale = origMeshScale * scaleMeshSize;
	}
	
}
