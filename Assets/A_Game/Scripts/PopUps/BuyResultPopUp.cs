using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyResultPopUp : MonoBehaviour
{
    [SerializeField] private Text _message;

    public void Open(string message)
    {
        _message.text = message;
        LocalizationManager.LocalizeText(_message);
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
