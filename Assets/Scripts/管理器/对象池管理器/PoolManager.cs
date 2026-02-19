using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<Component, object> pools = new();

    private GameObjectPool<T> GetOrCreatePool<T>(T prefab, int defaultCapacity = 20, int maxSize = 100)
        where T : Component
    {
        if (!pools.TryGetValue(prefab, out var poolObj))
        {
            var newPool = new GameObjectPool<T>(prefab, defaultCapacity, maxSize);
            pools.Add(prefab, newPool);
            poolObj = newPool;
        }

        return poolObj as GameObjectPool<T>;
    }

    /// 🔹 只获取（不激活）
    public T Get<T>(T prefab) where T : Component
    {
        var pool = GetOrCreatePool(prefab);
        return pool.Get();
    }

    /// 🔹 手动激活
    public void Activate<T>(T prefab, T obj) where T : Component
    {
        var pool = GetOrCreatePool(prefab);
        pool.Activate(obj);
    }

    /// 🔹 快捷生成（旧模式）
    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, int defaultCapacity = 20, int maxSize = 100, bool autoActive = true) where T : Component
    {
        var pool = GetOrCreatePool(prefab, defaultCapacity, maxSize);

        var obj = pool.Get();
        obj.transform.SetPositionAndRotation(position, rotation);

        if (autoActive)
        {
            pool.Activate(obj);
        }

        return obj;
    }

    public void Release<T>(T prefab, T obj) where T : Component
    {
        var pool = GetOrCreatePool(prefab);
        pool.Release(obj);
    }
}
