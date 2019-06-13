using UnityEngine;
using System.Collections;
using ParticlePlayground;


public class FTME02_ParticleScaler : MonoBehaviour {

	public float scaleSize = 1f;
	public bool scaleVelocity = true;
	public bool scaleParticleSize = true;
	public bool scaleLifetimePositioning = true;
	public bool scaleOverflowOffset = true;
	public bool scaleScatterSize = true;

	float origVelocityScale;
	float origScale;
	float origLifetimePositioningScale;
	Vector3 origOverflowOffset;
	Vector3 origScatterScale;
	
	PlaygroundParticlesC particles;

	public void ParticleScaleOnEnable () {
		if (particles==null)
			particles = GetComponent<PlaygroundParticlesC>();
		origVelocityScale = particles.velocityScale;
		origScale = particles.scale;
		origLifetimePositioningScale = particles.lifetimePositioningScale;
		origOverflowOffset = particles.overflowOffset;
		origScatterScale = particles.scatterScale;
	}

	public void ParticleScaleOnDisable () {
		particles.velocityScale = origVelocityScale;
		particles.scale = origScale;
		particles.lifetimePositioningScale = origLifetimePositioningScale;
		particles.overflowOffset = origOverflowOffset;
		particles.scatterScale = origScatterScale;
	}

	public void ParticleScale () {

		if (scaleVelocity)
			particles.velocityScale = origVelocityScale*scaleSize;
		if (scaleParticleSize)
			particles.scale = origScale*scaleSize;
		if (scaleLifetimePositioning)
			particles.lifetimePositioningScale = origLifetimePositioningScale*scaleSize;
		if (scaleOverflowOffset)
			particles.overflowOffset = origOverflowOffset*scaleSize;
		if (scaleScatterSize)
			particles.scatterScale = origScatterScale*scaleSize;

		//scaleSize = 1f;
	}

}
