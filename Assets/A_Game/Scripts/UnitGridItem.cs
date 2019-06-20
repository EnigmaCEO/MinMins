using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitGridItem : MonoBehaviour
{
    public void OnFightButtonDown()
    {
        if (name == "1")
            SceneManager.LoadScene("UnitSelect");
    }

    public void OnInfoButtonDown()
    {
        if (name == "1")
            SceneManager.LoadScene("UnitSelect");
    }
}
