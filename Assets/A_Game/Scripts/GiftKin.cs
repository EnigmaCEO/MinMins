using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.Storage;

public class GiftKin : MonoBehaviour
{
    public Text giftMsg;

    private void Start()
    {
        KinManager.Instance.RegisterCallback((obj, val) =>
        {
            Debug.Log(obj);
        });
    }

    public void Send()
    {
        KinManager.Instance.SendKin(5m, "Kin Gift");
        float balance = ObscuredPrefs.GetFloat("KinBalanceUser", 50f);
        if (balance > 0)
        {
            ObscuredPrefs.SetFloat("KinBalanceUser", balance - 5f);
            giftMsg.text = "Gift Sent";
        } else
        {
            giftMsg.text = "";
        }
        gameObject.SetActive(false);

    }
}
