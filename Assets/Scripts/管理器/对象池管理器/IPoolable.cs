public interface IPoolable
{

    /// <summary>
    /// 设置对象所属的池，允许对象在需要时回收自己
    /// </summary>
    void SetPool(IMyPool pool);

    /// <summary>
    /// 回收对象到池中，准备下次使用
    /// </summary>
    void Release();
}

