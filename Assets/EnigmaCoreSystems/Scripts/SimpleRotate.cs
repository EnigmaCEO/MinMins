using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [SerializeField] private float _rotateSpeed = 40.0f;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 0, _rotateSpeed) * Time.deltaTime);
    }
}
