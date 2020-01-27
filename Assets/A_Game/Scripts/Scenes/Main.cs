using Enigma.CoreSystems;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CodeStage.AntiCheat.Storage;

public class Main : EnigmaScene
{
    [SerializeField] GameObject _loginModal;
    [SerializeField] LoginForPvpPopUp _loginForPvpPopUp;

    [SerializeField] GameObject _enjinWindow;
    [SerializeField] private int _checkEnjinLinkingDelay = 2;
    [SerializeField] GameObject _loginButton;
    [SerializeField] GameObject _logoutButton;
    [SerializeField] GameObject _kinPopUp;
    [SerializeField] GameObject _enjinIcon;
    [SerializeField] Text _pvprating;
    KinManager _kinWrapper;

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
        _loginModal.SetActive(false);
        _enjinIcon.SetActive(false);
        _enjinWindow.SetActive(false);

        _pvprating.text = "";

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

        Init();
    }

    public void Init()
    {
        

        bool loggedIn = NetworkManager.LoggedIn;

        _loginButton.gameObject.SetActive(!loggedIn);
        _logoutButton.gameObject.SetActive(loggedIn);

        _loginModal.GetComponent<EnjinLogin>().Initialize();

        if (!NetworkManager.LoggedIn && _loginModal.GetComponent<EnjinLogin>().loginUsername.text != "" && _loginModal.GetComponent<EnjinLogin>().loginPassword.text != "")
        {
            _loginModal.GetComponent<EnjinLogin>().loginSubmit();
        }

        

        //_kinPopUp.SetActive(true);

        if(GameNetwork.Instance.IsEnjinLinked)
        {
            _enjinIcon.SetActive(true);
            Text text = _enjinIcon.GetComponentInChildren<Text>();
            text.text = "";
            if (GameNetwork.Instance.HasEnjinEnigmaToken) text.text += "\nEnjin MFT";
            if (GameNetwork.Instance.HasEnjinMinMinsToken) text.text += "\nMin-Mins Token";
            if (GameNetwork.Instance.HasEnjinMaxim) text.text += "\nMaxim Legend";
            if (GameNetwork.Instance.HasEnjinWitek) text.text += "\nWitek Legend";
            if (GameNetwork.Instance.HasEnjinBryana) text.text += "\nBryana Legend";
            if (GameNetwork.Instance.HasEnjinTassio) text.text += "\nTassio Legend";
            if (GameNetwork.Instance.HasEnjinSimon) text.text += "\nSimon Legend";
        }

        if(loggedIn)
        {
            _pvprating.text = LocalizationManager.GetTermTranslation("PvP Rating") + ": " + GameStats.Instance.Rating;
        }
        
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
            _loginForPvpPopUp.Open();
    }

    public void OnStoreButtonDown()
    {
        print("OnStoreButtonDown");
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);
        SceneManager.LoadScene(GameConstants.Scenes.STORE);
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

    public void StartEnjinQR(string enjinCode) {
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
        SimpleJSON.JSONNode response_hash = response[0];
        Debug.Log(response_hash);
        string status = response_hash["status"].ToString().Trim('"');

        if (status == "SUCCESS")
        {
            string enjinCode = response_hash["user_data"]["enjin_code"].ToString().Trim('"');
            if (enjinCode == "null")
            {
                GameNetwork.Instance.IsEnjinLinked = true;
                //enjinSupport.gameObject.SetActive(true);

                updateEnjinItems(response_hash);
                if(_enjinWindow)
                    _enjinWindow.gameObject.SetActive(false);
            }
            else if ((_enjinWindow  != null) && _enjinWindow.GetActive())
                StartCoroutine(handleEnjinLinkingCheck(_checkEnjinLinkingDelay));
        }
        //else
        //{
        //    _errorText.text = UIManager.GetLocalizedText("Unable to check Enjin Linking at the moment. Retrying...");
        //    _errorText.gameObject.SetActive(true);
        //    StartCoroutine(handleEnjinLinkingCheck(_checkEnjinLinkingDelay));
        //}
    }

    private void updateEnjinItems(SimpleJSON.JSONNode response_hash)
    {

        SimpleJSON.JSONNode userData = response_hash["user_data"];

        //GameStats.Instance.HasEnjinWeapon = (userData["enjin_hammer"].AsInt == 1);
        //GameStats.Instance.HasEnjinShield = (userData["enjin_shield"].AsInt == 1);
        //GameStats.Instance.HasEnjinEnigmaToken = (userData["enigma_token"].AsInt == 1);


        //Enjin gear Hack =============================
        //PlayerStats.HasEnjinWeapon = true;  
        //PlayerStats.HasEnjinShield = true;
        //PlayerStats.HasEnjinShalwendToken = true;
        //PlayerStats.HasEnjinEnigmaToken = true;
        //=============================================

        /*if (PlayerStats.HasEnjinEnigmaToken)
        {
            enigmaMFT.gameObject.SetActive(true);
        }


        if (PlayerStats.HasEnjinShalwendToken)
        {
            for (int i = 0; i < data.m_BoughtWeapon.Length; i++)
                data.m_BoughtWeapon[i] = 1;

            for (int i = 0; i < data.m_BoughtShield.Length; i++)
                data.m_BoughtShield[i] = 1;

            shalwendMFT.gameObject.SetActive(true);
        }

        if (PlayerStats.HasEnjinWeapon)
        {
            if (data.m_Weapon == 3)
                data.m_Weapon = 4;

            data.m_BoughtWeapon[3] = 1;

            enjinHammer.gameObject.SetActive(true);
        }

        if (PlayerStats.HasEnjinShield)
        {
            if (data.m_Shield == 3)
                data.m_Shield = 4;

            data.m_BoughtShield[3] = 1;

            enjinShield.gameObject.SetActive(true);
        }

        data.SaveData();*/
    }

    public void closeQRDialog()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        _enjinWindow.SetActive(false);
    }

    public void Logout()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);

        NetworkManager.Logout();
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

    public void closeKinDialog()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        decimal balance = _kinWrapper.GetBalance();
        if (balance == 0)
        {
            _kinWrapper.FundKin();
        }
        _kinPopUp.SetActive(false);
    }
}
