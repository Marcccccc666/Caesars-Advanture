using UnityEngine;

[CreateAssetMenu(fileName = "霸体蓄力", menuName = "Buffs/剑/霸体蓄力")]
public class UnyieldingChargeBuff : BuffDefinition
{
    [SerializeField, Range(0f, 0.95f)] private float damageReduction = 0.5f;

    public override void Apply()
    {
        BuffManager.Instance?.AddHeavyChargeDamageReduction(damageReduction);
    }

    public override void Remove()
    {
        BuffManager.Instance?.AddHeavyChargeDamageReduction(-damageReduction);
    }
}
