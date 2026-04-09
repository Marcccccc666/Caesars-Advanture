using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    private static bool isQuitting = false;

    /// <summary>
    /// 是否在创建实例时调用DontDestroyOnLoad
    /// </summary>
    protected virtual bool DontDestroyOnLoadCreated => true;

    public static T Instance
    {   
        get
        {
            if(isQuitting)
            {
                return null;
            }
            
            if (instance == null)
            {
                // 先尝试在场景中查找
                instance = FindAnyObjectByType<T>();

                // 如果场景中也没有，就自动创建一个
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
        // 如果还没有实例，就把自己注册为单例
        if (Instance == null)
        {
            instance = this as T;

            if (DontDestroyOnLoadCreated)
            {
                DontDestroyOnLoad(this);
            }
        }
        // 如果已经存在并且不是自己 → 删除自己
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        
    }

    
    protected virtual void OnEnable()
    {
        GameManager.Instance.GameResetAction += OnRest;
    }

    protected virtual void OnDisable()
    {
        if (Instance == this)
        {
            isQuitting = true;
        }

        if(GameManager.Instance)
        {
            GameManager.Instance.GameResetAction -= OnRest;
        }
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            isQuitting = true;
        }

        if(GameManager.Instance)
        {
            GameManager.Instance.GameResetAction -= OnRest;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        if (Instance == this)
        {
            isQuitting = true;
        }
    }

    protected virtual void OnRest()
    {
        
    }

}