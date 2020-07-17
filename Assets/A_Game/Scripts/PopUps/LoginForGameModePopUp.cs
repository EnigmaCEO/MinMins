using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginForGameModePopUp : MonoBehaviour
{
    private void Start()
    {
        Close();
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
