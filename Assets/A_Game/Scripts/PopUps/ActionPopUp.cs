using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionPopUp : MonoBehaviour
{
    public Button DismissButton;

    [SerializeField ] private Image _unitImage;
    [SerializeField] private Text _buttonText;

    public void Open(string unitName, MinMinUnit.Types unitType, bool enableActionButton)
    {
        _unitImage.sprite = Resources.Load<Sprite>("Images/Units/" + unitName);

        if (enableActionButton)
        {
            if (unitType == MinMinUnit.Types.Bomber || unitType == MinMinUnit.Types.Destroyer)
            {
                DismissButton.GetComponentInChildren<Text>().text = "ATTACK";
            }
            if (unitType == MinMinUnit.Types.Scout)
            {
                DismissButton.GetComponentInChildren<Text>().text = "SCOUT";
            }
            if (unitType == MinMinUnit.Types.Healer)
            {
                DismissButton.GetComponentInChildren<Text>().text = "HEAL";
            }
            if (unitType == MinMinUnit.Types.Tank)
            {
                DismissButton.GetComponentInChildren<Text>().text = "GUARD";
            }
        }

        DismissButton.gameObject.SetActive(enableActionButton);

        gameObject.SetActive(true);
    }


    public void Close()
    {
        gameObject.SetActive(false);
    }
}
