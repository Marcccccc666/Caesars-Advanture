using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Enemy3Projectile : MonoBehaviour
{
    [SerializeField, ChineseLabel("默认伤害")] private int defaultDamage = 1;
    [SerializeField, ChineseLabel("默认速度")] private float defaultSpeed = 8f;
    [SerializeField, ChineseLabel("默认存活时长")] private float defaultLifetime = 3f;

    private Rigidbody2D rb2D;
    private Collider2D projectileCollider;
    private int damage;
    private bool initialized;
    private bool consumed;
    private Transform ownerTransform;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();
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
        rb2D.linearVelocity = normalizedDirection * Mathf.Max(0f, speed);

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
}
