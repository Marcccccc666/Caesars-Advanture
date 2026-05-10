using UnityEngine;

[CreateAssetMenu(fileName = "残影斩", menuName = "Buffs/剑/残影斩")]
public class AfterimageSlashBuff : BuffDefinition
{
    [SerializeField] private BulletAttack slashProjectilePrefab;
    [SerializeField, Min(1)] private int attacksPerTrigger = 4;
    [SerializeField, Min(0.1f)] private float speed = 10f;
    [SerializeField, Range(0.1f, 2f)] private float damageRatio = 0.8f;

    private int currentAttackCount;

    public override void Apply()
    {
        BuffManager.Instance.AttackTriggered += OnAttackTriggered;
    }

    public override void Remove()
    {
        currentAttackCount = 0;
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.AttackTriggered -= OnAttackTriggered;
        }
    }

    private void OnAttackTriggered(Transform sourceTransform)
    {
        if (!SwordBuffUtility.IsCurrentQuickSword())
        {
            return;
        }

        currentAttackCount++;
        if (currentAttackCount < Mathf.Max(1, attacksPerTrigger))
        {
            return;
        }

        currentAttackCount = 0;
        int damage = SwordBuffUtility.GetScaledSwordDamage(damageRatio);
        SwordBuffUtility.SpawnProjectile(slashProjectilePrefab, sourceTransform, speed, damage);
    }
}
