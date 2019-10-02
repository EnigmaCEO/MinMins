/****************************************************
 *  (c) 2012 narayana games UG (haftungsbeschr�nkt) *
 *  This is just an example, do with it whatever    *
 *  you like ;-)                                    *
 ****************************************************/

using UnityEngine;
using System.Collections;

public class WorldSpaceMessagesCSharp : MonoBehaviour {

    public bool isShot = false;
    public Vector3 initialVelocity = Vector3.zero;
    public float lifeTime = 1F;

    public bool controlColors = false;

    bool hasStarted = false;

	// Use this for initialization
	void Start() {
        StartCoroutine(GenerateMessages());
        if (isShot) {
            if (this.GetComponent<Rigidbody>() == null) {
                Debug.LogError("Need a rigidbody to shoot", this);
                return;
            }
            this.GetComponent<Rigidbody>().velocity = transform.rotation * initialVelocity;
            StartCoroutine(SelfDestroy());
        }
        hasStarted = true;
	}

    public void OnEnable() {
        if (hasStarted) {
            Start();
        }
    }

    public IEnumerator GenerateMessages() {
        ScoreFlashFollow3D follow3D = GetComponent<ScoreFlashFollow3D>();
        while (this.enabled) {

            string message = this.GetComponent<Rigidbody>().velocity.y > 0
                ? string.Format("+{0:0}", (int) this.GetComponent<Rigidbody>().velocity.y)
                : string.Format("{0:0}", (int) this.GetComponent<Rigidbody>().velocity.y);

            if (controlColors) {
                Color color = this.GetComponent<Rigidbody>().velocity.y > 0 ? Color.green : Color.red;

                if (follow3D == null) {
                    Vector2 offset = Vector2.zero;
                    ScoreFlash.Instance.PushWorld(transform.position, offset, message, color);
                } else {
                    ScoreFlash.Instance.PushWorld(follow3D, message, color);
                }
            } else {
                if (follow3D == null) {
                    Vector2 offset = Vector2.zero;
                    ScoreFlash.Instance.PushWorld(transform.position, offset, message);
                } else {
                    ScoreFlash.Instance.PushWorld(follow3D, message);
                }
            }

            yield return new WaitForSeconds(0.3F);
        }
    }

    public IEnumerator SelfDestroy() {
        yield return new WaitForSeconds(lifeTime);
        Destroy(this.gameObject);
    }
}
