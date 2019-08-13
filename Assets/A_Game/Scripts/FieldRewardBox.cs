using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldRewardBox : MonoBehaviour
{
    public delegate void SimpleDelegate();
    public SimpleDelegate OnHit;

    private void onHit()  //Called by attack collision
    {
        OnHit();
    }
}
