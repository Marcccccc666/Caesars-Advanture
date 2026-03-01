using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    private static bool isQuitting = false;

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
                    GameObject singletonObject = new GameObject(typeof(T).Name + " (Singleton)");
                    instance = singletonObject.AddComponent<T>();
                    DontDestroyOnLoad(singletonObject);
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

        }
        // 如果已经存在并且不是自己 → 删除自己
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this.gameObject);

        OnAwake();
    }


    protected virtual void OnAwake() { return; }

    protected virtual void OnDestroy()
    {
        isQuitting = true;
    }

    protected virtual void OnApplicationQuit()
    {
        isQuitting = true;
    }

}