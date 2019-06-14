using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnigmaCoreSystemsSetup : MonoBehaviour
{
    [SerializeField] private bool _adsManager = true;
    [SerializeField] private bool _shopManager = true;
    [SerializeField] private bool _networkManager = true;
    [SerializeField] private bool _iAPManager = true;
    [SerializeField] private bool _localizationManager = true;
    [SerializeField] private bool _fileManager = true;
    [SerializeField] private bool _inputManager = true;
    [SerializeField] private bool _ratingManager = true;
    [SerializeField] private bool _sceneManager = true;
    [SerializeField] private bool _soundManager = true;

    [SerializeField] private string _systemsPrefabsResourcesFolder = "Prefabs/Systems";
    [SerializeField] private string _systemsContainerName = "Enigma.CoreSystems";

    [SerializeField] private string _adsManagerPrefabName = "AdsManager";
    [SerializeField] private string _shopManagerPrefabName = "ShopManager";
    [SerializeField] private string _networkManagerPrefabName = "NetworkManager";
    [SerializeField] private string _iAPManagerPrefabName = "IAPManager";
    [SerializeField] private string _localizeManagerPrefabName = "LocalizationManager";
    [SerializeField] private string _fileManagerPrefabName = "FileManager";
    [SerializeField] private string _inputManagerPrefabName = "InputManager";
    [SerializeField] private string _ratingManagerPrefabName = "RatingManager";
    [SerializeField] private string _sceneManagerPrefabName = "SceneManager";
    [SerializeField] private string _soundManagerPrefabName = "SoundManager";

    private Transform _systemsContainer;

    void Awake ()
    {
        GameObject systemsContainerGameObject = GameObject.Find(_systemsContainerName);

        if (systemsContainerGameObject == null)
        {
            systemsContainerGameObject = new GameObject(_systemsContainerName);
            GameObject.DontDestroyOnLoad(systemsContainerGameObject);
        }

        _systemsContainer = systemsContainerGameObject.transform;
        checkAllSystems();

        if (_systemsContainer.childCount == 0)
            Destroy(systemsContainerGameObject);
	}

    private void checkAllSystems()
    {
        checkSystem(_adsManager, _adsManagerPrefabName);
        checkSystem(_shopManager, _shopManagerPrefabName);
        checkSystem(_networkManager, _networkManagerPrefabName);
        checkSystem(_iAPManager, _iAPManagerPrefabName);
        checkSystem(_localizationManager, _localizeManagerPrefabName);
        checkSystem(_fileManager, _fileManagerPrefabName);
        checkSystem(_inputManager, _inputManagerPrefabName);
        checkSystem(_ratingManager, _ratingManagerPrefabName);
        checkSystem(_sceneManager, _sceneManagerPrefabName);
        checkSystem(_soundManager, _soundManagerPrefabName);
    }

    private void checkSystem(bool systemFlag, string systemPrefabName)
    {
        string systemFinalName = "Enigma.CoreSystems." + systemPrefabName;
        GameObject system = GameObject.Find(systemFinalName);

        if (systemFlag == true)
        {
            if (system == null)
                system = Instantiate<GameObject>(Resources.Load<GameObject>(_systemsPrefabsResourcesFolder + "/" + systemPrefabName));
                
            system.name = systemFinalName;
            system.transform.parent = _systemsContainer;
        }
        else
            Destroy(system);
    }
}
