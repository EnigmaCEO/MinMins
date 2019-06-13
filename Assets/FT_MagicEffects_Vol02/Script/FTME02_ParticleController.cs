using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ParticlePlayground;

[ExecuteInEditMode()]
public class FTME02_ParticleController : MonoBehaviour {
	//scale parameter
	public float scale = 1f;
	public float scaleOnlyLine = 1f;
	public float scaleLife = 1f;

	//public float deadtime = 10;

	//lineRenderer parameter
	public bool hasLineRenderer;
	public bool onlyLineRendererScaling; 

	//lifetime parameter
	List<float> origLifetime = new List<float>(); 
	List<float> origSpeed = new List<float>(); 
	Animator[] anim;
	//color parameter
	public List<Gradient> particleColor = new List<Gradient>(); 
	//particleplayground
	public PlaygroundParticlesC[] particleSystems;
	//child script
	FTME02_ScaleStateSize[] childScaleState;
	FTME02_ParticleScaler[] childScaleParticle;
	FTME02_ScaleManipulatorSize[] childScaleManipulator;
	FTME02_ScaleMesh[] childScaleMesh;
	FTME02_ScaleLight[] childScaleLight;
	FTME02_ScaleLine[] childScaleLine;

	void OnEnable () {

		//Get ParticlePlayground and other component.
		if (childScaleState==null){
			childScaleState = transform.GetComponentsInChildren<FTME02_ScaleStateSize>();
		}
		if (childScaleParticle==null){
			childScaleParticle = transform.GetComponentsInChildren<FTME02_ParticleScaler>();
		}
		if (childScaleManipulator==null){
			childScaleManipulator = transform.GetComponentsInChildren<FTME02_ScaleManipulatorSize>();
		}
		if (childScaleMesh==null){
			childScaleMesh = transform.GetComponentsInChildren<FTME02_ScaleMesh>();
		}
		if (childScaleLight==null){
			childScaleLight = transform.GetComponentsInChildren<FTME02_ScaleLight>();
		}
		if (childScaleLine==null && hasLineRenderer == true){
			childScaleLine = transform.GetComponentsInChildren<FTME02_ScaleLine>();
		}

		//Set initial value.
		for (int i = 0; i < childScaleState.Length; i++) {
			childScaleState[i].ScaleStateSizeOnEnable();
		}		
		for (int i = 0; i < childScaleParticle.Length; i++) {
			childScaleParticle[i].ParticleScaleOnEnable();
		}
		for (int i = 0; i < childScaleManipulator.Length; i++) {
			childScaleManipulator[i].ScaleManipulatorSizeOnEnable();
		}
		for (int i = 0; i < childScaleMesh.Length; i++) {
			childScaleMesh[i].ScaleMeshOnEnable();
		}
		for (int i = 0; i < childScaleLight.Length; i++) {
			childScaleLight[i].ScaleLightRangeOnEnable();
		}
		if (hasLineRenderer == true) {
			for (int i = 0; i < childScaleLine.Length; i++) {
				childScaleLine [i].ScaleLineOnEnable ();
			}
		}

		anim = transform.GetComponentsInChildren<Animator>();
		//particleSystems = transform.GetComponentsInChildren<PlaygroundParticlesC>();

		if (particleColor.Count == 0){
			for (int i = 0; i < particleSystems.Length; i++) {
				particleColor.Add(particleSystems[i].lifetimeColor);
			}
		}


	}

	void OnDisable () {
		for (int i = 0; i < childScaleState.Length; i++) {
			childScaleState[i].ScaleStateSizeOnDisable();
		}
		for (int i = 0; i < childScaleParticle.Length; i++) {
			childScaleParticle[i].ParticleScaleOnDisable();
		}
		for (int i = 0; i < childScaleManipulator.Length; i++) {
			childScaleManipulator[i].ScaleManipulatorSizeOnDisable();
		}
		for (int i = 0; i < childScaleMesh.Length; i++) {
			childScaleMesh[i].ScaleMeshOnDisable();
		}
		for (int i = 0; i < childScaleLight.Length; i++) {
			childScaleLight[i].ScaleLightRangeOnDisable();
		}
		if (hasLineRenderer == true) {
			for (int i = 0; i < childScaleLine.Length; i++) {
				childScaleLine [i].ScaleLineOnDisable ();
			}
		}
	}
		
	public void Update () {
		//Scaling particle
		for (int i = 0; i < childScaleState.Length; i++) {
			childScaleState[i].scaleState = scale;
			childScaleState[i].ScaleStateSize();
		}
		for (int i = 0; i < childScaleParticle.Length; i++) {
			childScaleParticle[i].scaleSize = scale;
			childScaleParticle[i].ParticleScale();
		}
		for (int i = 0; i < childScaleManipulator.Length; i++) {
			childScaleManipulator[i].scaleManipulatorSize = scale;
			childScaleManipulator[i].ScaleManipulatorSize();
		}
		for (int i = 0; i < childScaleMesh.Length; i++) {
			childScaleMesh[i].scaleMeshSize = scale;
			childScaleMesh[i].ScaleMesh();
		}
		for (int i = 0; i < childScaleLight.Length; i++) {
			childScaleLight[i].scaleLightRangeSize = scale;
			childScaleLight[i].ScaleLightRange();
		}
		if (hasLineRenderer == true) {
			if (onlyLineRendererScaling == false) {
					for (int i = 0; i < childScaleLine.Length; i++) {
						childScaleLine [i].scaleLineSize = scale;
						childScaleLine [i].ScaleLine ();
					}
			} else {
					for (int i = 0; i < childScaleLine.Length; i++) {
						childScaleLine [i].scaleLineSize = scaleOnlyLine;
						childScaleLine [i].ScaleLine ();
					}
			}
		}

		//Scale lifetime. 
		for (int i = 0; i < anim.Length; i++) {
			origSpeed.Add(anim[i].speed);
			anim[i].speed = origSpeed[i]*(1/scaleLife);
		}	
		for (int i = 0; i < particleSystems.Length; i++) {
			origLifetime.Add(particleSystems[i].lifetime);
			particleSystems [i].lifetime = origLifetime[i] * scaleLife;
		}

		for (int i = 0; i < particleColor.Count; i++) {
			particleSystems[i].lifetimeColor = particleColor[i];
		}


	}

	public void GetColor () {
		if (particleColor.Count == 0){
			for (int i = 0; i < particleSystems.Length; i++) {
				particleColor.Add(particleSystems[i].lifetimeColor);
			}
		}
	}

	public void ClearColor () {
		if(particleColor != null)
			particleColor.Clear();
	}


	


	/*void Awake (){
		if (deadtime != 0) {
			Destroy (gameObject, deadtime);
		}
	}*/

}
