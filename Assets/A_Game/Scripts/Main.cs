using Enigma.CoreSystems;
using UnityEngine;

public class Main : MonoBehaviour
{
    [SerializeField] GameObject _notEnoughUnitsPopUp;

    void Start()
    {
        NetworkManager.SetServer("http://www.enigma-games.com/Minmins");
        NetworkManager.SetGame(Application.productName.ToLower());

        _notEnoughUnitsPopUp.SetActive(false);
    }

    public void OnSinglePlayerButtonDown()
    {
        print("OnSinglePlayerButtonDown");
        GameStats.Instance.Mode = GameStats.Modes.SinglePlayer;
        NetworkManager.Connect(true);
        GoToLevels();
    }

    public void OnPvpButtonDown()
    {
        print("OnPvpButtonDown");
        GameStats.Instance.Mode = GameStats.Modes.Pvp;
        GoToLevels();
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

    private void GoToLevels()
    {
        if (GameInventory.Instance.HasEnoughUnitsForBattle())
            SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
        else
            _notEnoughUnitsPopUp.SetActive(true);
    }
}
