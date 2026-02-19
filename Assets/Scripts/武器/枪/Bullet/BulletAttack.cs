using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletAttack : MonoBehaviour, IPoolable<BulletAttack>
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
    
    private IObjectPool<BulletAttack> pool;

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
        else if(collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
           Release();
        }
    }

    

    public void Initialize(Vector2 direction, float speed, int damage, int penetration)
    {
        bulletDamage = damage;
        bulletPenetration = penetration;
        moveDirection = direction;
        moveSpeed = speed;
    }

    public void SetPool(IObjectPool<BulletAttack> pool)
    {
        this.pool = pool;
    }

    /// <summary>
    /// 当子弹被生成时调用，执行必要的初始化逻辑
    /// </summary>
    public void OnSpawn()
    {
        // 可以在这里添加任何需要在子弹生成时执行的逻辑，例如重置状态、播放动画等
    }
    
    /// <summary>
    /// 将子弹释放回对象池，准备下次使用
    /// </summary>
    public void Release()
    {
        pool.Release(this);
    }

    /// <summary>
    /// 当对象被释放回池中时调用，重置子弹状态以准备下次使用
    /// </summary>
    public void OnDespawn()
    {
        RG2D.linearVelocity = Vector2.zero;
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
