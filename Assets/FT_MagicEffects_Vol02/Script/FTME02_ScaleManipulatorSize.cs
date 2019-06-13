using UnityEngine;
using System.Collections;
using ParticlePlayground;

public class FTME02_ScaleManipulatorSize : MonoBehaviour {

	float origManiScale;
	float origManiStrength;
	public float scaleManipulatorSize = 1f;
	PlaygroundParticlesC particles;

	public void ScaleManipulatorSizeOnEnable () {
		if (particles==null)
			particles = GetComponent<PlaygroundParticlesC>();
		foreach (ManipulatorObjectC manipulator in particles.manipulators) {
				origManiScale = manipulator.size;
				origManiStrength = manipulator.strength;
		}
	}

	public void ScaleManipulatorSizeOnDisable () {
		foreach (ManipulatorObjectC manipulator in particles.manipulators) {
			manipulator.size = origManiScale;
			manipulator.strength = origManiStrength;
		}		
	}

	public void ScaleManipulatorSize () {
		foreach (ManipulatorObjectC manipulator in particles.manipulators) {
			manipulator.size = origManiScale * scaleManipulatorSize;
			manipulator.strength = origManiStrength * scaleManipulatorSize;
		}
	}
	
}
