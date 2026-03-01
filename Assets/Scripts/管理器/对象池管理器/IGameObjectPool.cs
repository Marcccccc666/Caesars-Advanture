public interface IGameObjectPool
{
    void ReleaseAll();
    void Clear();
    int ActiveCount { get; }
    int InactiveCount { get; }
}