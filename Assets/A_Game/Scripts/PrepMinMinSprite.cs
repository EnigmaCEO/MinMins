using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepMinMinSprite : MonoBehaviour
{
    private WarPrepDragger _warPrepDragger;
    private PolygonCollider2D _polygonCollider;

    private Transform _draggerTransform;
    private Vector3 _originalOffset;

    private void Awake()
    {
        _polygonCollider = GetComponent<PolygonCollider2D>();
        _polygonCollider.isTrigger = false;
        _polygonCollider.enabled = false;
    }

    public void SetDragger(WarPrepDragger dragger)
    {
        _warPrepDragger = dragger;
        _draggerTransform = _warPrepDragger.transform;
        _originalOffset = transform.localPosition;
    }

    public void EnablePolygonCollider()
    {
        _polygonCollider.enabled = true;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        //Debug.Log(coll.gameObject.transform.parent.name + " collision");

        _warPrepDragger.HandleDroppedCollision();
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        transform.localPosition = _originalOffset;
    }

    void OnCollisionStay2D(Collision2D coll)
    {
        float x1, y1;

        if (transform.localPosition.x >= coll.transform.localPosition.x)
        {
            x1 = 0.1f;
        }
        else
        {
            x1 = -0.1f;
        }

        if (transform.localPosition.y >= coll.transform.localPosition.y)
        {
            y1 = 0.1f;
        }
        else
        {
            y1 = -0.1f;
        }

        _draggerTransform.Translate(x1, y1, 0);
    }
}
