using UnityEngine;

/// <summary>
/// 武器管理器
/// </summary>
public class WeaponManager: Singleton<WeaponManager>
{
    /// <summary>
    /// 当前武器
    /// </summary>
    [SerializeField]private WeaponBaseData currentWeapon;

    /// <summary>
    /// 获取当前武器
    /// </summary>
    public WeaponBaseData GetCurrentWeapon
    {
        get => currentWeapon;
    }

    /// <summary>
    /// 切换武器
    /// </summary>
    public void SwitchWeapon(WeaponBaseData newWeapon)
    {
        currentWeapon = newWeapon;
    }


}
