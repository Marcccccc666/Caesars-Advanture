using UnityEngine;

public class WeaponBaseData : ScriptableObject
{
    [SerializeField, ChineseLabel("武器名称")] private string weaponName;
    /// <summary>
    /// 武器名称
    /// </summary>
    public string WeaponName => weaponName;
    [SerializeField, ChineseLabel("武器伤害")] private int weaponDamage;
    /// <summary>
    /// 武器伤害
    /// </summary>
    public int WeaponDamage => weaponDamage;
    [SerializeField, ChineseLabel("攻击间隔")] private float attackInterval;
    /// <summary>
    /// 攻击间隔
    /// </summary>
    public float AttackInterval => attackInterval;

    [SerializeField, ChineseLabel("武器旋转速度")] private float weaponRotationSpeed;

    /// <summary>
    /// 武器旋转速度
    /// </summary>
    public float WeaponRotationSpeed => weaponRotationSpeed;
}
