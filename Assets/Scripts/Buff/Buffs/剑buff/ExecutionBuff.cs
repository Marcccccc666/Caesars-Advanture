using UnityEngine;

[CreateAssetMenu(fileName = "处决", menuName = "Buffs/剑/处决")]
public class ExecutionBuff : BuffDefinition
{
    [SerializeField, Range(0.01f, 1f)] private float healthThreshold = 0.3f;
    [SerializeField, Min(1)] private int bonusDamage = 4;

    public override void Apply()
    {
        BuffManager.Instance.AttackHitTriggered += OnAttackHit;
    }

    public override void Remove()
    {
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.AttackHitTriggered -= OnAttackHit;
        }
    }

    private void OnAttackHit(Transform target)
    {
        if (!SwordBuffUtility.IsCurrentHeavySword())
        {
            return;
        }

        if (!SwordBuffUtility.TryGetEnemyData(target, out EnemyData enemyData) || enemyData.MaxHealth <= 0)
        {
            return;
        }

        if (enemyData.CurrentHealth <= Mathf.RoundToInt(enemyData.MaxHealth * healthThreshold))
        {
            enemyData.Damage(bonusDamage);
        }
    }
}
