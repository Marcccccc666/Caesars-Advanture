using UnityEngine;

[CreateAssetMenu(fileName = "重压", menuName = "Buffs/剑/重压")]
public class CrushingWeightBuff : BuffDefinition
{
    [SerializeField, Range(0.1f, 1f)] private float slowMultiplier = 0.45f;
    [SerializeField, Min(0.1f)] private float duration = 1.5f;

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

        SwordEnemyStatus status = SwordBuffUtility.GetOrAddEnemyStatus(target);
        status?.ApplySlow(slowMultiplier, duration);
    }
}
