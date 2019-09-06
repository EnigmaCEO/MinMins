using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.CoreSystems
{
    [RequireComponent(typeof(PhotonView))]
    public class NetworkEntity : MonoBehaviour
    {
        private PhotonView _photonView;

        protected virtual void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            if(!_photonView.ObservedComponents.Contains(this))
                _photonView.ObservedComponents.Add(this);
        }

        protected virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //For now to prevent photon error when observing component
        }

        protected void setNetworkViewId(int id)
        {
            _photonView.viewID = id;
        }

        public void SendRpcToAll(string methodName, params object[] parameters)
        {
            _photonView.RPC(methodName, PhotonTargets.All, parameters);
        }

        public void SendRpcToMasterClient(string methodName, params object[] parameters)
        {
            _photonView.RPC(methodName, PhotonTargets.MasterClient, parameters);
        }
    }
}
