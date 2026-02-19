using UnityEngine;
using UnityEngine.Pool;

public abstract class PoolableObject<T> : MonoBehaviour where T : PoolableObject<T>
{
    private IObjectPool<T> pool;

    /// <summary>
    /// 设置对象池
    /// </summary>
    /// <param name="pool">对象池</param>
    public void SetPool(IObjectPool<T> pool)
    {
        this.pool = pool;
    }

    /// <summary>
    /// 释放对象回对象池
    /// </summary>
    public void Release()
    {
        if (pool != null)
        {
            pool.Release((T)this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 当对象被获取时调用
    /// </summary>
    public virtual void OnSpawn() {}

    /// <summary>
    /// 当对象被回收时调用
    /// </summary>
    public virtual void OnDespawn(){}
}
