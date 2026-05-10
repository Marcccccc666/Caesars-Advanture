using UnityEngine;

[CreateAssetMenu(fileName = "饮血", menuName = "Buffs/剑/饮血")]
public class LifeStealBuff : BuffDefinition
{
    [SerializeField, Min(1)] private int healAmount = 1;

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

        CharacterManager.Instance.GetCurrentPlayerCharacterData?.Heal(healAmount);
    }
}
