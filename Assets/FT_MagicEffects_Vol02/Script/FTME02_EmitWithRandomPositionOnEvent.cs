using UnityEngine;
using System.Collections;
using ParticlePlayground;

[ExecuteInEditMode()]
public class FTME02_EmitWithRandomPositionOnEvent : MonoBehaviour {

	public PlaygroundParticlesC eventParticles;                    // The particle system you wish to listen on
	public PlaygroundParticlesC emissionParticles;                // The particle system you wish to emit from upon the event
	public int eventNumber = 0;                                    // The event number in the list of the eventParticles events.
	public Vector3 randomRangeMin = new Vector3(-1f,-1f,-1f);    // The minimum random range you wish to add to the emitted particle.
	public Vector3 randomRangeMax = new Vector3(1f,1f,1f);        // The maximum random range you wish to add to the emitted particle.
	
	System.Random random = new System.Random();                    // Use System.Random instead of Random.Range due to the multithreaded environment.
	
	void Start () {
		
		// Early out if this isn't setup yet (as we have ExecuteInEditMode enabled)
		if (eventParticles==null||emissionParticles==null)
			return;
		
		// Assign your listener to the delegate
		if (eventParticles.events.Count!=0 && eventNumber<eventParticles.events.Count)
			eventParticles.events[eventNumber].particleEvent += EmitWithRandomPosition;
	}
	
	/// <summary>
	/// This function will run whenever the event broadcasts from the assigned eventParticles.
	/// Note that this is running on another thread if nothing else is specified for the Thread Aggregator,
	/// you can therefore only use thread-safe methods within the scope of the listener.
	/// </summary>
	/// <param name="eventParticle">Particle that broadcasted the event.</param>
	void EmitWithRandomPosition (PlaygroundEventParticle eventParticle) {
		emissionParticles.ThreadSafeEmit (
			eventParticle.position+RandomRange(),
			new Vector3(0,0,0),
			eventParticle.color
			);
	}
	
	/// <summary>
	/// Borrow the multithreaded random range in Playground.
	/// </summary>
	/// <returns>The random range.</returns>
	Vector3 RandomRange () {
		return PlaygroundParticlesC.RandomRange (random, randomRangeMin, randomRangeMax);
	}
}