var animList : AnimationClip[];
var actualAnim:AnimationClip;

var minSpeed:float=0.7;
var maxSpeed:float=1.5;

function Start () {
var rnd=Mathf.Floor(Random.Range(0,animList.length));
actualAnim = animList[rnd];
GetComponent.<Animation>().Play(actualAnim.name);
//this.GetComponent<Animation>()[actualAnim.name].speed = Random.Range(minSpeed, maxSpeed);
	for (var state : AnimationState in GetComponent.<Animation>()) {
		if (state.name==actualAnim.name) state.speed = Random.Range(minSpeed, maxSpeed);
	}


}