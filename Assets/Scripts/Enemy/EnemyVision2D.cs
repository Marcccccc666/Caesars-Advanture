using UnityEngine;

/// <summary>
/// 通用敌人感知组件：
/// - 仇恨范围判定
/// - 攻击范围判定
/// - 视线阻挡判定
/// - 追击/攻击/待机条件聚合
/// </summary>
public class EnemyVision2D : MonoBehaviour
{
    [Header("感知点位")]
    [SerializeField, ChineseLabel("仇恨判定点")] private Transform detectionPoint;
    [SerializeField, ChineseLabel("攻击判定点")] private Transform attackPoint;

    [Header("范围")]
    [SerializeField, ChineseLabel("默认仇恨范围")] private float defaultHateRange = 8f;
    [SerializeField, ChineseLabel("默认攻击范围")] private float defaultAttackRange = 4f;

    [Header("阻挡判定")]
    [SerializeField, ChineseLabel("障碍层")] private LayerMask obstacleMask;
    [SerializeField, ChineseLabel("攻击需视线")] private bool requireLineOfSightToAttack = true;
    [SerializeField, ChineseLabel("攻击需在仇恨范围")] private bool requireHateRangeForAttack = true;

    private float hateRange = 0f;
    private float attackRange = 0f;

    private CharacterManager characterManager => CharacterManager.Instance;
    public Transform PlayerTransform
    {
        get
        {
            if (characterManager == null || characterManager.GetCurrentPlayerCharacterData == null)
            {
                return null;
            }

            return characterManager.GetCurrentPlayerCharacterData.transform;
        }
    }

    public float HateRange => hateRange;
    public float AttackRange => attackRange;
    public LayerMask ObstacleMask => obstacleMask;

    private Vector2 DetectionPosition =>
        detectionPoint != null ? (Vector2)detectionPoint.position : (Vector2)transform.position;

    private Vector2 AttackOrigin =>
        attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;

    private void Awake()
    {
        if (obstacleMask.value == 0)
        {
            int inferredMask = LayerMask.GetMask("Wall", "Obstacle");
            if (inferredMask != 0)
            {
                obstacleMask = inferredMask;
            }
        }

        RefreshRangesFromGizmos();
    }

    public void SetDetectionPoint(Transform point)
    {
        detectionPoint = point;
    }

    public void SetAttackPoint(Transform point)
    {
        attackPoint = point;
    }

    public void SetObstacleMask(LayerMask mask)
    {
        obstacleMask = mask;
    }

    public void RefreshRangesFromGizmos()
    {
        AttackRangeGizmo attackGizmo = GetComponentInChildren<AttackRangeGizmo>();
        attackRange = attackGizmo != null ? attackGizmo.GetAttackRange : defaultAttackRange;

        HateRangeGizmo hateGizmo = GetComponentInChildren<HateRangeGizmo>();
        hateRange = hateGizmo != null ? hateGizmo.GetHateRange : defaultHateRange;
    }

    public bool HasPlayer()
    {
        return PlayerTransform != null;
    }

    public bool IsPlayerInHateRange()
    {
        if (!HasPlayer())
        {
            return false;
        }

        if (hateRange <= 0f)
        {
            return true;
        }

        return Vector2.Distance(DetectionPosition, PlayerTransform.position) <= hateRange;
    }

    public bool IsPlayerInAttackRange()
    {
        if (!HasPlayer() || attackRange <= 0f)
        {
            return false;
        }

        return Vector2.Distance(AttackOrigin, PlayerTransform.position) <= attackRange;
    }

    public bool HasLineOfSightToPlayer()
    {
        if (!HasPlayer())
        {
            return false;
        }

        if (obstacleMask.value == 0)
        {
            return true;
        }

        RaycastHit2D obstacleHit = Physics2D.Linecast(AttackOrigin, PlayerTransform.position, obstacleMask);
        return obstacleHit.collider == null;
    }

    public bool CanAttack()
    {
        if (!IsPlayerInAttackRange())
        {
            return false;
        }

        if (requireHateRangeForAttack && !IsPlayerInHateRange())
        {
            return false;
        }

        if (requireLineOfSightToAttack && !HasLineOfSightToPlayer())
        {
            return false;
        }

        return true;
    }

    public bool ShouldChase()
    {
        if (!IsPlayerInHateRange())
        {
            return false;
        }

        if (!IsPlayerInAttackRange())
        {
            return true;
        }

        if (requireLineOfSightToAttack && !HasLineOfSightToPlayer())
        {
            return true;
        }

        return false;
    }

    public bool ShouldIdle()
    {
        return !IsPlayerInHateRange();
    }

    public Vector2 GetDirectionToPlayer()
    {
        if (!HasPlayer())
        {
            return Vector2.zero;
        }

        Vector2 direction = (Vector2)PlayerTransform.position - (Vector2)transform.position;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return Vector2.zero;
        }

        return direction.normalized;
    }

    public Vector2 GetPlayerPosition(Vector2 fallback)
    {
        return HasPlayer() ? (Vector2)PlayerTransform.position : fallback;
    }
}
