using UnityEngine;

public class ActivateInverted : MonoBehaviour {

    public GameObject targetObject;

    public void Awake() {
        if (targetObject == null) {
            targetObject = this.gameObject;
        }
    }

    public void SetActive(bool deactivate) {
        targetObject.SetActive(!deactivate);
    }
}
