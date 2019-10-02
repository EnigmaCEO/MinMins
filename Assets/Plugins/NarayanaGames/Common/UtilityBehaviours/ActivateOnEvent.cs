using UnityEngine;

public class ActivateOnEvent : MonoBehaviour {

    public GameObject targetObject;

    public void Awake() {
        if (targetObject == null) {
            targetObject = this.gameObject;
        }
    }

    public void SetActive(bool activate) {
        if (activate) {
            Awake();
            targetObject.SetActive(true);
        }
    }
}
