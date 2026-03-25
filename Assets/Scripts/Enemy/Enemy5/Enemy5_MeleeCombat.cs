using UnityEngine;

public class Enemy5_MeleeCombat : MonoBehaviour
{
    [SerializeField, ChineseLabel("攻击点")] private Transform attackPoint;
    [SerializeField, ChineseLabel("玩家层")] private LayerMask playerLayer;
    [SerializeField, ChineseLabel("攻击半径")] private float attackRadius = 0.75f;
    [SerializeField, ChineseLabel("默认伤害")] private int fallbackDamage = 1;

    private EnemyData enemyData;
    private CharacterManager characterManager => CharacterManager.Instance;

    public Transform AttackPoint => attackPoint != null ? attackPoint : transform;
    public float AttackRadius => attackRadius;

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
    }

    public bool TryHitPlayer()
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
            ? Physics2D.OverlapCircleAll(center, attackRadius, playerLayer)
            : Physics2D.OverlapCircleAll(center, attackRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || !hit.CompareTag("Player"))
            {
                continue;
            }

            int damage = enemyData != null ? enemyData.CurrentAttack : fallbackDamage;
            playerData.Damage(damage);
            BuffManager.Instance?.PlayerDamagedTriggered?.Invoke(playerData.transform);
            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Transform point = attackPoint != null ? attackPoint : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(point.position, attackRadius);
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
