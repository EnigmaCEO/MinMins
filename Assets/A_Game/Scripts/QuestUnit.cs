using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestUnit : MonoBehaviour
{
    private Action<string> _onClick;
    private Action<string> _onReveal;

    public void SetUp(string unitName, Action<string> onClick, Action<string> onReveal)
    {
        name = unitName;

        _onClick = onClick;
        _onReveal = onReveal;

        string spritePath = "Images/Units/" + unitName;
        Sprite unitSprite = Resources.Load<Sprite>(spritePath);

        if (unitSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = unitSprite;
        }
        else
        {
            Debug.LogError(nameof(QuestUnit) + "::" + nameof(SetUp) + " -> Sprite was not found at path: " + spritePath);
        }

        gameObject.AddComponent<PolygonCollider2D>().isTrigger = false;
    }

    public void OnMouseDown()
    {
        _onClick(name);
    }

    protected void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.Log("QuestUnit::OnTriggerEnter2D: " + coll.name);
        _onReveal(name);    
    }
}
