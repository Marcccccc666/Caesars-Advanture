using UnityEngine;

public class SwordEnemyStatus : MonoBehaviour
{
    private EnemyData enemyData;

    private float armorBreakUntilTime;
    private float armorBreakBonusRatio;

    private float bleedUntilTime;
    private float bleedTickInterval = 1f;
    private float bleedTickTimer;
    private int bleedDamagePerTick;
    private int bleedStacks;

    private float slowUntilTime;

    public bool HasBleed => bleedStacks > 0 && Time.time < bleedUntilTime;

    private void Awake()
    {
        enemyData = GetComponent<EnemyData>();
    }

    private void Update()
    {
        if (enemyData == null || enemyData.CurrentHealth <= 0)
        {
            return;
        }

        UpdateSlow();
        UpdateBleed();
    }

    public void ApplyArmorBreak(float duration, float bonusRatio)
    {
        armorBreakUntilTime = Mathf.Max(armorBreakUntilTime, Time.time + duration);
        armorBreakBonusRatio = Mathf.Max(armorBreakBonusRatio, bonusRatio);
    }

    public float GetDamageMultiplier()
    {
        if (Time.time >= armorBreakUntilTime)
        {
            armorBreakBonusRatio = 0f;
            return 1f;
        }

        return 1f + armorBreakBonusRatio;
    }

    public void ApplyBleed(int addedStacks, int damagePerTick, float duration, float tickInterval, int maxStacks)
    {
        bleedStacks = Mathf.Clamp(bleedStacks + addedStacks, 0, Mathf.Max(1, maxStacks));
        bleedDamagePerTick = Mathf.Max(1, damagePerTick);
        bleedUntilTime = Mathf.Max(bleedUntilTime, Time.time + duration);
        bleedTickInterval = Mathf.Max(0.1f, tickInterval);
        bleedTickTimer = bleedTickInterval;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (enemyData == null || enemyData.EnemyBaseData == null)
        {
            return;
        }

        float slowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        slowUntilTime = Mathf.Max(slowUntilTime, Time.time + duration);
        enemyData.CurrentMoveSpeed = enemyData.EnemyBaseData.moveSpeed * slowMultiplier;
    }

    private void UpdateBleed()
    {
        if (!HasBleed)
        {
            bleedStacks = 0;
            return;
        }

        bleedTickTimer -= Time.deltaTime;
        if (bleedTickTimer > 0f)
        {
            return;
        }

        bleedTickTimer = bleedTickInterval;
        enemyData.Damage(Mathf.Max(1, bleedDamagePerTick * bleedStacks));
    }

    private void UpdateSlow()
    {
        if (enemyData == null || enemyData.EnemyBaseData == null)
        {
            return;
        }

        if (Time.time < slowUntilTime)
        {
            return;
        }

        if (!Mathf.Approximately(enemyData.CurrentMoveSpeed, enemyData.EnemyBaseData.moveSpeed))
        {
            enemyData.CurrentMoveSpeed = enemyData.EnemyBaseData.moveSpeed;
        }
    }
}
