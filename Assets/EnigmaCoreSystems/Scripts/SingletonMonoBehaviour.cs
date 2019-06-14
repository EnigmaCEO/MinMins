using UnityEngine;

public abstract class SingletonMonobehaviour<T> : MonoBehaviour where T : SingletonMonobehaviour<T>
{

    private static T _instance = null;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType(typeof(T)) as T;
                if (_instance == null)
                {
                    GameObject instanceObject = Instantiate(Resources.Load("Prefabs/SingletonMonobehaviours/QuizConfig", typeof(GameObject))) as GameObject;
                    _instance = instanceObject.GetComponent<T>(); 
                    _instance.OnAwake();
                }

            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this as T;
    }

    public virtual void OnAwake() { }


    private void OnApplicationQuit()
    {
        _instance = null;
    }
}
