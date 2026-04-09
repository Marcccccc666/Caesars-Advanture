using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    protected static T instance;
    protected static bool isQuitting = false;

    protected virtual bool DontDestroyOnLoadCreated => true;

    public static T Instance
    {
        get
        {
            if (isQuitting)
            {
                return null;
            }

            if (instance == null)
            {
                instance = FindAnyObjectByType<T>();

                if (instance == null)
                {
                    GameObject singletonObject = new(typeof(T).Name + " (Singleton)");
                    instance = singletonObject.AddComponent<T>();

                    if (instance.DontDestroyOnLoadCreated)
                    {
                        DontDestroyOnLoad(singletonObject);
                    }
                }
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        isQuitting = false;

        if (instance == null)
        {
            instance = this as T;

            if (DontDestroyOnLoadCreated)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnEnable()
    {
        if (GameManager.Instance != null && instance == this)
        {
            GameManager.Instance.GameResetAction += OnRest;
        }
    }

    protected virtual void OnDisable()
    {
        if (GameManager.Instance != null && instance == this)
        {
            GameManager.Instance.GameResetAction -= OnRest;
        }
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameResetAction -= OnRest;
            }
        }
    }

    protected virtual void OnApplicationQuit()
    {
        isQuitting = true;
    }

    protected virtual void OnRest()
    {
    }
}