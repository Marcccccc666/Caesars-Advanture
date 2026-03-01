using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField, ChineseLabel("当前武器数据")] private WeaponData currentWeaponDataProfab;
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
        if(currentWeaponDataProfab == weaponData)
        {
            return;
        }
        //回收旧武器
        if(currentWeaponDataProfab != null)
        {
            poolManager.Release(currentWeaponDataProfab, currentWeaponObject);
        }

        //生成新武器
        currentWeaponDataProfab = weaponData;
        currentWeaponObject = poolManager.Spawn(
            prefab: currentWeaponDataProfab,
            position: this.transform.position,
            rotation: this.transform.rotation,
            parent: this.transform,
            defaultCapacity: 1,
            maxSize: 1,
            autoActive: true);
        
    }
}
