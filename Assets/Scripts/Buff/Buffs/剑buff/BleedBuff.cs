using UnityEngine;

[CreateAssetMenu(fileName = "流血", menuName = "Buffs/剑/流血")]
public class BleedBuff : BuffDefinition
{
    [SerializeField, Min(1)] private int stacksPerHit = 1;
    [SerializeField, Min(1)] private int damagePerTick = 1;
    [SerializeField, Min(0.1f)] private float duration = 4f;
    [SerializeField, Min(0.1f)] private float tickInterval = 1f;
    [SerializeField, Min(1)] private int maxStacks = 5;

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

        SwordEnemyStatus status = SwordBuffUtility.GetOrAddEnemyStatus(target);
        status?.ApplyBleed(stacksPerHit, damagePerTick, duration, tickInterval, maxStacks);
    }
}
