using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<Component, IGameObjectPool> pools = new();

    private GameObjectPool<T> GetOrCreatePool<T>(T prefab, int defaultCapacity = 20, int maxSize = 100)
        where T : Component
    {
        if (!pools.TryGetValue(prefab, out var poolObj))
        {
            var newPool = new GameObjectPool<T>(prefab, defaultCapacity, maxSize);
            pools.Add(prefab, newPool);
            return newPool;
        }

        return poolObj as GameObjectPool<T>;
    }


    public T Spawn<T>(
        T prefab,
        Vector3 position,
        Quaternion rotation,
        int defaultCapacity = 20,
        int maxSize = 100,
        bool autoActive = true,
        Transform parent = null) where T : Component
    {
        var pool = GetOrCreatePool(prefab, defaultCapacity, maxSize);

        var obj = pool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);

        if (parent != null)
            obj.transform.SetParent(parent);

        if (autoActive)
            pool.Activate(obj);

        return obj;
    }

    public void Activate<T>(T prefab, T obj) where T : Component
    {
        if (obj == null) return;
        var pool = GetOrCreatePool(prefab);
        pool.Activate(obj);
    }

    public void Release<T>(T prefab, T obj) where T : Component
    {
        if (obj == null) return;

        var pool = GetOrCreatePool(prefab);
        pool.Release(obj);
    }

    /// <summary>
    /// 🔥 回收某一个池
    /// </summary>
    /// <param name="prefab"> 要回收的池的预制体 </param>
    public void ReleasePool<T>(T prefab) where T : Component
    {
        if (pools.TryGetValue(prefab, out var poolObj))
        {
            poolObj.ReleaseAll();
        }
        else
        {
            Debug.LogWarning($"没有找到预制体 {prefab.name} 对应的对象池");
            return;
        }
    }

    /// <summary>
    /// 🔥 回收所有池
    /// </summary>
    public void ReleaseAllPools()
    {
        foreach (var pool in pools.Values)
        {
            pool.ReleaseAll();
        }
    }

    /// <summary>
    /// 🔥 清空所有池
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var pool in pools.Values)
        {
            pool.Clear();
        }

        pools.Clear();
    }

    
}