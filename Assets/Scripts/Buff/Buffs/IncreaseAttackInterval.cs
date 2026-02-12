using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseAttackInterval", menuName = "Buffs/Increase Attack Interval")]
public class IncreaseAttackInterval : BuffDefinition
{
    [SerializeField, ChineseLabel("攻击间隔减少(秒)")] private float attackIntervalBonus = 0.5f;
    
    public override void Apply()
    {
        WeaponManager.Instance.AddAttackIntervalBonus(attackIntervalBonus);
    }

}
