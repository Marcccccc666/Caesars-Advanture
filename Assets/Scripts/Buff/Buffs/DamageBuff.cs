using UnityEngine;

[CreateAssetMenu(fileName = "DamageBuff", menuName = "Buffs/Damage Buff")]
public class DamageBuff : BuffDefinition
{
    [SerializeField, ChineseLabel("伤害数值加成")] private int damageBonus = 1;

    public override void Apply()
    {
        var weaponManager = WeaponManager.Instance;
        if (weaponManager == null)
        {
            return;
        }

        weaponManager.AddDamageBonus(damageBonus);
    }
}
