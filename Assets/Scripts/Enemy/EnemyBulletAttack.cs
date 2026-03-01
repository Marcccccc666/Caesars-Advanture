using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyBulletAttack : MonoBehaviour, IPoolable<EnemyBulletAttack>
{
    [SerializeField, ChineseLabel("默认伤害")] private int defaultDamage = 1;
    [SerializeField, ChineseLabel("默认速度")] private float defaultSpeed = 8f;
    [SerializeField, ChineseLabel("默认存活时长")] private float defaultLifetime = 3f;
    [SerializeField, ChineseLabel("阻挡层(可选)")] private LayerMask blockingLayerMask;

    private Rigidbody2D rb2D;
    private Collider2D bulletCollider;
    private int bulletDamage;
    private bool launched;
    private bool consumed;
    private Transform ownerTransform;
    private bool hasLastPosition;
    private Vector2 lastPosition;

    private float lifetime = 0.1f;

    private DownTimer downTimer;

    private MultiTimerManager timerManager => MultiTimerManager.Instance;
    private IObjectPool<EnemyBulletAttack> pool;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        bulletCollider = GetComponent<Collider2D>();
        bulletDamage = Mathf.Max(0, defaultDamage);
        rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 防止 prefab 的 Layer Overrides 过滤掉玩家/墙体触发。
        bulletCollider.includeLayers = Physics2D.AllLayers;
        bulletCollider.excludeLayers = 0;

    }

    private void Start()
    {
        if (launched)
        {
            return;
        }

        rb2D.linearVelocity = (Vector2)transform.right * Mathf.Max(0f, defaultSpeed);
        lifetime = Mathf.Max(lifetime, defaultLifetime);
    }

    private void OnEnable()
    {
        if (rb2D == null)
        {
            return;
        }

        hasLastPosition = true;
        lastPosition = rb2D.position;
    }

    private void FixedUpdate()
    {
        if (consumed || rb2D == null)
        {
            return;
        }

        Vector2 currentPosition = rb2D.position;
        if (!hasLastPosition)
        {
            hasLastPosition = true;
            lastPosition = currentPosition;
            return;
        }

        SweepForHit(lastPosition, currentPosition);
        lastPosition = currentPosition;
    }

    private void Update()
    {
        if (downTimer.IsComplete())
        {
            Consume();
        }
    }

    public void SetBulletDamage(int damage)
    {
        bulletDamage = Mathf.Max(0, damage);
    }

    public void Launch(Vector2 direction, float speed, float lifetime, Transform owner = null)
    {
        launched = true;
        ownerTransform = owner;

        if (ownerTransform != null && bulletCollider != null)
        {
            Collider2D[] ownerColliders = ownerTransform.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < ownerColliders.Length; i++)
            {
                Collider2D ownerCollider = ownerColliders[i];
                if (ownerCollider == null || ownerCollider == bulletCollider)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(bulletCollider, ownerCollider, true);
            }
        }

        Vector2 normalizedDirection =
            direction.sqrMagnitude < 0.0001f ? Vector2.right : direction.normalized;
        rb2D.linearVelocity = Vector2.zero;
        rb2D.AddForce(normalizedDirection * Mathf.Max(0f, speed), ForceMode2D.Impulse);

        float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        downTimer = timerManager.Create_DownTimer("EnemyBulletAttack"+ gameObject.GetInstanceID(), lifetime);
        downTimer.SetDuration(Mathf.Max(0.1f, lifetime));
        downTimer.StartTimer();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleHit(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider == null)
        {
            return;
        }

        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D collision)
    {
        if (consumed || collision == null)
        {
            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            return;
        }

        if (IsOwnerCollider(collision))
        {
            return;
        }

        if (IsPlayerCollider(collision))
        {
            CharacterDate playerData = GetPlayerData(collision);
            if (playerData != null)
            {
                playerData.Damage(bulletDamage);
            }

            Consume();
            return;
        }

        if (collision.CompareTag("Wall") || !collision.isTrigger)
        {
            Consume();
            return;
        }

        if (blockingLayerMask.value != 0 && IsLayerInMask(collision.gameObject.layer, blockingLayerMask))
        {
            Consume();
        }
    }

    private void SweepForHit(Vector2 from, Vector2 to)
    {
        if (consumed)
        {
            return;
        }

        Vector2 delta = to - from;
        float distance = delta.magnitude;
        if (distance <= 0.0001f)
        {
            return;
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(from, delta.normalized, distance);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i].collider;
            if (hit == null || hit == bulletCollider)
            {
                continue;
            }

            HandleHit(hit);
            if (consumed)
            {
                return;
            }
        }
    }

    private void Consume()
    {
        consumed = true;
        Release();
    }

    private bool IsOwnerCollider(Collider2D collision)
    {
        if (collision == null || ownerTransform == null)
        {
            return false;
        }

        Transform hitTransform = collision.transform;
        return hitTransform == ownerTransform || hitTransform.IsChildOf(ownerTransform);
    }

    private bool IsPlayerCollider(Collider2D collision)
    {
        if (collision == null)
        {
            return false;
        }

        if (collision.CompareTag("Player"))
        {
            return true;
        }

        CharacterDate data = collision.GetComponentInParent<CharacterDate>();
        if (data != null)
        {
            return true;
        }

        CharacterManager manager = CharacterManager.Instance;
        if (manager == null || manager.GetCurrentPlayerCharacterData == null)
        {
            return false;
        }

        Transform playerRoot = manager.GetCurrentPlayerCharacterData.transform;
        Transform hitTransform = collision.transform;
        return hitTransform == playerRoot || hitTransform.IsChildOf(playerRoot);
    }

    private CharacterDate GetPlayerData(Collider2D collision)
    {
        if (collision == null)
        {
            return null;
        }

        CharacterDate directData = collision.GetComponentInParent<CharacterDate>();
        if (directData != null)
        {
            return directData;
        }

        CharacterManager manager = CharacterManager.Instance;
        return manager != null ? manager.GetCurrentPlayerCharacterData : null;
    }

    private static bool IsLayerInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    public void SetPool(IObjectPool<EnemyBulletAttack> pool)
    {
        this.pool = pool;
    }

    public void Release()
    {
        if(timerManager != null && downTimer != null)
        {
            timerManager.Pause_DownTimer(downTimer);
        }

        pool.Release(this);
    }
}
