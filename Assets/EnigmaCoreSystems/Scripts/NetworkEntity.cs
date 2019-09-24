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

        public bool ReadyInScene { get; protected set; }


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

        protected void StartReadyOnSceneCheck(string sceneObjectToFind)
        {
            StartCoroutine(checkReadyOnSceneCheck(sceneObjectToFind));
        }

        private IEnumerator checkReadyOnSceneCheck(string sceneObjectToFind)
        {
            while (!ReadyInScene)
            {
                GameObject sceneObject = GameObject.Find(sceneObjectToFind);
                if (sceneObject != null)
                    onReadyInScene(sceneObject);

                yield return new WaitForSeconds(_readyOnSceneCheckDelay);
            }
        }

        protected virtual void onReadyInScene(GameObject sceneObjectFound)
        {
            Debug.LogWarning("NewtorkEntity::onReadyInScene -> name: " + this.name);
            ReadyInScene = true;
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
