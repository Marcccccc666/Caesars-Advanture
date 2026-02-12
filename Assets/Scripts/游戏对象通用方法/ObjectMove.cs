using UnityEngine;

public static class ObjectMove
{
    /// <summary>
    /// 移动游戏对象
    /// </summary>
    /// <param name="Object">游戏对象刚体</param>
    /// <param name="direction">移动方向</param>
    /// <param name="speed">移动速度</param>
    public static void MoveObject(Rigidbody2D Object, Vector3 direction, float speed)
    {
        Vector3 movement = direction.normalized * speed * Time.fixedDeltaTime;
        Object.MovePosition(Object.position + (Vector2)movement);
    }

    /// <summary>
    /// 移动游戏对象
    /// </summary>
    /// <param name="Object">游戏对象变换组件</param>
    /// <param name="direction">移动方向</param>
    /// <param name="speed">移动速度</param>
    public static void MoveObject(Transform Object, Vector3 direction, float speed)
    {
        Vector3 movement = direction.normalized * speed * Time.deltaTime;
        Object.position += movement;
    }
    
}
