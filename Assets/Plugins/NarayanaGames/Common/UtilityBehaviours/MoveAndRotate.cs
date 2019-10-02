using UnityEngine;
using System.Collections;

public class MoveAndRotate : MonoBehaviour {

    public Quaternion moveDirection = Quaternion.identity;
    public float moveVelocity = 1F;

    public Vector3 rotationSpeed = Vector3.one;

    public Transform target = null;

    public void Awake() {
        if (target == null) {
            target = this.transform;
        }
        moveDirection = target.rotation;
    }

	public void Update() {
        if (moveVelocity != 0) {
            target.Translate(moveDirection * (moveVelocity * Time.deltaTime * Vector3.forward), Space.World);
        }
        if (rotationSpeed != Vector3.one) {
            target.Rotate(Time.deltaTime * rotationSpeed);
        }
	}
}
