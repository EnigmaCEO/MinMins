
using UnityEngine;
using UnityEngine.UI;

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
    }

    public void loginSubmit() {
        showLoadingScreen();
    }

    private void showLoadingScreen() {
        alertText.text = "";
        loginGroup.SetActive(false);
        registerGroup.SetActive(false);
        loadingGroup.SetActive(true);
    }
}
