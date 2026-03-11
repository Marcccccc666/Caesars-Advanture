using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<Component, IGameObjectPool> pools = new();


    public GameObjectPool<T> GetOrCreatePool<T>(T prefab, int defaultCapacity = 10, int maxSize = 100)
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

    public T Spawn<T>(T prefab, Vector3 pos, Quaternion rot, bool setActive = true,Transform parent = null, int defaultCapacity = 10, int maxSize = 100)
        where T : Component
    {
        var pool = GetOrCreatePool(prefab, defaultCapacity, maxSize);
        var obj = pool.Get();

        obj.transform.SetPositionAndRotation(pos, rot);

        if (parent != null)
            obj.transform.SetParent(parent);

        obj.gameObject.SetActive(setActive);
        return obj;
    }

    /// <summary>
    /// 回收指定Profab的所有实例
    /// </summary>
    public void ReleasePool<T>(T prefab)
        where T : Component
    {
        if (pools.TryGetValue(prefab, out var poolObj))
        {
            poolObj.ReleaseAll();
        }
    }

    /// <summary>
    /// 回收指定实例
    /// </summary>
    public void Release<T>(T profab, T obj)
        where T : Component
    {
        if (pools.TryGetValue(profab, out var poolObj))
        {
            if(poolObj is IMyPool myPool)
            {
                myPool.Release(obj);
            }
        }
    }
}