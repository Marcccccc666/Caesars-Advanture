using UnityEngine;

[CreateAssetMenu(fileName = "扩刃", menuName = "Buffs/剑/扩刃")]
public class BladeExtensionBuff : BuffDefinition
{
    [SerializeField, Min(0.01f)] private float rangeMultiplierBonus = 0.25f;

    public override void Apply()
    {
        WeaponManager.Instance?.AddSwordAttackRangeMultiplier(rangeMultiplierBonus);
    }

    public override void Remove()
    {
        WeaponManager.Instance?.AddSwordAttackRangeMultiplier(-rangeMultiplierBonus);
    }
}
