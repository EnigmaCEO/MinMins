using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enigma.CoreSystems
{
    [RequireComponent(typeof(PhotonView))]
    public class NetworkEntity : MonoBehaviour
    {
        [SerializeField] private float _readyOnSceneCheckDelay = 1;
        private PhotonView _photonView;

        public delegate void CheckReadyOnSceneDelegate(GameObject objectToFind);
        static public CheckReadyOnSceneDelegate OnReadyInSceneCallback;

        private string _sceneObjectToFind = "";

        public bool RequiresSceneReady { get; protected set; }


        protected virtual void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            if(!_photonView.ObservedComponents.Contains(this))
                _photonView.ObservedComponents.Add(this);

            RequiresSceneReady = false;
        }

        protected virtual void Update()
        {
            if (RequiresSceneReady) 
            {
                if (_sceneObjectToFind != "")
                {
                    GameObject sceneObject = GameObject.Find(_sceneObjectToFind);
                    if (sceneObject != null)
                        onReadyInScene(sceneObject);
                    else
                        Debug.LogWarning("NetworkEntity::LateUpdate -> sceneObjectToFind: " + _sceneObjectToFind + " not yet found by: " + this.name);
                }
            }
        }

        protected virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //For now to prevent photon error when observing component
        }

        protected void StartReadyOnSceneCheck(string sceneObjectToFind)
        {
            //Debug.LogWarning("NetworkEntity::StartReadyOnSceneCheck -> name: " + this.name + " sceneObjectToFind: " + sceneObjectToFind);
            _sceneObjectToFind = sceneObjectToFind;
            RequiresSceneReady = true;
        }

        protected virtual void onReadyInScene(GameObject sceneObjectFound)
        {
            //Debug.LogWarning("NewtorkEntity::onReadyInScene -> name: " + this.name);
            RequiresSceneReady = false;
        }

        protected void setNetworkViewId(int id)
        {
            _photonView.viewID = id;
        }

        protected object[] GetInstantiationData()
        {
            return _photonView.instantiationData;
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
