using UnityEngine;

public class Enemy7_GroundSlamCombat : MonoBehaviour
{
    [SerializeField, ChineseLabel("攻击点")] private Transform attackPoint;
    [SerializeField, ChineseLabel("玩家层")] private LayerMask playerLayer;
    [SerializeField, ChineseLabel("默认伤害半径")] private float defaultDamageRadius = 1f;
    [SerializeField, ChineseLabel("击退力度")] private float knockbackForce = 8f;
    [SerializeField, ChineseLabel("默认伤害")] private int fallbackDamage = 1;

    private EnemyData enemyData;
    private float damageRadius;
    private CharacterManager characterManager => CharacterManager.Instance;

    public Transform AttackPoint => attackPoint != null ? attackPoint : transform;
    public float DamageRadius => damageRadius;

    private void Awake()
    {
        if (enemyData == null)
        {
            enemyData = GetComponentInParent<EnemyData>();
        }

        if (playerLayer.value == 0)
        {
            int inferredMask = LayerMask.GetMask("player");
            if (inferredMask != 0)
            {
                playerLayer = inferredMask;
            }
        }

        RefreshDamageRadiusFromGizmo();
    }

    public void RefreshDamageRadiusFromGizmo()
    {
        Transform searchRoot = enemyData != null ? enemyData.transform : transform.root;
        AttackRangeGizmo gizmo = searchRoot != null
            ? searchRoot.GetComponentInChildren<AttackRangeGizmo>(true)
            : GetComponentInChildren<AttackRangeGizmo>(true);
        damageRadius = gizmo != null ? gizmo.GetAttackRange : Mathf.Max(0.1f, defaultDamageRadius);
    }

    public bool PerformGroundSlam()
    {
        CharacterDate playerData = characterManager != null
            ? characterManager.GetCurrentPlayerCharacterData
            : null;
        if (playerData == null)
        {
            return false;
        }

        Vector2 center = AttackPoint.position;
        Collider2D[] hits = playerLayer.value != 0
            ? Physics2D.OverlapCircleAll(center, damageRadius, playerLayer)
            : Physics2D.OverlapCircleAll(center, damageRadius);

        bool hitAny = false;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || !hit.CompareTag("Player"))
            {
                continue;
            }

            int damage = enemyData != null ? enemyData.CurrentAttack : fallbackDamage;
            playerData.Damage(damage);
            ApplyKnockback(playerData, center);
            BuffManager.Instance?.PlayerDamagedTriggered?.Invoke(playerData.transform);
            hitAny = true;
            break;
        }

        return hitAny;
    }

    private void ApplyKnockback(CharacterDate playerData, Vector2 attackCenter)
    {
        if (playerData == null)
        {
            return;
        }

        Rigidbody2D playerRb = playerData.GetComponent<Rigidbody2D>();
        if (playerRb == null)
        {
            return;
        }

        Vector2 direction = playerRb.position - attackCenter;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = (Vector2)playerData.transform.position - (Vector2)transform.position;
        }
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = Vector2.up;
        }

        playerRb.linearVelocity = Vector2.zero;
        playerRb.AddForce(direction.normalized * Mathf.Max(0f, knockbackForce), ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        RefreshDamageRadiusFromGizmo();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(AttackPoint.position, damageRadius > 0f ? damageRadius : defaultDamageRadius);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (enemyData == null)
        {
            enemyData = GetComponentInParent<EnemyData>();
        }
    }
#endif
}
