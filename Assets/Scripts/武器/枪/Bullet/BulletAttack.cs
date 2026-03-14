using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D))]
public class BulletAttack : MonoBehaviour, IPoolable
{
    /// <summary>
    /// 子弹伤害值，默认为10
    /// </summary>
    private int bulletDamage = 10;

    /// <summary>
    /// 当前穿透的敌人数量，默认为0
    /// </summary>
    private int bulletPenetration = 0;

    /// <summary>
    /// 子弹反弹次数，默认为0
    /// </summary>
    private int bulletBounce = 0;

    private Vector2 moveDirection;

    private float moveSpeed;

    [SerializeField] private Rigidbody2D RG2D;

    private GameManager GameManager => GameManager.Instance;
    private EnemyManager enemyManager => EnemyManager.Instance;

    private void Awake()
    {
        if (RG2D == null)
        {
            OnValidate();
        }
    }

    void OnEnable()
    {
        GameManager.GamePausedAction += OnPaused;
        GameManager.GameResumedAction += OnResumed;
        GameManager.GameSceneChangedAction += Release;

        RG2D.linearVelocity = moveDirection.normalized * moveSpeed;
    }

    private void FixedUpdate()
    {
        if(!GameManager.IsPlayerControllable)
        {
            return;
        }

        Vector2 v = RG2D.linearVelocity;

        if (v.sqrMagnitude > 0.001f)
        {
            // 🔥 强制保持速度大小不变
            RG2D.linearVelocity = v.normalized * moveSpeed;

            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            RG2D.rotation = angle;
        }
    }

    private void OnDisable()
    {
        if(GameManager)
        {
            GameManager.GamePausedAction -= OnPaused;
            GameManager.GameResumedAction -= OnResumed;
            GameManager.GameSceneChangedAction -= Release;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Enemy"))
        {
            int enemyID = collision.gameObject.GetInstanceID();
            Dictionary<int, EnemyData> enemyDataDict = enemyManager.GetEnemyDataDict;

            if(!enemyDataDict.TryGetValue(enemyID, out EnemyData enemyData))
            {
                Release();
                return;
            }
            enemyData.Damage(bulletDamage);

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
            if (bulletBounce > 0)
            {
                bulletBounce--;

                // 🔥 用 Raycast 获取精确法线
                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position,
                    moveDirection,
                    0.3f,
                    LayerMask.GetMask("Wall", "Obstacle")
                );

                if (hit.collider != null)
                {
                    // 计算反射方向
                    moveDirection = Vector2.Reflect(moveDirection, hit.normal).normalized;

                    // 更新速度
                    RG2D.linearVelocity = moveDirection * moveSpeed;

                    float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                    RG2D.rotation = angle;

                    // 防止卡在墙里
                    transform.position = hit.point + hit.normal * 0.05f;
                }
                else
                {
                    // 如果没打到法线（极少情况）
                    moveDirection = -moveDirection;
                    RG2D.linearVelocity = moveDirection * moveSpeed;
                }
            }
            else
            {
                Release();
            }
        }
    }

    

    public void Initialize(Vector2 direction, float speed, int damage, int penetration, int bounce)
    {
        bulletDamage = damage;
        bulletPenetration = penetration;
        bulletBounce = bounce;
        moveDirection = direction;
        moveSpeed = speed;
    }

    /// <summary>
    /// 当游戏暂停时，停止子弹的移动
    /// </summary>
    private Vector2 chachedVelocity = Vector2.zero;
    private void OnPaused()
    {
        chachedVelocity = RG2D.linearVelocity;
        RG2D.linearVelocity = Vector2.zero;
    }

    private void OnResumed()
    {
        RG2D.linearVelocity = chachedVelocity;
        chachedVelocity = Vector2.zero;
    }

    #region 对象池设置

    private IMyPool pool;

    public void SetPool(IMyPool pool)
    {
        this.pool = pool;
    }
    
    /// <summary>
    /// 将子弹释放回对象池，准备下次使用
    /// </summary>
    public void Release()
    {
        RG2D.linearVelocity = Vector2.zero;
        if(GameManager)
        {
            GameManager.GamePausedAction -= OnPaused;
            GameManager.GameResumedAction -= OnResumed;
        }
        pool.Release(this);
    }

    #endregion

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
