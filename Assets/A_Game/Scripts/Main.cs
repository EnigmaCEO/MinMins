using Enigma.CoreSystems;
using UnityEngine;

public class Main : EnigmaScene
{
    [SerializeField] GameObject _loginModal;

    void Start()
    {
        NetworkManager.Disconnect();
        _loginModal.SetActive(false);
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
        GameStats.Instance.Mode = GameStats.Modes.Pvp;
        goToLevels();
    }

    public void OnStoreButtonDown()
    {
        print("OnStoreButtonDown");
        SceneManager.LoadScene(GameConstants.Scenes.STORE);
    }

    public void ShowLoginForm()
    {
        _loginModal.SetActive(true);
    }

    private void goToLevels()
    {
        SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
    }
}
