using UnityEngine;

[CreateAssetMenu(fileName = "满蓄冲击", menuName = "Buffs/剑/满蓄冲击")]
public class FullChargeShockwaveBuff : BuffDefinition
{
    [SerializeField] private ExplosionController explosionPrefab;
    [SerializeField, Range(0.1f, 3f)] private float damageRatio = 1f;

    private bool pendingShockwave;

    public override void Apply()
    {
        if (explosionPrefab != null)
        {
            PoolManager.Instance.GetOrCreatePool(explosionPrefab);
        }

        BuffManager.Instance.AttackTriggered += OnAttackTriggered;
        BuffManager.Instance.AttackHitTriggered += OnAttackHit;
    }

    public override void Remove()
    {
        pendingShockwave = false;
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.AttackTriggered -= OnAttackTriggered;
            BuffManager.Instance.AttackHitTriggered -= OnAttackHit;
        }
    }

    private void OnAttackTriggered(Transform sourceTransform)
    {
        pendingShockwave = SwordBuffUtility.TryGetCurrentHeavySwordData(out HeavySwordData heavySwordData)
            && heavySwordData.CurrentCharge >= heavySwordData.M_WeaponBaseData.MaxCharge;
    }

    private void OnAttackHit(Transform target)
    {
        if (!pendingShockwave)
        {
            return;
        }

        pendingShockwave = false;
        SwordBuffUtility.SpawnExplosion(explosionPrefab, target.position, SwordBuffUtility.GetScaledSwordDamage(damageRatio));
    }
}
