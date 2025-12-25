using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    /// <summary>
    /// Determines whether the singleton should persist between scenes.
    /// Override this in derived classes if needed.
    /// </summary>
    protected virtual bool DontDestroyOnLoadEnabled => true;

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                    {
                        GameObject singletonObj = new GameObject(typeof(T).Name);
                        _instance = singletonObj.AddComponent<T>();
                    }

                    if (_instance is Singleton<T> singleton && singleton.DontDestroyOnLoadEnabled)
                    {
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;

            if (DontDestroyOnLoadEnabled)
                DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            // Replace the old instance with the new one (optional)
            Destroy(_instance.gameObject); // Clean up the old one
            _instance = this as T;         // Take over

            if (DontDestroyOnLoadEnabled)
                DontDestroyOnLoad(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        
    }

    private void OnDestroy()
    {
        
    }
}
