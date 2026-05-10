using UnityEngine;

public class CaesarData : CharacterDate
{
    [Header("受伤无敌")]
    [SerializeField, Min(0f), ChineseLabel("受伤后无敌时长(秒)")] private float damageInvincibleDuration = 0.75f;

    private float invincibleUntilTime;

    public override void InitObjectData()
    {
        base.InitObjectData();
        invincibleUntilTime = 0f;
    }

    public override void Damage(int damage)
    {
        if (damage <= 0 || CurrentHealth <= 0)
        {
            return;
        }

        if (BuffManager.Instance != null)
        {
            damage = BuffManager.Instance.GetModifiedPlayerDamage(damage);
        }

        if (Time.time < invincibleUntilTime)
        {
            return;
        }

        base.Damage(damage);

        if (CurrentHealth > 0)
        {
            invincibleUntilTime = Time.time + damageInvincibleDuration;
        }
    }
}
