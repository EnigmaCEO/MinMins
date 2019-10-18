using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.CoreSystems
{
    public abstract class EnigmaScene : MonoBehaviour
    {
        // Start is called before the first frame update
        public virtual void Awake()
        {
            NetworkManager.SetServer("https://www.enigma-games.com/MinMins");
            NetworkManager.SetGame(Application.productName.ToLower());
            Debug.Log(NetworkManager.GetGame());
        }
    }
}
