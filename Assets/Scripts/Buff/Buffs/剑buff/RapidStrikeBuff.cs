using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "迅击", menuName = "Buffs/剑/迅击")]
public class RapidStrikeBuff : BuffDefinition
{
    [SerializeField, Min(0.01f)] private float attackIntervalBonusPerStack = 0.03f;
    [SerializeField, Min(1)] private int maxStacks = 5;
    [SerializeField, Min(0.1f)] private float resetDelay = 1.5f;

    private int currentStacks;
    private float currentBonus;
    private Coroutine resetCoroutine;

    public override void Apply()
    {
        BuffManager.Instance.AttackHitTriggered += OnAttackHit;
    }

    public override void Remove()
    {
        if (resetCoroutine != null && BuffManager.Instance != null)
        {
            BuffManager.Instance.StopCoroutine(resetCoroutine);
        }

        ResetBonus();
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

        int nextStacks = Mathf.Min(maxStacks, currentStacks + 1);
        float nextBonus = nextStacks * attackIntervalBonusPerStack;
        WeaponManager.Instance.AddAttackIntervalBonus(nextBonus - currentBonus);

        currentStacks = nextStacks;
        currentBonus = nextBonus;

        if (resetCoroutine != null)
        {
            BuffManager.Instance.StopCoroutine(resetCoroutine);
        }

        resetCoroutine = BuffManager.Instance.StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        ResetBonus();
    }

    private void ResetBonus()
    {
        if (currentBonus > 0f && WeaponManager.Instance != null)
        {
            WeaponManager.Instance.AddAttackIntervalBonus(-currentBonus);
        }

        currentStacks = 0;
        currentBonus = 0f;
        resetCoroutine = null;
    }
}
