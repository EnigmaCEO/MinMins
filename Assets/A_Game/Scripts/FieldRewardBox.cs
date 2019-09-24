using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldRewardBox : MonoBehaviour
{
    public delegate void SimpleDelegate();
    public SimpleDelegate OnHitCallback;

    public void Hit()  //Called by attack collision
    {
        OnHitCallback();
        GetComponent<PolygonCollider2D>().enabled = false;

        StartCoroutine(handleDelayedDestroy());
    }

    private IEnumerator handleDelayedDestroy()
    {
        yield return new WaitForSeconds(GameConfig.Instance.RewardChestDestructionDelay);
        Destroy(this.gameObject);
    }
}
