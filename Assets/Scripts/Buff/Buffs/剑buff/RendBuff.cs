using UnityEngine;

[CreateAssetMenu(fileName = "割裂", menuName = "Buffs/剑/割裂")]
public class RendBuff : BuffDefinition
{
    [SerializeField, Min(1)] private int bonusDamage = 2;

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
        if (!SwordBuffUtility.IsCurrentQuickSword())
        {
            return;
        }

        if (!target.TryGetComponent(out SwordEnemyStatus status) || !status.HasBleed)
        {
            return;
        }

        if (SwordBuffUtility.TryGetEnemyData(target, out EnemyData enemyData))
        {
            enemyData.Damage(bonusDamage);
        }
    }
}
