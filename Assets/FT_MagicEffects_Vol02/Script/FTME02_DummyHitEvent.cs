using UnityEngine;
using System.Collections;
using ParticlePlayground;

public class FTME02_DummyHitEvent : MonoBehaviour {

	Rigidbody dummyObjectRigid;
	public PlaygroundParticlesC[] loopOfParticles;
	public Light lightObject;
	public Transform trailParent;
	public Transform meshParent;
	public float deadTime = 1f;
	FTME02_LightRandomAndFadeByScript lightFadeOff;
	FTME02_TrailRotation trailFade;
	FTME02_MeshFadeOut meshFade;
	FTME02_MeshRotate meshRot;
	GameObject parent;

	void Start () {
	
	}

	void OnTriggerEnter(Collider other) {
		dummyObjectRigid = transform.GetComponent<Rigidbody>();
		dummyObjectRigid.velocity = Vector3.zero;
		lightFadeOff = lightObject.GetComponent<FTME02_LightRandomAndFadeByScript>();
		lightFadeOff.fadeOutSwitch = true;
		parent = gameObject.transform.parent.gameObject;

		if(trailParent){
			trailFade = trailParent.GetComponent<FTME02_TrailRotation>();
			trailFade.FadeOut();
			trailFade.fadeOutSwitch = true;
		}
		if(meshParent){
			meshFade = meshParent.GetComponent<FTME02_MeshFadeOut>();
			meshFade.fadeOutSwitch = true;
		}
		if(meshParent && meshParent.GetComponentInChildren<FTME02_MeshRotate>()){
			meshRot = meshParent.GetComponentInChildren<FTME02_MeshRotate>();
			meshRot.stopSwitch = true;
		}
		if(loopOfParticles.Length != 0 ){
			for (int i = 0; i < loopOfParticles.Length; i++) {
				loopOfParticles[i].loop = false;
			}
		}

		Destroy(parent,deadTime);
	}
}
