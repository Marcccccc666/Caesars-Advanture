using UnityEngine;

public static class ObjectRotation
{
    /// <summary>
    /// 使游戏对象平滑旋转朝向目标
    /// </summary>
    /// <param name="SelfTransform">自身变换组件</param>
    /// <param name="targetPosition">目标变换组件</param>
    /// <param name="rotationSpeed">旋转速度</param>
    public static void RotateTowardsTarget(Transform SelfTransform, Vector2 targetPosition, float rotationSpeed)
    {
         Vector2 direction = targetPosition - (Vector2)SelfTransform.position;

    if (direction.sqrMagnitude < 0.0001f)
        return;

    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

    Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

    SelfTransform.rotation = Quaternion.RotateTowards(
        SelfTransform.rotation,
        targetRotation,
        rotationSpeed * Time.deltaTime
    );
    }

    /// <summary>
    /// 使游戏对象平滑旋转朝向移动方向
    /// </summary>
    /// <param name="objectTransform">游戏对象变换组件</param>
    /// <param name="moveDirection">移动方向</param>
    /// <param name="rotationSpeed">旋转速度</param>
    public static void RotateTowardsMoveDirection(Transform characterTransform, Vector2 moveDirection, float rotationSpeed )
    {
        if (moveDirection.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        characterTransform.rotation = Quaternion.Slerp(
            characterTransform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

}
