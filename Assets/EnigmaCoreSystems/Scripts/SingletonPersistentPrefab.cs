using UnityEngine;

public abstract class SingletonPersistentPrefab<T> : SingletonPrefab<T> where T : SingletonPersistentPrefab<T>
{
    virtual protected void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
