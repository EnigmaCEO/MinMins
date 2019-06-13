
var minSpeed:float=0.7;
var maxSpeed:float=1.5;

function Start () {
	for (var state : AnimationState in GetComponent.<Animation>()) {
		state.speed = Random.Range(minSpeed, maxSpeed);
	}

}