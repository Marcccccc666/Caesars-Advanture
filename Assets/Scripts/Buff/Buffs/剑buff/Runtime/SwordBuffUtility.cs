using UnityEngine;

public static class SwordBuffUtility
{
    public static bool TryGetCurrentSwordData(out SwordDate swordData)
    {
        swordData = WeaponManager.Instance != null ? WeaponManager.Instance.GetCurrentWeapon as SwordDate : null;
        return swordData != null;
    }

    public static bool TryGetCurrentHeavySwordData(out HeavySwordData heavySwordData)
    {
        heavySwordData = WeaponManager.Instance != null ? WeaponManager.Instance.GetCurrentWeapon as HeavySwordData : null;
        return heavySwordData != null;
    }

    public static bool IsCurrentQuickSword()
    {
        return WeaponManager.Instance != null && WeaponManager.Instance.GetCurrentWeapon is QuickSwordDate;
    }

    public static bool IsCurrentHeavySword()
    {
        return WeaponManager.Instance != null && WeaponManager.Instance.GetCurrentWeapon is HeavySwordData;
    }

    public static bool TryGetEnemyData(Transform target, out EnemyData enemyData)
    {
        enemyData = null;
        if (target == null || EnemyManager.Instance == null)
        {
            return false;
        }

        return EnemyManager.Instance.GetEnemyDataDict.TryGetValue(target.gameObject.GetInstanceID(), out enemyData);
    }

    public static SwordEnemyStatus GetOrAddEnemyStatus(Transform target)
    {
        if (target == null)
        {
            return null;
        }

        if (!target.TryGetComponent(out SwordEnemyStatus status))
        {
            status = target.gameObject.AddComponent<SwordEnemyStatus>();
        }

        return status;
    }

    public static Vector2 GetAimDirection(Transform sourceTransform)
    {
        Vector2 direction = Vector2.right;
        if (InputManager.Instance != null && sourceTransform != null)
        {
            direction = InputManager.Instance.MouseWorldPosition - (Vector2)sourceTransform.position;
        }

        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            direction = sourceTransform != null ? (Vector2)sourceTransform.right : Vector2.right;
        }

        return direction.normalized;
    }

    public static void SpawnProjectile(BulletAttack projectilePrefab, Transform sourceTransform, float speed, int damage)
    {
        if (projectilePrefab == null || sourceTransform == null || PoolManager.Instance == null)
        {
            return;
        }

        Vector2 direction = GetAimDirection(sourceTransform);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        BulletAttack bulletAttack = PoolManager.Instance.Spawn(
            projectilePrefab,
            sourceTransform.position,
            Quaternion.Euler(0f, 0f, angle),
            setActive: false);

        bulletAttack.Initialize(direction, speed, damage, 0, 0);
        bulletAttack.gameObject.SetActive(true);
    }

    public static int GetScaledSwordDamage(float ratio)
    {
        if (!TryGetCurrentSwordData(out SwordDate swordData))
        {
            return 0;
        }

        int baseDamage = swordData.WeaponBaseData.WeaponDamage;
        int finalDamage = WeaponManager.Instance.GetFinalDamage(baseDamage);
        return Mathf.Max(1, Mathf.RoundToInt(finalDamage * ratio));
    }

    public static void SpawnExplosion(ExplosionController explosionPrefab, Vector3 position, int damage)
    {
        if (explosionPrefab == null || PoolManager.Instance == null)
        {
            return;
        }

        PoolManager.Instance.GetOrCreatePool(explosionPrefab);
        ExplosionController explosion = PoolManager.Instance.Spawn(
            explosionPrefab,
            position,
            Quaternion.identity,
            setActive: false);

        explosion.Initialize(damage);
        explosion.gameObject.SetActive(true);
    }
}
