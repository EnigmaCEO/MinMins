using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.CoreSystems {
    public abstract class EnigmaScene : MonoBehaviour
    {
        // Start is called before the first frame update
        public virtual void Awake()
        {
            NetworkManager.SetServer("http://www.enigma-games.com/Shalwend");
            NetworkManager.SetGame(Application.productName.ToLower());
            Debug.Log(NetworkManager.GetGame());
        }
    }
}
