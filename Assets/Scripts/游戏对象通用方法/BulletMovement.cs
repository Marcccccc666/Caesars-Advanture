using UnityEngine;

public enum BulletType
{
    /// <summary>
    /// 直线弹道
    /// </summary>
    Parallel, 
    /// <summary>
    /// 扇形弹道
    /// </summary>
    fan_shaped
}

public static class BulletMovement
{
    /// <summary>
    /// 根据弹道类型计算子弹的初始位置或方向
    /// <para>对于直线弹道，需要输入间隔距离和子弹数量，返回每个子弹的初始位置</para>
    /// <para>对于扇形弹道，需要输入扇形角度和子弹数量，返回每个子弹的初始方向</para>
    /// </summary>
    /// <param name="bulletSpawnPoint">子弹生成点</param>
    /// <param name="bulletType">弹道类型</param>
    /// <param name="intervalDistance">子弹间隔距离（仅直线弹道）</param>
    /// <param name="fanAngle">扇形角度（仅扇形弹道）</param>
    /// <param name="bulletCount">子弹数量</param>
    public static Vector3[] BulletMoveTypes(Transform bulletSpawnPoint, BulletType bulletType, int bulletCount, float intervalDistance = 0, float fanAngle = 0)
    {
        switch (bulletType)
        {
            case BulletType.Parallel:
                return MoveBullet(bulletSpawnPoint, intervalDistance, bulletCount);
            case BulletType.fan_shaped:
                return FanShapedMovement(bulletSpawnPoint, fanAngle, bulletCount);
            default:
                return new Vector3[0];
        }
    }


    /// <summary>
    /// 直线弹道子弹的初试位置 
    /// </summary>
    /// <param name="bulletSpawnPoint">子弹生成点</param>
    /// <param name="intervalDistance">子弹间隔距离</param>
    /// <param name="bulletCount">子弹数量</param>
    private static Vector3[] MoveBullet(Transform bulletSpawnPoint,float intervalDistance,int bulletCount)
    {
        if (bulletCount == 1)
        {
            Vector3[] singlePosition = new Vector3[1];
            singlePosition[0] = bulletSpawnPoint.position;
            return singlePosition;
        }

        Vector2 shootDirection = bulletSpawnPoint.right.normalized;

        Vector2 perpendicularDirection = new Vector2(-shootDirection.y, shootDirection.x);
        Vector3[] positions = new Vector3[bulletCount];
        for (int i = 0; i < bulletCount; i++)
        {
            float offset = (i - (bulletCount - 1) / 2f) * intervalDistance;
            Vector2 spawnPosition = (Vector2)bulletSpawnPoint.position + perpendicularDirection * offset;
            positions[i] = spawnPosition;
        }
        return positions;
    }
    
    /// <summary>
    /// 扇形弹道移动
    /// <para> 每个子弹都从子弹生成点出发</para>
    /// <para> 返回每个子弹的向量</para>
    /// </summary>
    private static Vector3[] FanShapedMovement(Transform bulletSpawnPoint, float fanAngle, int bulletCount)
    {
        Vector2 shootDirection = bulletSpawnPoint.right.normalized;
        Vector3[] directions = new Vector3[bulletCount];

        if (bulletCount == 1)
        {
            directions[0] = shootDirection;
            return directions;
        }

        float angleStep = fanAngle / (bulletCount - 1);
        float startAngle = -fanAngle / 2;

        for (int i = 0; i < bulletCount; i++)
        {
            float currentAngle = startAngle + i * angleStep;
            Quaternion rotation = Quaternion.Euler(0, 0, currentAngle);
            Vector2 rotatedDirection = rotation * shootDirection;
            directions[i] = rotatedDirection.normalized;
        }
        return directions;
    }
}
