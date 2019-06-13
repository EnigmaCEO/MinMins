using UnityEngine;
using System.Collections;

public class FTME02_ScaleLight : MonoBehaviour {
	
	Light myLight;
	float origLightScale;
	public float scaleLightRangeSize = 1f;

	public void ScaleLightRangeOnEnable () {
		myLight = GetComponent<Light>();
		origLightScale = myLight.range;
	}

	public void ScaleLightRangeOnDisable () {
		myLight.range = origLightScale;
	}
	
	public void ScaleLightRange () {
		myLight.range = origLightScale * scaleLightRangeSize;	
	}
	
}
