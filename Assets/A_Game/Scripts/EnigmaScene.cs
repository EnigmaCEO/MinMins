using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.CoreSystems
{
    public abstract class EnigmaScene : MonoBehaviour
    {
        public virtual void Awake()
        {
            NetworkManager.SetServer("https://www.enigma-games.com/MinMins");
            NetworkManager.SetGame(Application.productName.ToLower());
            Debug.Log(NetworkManager.GetGame());

            if (SoundManager.Instance)
            {
                SoundManager.SetMainAudioListener();
            }
        }
    }
}
