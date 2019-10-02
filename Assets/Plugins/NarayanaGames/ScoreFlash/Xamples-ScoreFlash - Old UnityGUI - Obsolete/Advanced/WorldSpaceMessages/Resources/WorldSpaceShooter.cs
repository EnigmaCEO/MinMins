using UnityEngine;
using System.Collections;

public class WorldSpaceShooter : MonoBehaviour {

    public WorldSpaceMessagesCSharp projectile;
    public float shootDelaySeconds = 1F;

    private bool hasStarted = false;

	// Use this for initialization
	void Start() {
        StartCoroutine(ShootProjectile());
        hasStarted = true;
	}

    public void OnEnable() {
        if (hasStarted) {
            Start();
        }
    }

    public IEnumerator ShootProjectile() {
        while (true) {
            yield return new WaitForEndOfFrame();
            WorldSpaceMessagesCSharp obj = (WorldSpaceMessagesCSharp)Instantiate(projectile, this.transform.position, this.transform.rotation);
            obj.gameObject.transform.parent = this.transform;
            yield return new WaitForSeconds(shootDelaySeconds);
        }
    }

}
