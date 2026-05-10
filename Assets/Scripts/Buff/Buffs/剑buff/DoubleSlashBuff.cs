using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "连斩", menuName = "Buffs/剑/连斩")]
public class DoubleSlashBuff : BuffDefinition
{
    [SerializeField] private BulletAttack slashProjectilePrefab;
    [SerializeField, Min(0f)] private float followDelay = 0.08f;
    [SerializeField, Min(0.1f)] private float speed = 8f;
    [SerializeField, Range(0.1f, 2f)] private float damageRatio = 0.6f;

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
        if (!SwordBuffUtility.TryGetCurrentSwordData(out _))
        {
            return;
        }

        BuffManager.Instance.StartCoroutine(SpawnDelayedSlash(sourceTransform));
    }

    private IEnumerator SpawnDelayedSlash(Transform sourceTransform)
    {
        yield return new WaitForSeconds(followDelay);

        int damage = SwordBuffUtility.GetScaledSwordDamage(damageRatio);
        SwordBuffUtility.SpawnProjectile(slashProjectilePrefab, sourceTransform, speed, damage);
    }
}
