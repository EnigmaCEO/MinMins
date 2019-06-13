using UnityEngine;
using System.Collections;
using ParticlePlayground;

public class FTME02_LoopSwitch : MonoBehaviour {

	public float loopOffTiming = 0;
	float currentTime;
	public Transform parentTransform;
	PlaygroundParticlesC[] particleSystems;

	// Use this for initialization
	void Start () {
		particleSystems = parentTransform.GetComponentsInChildren<PlaygroundParticlesC>();
		currentTime = 0;	
	}
	
	// Update is called once per frame
	void Update () {
		if (loopOffTiming != 0) {
						currentTime += Time.deltaTime;
						if (currentTime > loopOffTiming) {
								foreach (PlaygroundParticlesC particles in particleSystems)
										particles.loop = false;
						}
				}
			
	}
}
