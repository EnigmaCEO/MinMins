﻿using Enigma.CoreSystems;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CodeStage.AntiCheat.Storage;
using GameEnums;
using UnityEngine.UDP;
using System;

public class Main : EnigmaScene
{
    [SerializeField] GameObject _loginModal;
    [SerializeField] LoginForGameModePopUp _loginForPvpPopUp;
    //[SerializeField] LoginForGameModePopUp _loginForQuestPopUp;
    [SerializeField] GameObject _restorePopUp;

    [SerializeField] GameObject _enjinWindow;
    [SerializeField] private int _checkEnjinLinkingDelay = 2;
    [SerializeField] GameObject _loginButton;
    [SerializeField] GameObject _logoutButton;
    [SerializeField] Button _restoreButton;
    [SerializeField] GameObject _enjinIcon;
    [SerializeField] Text _pvprating;

    [SerializeField] GameObject _questButton;

    void Start()
    {
        if (!NetworkManager.LoggedIn)
        {
            GameNetwork.Instance.ResetLoginValues();
        }

        SoundManager.Stop();
        SoundManager.MusicVolume = 0.5f;
        SoundManager.Play("main", SoundManager.AudioTypes.Music, "", true);

        NetworkManager.Disconnect();

        _restorePopUp.SetActive(false);
        _loginModal.SetActive(false);
        _enjinIcon.SetActive(false);
        _enjinWindow.SetActive(false);

        _questButton.SetActive(false);

        _restoreButton.onClick.AddListener(delegate { OnRestoreButtonDown(); });
        _restoreButton.gameObject.SetActive(false);

        _pvprating.text = "";
        /*
                _kinWrapper = GameObject.Find("/KinManager").GetComponent<KinManager>();

                _kinWrapper.RegisterCallback((obj, val) =>
                {
                    //_pvprating.text += "\n" + obj.ToString();

                    if (obj.ToString() == "Account funded")
                    {
                        Text text = GameObject.Find("/Canvas/kin_icon/Text").GetComponent<Text>();
                        text.text = "50";
                        ObscuredPrefs.SetFloat("KinBalanceUser", 50f);
                    }


                });




                string kin = _kinWrapper.GetUserPublicAddress();

                if (kin != null && kin != "")
                {
                    Debug.Log(kin);


                    decimal balance = _kinWrapper.GetBalance();
                    if (balance == 0)
                    {
                        //_pvprating.text += kin + "\n" + balance;
                        _kinPopUp.SetActive(true);
                        //ObscurePrefs.SetString("Kin", "2");
                        //_kinWrapper.FundKin();
                    } else
                    {

                        Text text = GameObject.Find("/Canvas/kin_icon/Text").GetComponent<Text>();
                        text.text = balance.ToString();

                    }
                }
                else
                    _kinPopUp.SetActive(false);
        */
        Init();

        updateRestoreButton();   
        handleRestorePopUp();
    }

    public void Init()
    {
        bool loggedIn = NetworkManager.LoggedIn;

        _loginButton.gameObject.SetActive(!loggedIn);
        _logoutButton.gameObject.SetActive(loggedIn);

        _loginModal.GetComponent<EnjinLogin>().Initialize();

        if (!EnigmaHacks.Instance.PreventAutoLogin)
        {
            if (!NetworkManager.LoggedIn && _loginModal.GetComponent<EnjinLogin>().loginUsername.text != "" && _loginModal.GetComponent<EnjinLogin>().loginPassword.text != "")
            {
                _loginModal.GetComponent<EnjinLogin>().loginSubmit();
            }
        }

        //_kinPopUp.SetActive(true);

        //=========== Only needed here if using logged backup because server could have a backup when prefs do not (new phone) =====
        //updateRestoreButton();   
        //handleRestorePopUp();
        //===========================================================================================================================

        UpdateEnjinDisplay();

        if (loggedIn)
        {
            _pvprating.text = LocalizationManager.GetTermTranslation("PvP Rating") + ": " + GameStats.Instance.Rating;
        }

    }

    private void handleRestorePopUp()
    {
        if (_restoreButton.gameObject.GetActive())
        {
            _restorePopUp.SetActive(true);
        }
    }

    private void updateRestoreButton()
    {
        _restoreButton.gameObject.SetActive(false);
        GameInventory gameInventory = GameInventory.Instance;
        //bool loggedIn = NetworkManager.LoggedIn;

        if (/*(loggedIn && GameStats.Instance.IsThereServerBackup) ||*/ gameInventory.IsTherePrefsBackupSave())
        {
            if(FileManager.Instance.CheckFileNullOrEmpty())
            {
                _restoreButton.gameObject.SetActive(true);
            }
            else
            {
                Debug.Log("updateRestoreButton ->  There is file saved.");
            }
        }
        else
        {
            Debug.Log("updateRestoreButton ->  No server or pref backup to restore.");
        }
    }

    public void OnRestoreButtonDown()
    {
        GameInventory.Instance.LoadBackupSave();
        _restoreButton.gameObject.SetActive(false);

        if (_restorePopUp.GetActive())
        {
            _restorePopUp.SetActive(false);
        }
    }

    public void OnRestorePopUpDismissButtonDown()
    {
        _restorePopUp.SetActive(false);
    }

    public void OnSinglePlayerButtonDown()
    {
        print("OnSinglePlayerButtonDown");
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);
        GameStats.Instance.Mode = GameStats.Modes.SinglePlayer;
        goToLevels();
    }

    public void OnPvpButtonDown()
    {
        print("OnPvpButtonDown");
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);

        if (NetworkManager.LoggedIn)
        {
            GameStats.Instance.Mode = GameStats.Modes.Pvp;
            goToLevels();
        }
        else
        {
            _loginForPvpPopUp.Open();
        }
    }

    public void OnStoreButtonDown()
    {
        print("OnStoreButtonDown");
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);
        SceneManager.LoadScene(GameConstants.Scenes.STORE);
    }

    public void OnQuestButtonDown()
    {
        print("OnQuestButtonDown");

        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);

        //if (NetworkManager.LoggedIn)
        //{
            GameStats.Instance.Mode = GameStats.Modes.Quest;
            goToLevels();
        //}
        //else
        //{
        //    _loginForQuestPopUp.Open();
        //}
    }

    public void ShowLoginForm()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);
        _loginModal.SetActive(true);
        _loginModal.GetComponent<EnjinLogin>().resetForm();
    }

    private void goToLevels()
    {
        SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
    }

    public void StartEnjinQR(string enjinCode) 
    {
        _enjinWindow.SetActive(true);
        _enjinWindow.GetComponent<EnjinQRManager>().ShowImage(enjinCode);
        Debug.Log("Checking server for Link1");
        StartCoroutine(handleEnjinLinkingCheck(1));
    }

    public void OnEnjinWindowExitButtonDown()
    {
        //Enjin.CleanUpPlatform();
        StopCoroutine("handleEnjinLinkingCheck");
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        _enjinWindow.gameObject.SetActive(false);
    }

    IEnumerator handleEnjinLinkingCheck(int delay)
    {
        Debug.Log("Checking server for Link2 " + delay);
        yield return new WaitForSeconds(delay);
        Debug.Log("Checking server for Link3");
        Hashtable val = new Hashtable();
        val.Add(NetworkManager.TransactionKeys.GAME, GameNetwork.TRANSACTION_GAME_NAME);
        NetworkManager.Transaction(NetworkManager.Transactions.ENJIN_LINKED, val, onLinkedTransaction);

        yield break;
    }

    private void onLinkedTransaction(SimpleJSON.JSONNode response)
    {
        if (NetworkManager.CheckInvalidServerResponse(response, nameof(onLinkedTransaction)))
        {
            return;
        }

        SimpleJSON.JSONNode response_hash = response[0];

        string enjinCode = response_hash["user_data"]["enjin_code"].ToString().Trim('"');
        if (enjinCode == "null")
        {
            GameNetwork.Instance.IsEnjinLinked = true;
            //enjinSupport.gameObject.SetActive(true);

            if (_enjinIcon != null)
            {
                _enjinIcon.SetActive(true);
            }

            //updateEnjinItems(response_hash);
            GameNetwork.Instance.UpdateEnjinGoodies(response_hash);
            UpdateEnjinDisplay();

            if (_enjinWindow != null)
            {
                _enjinWindow.gameObject.SetActive(false);
            }
        }
        else if ((_enjinWindow != null) && _enjinWindow.GetActive())
        {
            StartCoroutine(handleEnjinLinkingCheck(_checkEnjinLinkingDelay));
        }

        //else
        //{
        //    _errorText.text = UIManager.GetLocalizedText("Unable to check Enjin Linking at the moment. Retrying...");
        //    _errorText.gameObject.SetActive(true);
        //    StartCoroutine(handleEnjinLinkingCheck(_checkEnjinLinkingDelay));
        //}
    }

 
    //private void updateEnjinItems(SimpleJSON.JSONNode response_hash)
    //{

    //    SimpleJSON.JSONNode userData = response_hash["user_data"];

    //    //GameStats.Instance.HasEnjinWeapon = (userData["enjin_hammer"].AsInt == 1);
    //    //GameStats.Instance.HasEnjinShield = (userData["enjin_shield"].AsInt == 1);
    //    //GameStats.Instance.HasEnjinEnigmaToken = (userData["enigma_token"].AsInt == 1);


    //    //Enjin gear Hack =============================
    //    //PlayerStats.HasEnjinWeapon = true;  
    //    //PlayerStats.HasEnjinShield = true;
    //    //PlayerStats.HasEnjinShalwendToken = true;
    //    //PlayerStats.HasEnjinEnigmaToken = true;
    //    //=============================================

    //    /*if (PlayerStats.HasEnjinEnigmaToken)
    //    {
    //        enigmaMFT.gameObject.SetActive(true);
    //    }


    //    if (PlayerStats.HasEnjinShalwendToken)
    //    {
    //        for (int i = 0; i < data.m_BoughtWeapon.Length; i++)
    //            data.m_BoughtWeapon[i] = 1;

    //        for (int i = 0; i < data.m_BoughtShield.Length; i++)
    //            data.m_BoughtShield[i] = 1;

    //        shalwendMFT.gameObject.SetActive(true);
    //    }

    //    if (PlayerStats.HasEnjinWeapon)
    //    {
    //        if (data.m_Weapon == 3)
    //            data.m_Weapon = 4;

    //        data.m_BoughtWeapon[3] = 1;

    //        enjinHammer.gameObject.SetActive(true);
    //    }

    //    if (PlayerStats.HasEnjinShield)
    //    {
    //        if (data.m_Shield == 3)
    //            data.m_Shield = 4;

    //        data.m_BoughtShield[3] = 1;

    //        enjinShield.gameObject.SetActive(true);
    //    }

    //    data.SaveData();*/
    //}

    public void closeQRDialog()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        _enjinWindow.SetActive(false);
    }

    public void Logout()
    {
        _questButton.gameObject.SetActive(false);

        NetworkManager.Logout();

        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);

        GameNetwork.Instance.ResetLoginValues();

        _loginButton.gameObject.SetActive(true);
        _logoutButton.gameObject.SetActive(false);

        Text text = _enjinIcon.GetComponentInChildren<Text>();
        text.text = "";
        _enjinIcon.SetActive(false);
        _pvprating.text = "";
        StopCoroutine("handleEnjinLinkingCheck");
        _enjinWindow.gameObject.SetActive(false);
    }

    private void addToEnjinItemDisplay(Text text, bool flag, string label)
    {
        if (flag)
        {
            text.text += "\n" + label;
        }
    }
    
    public void UpdateEnjinDisplay() 
    {
        GameNetwork gameNetwork = GameNetwork.Instance;

        if (NetworkManager.LoggedIn)
        {
            Quests activeQuest = GameStats.Instance.ActiveQuest;
            _questButton.SetActive(activeQuest != Quests.None);

            if (_questButton.activeInHierarchy)
            {
                _questButton.transform.Find("QuestName").GetComponent<Text>().text = activeQuest.ToString();

                string iconPath = "Images/Quests/" + activeQuest.ToString() + " Icon";
                Sprite questIcon = (Sprite)Resources.Load<Sprite>(iconPath);

                if (questIcon != null)
                {
                    _questButton.transform.Find("icon").GetComponent<Image>().sprite = questIcon;
                }
                else
                {
                    Debug.Log("Quest Icon image was not found at path: " + iconPath + " . Please check active quest is correct and image is in the right path.");
                }
            }
        }

        if (gameNetwork.IsEnjinLinked)
        {
            _enjinIcon.SetActive(true);
            Text text = _enjinIcon.GetComponentInChildren<Text>();
            text.text = ""; 

            addToEnjinItemDisplay(text, gameNetwork.HasEnjinMft, "Enjin MFT");
            addToEnjinItemDisplay(text, gameNetwork.HasEnjinEnigmaToken, "Enigma Token");
            addToEnjinItemDisplay(text, gameNetwork.HasEnjinMinMinsToken, "Min-Mins Token");

            addToEnjinItemDisplay(text, gameNetwork.HasEnjinMaxim, "Maxim Legend");
            addToEnjinItemDisplay(text, gameNetwork.HasEnjinWitek, "Witek Legend");
            addToEnjinItemDisplay(text, gameNetwork.HasEnjinBryana, "Bryana Legend");
            addToEnjinItemDisplay(text, gameNetwork.HasEnjinTassio, "Tassio Legend");
            addToEnjinItemDisplay(text, gameNetwork.HasEnjinSimon, "Simon Legend");

            addToEnjinItemDisplay(text, gameNetwork.HasEnjinEsther, "Esther Legend");
            addToEnjinItemDisplay(text, gameNetwork.HasEnjinAlex, "Alex Legend");
            addToEnjinItemDisplay(text, gameNetwork.HasEnjinEvan, "Evan Legend");
            addToEnjinItemDisplay(text, gameNetwork.HasEnjinLizz, "Lizz Legend");
            addToEnjinItemDisplay(text, gameNetwork.HasEnjinBrad, "Brad Legend");

            addToEnjinItemDisplay(text, gameNetwork.HasKnightHealer, "Deadly Knight: Healer");
            addToEnjinItemDisplay(text, gameNetwork.HasKnightBomber, "Deadly Knight: Bomber");
            addToEnjinItemDisplay(text, gameNetwork.HasKnightDestroyer, "Deadly Knight: Destroyer");
            addToEnjinItemDisplay(text, gameNetwork.HasKnightScout, "Deadly Knight: Scout");
            addToEnjinItemDisplay(text, gameNetwork.HasKnightTank, "Deadly Knight: Tank");

            addToEnjinItemDisplay(text, gameNetwork.HasDemonHealer, "Demon King: Healer");
            addToEnjinItemDisplay(text, gameNetwork.HasDemonBomber, "Demon King: Bomber");
            addToEnjinItemDisplay(text, gameNetwork.HasDemonDestroyer, "Demon King: Destroyer");
            addToEnjinItemDisplay(text, gameNetwork.HasDemonScout, "Demon King: Scout");
            addToEnjinItemDisplay(text, gameNetwork.HasDemonTank, "Demon King: Tank");
        }
    }
}
