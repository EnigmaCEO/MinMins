using Enigma.CoreSystems;
using UnityEngine;
using System.Collections;

public class Main : EnigmaScene
{
    [SerializeField] GameObject _loginModal;
    [SerializeField] LoginForPvpPopUp _loginForPvpPopUp;

    [SerializeField] GameObject _enjinWindow;
    [SerializeField] private int _checkEnjinLinkingDelay = 2;
    [SerializeField] GameObject _loginButton;
    [SerializeField] GameObject _logoutButton;
    [SerializeField] GameObject _kinPopUp;

    void Start()
    {
        NetworkManager.Disconnect();
        _loginModal.SetActive(false);
        Init();
    }

    public void Init()
    {
        SoundManager.Stop();
        SoundManager.MusicVolume = 0.5f;
        SoundManager.Play("main", SoundManager.AudioTypes.Music, "", true);

        bool loggedIn = NetworkManager.LoggedIn;

        _loginButton.gameObject.SetActive(!loggedIn);
        _logoutButton.gameObject.SetActive(loggedIn);

        _loginModal.GetComponent<EnjinLogin>().Initialize();

        _enjinWindow.SetActive(false);


        string kin = PlayerPrefs.GetString("Kin", "0");

        if (kin == "0")
        {
            _kinPopUp.SetActive(true);
            PlayerPrefs.SetString("Kin", "1");
        }
        else
            _kinPopUp.SetActive(false);

        _kinPopUp.SetActive(true);
    }

    public void OnSinglePlayerButtonDown()
    {
        print("OnSinglePlayerButtonDown");
        GameStats.Instance.Mode = GameStats.Modes.SinglePlayer;
        goToLevels();
    }

    public void OnPvpButtonDown()
    {
        print("OnPvpButtonDown");

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
        SceneManager.LoadScene(GameConstants.Scenes.STORE);
    }

    public void ShowLoginForm()
    {
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
                _enjinWindow.gameObject.SetActive(false);
            }
            else if (_enjinWindow.GetActive())
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
        _enjinWindow.SetActive(false);
    }

    public void Logout()
    {
        NetworkManager.Logout();
        GameNetwork.Instance.ResetLoginValues();
        Init();
    }

    public void closeKinDialog()
    {
        _kinPopUp.SetActive(false);
    }
}
