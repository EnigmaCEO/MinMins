using UnityEngine;
using System.Collections;
using ParticlePlayground;


public class FTME02_ParticleProjectile : MonoBehaviour {

	public Transform parentTransform;
	public Transform dummyObject;
	public float speed;
	PlaygroundParticlesC[] particleSystems;
	Rigidbody dummyObjectRigid;
	Vector3 pDirection;
	float angle;



	void Start () {
	}
		
	void Awake () {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, 5000f)) {
			pDirection = new Vector3(hit.point.x - parentTransform.position.x,
			                         hit.point.y - parentTransform.position.y,
			                         hit.point.z - parentTransform.position.z);

			pDirection = pDirection.normalized;
			pDirection *= speed;
		}

		if (particleSystems==null){
			particleSystems = parentTransform.GetComponentsInChildren<PlaygroundParticlesC>();
		}
		for (int i = 0; i < particleSystems.Length; i++) {
			particleSystems[i].applyInitialLocalVelocity = true;
			particleSystems[i].initialLocalVelocityMax = pDirection;
			particleSystems[i].initialLocalVelocityMin = pDirection;
		}
		if(dummyObject){
			dummyObjectRigid = dummyObject.GetComponent<Rigidbody>();
			dummyObjectRigid.AddForce (pDirection,ForceMode.VelocityChange);
			dummyObject.LookAt(hit.point);

		}

	}
}
