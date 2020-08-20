using UnityEngine;

public abstract class SingletonMonobehaviour<T> : MonoBehaviour where T : SingletonMonobehaviour<T>
{
    public bool PersistBetweenScenes = false;
    private static T _instance = null;

    public static bool HasInstance()
    {
        return (_instance != null);
    }


    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType(typeof(T)) as T;
                if (_instance == null)
                {
                    string instanceName = typeof(T).ToString();
                    GameObject instanceObject = Instantiate(Resources.Load<GameObject>("Prefabs/SingletonMonobehaviours/" + instanceName));
                    instanceObject.name = instanceName;
                    _instance = instanceObject.GetComponent<T>();
                    if (_instance.PersistBetweenScenes)
                        DontDestroyOnLoad(instanceObject);
                }

            }
            return _instance;
        }
    }
}
