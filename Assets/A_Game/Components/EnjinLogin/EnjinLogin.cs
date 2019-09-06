
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Enigma.CoreSystems;

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

    public void closeDialog() {
        gameObject.SetActive(false);
    }

    public void switchToRegister() {
        loginGroup.SetActive(false);
        registerGroup.SetActive(true);
    }

    public void switchToLogin() {
        loginGroup.SetActive(true);
        registerGroup.SetActive(false);
    }

    public void switchETHField() {
        ETHField.gameObject.SetActive(!ETHField.gameObject.activeSelf);
    }

    public void validateRegisterData() {
        alertText.text = "";
        bool valid = registerUsername.text != "" && registerEmail.text != "" && registerPassword.text != "" && registerConfirm.text != "";
        bool reason;

        if (registerEmail.text != "") {
            reason = registerEmail.text.IndexOf('@') > 0;
            valid = valid && reason;
            if (!reason) alertText.text = "E-mail is not valid";
        }
        if (registerPassword.text != "" && registerConfirm.text != "") {
            reason = registerPassword.text == registerConfirm.text;
            valid = valid && reason;
            if (!reason) alertText.text = "Passwords don't match";
        }
        if (ETHField.gameObject.activeSelf) {
            reason = ETHField.text != "";
            valid = valid && reason;
            if (!reason) alertText.text = "Please fill the Enjin ETH address";
        }
        if (valid) {
            alertText.text = "";
        }

        registerButton.interactable = valid;
        alertText.color = registerButton.interactable ? successColor : failColor;
        alertText.text = registerButton.interactable ? "" : alertText.text;
    }

    public void validateLoginData() {
        alertText.text = "";
        loginButton.interactable = loginUsername.text != "" && loginPassword.text != "";
        alertText.color = loginButton.interactable ? successColor : failColor;
        alertText.text = loginButton.interactable ? "" : alertText.text;
    }

    public void registerSubmit() {
        showLoadingScreen();

        Hashtable extras = new Hashtable();

        string ipAddress = NetworkManager.Ip; 
        string country = NetworkManager.Country;  
        extras.Add("ip", ipAddress);
        extras.Add("country", country);

        NetworkManager.Register(
            registerUsername.text,
            registerPassword.text,
            registerEmail.text,
            ETHField.text,
            ShalwendConfigs.TRANSACTION_GAME_NAME,
            Application.productName.ToLower(),
            onRegistration,
            extras
        );
    }

    public void loginSubmit() {
        showLoadingScreen();

        Hashtable extras = new Hashtable();
        string ipAddress = NetworkManager.Ip; 
        string country = NetworkManager.Country;  
        extras.Add("ip", ipAddress);
        extras.Add("country", country);

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
        if (response != null) {
        //     print("onRegistration: " + response.ToString());

            SimpleJSON.JSONNode response_hash = response[0];
            string status = response_hash[NetworkManager.TransactionKeys.STATUS].ToString().Trim('"');

            if (status == NetworkManager.StatusOptions.SUCCESS) {
                // print("onRegistration SUCCESS");
                // completeLogin(response_hash);
                closeDialog();
            }
            else {
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
                alertText.text = term;
            }
        } else {
            EnableLoginWindow();
            alertText.color = failColor;
            alertText.text = "Connection Error";
        }
    }

    private void completeLogin(SimpleJSON.JSONNode response_hash)
    {
        print("Complete login:");
        GameStats.Instance.Rating = response_hash[NetworkManager.TransactionKeys.USER_DATA][GameNetwork.TransactionKeys.RATING].AsInt;
        //GameNetwork.SetLocalPlayerRating(rating, GameNetwork.VirtualPlayerIds.HOST);

        string enjinId = response_hash[NetworkManager.TransactionKeys.USER_DATA][NetworkManager.EnjinTransKeys.ENJIN_ID].ToString().Trim('"');
        //if(true)  //hack
        if (enjinId != "null")
        {
            string enjinCode = response_hash[NetworkManager.TransactionKeys.USER_DATA][NetworkManager.EnjinTransKeys.ENJIN_CODE].ToString().Trim('"');
            //if(true)  //hack
            if (enjinCode != "null")
            {
                //_enjinWindow.SetActive(true);
                //_enjinWindow.GetComponent<EnjinQRManager>().ShowImage(enjinCode);

                //StartCoroutine(handleEnjinLinkingCheck(0));
            }
            else
            {
                print("User has already linked with Enjin");
                GameStats.Instance.IsEnjinLinked = true;
            }
        }
        else
            print("User is not using Crypto.");

        GameStats.Instance.IsEnjinLinked = true; //hack
    }

    private void onLogin(SimpleJSON.JSONNode response)
    {
        if (response != null) {

            SimpleJSON.JSONNode response_hash = response[0];
            string status = response_hash[NetworkManager.TransactionKeys.STATUS].ToString().Trim('"');

            if (status == NetworkManager.StatusOptions.SUCCESS) {
                // print("onRegistration SUCCESS");
                // completeLogin(response_hash);
                closeDialog();
            }
            else {
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
                alertText.text = term;
            }
        } else {
            EnableLoginWindow();
            alertText.color = failColor;
            alertText.text = "Connection Error";
        }
    }
}
