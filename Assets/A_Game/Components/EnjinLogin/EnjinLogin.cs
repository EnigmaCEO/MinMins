
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Enigma.CoreSystems;
using CodeStage.AntiCheat.Storage;
using SimpleJSON;
using GameEnums;

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
    [SerializeField] private Main _main;

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
            GameSounds.Instance.PlayUiAdvanceSound();
        }
        else
        {
            GameSounds.Instance.PlayUiBackSound();
        }
    }

    public void closeButtonDown()
    {
        GameSounds.Instance.PlayUiBackSound();
        closeDialog();
    }

    public void closeDialog()
    {
        gameObject.SetActive(false);
    }

    public void switchToRegister()
    {
        GameSounds.Instance.PlayUiAdvanceSound();
        loginGroup.SetActive(false);
        registerGroup.SetActive(true);
    }

    public void switchToLogin()
    {
        GameSounds.Instance.PlayUiAdvanceSound();

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
            GameSounds.Instance.PlayUiBackSound();
        }
        else
        {
            GameSounds.Instance.PlayUiAdvanceSound();
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
        GameSounds.Instance.PlayUiAdvanceSound();
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
        GameSounds.Instance.PlayUiAdvanceSound();
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
                {
                    term = "Register Error";
                }
                else if (status == NetworkManager.StatusOptions.ERR_INVALID_PASSWORD)
                {
                    term = "Invalid Password";
                }
                else if (status == NetworkManager.StatusOptions.ERR_INVALID_USERNAME)
                {
                    term = "Invalid Username";
                }
                else
                {
                    term = "Server Error";
                }

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

    private void completeLogin(SimpleJSON.JSONNode response_hash)
    {
        if (response_hash == null)
        {
            Debug.LogError("Complete login response_hash is null.");
            return;
        }
        else
        {
            print("Complete login -> response_hash: " + response_hash.ToString());
        }

        GameStats gameStats = GameStats.Instance;
        GameNetwork gameNetwork = GameNetwork.Instance;
        GameHacks gameHacks = GameHacks.Instance;
        GameInventory gameInventory = GameInventory.Instance;

        JSONNode userDataNode = response_hash[NetworkManager.TransactionKeys.USER_DATA];
        if (userDataNode != null)
        {
            JSONNode hasPurchasedNode = userDataNode[NetworkManager.TransactionKeys.PURCHASED];
            gameStats.HasPurchased = ((hasPurchasedNode != null) && (hasPurchasedNode.AsInt == 1));

            JSONNode isThereServerBackupNode = userDataNode[GameNetwork.TransactionKeys.BACKUP];
            gameStats.IsThereServerBackup = ((isThereServerBackupNode != null) && (isThereServerBackupNode.AsInt == 1));

            JSONNode ratingNode = userDataNode[GameNetwork.TransactionKeys.RATING];
            if (ratingNode != null)
            {
                GameStats.Instance.Rating = ratingNode.AsInt;
            }

            JSONNode userNameNode = userDataNode[GameNetwork.TransactionKeys.USERNAME];
            if (userNameNode != null)
            {
                NetworkManager.SetLocalPlayerNickName(userNameNode.ToString().Trim('"'));
            }
        }

        string enjinId = "null";
        if (userDataNode != null)
        {
            JSONNode enjinIdNode = userDataNode[NetworkManager.TransactionKeys.ENJIN_ID];
            if (enjinIdNode != null)
            {
                enjinId = enjinIdNode.ToString().Trim('"');
            }
        }

        Debug.Log("Enjin ID: " + enjinId);
        bool enjinIdNull = (enjinId == "null");

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        EnigmaHacks enigmaHacks = EnigmaHacks.Instance;
        if (enigmaHacks.EnjinIdIsNull.Enabled)
        {
            enjinIdNull = enigmaHacks.EnjinIdIsNull.ValueAsBool;
        }
#endif

        if (!enjinIdNull)
        {
            string enjinCode = "null";
            if (userDataNode != null)
            {
                JSONNode enjinCodeNode = userDataNode[NetworkManager.TransactionKeys.ENJIN_CODE];
                if (enjinCodeNode != null)
                {
                    enjinCode = enjinCodeNode.ToString().Trim('"');
                }
            }

            Debug.Log("Link Code: " + enjinCode);

            bool enjinCodeNull = (enjinCode == "null");

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (enigmaHacks.EnjinCodeIsNull.Enabled)
            {
                enjinCodeNull = enigmaHacks.EnjinIdIsNull.ValueAsBool;
            }
#endif
            if (!enjinCodeNull)
            {
                _main.StartEnjinQR(enjinCode);
            }
            else
            {
                print("User has already linked with Enjin");
                gameNetwork.IsEnjinLinked = true;
            }

            gameNetwork.UpdateEnjinGoodies(response_hash);

            JSONNode trainingRewardsNode = response_hash[NetworkManager.TransactionKeys.REWARDS];
            if (trainingRewardsNode != null)
            {
                JSONArray trainingRewards = trainingRewardsNode as JSONArray;

                foreach (JSONNode item in trainingRewards)
                {
                    JSONNode itemNode = item["level"];
                    
                    if (itemNode != null)
                    {
                        string itemString = itemNode.ToString();
                        Debug.Log("training rewards: " + itemString);
                        int level = int.Parse(itemString.Trim('"'));
                        gameNetwork.rewardedTrainingLevels[level - 1] = 1;
                    }
                }
            }
        }
        else
        {
            print("User is not using Crypto.");
        }

        //#if DEVELOPMENT_BUILD || UNITY_EDITOR
        //        if (enigmaHacks.EnjinLinked)
        //        {
        //            GameNetwork.Instance.IsEnjinLinked = true;
        //        }
        //#endif

        //GameObject.Find("/Main").GetComponent<Main>().Init();
        _main.Init();
    }

    private void onLogin(SimpleJSON.JSONNode response)
    {
        if (response != null)
        {
            Debug.Log("onLogin: " + response.ToString());

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
