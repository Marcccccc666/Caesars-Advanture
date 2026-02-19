using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletAttack : PoolableObject<BulletAttack>
{
    /// <summary>
    /// 子弹伤害值，默认为10，可以通过SetBulletDamage方法进行设置
    /// </summary>
    private int bulletDamage = 10;

    /// <summary>
    /// 当前穿透的敌人数量，默认为0，可以通过SetBulletPenetration方法进行设置
    /// </summary>
    private int bulletPenetration = 0;

    private Vector2 moveDirection;

    private float moveSpeed;

    [SerializeField] private Rigidbody2D RG2D;

    private GameManager GameManager => GameManager.Instance;
    private WeaponManager weaponManager => WeaponManager.Instance;
    private EnemyManager enemyManager => EnemyManager.Instance;
    private PoolManager poolManager => PoolManager.Instance;

    private void Awake()
    {
        if (RG2D == null)
        {
            OnValidate();
        }
    }

    private void FixedUpdate()
    {
        if(!GameManager.IsPlayerControllable)
        {
            return;
        }
        RG2D.linearVelocity = moveDirection * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            int enemyID = collision.gameObject.GetInstanceID();
            EnemyData enemyData = enemyManager.GetEnemyDataDict[enemyID];
            enemyData.Damage(bulletDamage);

            if(enemyData.CurrentHealth <= 0)
            {
                enemyManager.RemoveEnemyData(enemyID);
            }

            // 如果子弹穿透值大于0，则继续穿透下一个敌人
            if (bulletPenetration > 0)
            {
                bulletPenetration--;
            }
            else
            {
                Release();
            }
        }
        else if(collision.CompareTag("Wall"))
        {
           Release();
        }
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        RG2D.linearVelocity = Vector2.zero;
    }

    public void Initialize(Vector2 direction, float speed, int damage, int penetration)
    {
        bulletDamage = damage;
        bulletPenetration = penetration;
        moveDirection = direction;
        moveSpeed = speed;
    }

#region UNITY_EDITOR
    private void OnValidate()
    {
        if (RG2D == null)
        {
            RG2D = GetComponent<Rigidbody2D>();
            if (RG2D == null)
            {
                Debug.LogError("BulletAttack脚本未找到Rigidbody2D组件，请检查！");
            }
        }
    }
#endregion
}
