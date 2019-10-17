using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KinManager : SingletonMonobehaviour<KinManager>
{
    [SerializeField] string _url = "https://min-mins.herokuapp.com/"; 
    [SerializeField] string _serverAddress = "GCHFGWHCU7GUF2E773D54XHFFE4662PP7BVPJZCDXC2ZNEJUVXXYEDVA"; 

    private KinWrapper _kinWrapper;


    private void Awake()
    {
        _kinWrapper = GetComponent<KinWrapper>();
    }

    void Start()
    {
        _kinWrapper.Initialize(ListenKin, _url, _serverAddress);
    }

    public decimal GetBalance()
    {
        return _kinWrapper.Balance();
    }

    public string GetUserPublicAddress()
    {
        return _kinWrapper.PublicAddress();
    }

    public void RegisterCallback(Action<object, string> callback)
    {
        _kinWrapper.RegisterCallback(callback);
    }

    public bool DeleteAccount()
    {
        return _kinWrapper.DeleteAccount();
    }

    public void SendKin(decimal amount, string memo)
    {
        _kinWrapper.SendKin(5m, memo);
    }

    public void EarnKin(decimal amount, string memo)
    {
        _kinWrapper.EarnKin(10m, memo);
    }

    private void ListenKin(object eventData, string type)
    {
        Debug.Log("KinManager::ListenKin -> eventData: " + eventData.ToString() + " type: " + type);
    }
}
