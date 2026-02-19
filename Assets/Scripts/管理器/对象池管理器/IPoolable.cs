using UnityEngine;
using UnityEngine.Pool;

public interface IPoolable<T> where T : Component
{
    void SetPool(IObjectPool<T> pool);

    /// <summary>
    /// 当对象从池中获取时调用，执行必要的初始化
    /// </summary>
    void OnSpawn();

    /// <summary>
    /// 当对象被释放回池中时调用，执行必要的清理和状态
    /// </summary>
    void OnDespawn();

    /// <summary>
    /// 回收对象到池中，准备下次使用
    /// </summary>
    void Release();
}

