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
        Debug.Log("QuestUnit::OnMouseDown-> unitName: " + this.name);
        _onClick(name);
    }

    protected void OnTriggerEnter2D(Collider2D coll)
    {
        Debug.Log("QuestUnit::OnTriggerEnter2D-> unitName: " + this.name + " collider game object: " + coll.name);
        _onReveal(name);    
    }
}
