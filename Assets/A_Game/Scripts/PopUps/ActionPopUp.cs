using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionPopUp : MonoBehaviour
{
    public Image UnitImage;

    public void Open(string unitName)
    {
        UnitImage.sprite = Resources.Load<Sprite>("Images/Units/" + unitName);
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
