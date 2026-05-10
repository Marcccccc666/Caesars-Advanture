using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "余震", menuName = "Buffs/剑/余震")]
public class AftershockBuff : BuffDefinition
{
    [SerializeField] private ExplosionController explosionPrefab;
    [SerializeField, Min(0.05f)] private float delay = 0.2f;
    [SerializeField, Range(0.1f, 3f)] private float damageRatio = 0.7f;

    public override void Apply()
    {
        if (explosionPrefab != null)
        {
            PoolManager.Instance.GetOrCreatePool(explosionPrefab);
        }

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

        BuffManager.Instance.StartCoroutine(SpawnDelayedExplosion(target.position));
    }

    private IEnumerator SpawnDelayedExplosion(Vector3 position)
    {
        yield return new WaitForSeconds(delay);
        SwordBuffUtility.SpawnExplosion(explosionPrefab, position, SwordBuffUtility.GetScaledSwordDamage(damageRatio));
    }
}
