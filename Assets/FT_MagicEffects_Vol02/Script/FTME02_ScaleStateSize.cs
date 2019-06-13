using UnityEngine;
using System.Collections;
using ParticlePlayground;

public class FTME02_ScaleStateSize : MonoBehaviour {

	float origStateScale;
	public float scaleState = 1f;
	PlaygroundParticlesC particles;

	public void ScaleStateSizeOnEnable () {
		if (particles==null)
			particles = GetComponent<PlaygroundParticlesC>();
		foreach (ParticleStateC activeState in particles.states) {
			origStateScale = activeState.stateScale;		
		}
	}

	public void ScaleStateSizeOnDisable () {
		foreach (ParticleStateC activeState in particles.states) {
			activeState.stateScale = origStateScale;		
		}		
	}

	public void ScaleStateSize () {
		foreach (ParticleStateC activeState in particles.states) {
			activeState.stateScale = origStateScale * scaleState;
		}
	}
}
