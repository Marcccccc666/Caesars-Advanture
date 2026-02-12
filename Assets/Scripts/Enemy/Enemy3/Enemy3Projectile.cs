using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Enemy3Projectile : MonoBehaviour
{
    [SerializeField, ChineseLabel("默认伤害")] private int defaultDamage = 1;
    [SerializeField, ChineseLabel("默认速度")] private float defaultSpeed = 8f;
    [SerializeField, ChineseLabel("默认存活时长")] private float defaultLifetime = 3f;
    [SerializeField, ChineseLabel("阻挡层(可选)")] private LayerMask blockingLayerMask;

    private Rigidbody2D rb2D;
    private Collider2D projectileCollider;
    private int damage;
    private bool initialized;
    private bool consumed;
    private Transform ownerTransform;
    private bool hasLastPosition;
    private Vector2 lastPosition;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
        rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 防止 prefab 的 Layer Overrides 过滤掉玩家/墙体触发。
        projectileCollider.includeLayers = Physics2D.AllLayers;
        projectileCollider.excludeLayers = 0;
    }

    private void Start()
    {
        if (initialized)
        {
            return;
        }

        damage = defaultDamage;
        rb2D.linearVelocity = (Vector2)transform.right * defaultSpeed;
        Destroy(gameObject, Mathf.Max(0.1f, defaultLifetime));
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

    public void Initialize(
        Vector2 direction,
        float speed,
        int bulletDamage,
        float lifetime,
        Transform owner = null
    )
    {
        initialized = true;
        damage = Mathf.Max(0, bulletDamage);
        ownerTransform = owner;

        if (ownerTransform != null && projectileCollider != null)
        {
            Collider2D[] ownerColliders = ownerTransform.GetComponentsInChildren<Collider2D>(true);
            for (int i = 0; i < ownerColliders.Length; i++)
            {
                Collider2D ownerCollider = ownerColliders[i];
                if (ownerCollider == null || ownerCollider == projectileCollider)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(projectileCollider, ownerCollider, true);
            }
        }

        Vector2 normalizedDirection = direction.sqrMagnitude < 0.0001f ? Vector2.right : direction.normalized;
        rb2D.linearVelocity = Vector2.zero;
        rb2D.AddForce(normalizedDirection * Mathf.Max(0f, speed), ForceMode2D.Impulse);

        float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        Destroy(gameObject, Mathf.Max(0.1f, lifetime));
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
                playerData.Damage(damage);
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
            if (hit == null || hit == projectileCollider)
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
        Destroy(gameObject);
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
}
