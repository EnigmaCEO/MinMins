using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.CoreSystems
{
    public class NetworkEntity : MonoBehaviour
    {
        private PhotonView _photonView;

        protected virtual void Awake()
        {
            _photonView = GetComponent<PhotonView>();
        }

        public void SendRpcAll(string methodName, params object[] parameters)
        {
            _photonView.RPC(methodName, PhotonTargets.All, parameters);
        }
    }
}
