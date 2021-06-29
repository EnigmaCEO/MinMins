using UnityEngine;

public abstract class SingletonPrefab<T> : MonoBehaviour where T : SingletonPrefab<T>
{
    private static T _instance = null;

    static public bool HasInstance
    {
        get
        {
            return (_instance != null);
        }
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
                    GameObject singletonPrefab = Resources.Load<GameObject>("Prefabs/SingletonPrefabs/" + instanceName);

                    if (singletonPrefab != null)
                    {
                        GameObject instanceObject = Instantiate(singletonPrefab);
                        instanceObject.name = instanceName;
                        Debug.LogWarning("singletonPrefab: " + instanceName + " was created.");

                        _instance = instanceObject.GetComponent<T>();

                        _instance.SetUpWithCreationPrefab(singletonPrefab);
                    }
                    else
                    {
                        Debug.LogError("Prefab for singleton " + instanceName + " was not found.");
                    }
                }
            }
            return _instance;
        }
    }

    virtual public void Prepare()
    {
        Debug.Log(Instance.name + "::Prepare -> Instance ready: " + Instance);
    }

    virtual public void Dispose()
    {
        Destroy(_instance.gameObject);
        _instance = null;
    }

    virtual protected void SetUpWithCreationPrefab(GameObject prefab)
    {
        transform.position = prefab.transform.position;
    }
}
