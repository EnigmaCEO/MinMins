using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enigma.CoreSystems;
using UnityEngine.UI;

public class LevelGridItem : MonoBehaviour
{
    [SerializeField] private Text _label;
    [SerializeField] private Text _labelShadow;

    public void SetLabel(string text)
    {
        _label.text = text;
        _labelShadow.text = text;
    }

    public void OnFightButtonDown()
    {
        if(name == "1")
            SceneManager.LoadScene(GameConstants.Scenes.UNIT_SELECT);
    }
}
