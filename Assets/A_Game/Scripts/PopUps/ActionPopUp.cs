using GameEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionPopUp : MonoBehaviour
{
    public Button DismissButton;

    [SerializeField ] private Image _unitImage;
    [SerializeField] private Text _buttonText;

    public void Open(string unitName, UnitRoles unitRole, bool enableActionButton)
    {
        _unitImage.sprite = Resources.Load<Sprite>("Images/Units/" + unitName);

        if (enableActionButton)
        {
            if (unitRole == UnitRoles.Bomber || unitRole == UnitRoles.Destroyer)
            {
                DismissButton.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("ATTACK");
            }
            if (unitRole == UnitRoles.Scout)
            {
                DismissButton.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("SCOUT");
            }
            if (unitRole == UnitRoles.Healer)
            {
                DismissButton.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("HEAL");
            }
            if (unitRole == UnitRoles.Tank)
            {
                DismissButton.GetComponentInChildren<Text>().text = LocalizationManager.GetTermTranslation("GUARD");
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
