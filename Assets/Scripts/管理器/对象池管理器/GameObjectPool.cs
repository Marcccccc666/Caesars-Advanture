using UnityEngine;
using UnityEngine.Pool;

public class GameObjectPool<T> where T : Component
{
    private IObjectPool<T> pool;
    private T prefab;

    public GameObjectPool(T prefab, int defaultCapacity = 20, int maxSize = 100, bool collectionCheck = true)
    {
        this.prefab = prefab;

        pool = new ObjectPool<T>(
            Create,
            null,          // 不在 OnGet 激活
            OnRelease,
            OnDestroy,
            collectionCheck,
            defaultCapacity,
            maxSize
        );
    }

    /// <summary>
    /// 创建新对象并设置池引用（如果实现了 IPoolable 接口）
    /// </summary>
    /// <returns></returns>
    private T Create()
    {
        T obj = Object.Instantiate(prefab);

        if (obj is IPoolable<T> poolable)
        {
            poolable.SetPool(pool);
        }

        obj.gameObject.SetActive(false);
        return obj;
    }

    /// <summary>
    /// 释放对象时调用，执行必要的清理并禁用对象
    /// </summary>
    private void OnRelease(T obj)
    {
        if (obj is IPoolable<T> poolable)
        {
            poolable.OnDespawn();
        }

        obj.gameObject.SetActive(false);
    }

    private void OnDestroy(T obj)
    {
        Object.Destroy(obj.gameObject);
    }

    /// 🔹 只获取，不激活
    public T Get()
    {
        return pool.Get();
    }

    /// 🔹 手动激活
    public void Activate(T obj)
    {
        obj.gameObject.SetActive(true);
        if (obj is IPoolable<T> poolable)
        {
            poolable.OnSpawn();
        }
    }

    /// 🔹 一键模式（旧行为）
    public T Spawn(Vector3 pos, Quaternion rot)
    {
        var obj = Get();
        obj.transform.SetPositionAndRotation(pos, rot);
        Activate(obj);
        return obj;
    }

    public void Release(T obj)
    {
        pool.Release(obj);
    }
}
