using Enigma.CoreSystems;
using UnityEngine;

public class Main : MonoBehaviour
{
    void Start()
    {
        NetworkManager.SetServer("http://www.enigma-games.com/Minmins");
        NetworkManager.SetGame(Application.productName.ToLower());
    }
}
