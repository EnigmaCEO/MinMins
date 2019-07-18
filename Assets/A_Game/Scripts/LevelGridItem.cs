using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma.CoreSystems;

public class LevelGridItem : MonoBehaviour
{
    public void OnFightButtonDown()
    {
        if(name == "1")
            SceneManager.LoadScene("UnitSelect");
    }
}
