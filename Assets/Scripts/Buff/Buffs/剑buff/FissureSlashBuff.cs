using UnityEngine;

[CreateAssetMenu(fileName = "裂地斩", menuName = "Buffs/剑/裂地斩")]
public class FissureSlashBuff : BuffDefinition
{
    [SerializeField] private BulletAttack fissureProjectilePrefab;
    [SerializeField, Min(0.1f)] private float speed = 6f;
    [SerializeField, Range(0.1f, 2f)] private float damageRatio = 0.9f;

    public override void Apply()
    {
        BuffManager.Instance.AttackTriggered += OnAttackTriggered;
    }

    public override void Remove()
    {
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.AttackTriggered -= OnAttackTriggered;
        }
    }

    private void OnAttackTriggered(Transform sourceTransform)
    {
        if (!SwordBuffUtility.IsCurrentHeavySword())
        {
            return;
        }

        int damage = SwordBuffUtility.GetScaledSwordDamage(damageRatio);
        SwordBuffUtility.SpawnProjectile(fissureProjectilePrefab, sourceTransform, speed, damage);
    }
}
