using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "快意", menuName = "Buffs/剑/快意")]
public class ExhilarationBuff : BuffDefinition
{
    [SerializeField, Min(0.01f)] private float attackIntervalBonus = 0.08f;
    [SerializeField, Min(0.1f)] private float duration = 2f;

    private Coroutine activeCoroutine;
    private bool isApplied;

    public override void Apply()
    {
        BuffManager.Instance.EnemyKilledTriggered += OnEnemyKilled;
    }

    public override void Remove()
    {
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.EnemyKilledTriggered -= OnEnemyKilled;
            if (activeCoroutine != null)
            {
                BuffManager.Instance.StopCoroutine(activeCoroutine);
            }
        }

        ClearBuff();
    }

    private void OnEnemyKilled(Transform target)
    {
        if (!SwordBuffUtility.IsCurrentQuickSword() || WeaponManager.Instance == null)
        {
            return;
        }

        if (!isApplied)
        {
            WeaponManager.Instance.AddAttackIntervalBonus(attackIntervalBonus);
            isApplied = true;
        }

        if (activeCoroutine != null)
        {
            BuffManager.Instance.StopCoroutine(activeCoroutine);
        }

        activeCoroutine = BuffManager.Instance.StartCoroutine(RemoveLater());
    }

    private IEnumerator RemoveLater()
    {
        yield return new WaitForSeconds(duration);
        ClearBuff();
    }

    private void ClearBuff()
    {
        if (isApplied && WeaponManager.Instance != null)
        {
            WeaponManager.Instance.AddAttackIntervalBonus(-attackIntervalBonus);
        }

        isApplied = false;
        activeCoroutine = null;
    }
}
