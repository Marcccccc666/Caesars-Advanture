using UnityEngine;

[CreateAssetMenu(fileName = "破甲斩", menuName = "Buffs/剑/破甲斩")]
public class ArmorBreakBuff : BuffDefinition
{
    [SerializeField, Min(0.1f)] private float duration = 3f;
    [SerializeField, Range(0.05f, 1f)] private float bonusDamageRatio = 0.25f;

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
        if (!SwordBuffUtility.TryGetCurrentSwordData(out _))
        {
            return;
        }

        SwordEnemyStatus status = SwordBuffUtility.GetOrAddEnemyStatus(target);
        status?.ApplyArmorBreak(duration, bonusDamageRatio);
    }
}
