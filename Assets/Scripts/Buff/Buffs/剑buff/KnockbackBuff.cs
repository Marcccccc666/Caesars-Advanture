using UnityEngine;

[CreateAssetMenu(fileName = "震退", menuName = "Buffs/剑/震退")]
public class KnockbackBuff : BuffDefinition
{
    [SerializeField, Min(0.1f)] private float force = 8f;

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

        if (!target.TryGetComponent(out Rigidbody2D rb))
        {
            return;
        }

        CharacterDate playerData = CharacterManager.Instance.GetCurrentPlayerCharacterData;
        if (playerData == null)
        {
            return;
        }

        Vector2 direction = ((Vector2)target.position - (Vector2)playerData.transform.position).normalized;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }
}
