using UnityEngine;

public class Enemy3_RangedCombat : MonoBehaviour
{
    [Header("远程攻击")]
    [SerializeField, ChineseLabel("子弹刚体预制体")] private EnemyBulletAttack projectilePrefab;
    [SerializeField, ChineseLabel("子弹发射点")] private Transform firePoint;
    [SerializeField, ChineseLabel("子弹速度")] private float projectileSpeed = 8f;
    [SerializeField, ChineseLabel("子弹存活时长")] private float projectileLifetime = 3f;
    [SerializeField, ChineseLabel("攻击音效")] private AudioClip shootAudio;
    private Transform ownerTransform;

    public Transform FirePoint => firePoint;
    
    private PoolManager poolManager => PoolManager.Instance;

    private void Awake()
    {
        EnemyData ownerData = GetComponentInParent<EnemyData>();
        ownerTransform = ownerData != null ? ownerData.transform : transform;
    }

    public void FireTowards(Vector2 targetPosition, int damage)
    {
        if (projectilePrefab == null || firePoint == null)
        {
            return;
        }

        Vector2 direction = targetPosition - (Vector2)firePoint.position;
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = firePoint.right;
        }

        Vector2 normalizedDirection = direction.normalized;
        float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;
        EnemyBulletAttack projectileInstance = null;
        if (poolManager != null)
        {
            projectileInstance = poolManager.Spawn(
                prefab: projectilePrefab,
                pos: firePoint.position,
                rot: Quaternion.Euler(0f, 0f, angle),
                defaultCapacity: 40,
                maxSize: 200,
                setActive: true);
        }
        else
        {
            projectileInstance = Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.Euler(0f, 0f, angle)
            );
        }

        if (projectileInstance != null)
        {
            projectileInstance.SetBulletDamage(damage);
            projectileInstance.Launch(
                normalizedDirection,
                projectileSpeed,
                projectileLifetime,
                ownerTransform
            );
        }

        if (shootAudio != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(shootAudio, Camera.main.transform.position);
        }
    }
}
