
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Enigma.CoreSystems;
using CodeStage.AntiCheat.Storage;
using SimpleJSON;

public class EnjinLogin : MonoBehaviour
{
    public Color32 failColor = new Color(217, 0, 0);
    public Color32 successColor = new Color(0, 140, 130);
    public GameObject loginGroup;
    public GameObject registerGroup;
    public GameObject loadingGroup;
    public Text alertText;
    public InputField loginUsername;
    public InputField loginPassword;
    public InputField registerUsername;
    public InputField registerEmail;
    public InputField registerPassword;
    public InputField registerConfirm;
    public InputField ETHField;
    public Button loginButton;
    public Button registerButton;

    [SerializeField] private Toggle _rememberMeToggle;

    private string _lastUsername = "";
    private string _lastPassword = "";


    public void Initialize()
    {
        _rememberMeToggle.isOn = (ObscuredPrefs.GetInt("RememberMe", 0) == 1) ? true : false;

        loginUsername.text = ObscuredPrefs.GetString("username", "");
        loginPassword.text = ObscuredPrefs.GetString("password", "");

        loadingGroup.SetActive(false);
        registerGroup.SetActive(false);

        this.gameObject.SetActive(false);
    }

    public void rememberMeToggleChanged()
    {
        if (_rememberMeToggle.isOn)
        {
            SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);
        }
        else
        {
            SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        }
    }

    public void closeButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        closeDialog();
    }

    public void closeDialog()
    {
        gameObject.SetActive(false);
    }

    public void switchToRegister()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);
        loginGroup.SetActive(false);
        registerGroup.SetActive(true);
    }

    public void switchToLogin()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);

        loginGroup.SetActive(true);
        registerGroup.SetActive(false);

        //Errors display test  =========================
        //string term = "";

        //term = "Register Error";

        //term = "Invalid Password";

        //term = "Invalid Username";

        //term = "Server Error";

        //alertText.color = failColor;
        //setAlert(term);

        //setAlert("Connection Error");
        //setAlert("E-mail is not valid");
        //setAlert("Passwords don't match");
        //setAlert("Please fill the Enjin ETH address");
        //===================================================
    }

    public void switchETHField()
    {
        bool ethFieldIsActive = ETHField.gameObject.activeSelf;

        if (ethFieldIsActive)
        {
            SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
        }
        else
        {
            SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);
        }

        ETHField.gameObject.SetActive(!ethFieldIsActive);
    }

    public void validateRegisterData() {
         alertText.text = "";
        bool valid = registerUsername.text != "" && registerEmail.text != "" && registerPassword.text != "" && registerConfirm.text != "";
        bool reason;

        if (registerEmail.text != "") {
            reason = registerEmail.text.IndexOf('@') > 0;
            valid = valid && reason;
            if (!reason)
                setAlert("E-mail is not valid");
        }
        if (registerPassword.text != "" && registerConfirm.text != "") {
            reason = registerPassword.text == registerConfirm.text;
            valid = valid && reason;
            if (!reason)
                setAlert("Passwords don't match");
        }
        if (ETHField.gameObject.activeSelf) {
            reason = ETHField.text != "";
            valid = valid && reason;
            if (!reason)
                setAlert("Please fill the Enjin ETH address");
        }
        if (valid) {
            alertText.text = "";
        }

        registerButton.interactable = valid;
        alertText.color = registerButton.interactable ? successColor : failColor;
        setAlert(registerButton.interactable ? "" : alertText.text);
    }

    public void validateLoginData() {
        alertText.text = "";
        loginButton.interactable = loginUsername.text != "" && loginPassword.text != "";
        alertText.color = loginButton.interactable ? successColor : failColor;
        setAlert(loginButton.interactable ? "" : alertText.text);
    }

    public void registerSubmit()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);
        showLoadingScreen();

        Hashtable extras = new Hashtable();

        string ipAddress = NetworkManager.Ip; 
        string country = NetworkManager.Country;  
        extras.Add("ip", ipAddress);
        extras.Add("country", country);

        //_lastUsername = registerUsername.text;
        //_lastPassword = registerPassword.text;

        NetworkManager.Register(
            registerUsername.text,
            registerPassword.text,
            registerEmail.text,
            ETHField.text,
            GameNetwork.TRANSACTION_GAME_NAME,
            Application.productName.ToLower(),
            onRegistration,
            extras
        );
    }

    public void LoginSubtmiButtonDown()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);
        loginSubmit();
    }

    public void loginSubmit()
    {
        this.gameObject.SetActive(true);
        showLoadingScreen();

        Hashtable extras = new Hashtable();
        string ipAddress = NetworkManager.Ip; 
        string country = NetworkManager.Country;  
        extras.Add("ip", ipAddress);
        extras.Add("country", country);
        
        _lastUsername = loginUsername.text;
        _lastPassword = loginPassword.text;

        NetworkManager.Login(
            loginUsername.text,
            loginPassword.text,
            onLogin,
            extras
        );

    }

    private void showLoadingScreen() {
        alertText.text = "";
        loginGroup.SetActive(false);
        registerGroup.SetActive(false);
        loadingGroup.SetActive(true);
    }

    private void EnableLoginWindow()
    {
        alertText.text = "";
        loginGroup.SetActive(true);
        registerGroup.SetActive(false);
        loadingGroup.SetActive(false);
    }

    private void onRegistration(SimpleJSON.JSONNode response)
    {
        if (response != null)
        {
        //     print("onRegistration: " + response.ToString());

            SimpleJSON.JSONNode response_hash = response[0];
            string status = response_hash[NetworkManager.TransactionKeys.STATUS].ToString().Trim('"');

            if (status == NetworkManager.StatusOptions.SUCCESS)
            {
                // print("onRegistration SUCCESS");
                completeLogin(response_hash);
                closeDialog();
            }
            else
            {
                EnableLoginWindow();
                string term = "";

                if (status == NetworkManager.StatusOptions.ERR_REGISTER)
                    term = "Register Error";
                else if (status == NetworkManager.StatusOptions.ERR_INVALID_PASSWORD)
                    term = "Invalid Password";
                else if (status == NetworkManager.StatusOptions.ERR_INVALID_USERNAME)
                    term = "Invalid Username";
                else
                    term = "Server Error";

                alertText.color = failColor;
                setAlert(term);
            }
        } else {
            EnableLoginWindow();
            alertText.color = failColor;
            setAlert("Connection Error");
        }
    }

    private void completeLogin(SimpleJSON.JSONNode response_hash)
    {
        print("Complete login:");

        GameStats.Instance.HasPurchased = (response_hash[NetworkManager.TransactionKeys.PURCHASED].AsInt == 1);

        GameNetwork gameNetwork = GameNetwork.Instance;

        GameStats.Instance.Rating = response_hash[NetworkManager.TransactionKeys.USER_DATA][GameNetwork.TransactionKeys.RATING].AsInt;
        string userName = response_hash[NetworkManager.TransactionKeys.USER_DATA][GameNetwork.TransactionKeys.USERNAME];
        NetworkManager.SetLocalPlayerNickName(userName);

        string enjinId = response_hash[NetworkManager.TransactionKeys.USER_DATA][NetworkManager.TransactionKeys.ENJIN_ID].ToString().Trim('"');
        EnigmaHacks enigmaHacks = EnigmaHacks.Instance;

        Debug.Log("Enjin ID: " + enjinId);
        if ((enjinId != "null") || enigmaHacks.EnjinIdNotNull)
        {

            string enjinCode = response_hash[NetworkManager.TransactionKeys.USER_DATA][NetworkManager.TransactionKeys.ENJIN_CODE].ToString().Trim('"');
            Debug.Log("Link Code: " + enjinCode);
            if ((enjinCode != "null") || enigmaHacks.EnjinCodeNotNull)
            {
                GameObject.Find("/Main").GetComponent<Main>().StartEnjinQR(enjinCode);
            }
            else
            {
                print("User has already linked with Enjin");
                gameNetwork.IsEnjinLinked = true;
            }

            gameNetwork.HasEnjinMft = checkTokenAvailable(response_hash, NetworkManager.TransactionKeys.ENJIN_MFT);
            gameNetwork.HasEnjinMinMinsToken = checkTokenAvailable(response_hash, NetworkManager.TransactionKeys.MINMINS_TOKEN);
            gameNetwork.HasEnjinEnigmaToken = checkTokenAvailable(response_hash, NetworkManager.TransactionKeys.ENIGMA_TOKEN);

            gameNetwork.HasEnjinBryana = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.ENJIN_BRYANA);
            gameNetwork.HasEnjinMaxim = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.ENJIN_MAXIM);
            gameNetwork.HasEnjinSimon = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.ENJIN_SIMON);
            gameNetwork.HasEnjinTassio = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.ENJIN_TASSIO);
            gameNetwork.HasEnjinWitek = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.ENJIN_WITEK);

            gameNetwork.HasKnightBomber = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.KNIGHT_BOMBER);
            gameNetwork.HasKnightDestroyer = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.KNIGHT_DESTROYER);
            gameNetwork.HasKnightHealer = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.KNIGHT_HEALER);
            gameNetwork.HasKnightScout = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.KNIGHT_SCOUT);
            gameNetwork.HasKnightTank = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.KNIGHT_TANK);

            gameNetwork.HasDemonBomber = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.DEMON_BOMBER);
            gameNetwork.HasDemonDestroyer = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.DEMON_DESTROYER);
            gameNetwork.HasDemonHealer = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.DEMON_HEALER);
            gameNetwork.HasDemonScout = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.DEMON_SCOUT);
            gameNetwork.HasDemonTank = checkTokenAvailable(response_hash, GameNetwork.TransactionKeys.DEMON_TANK);

            gameNetwork.CheckAllEnjinTeamBoostTokens(response_hash);

            JSONArray rewards = response_hash[NetworkManager.TransactionKeys.REWARDS] as JSONArray;
            
            foreach (JSONNode item in rewards) {
                Debug.Log(item["level"].ToString());
                int level = int.Parse(item["level"].ToString().Trim('"'));
                gameNetwork.rewardedLevels[level - 1] = 1;
            }
        }
        else
            print("User is not using Crypto.");

        if(enigmaHacks.EnjinLinked)
            GameNetwork.Instance.IsEnjinLinked = true;

        GameObject.Find("/Main").GetComponent<Main>().Init();
    }

    private bool checkTokenAvailable(SimpleJSON.JSONNode response_hash, string transactionKey)
    {
        string tokenAvailable = "";
        SimpleJSON.JSONNode tokenNode = response_hash[NetworkManager.TransactionKeys.USER_DATA][transactionKey];

        if (tokenNode != null)
        {
            tokenAvailable = tokenNode;
        }

        return (tokenAvailable == "1");
    }

    private void onLogin(SimpleJSON.JSONNode response)
    {
        if (response != null)
        {
            SimpleJSON.JSONNode response_hash = response[0];
            string status = response_hash[NetworkManager.TransactionKeys.STATUS].ToString().Trim('"');

            if (status == NetworkManager.StatusOptions.SUCCESS)
            {
                // print("onRegistration SUCCESS");

                if (_rememberMeToggle.isOn)
                {
                    ObscuredPrefs.SetString("username", _lastUsername);
                    ObscuredPrefs.SetString("password", _lastPassword);

                    ObscuredPrefs.SetInt("RememberMe", 1);
                }
                else
                {
                    ObscuredPrefs.DeleteKey("username");
                    ObscuredPrefs.DeleteKey("password");

                    ObscuredPrefs.DeleteKey("RememberMe");
                }

                ObscuredPrefs.Save();

                completeLogin(response_hash);
                closeDialog();
            }
            else
            {
                EnableLoginWindow();
                string term = "";

                if (status == NetworkManager.StatusOptions.ERR_REGISTER)
                    term = "Register Error";
                else if (status == NetworkManager.StatusOptions.ERR_INVALID_PASSWORD)
                    term = "Invalid Password";
                else if (status == NetworkManager.StatusOptions.ERR_INVALID_USERNAME)
                    term = "Invalid Username";
                else
                    term = "Server Error";

                alertText.color = failColor;
                setAlert(term);
            }
        }
        else
        {
            EnableLoginWindow();
            alertText.color = failColor;
            setAlert("Connection Error");
        }
    }

    private void setAlert(string term)
    {
        alertText.text = term;
        LocalizationManager.LocalizeText(alertText);
    }

    public void resetForm()
    {
        alertText.text = "";
        switchToLogin();
        loadingGroup.gameObject.SetActive(false);
    }

    
}
