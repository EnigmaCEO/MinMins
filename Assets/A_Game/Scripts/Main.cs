using Enigma.CoreSystems;
using UnityEngine;

public class Main : EnigmaScene
{
    [SerializeField] GameObject _notEnoughUnitsPopUp;
    [SerializeField] GameObject _loginModal;

    void Start()
    {
        NetworkManager.Disconnect();
        _notEnoughUnitsPopUp.SetActive(false);
        _loginModal.SetActive(false);
    }

    public void OnSinglePlayerButtonDown()
    {
        print("OnSinglePlayerButtonDown");
        GameStats.Instance.Mode = GameStats.Modes.SinglePlayer;
        TryGoToLevels();
    }

    public void OnPvpButtonDown()
    {
        print("OnPvpButtonDown");
        GameStats.Instance.Mode = GameStats.Modes.Pvp;
        TryGoToLevels();
    }

    public void OnStoreButtonDown()
    {
        print("OnStoreButtonDown");
        SceneManager.LoadScene(GameConstants.Scenes.STORE);
    }

    public void OnNotEnoughUnitsPopUpDismissButtonDown()
    {
        _notEnoughUnitsPopUp.SetActive(false);
        SceneManager.LoadScene(GameConstants.Scenes.STORE);
    }

    public void ShowLoginForm()
    {
        _loginModal.SetActive(true);
    }

    private void TryGoToLevels()
    {
        if (GameInventory.Instance.HasEnoughUnitsForBattle())
            SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
        else
            _notEnoughUnitsPopUp.SetActive(true);
    }
}
