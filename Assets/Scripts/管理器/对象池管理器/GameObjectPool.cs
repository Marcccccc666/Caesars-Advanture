using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GameObjectPool<T> : IGameObjectPool where T : Component
{
    private IObjectPool<T> pool;
    private T prefab;
    private Transform poolRoot;

    private readonly HashSet<T> allObjects = new();
    private readonly HashSet<T> activeObjects = new();

    private PoolManager poolManager => PoolManager.Instance;

    public int ActiveCount => activeObjects.Count;
    public int InactiveCount => allObjects.Count - activeObjects.Count;

    public GameObjectPool(T prefab, int defaultCapacity = 20, int maxSize = 100, bool collectionCheck = true)
    {
        this.prefab = prefab;

        GameObject rootGo = new GameObject($"{typeof(T).Name}_Pool");
        rootGo.transform.SetParent(poolManager.transform);
        poolRoot = rootGo.transform;

        pool = new ObjectPool<T>(
            Create,
            OnGet,
            OnRelease,
            OnDestroy,
            collectionCheck,
            defaultCapacity,
            maxSize
        );
    }

    private T Create()
    {
        T obj = Object.Instantiate(prefab, poolRoot);

        allObjects.Add(obj);

        if (obj is IPoolable<T> poolable)
        {
            poolable.SetPool(pool);
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
        if (obj == null) return;

        activeObjects.Remove(obj);

        obj.transform.SetParent(poolRoot);
        obj.gameObject.SetActive(false);
    }

    private void OnDestroy(T obj)
    {
        if (obj == null) return;

        allObjects.Remove(obj);
        activeObjects.Remove(obj);

        Object.Destroy(obj.gameObject);
    }

    public T Get()
    {
        return pool.Get();
    }

    public void Activate(T obj)
    {
        if (obj == null) return;

        obj.gameObject.SetActive(true);
    }

    public T Spawn(Vector3 pos, Quaternion rot, Transform parent = null)
    {
        var obj = Get();
        obj.transform.SetPositionAndRotation(pos, rot);

        if (parent != null)
            obj.transform.SetParent(parent);

        Activate(obj);
        return obj;
    }

    public void Release(T obj)
    {
        if (obj == null) return;

        pool.Release(obj);
    }

    // 🔥 回收当前池所有活跃对象
    public void ReleaseAll()
    {
        foreach (var obj in new List<T>(activeObjects))
        {
            Release(obj);
        }
    }

    // 🔥 彻底销毁整个池
    public void Clear()
    {
        foreach (var obj in new List<T>(allObjects))
        {
            if (obj != null)
                Object.Destroy(obj.gameObject);
        }

        allObjects.Clear();
        activeObjects.Clear();
    }

    // 🔥 预热
    public void Prewarm(int count)
    {
        List<T> temp = new();

        for (int i = 0; i < count; i++)
        {
            temp.Add(pool.Get());
        }

        foreach (var obj in temp)
        {
            pool.Release(obj);
        }
    }
}