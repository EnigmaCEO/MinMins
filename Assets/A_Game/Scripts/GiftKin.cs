using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeStage.AntiCheat.Storage;
using UnityEngine.Networking;

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
        StartCoroutine(SendGift());
    }

    public IEnumerator SendGift()
    {
        //KinManager.Instance.SendKin(5m, "Kin Gift");
        if (KinManager.Instance.GetUserPublicAddress() != null) {
            string reqUrl = "https://min-mins.herokuapp.com?request=1";

            WWWForm form = new WWWForm();
            form.AddField("address", "GBAPZQBFDE2E5VOB77LCRCHL5PR25VLIMUQZ4BFP2FCYXK37NFNKWMK2");
            form.AddField("memo", "Gift Kin");
            form.AddField("amount", "5");


            var req = UnityWebRequest.Post(reqUrl, form);
            yield return req.SendWebRequest();

            if (req.isNetworkError || req.isHttpError)
            {
                giftMsg.text = "Gift Error";
            }
            else
            {
                float balance = ObscuredPrefs.GetFloat("KinBalanceUser", 50f);
                if (balance > 0)
                {
                    ObscuredPrefs.SetFloat("KinBalanceUser", balance - 5f);
                    giftMsg.text = "Gift Sent";
                }
                else
                {
                    giftMsg.text = "";
                }
            }
        }
        
        gameObject.SetActive(false);

    }
}
