using UnityEngine;

public class WeaponPointController : MonoBehaviour
{
    [SerializeField,ChineseLabel("当前武器的游戏对象")] private WeaponData currentWeaponObject;
    private WeaponManager weaponManager => WeaponManager.Instance;
    private PoolManager poolManager => PoolManager.Instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        var currentWeaponData = weaponManager.GetCurrentWeapon;
        if(currentWeaponData != null)
        {
            OnWeaponSwitched(currentWeaponData);
        }
    }

    void OnEnable()
    {
        weaponManager.OnWeaponSwitched += OnWeaponSwitched;
    }

    void OnDisable()
    {
        if(weaponManager != null)
            weaponManager.OnWeaponSwitched -= OnWeaponSwitched;
    }

    private void OnWeaponSwitched(WeaponData weaponData)
    {
        // 切换武器时更新武器到玩家位置
        currentWeaponObject = weaponData;
        weaponData.transform.SetParent(transform);
        weaponData.transform.SetLocalPositionAndRotation(this.transform.localPosition, this.transform.localRotation);
    }
}
