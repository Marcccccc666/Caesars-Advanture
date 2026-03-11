using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GameObjectPool<T> : IGameObjectPool, IMyPool
    where T : Component
{
    private IObjectPool<T> pool;
    private T prefab;
    private Transform poolRoot;

    private readonly HashSet<T> activeObjects = new();

    public int ActiveCount => activeObjects.Count;

    public int InactiveCount => pool.CountInactive;

    public GameObjectPool(T prefab, int defaultCapacity = 10, int maxSize = 100)
    {
        this.prefab = prefab;

        GameObject root = new GameObject($"{typeof(T).Name}_Pool");
        root.transform.SetParent(PoolManager.Instance.transform);
        poolRoot = root.transform;

        pool = new ObjectPool<T>(
            Create,
            OnGet,
            OnRelease,
            OnDestroy,
            true,
            defaultCapacity,
            maxSize
        );
    }

    private T Create()
    {
        T obj = Object.Instantiate(prefab, poolRoot);

        if (obj is IPoolable poolable)
        {
            poolable.SetPool(this);
        }

        obj.gameObject.SetActive(false);
        return obj;
    }

    private void OnGet(T obj)
    {
        activeObjects.Add(obj);
    }

    private void OnRelease(T obj)
    {
        activeObjects.Remove(obj);
        obj.transform.SetParent(poolRoot);
        obj.gameObject.SetActive(false);
    }

    private void OnDestroy(T obj)
    {
        if(obj != null)
        {
            Object.Destroy(obj.gameObject);
        }
    }

    public T Get()
    {
        return pool.Get();
    }

    public void Release(Component obj)
    {
        if (obj is T t)
        {
            pool.Release(t);
        }
        else
        {
            Debug.LogError($"类型错误: {obj.GetType()} 不能回收到 {typeof(T)} 池");
        }
    }

    public void ReleaseAll()
    {
        foreach (var obj in new List<T>(activeObjects))
        {
            pool.Release(obj);
        }
    }

    public void Clear()
    {
        pool.Clear();
    }

    /// <summary>
    /// 从池中移除特定的实力，并销毁它（适用于不再需要回收的对象）
    /// <para>有时用来整理池中是否有空对象，或者强制销毁某个对象而不是回收</para>
    /// </summary>
    public void RemoveAndDestroy(T obj)
    {
        if (activeObjects.Contains(obj))
        {
            activeObjects.Remove(obj);
            if(obj != null)
            {
                pool.Release(obj); // 先回收，触发OnRelease逻辑
                Object.Destroy(obj.gameObject);
            }
        }
        else
        {
            Debug.LogWarning($"对象 {obj.name} 不在池中，无法移除和销毁");
        }
    }
}