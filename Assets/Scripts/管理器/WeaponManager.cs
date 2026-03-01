using System;
using UnityEngine;

/// <summary>
/// 武器管理器
/// </summary>
public class WeaponManager: Singleton<WeaponManager>
{
    /// <summary>
    /// 当前武器Prefab数据
    /// </summary>
    [SerializeField]private WeaponData currentWeapon;

    [SerializeField, ChineseLabel("武器伤害倍率")] private float damageMultiplier = 1f;
    [SerializeField, ChineseLabel("武器伤害加成")] private int damageBonus = 0;

    [SerializeField, ChineseLabel("攻击间隔减少(秒)")] private float attackIntervalBonus = 0f;

    /// <summary>
    /// 武器切换事件，参数为新武器数据
    /// </summary>
    public Action<WeaponData> OnWeaponSwitched;

    /// <summary>
    /// 获取当前武器
    /// </summary>
    public WeaponData GetCurrentWeapon => currentWeapon;

    /// <summary>
    /// 切换武器
    /// </summary>
    public void SwitchWeapon(WeaponData newWeapon)
    {
        if(currentWeapon == newWeapon)
        {
            return; // 如果切换到同一把武器，直接返回
        }
        currentWeapon = newWeapon;
        OnWeaponSwitched?.Invoke(newWeapon);
    }

    /// <summary>
    /// 获取最终伤害
    /// </summary>
    public int GetFinalDamage(int baseDamage)
    {
        float scaled = baseDamage * damageMultiplier;
        int total = Mathf.RoundToInt(scaled) + damageBonus;
        return Mathf.Max(1, total);
    }

    /// <summary>
    /// 获取最终攻击间隔
    /// </summary>
    public float GetFinalAttackInterval(float baseInterval)
    {
        float finalInterval = baseInterval - attackIntervalBonus;
        return Mathf.Max(0.1f, finalInterval); // 最小攻击间隔为0.1秒
    }

    /// <summary>
    /// 增加伤害倍率（例如 0.2 表示 +20%）
    /// </summary>
    public void AddDamageMultiplier(float delta)
    {
        damageMultiplier = Mathf.Max(0f, damageMultiplier + delta);
    }

    /// <summary>
    /// 增加伤害数值
    /// </summary>
    public void AddDamageBonus(int bonus)
    {
        damageBonus += bonus;
    }

    /// <summary>
    /// 增加攻击间隔减少
    /// </summary>
    public void AddAttackIntervalBonus(float bonus)
    {
        attackIntervalBonus += bonus;
    }

    #region 升级武器
    /// <summary>
    /// 升级当前武器
    /// </summary>
    public Action UpgradeCurrentWeapon;
    #endregion
}
