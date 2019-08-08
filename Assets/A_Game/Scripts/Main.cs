using Enigma.CoreSystems;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        NetworkManager.SetServer("http://www.enigma-games.com/Minmins");
        NetworkManager.SetGame(Application.productName.ToLower());
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
        NetworkManager.Connect(false);
        GoToLevels();
    }

    private void GoToLevels()
    {
        SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
    }
}
