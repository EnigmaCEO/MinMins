using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    private KinWrapper kinWrapper;

    public Text textBalance;

    public void SendKin()
    {
        //send Kin to server
        string memo = "tutorial Spend";
        kinWrapper.SendKin(5m, memo);
    }

    public void RequestKin()
    {
        //request Kin from server
        string memo = "tutorial Earn";
        kinWrapper.EarnKin(10m, memo);
    }

    void ListenKin(object eventData, string type)
    {
        if (type == "balance")
        {
            textBalance.text = kinWrapper.Balance().ToString();
        }

        GameObject.Find("TutorialLog").GetComponent<Text>().text += "\n" +
            eventData.ToString();
    }

    void Start()
    {
        string url = "https://min-mins.herokuapp.com/"; //"https://mykin-server.com";
        string serverAddress = "GCHFGWHCU7GUF2E773D54XHFFE4662PP7BVPJZCDXC2ZNEJUVXXYEDVA"; //"GAFWSBEOGCCYVEC5YZUILDEDGHO27PODVTQJ45DSFBODKRXQ42MVLIZZ";
        kinWrapper = GameObject.Find("KinWrapper").GetComponent<KinWrapper>();
        kinWrapper.Initialize(ListenKin, url, serverAddress);
    }
}
