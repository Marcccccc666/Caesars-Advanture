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
    [SerializeField]private WeaponData currentWeaponPrefab;

    /// <summary>
    /// 当前武器实例
    /// </summary>
    [SerializeField]private WeaponData currentWeapon;

    /// <summary>
    /// 获取当前武器实例
    /// </summary>
    public WeaponData GetCurrentWeapon => currentWeapon;

    [SerializeField, ChineseLabel("武器伤害倍率")] private float damageMultiplier = 1f;
    [SerializeField, ChineseLabel("武器伤害加成")] private int damageBonus = 0;

    [SerializeField, ChineseLabel("攻击间隔减少(秒)")] private float attackIntervalBonus = 0f;
    
    [SerializeField, ChineseLabel("武器弹道数加成")] private int ballisticsBonus = 0;
    
    [SerializeField, ChineseLabel("武器穿透力加成")] private int penetrationBonus = 0;

    [SerializeField, ChineseLabel("子弹上限加成")] private int bulletCountBonus = 0;
    
    [SerializeField, ChineseLabel("子弹速度加成")] private float bulletSpeedBonus = 0f;

    [SerializeField, ChineseLabel("子弹反弹加成")] private int bulletBounceBonus = 0;
    
    private PoolManager poolManager => PoolManager.Instance;
#region 武器切换
    /// <summary>
    /// 武器切换事件，参数为（新武器Prefab数据， 新武器实例）
    /// </summary>
    public Action<WeaponData, WeaponData> OnWeaponSwitched;

    /// <summary>
    /// 切换武器
    /// </summary>
    public void SwitchWeapon(WeaponData newWeapon)
    {
        if(currentWeapon == newWeapon)
        {
            return; // 如果切换到同一把武器，直接返回
        }

        if(currentWeapon != null)
        {
            poolManager.Release(currentWeaponPrefab, currentWeapon); // 回收当前武器实例
        }

        currentWeaponPrefab = newWeapon;

         // 根据新武器设置Buff池数据
        currentWeapon = poolManager.Spawn(
            prefab: currentWeaponPrefab,
            position: transform.position,
            rotation: transform.rotation,
            parent: transform,
            defaultCapacity: 1,
            maxSize: 1,
            autoActive: true); // 先生成但不激活

        OnWeaponSwitched?.Invoke(currentWeaponPrefab, currentWeapon);
    }

    
#endregion

#region 升级武器
    /// <summary>
    /// 升级当前武器
    /// </summary>
    public Action UpgradeCurrentWeapon;

    
    private bool isUpgradeInProgress = false;
    /// <summary>
    /// 是否正在升级武器
    /// </summary>
    public bool IsUpgradeInProgress => isUpgradeInProgress;

    /// <summary>
    /// 设置是否正在升级武器
    /// </summary>
    public void SetIsUpgradeInProgress(bool inProgress)
    {
        isUpgradeInProgress = inProgress;
    }

    /// <summary>
    /// 升级当前武器（触发升级事件）
    /// </summary>
    public void UpgradeCurrentWeaponInvoke()
    {
        SetIsUpgradeInProgress(true);
        UpgradeCurrentWeapon?.Invoke();
    }


#endregion

#region 武器数值
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

    #region 弹道

    /// <summary>
    /// 武器弹道数加成
    /// </summary>
    public int BallisticsBonus => ballisticsBonus;

    /// <summary>
    /// 增加武器弹道数加成
     /// <para> 最终弹道数 = 基础弹道数 + 弹道数加成 </para>
    /// </summary>
    /// <param name="bonus"></param>
    public void AddBallisticsBonus(int bonus)
    {
        ballisticsBonus += bonus;
    }

    
    ///<summary>
    /// 获取武器弹道数
    /// </summary>
    public int GetFinalBallisticsCount(int baseCount)
    {
        int finalBallisticsCount = baseCount + ballisticsBonus;
        return Mathf.Max(1, finalBallisticsCount); // 最少1条弹道
    }
#endregion

#region 穿透力
    /// <summary>
    /// 武器穿透力加成
    /// </summary>
    public int PenetrationBonus => penetrationBonus;

    /// <summary>
    /// 增加武器穿透力加成
     /// <para> 最终穿透力 = 基础穿透力 + 穿透力加成 </para>
    /// </summary>
    /// <param name="bonus"></param>
    public void AddPenetrationBonus(int bonus)
    {
        penetrationBonus += bonus;
    }

    /// <summary>
    /// 获取武器穿透力
    /// <para> 最终穿透力 = 基础穿透力 + 穿透力加成 </para>
    /// </summary>
    /// <param name="basePenetration"> 武器基础穿透力 </param>
    /// <returns></returns>
    public int GetFinalPenetration(int basePenetration)
    {
        int finalPenetration = basePenetration + penetrationBonus;
        return Mathf.Max(0, finalPenetration); // 最少0穿透力
    }
#endregion

    #region 子弹数（仅枪械使用）

    /// <summary>
    /// 增加子弹上限加成
    /// </summary>
    public void AddBulletCountBonus(int bonus)
    {
        bulletCountBonus += bonus;
    }

    /// <summary>
    /// 最终子弹数
    /// <para> 最终子弹数 = 基础子弹数 + 子弹数加成 </para>
    /// </summary>
    public int GetFinalBulletCount(int baseBulletCount)
    {
        int finalBulletCount = baseBulletCount + bulletCountBonus;
        return Mathf.Max(1, finalBulletCount); // 最少1发子弹
    }

    #endregion

    #region 子弹速度（仅枪械使用）

    /// <summary>
    /// 增加子弹速度加成
    /// </summary>
    public void AddBulletSpeedBonus(float bonus)
    {
        bulletSpeedBonus += bonus;
    }

    /// <summary>
    /// 获取最终子弹速度
    /// <para> 最终子弹速度 = 基础子弹速度 + 子弹速度加成 </para>
    /// </summary>
    public float GetFinalBulletSpeed(float baseBulletSpeed)
    {
        float finalBulletSpeed = baseBulletSpeed + bulletSpeedBonus;
        return Mathf.Max(1f, finalBulletSpeed); // 最少1速度
    }

    #endregion

    #region 子弹反弹（仅枪械使用）
    /// <summary>
    /// 增加子弹反弹加成
    /// </summary>
    public void AddBulletBounceBonus(int bonus)
    {
        bulletBounceBonus += bonus;
    }

    public int GetFinalBulletBounce(int baseBulletBounce)
    {
        int finalBulletBounce = baseBulletBounce + bulletBounceBonus;
        return Mathf.Max(0, finalBulletBounce); // 最少0次反弹
    }
    #endregion
#endregion
}
