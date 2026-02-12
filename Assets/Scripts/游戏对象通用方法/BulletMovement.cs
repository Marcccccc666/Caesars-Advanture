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
    /// 直线弹道子弹的初试位置 
    /// </summary>
    /// <param name="bulletSpawnPoint">子弹生成点</param>
    /// <param name="intervalDistance">子弹间隔距离</param>
    /// <param name="bulletCount">子弹数量</param>
    public static Vector3[] MoveBullet(Transform bulletSpawnPoint,float intervalDistance,int bulletCount)
    {
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
    /// </summary>
    public static void FanShapedMovement(BulletAttack bullet, Vector3 direction, float speed)
    {
        bullet.GetRG2D.AddForce(direction.normalized * speed, ForceMode2D.Impulse);
    }
}
