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
        GameStats.Instance.Mode = GameStats.Modes.SinglePlayer;
        GoToLevels();
    }

    public void OnPvpButtonDown()
    {
        GameStats.Instance.Mode = GameStats.Modes.Pvp;
        GoToLevels();
    }

    private void GoToLevels()
    {
        SceneManager.LoadScene(GameConstants.Scenes.LEVELS);
    }
}
