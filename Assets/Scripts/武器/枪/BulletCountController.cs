using System.Collections.Generic;
using UnityEngine;

public class BulletCountController : MonoBehaviour
{
    [SerializeField, ChineseLabel("子弹UI预制体")] private Transform bulletUIPrefab;

    [SerializeField, ChineseLabel("子弹UI父物体")] private Transform bulletUIParent;

    private WeaponData currentWeaponData => WeaponManager.Instance?.GetCurrentWeapon;
    
    private Queue<Transform> bulletUIInstances = new();

    private WeaponManager weaponManager => WeaponManager.Instance;
    private PoolManager poolManager => PoolManager.Instance;
    private GameManager gameManager => GameManager.Instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        foreach (Transform child in bulletUIParent)
        {
            Destroy(child.gameObject);
        }
        bulletUIInstances?.Clear();

        if(weaponManager.GetCurrentWeapon != null)
        {
            UpdateCurrentWeaponBulletUI();
        }
    }

    private void OnEnable()
    {
        weaponManager.OnWeaponSwitched += GetNewWeaponAndUpdateUI;
        
        gameManager.GameSceneChangedAction += ClearAllBulletUI;

        if (currentWeaponData is GunData gunData)
        {
            gunData.OnBulletCountAdded += AddBulletUI;
            gunData.OnBulletCountDecreased += RecycleBulletUIInstances;
        }
    }

    private void OnDisable()
    {
        if(weaponManager)
        {
            weaponManager.OnWeaponSwitched -= GetNewWeaponAndUpdateUI;
        }

        if (currentWeaponData is GunData gunData && currentWeaponData != null)
        {
            gunData.OnBulletCountAdded -= AddBulletUI;
            gunData.OnBulletCountDecreased -= RecycleBulletUIInstances;
        }

        if(gameManager)
        {
            gameManager.GameSceneChangedAction -= ClearAllBulletUI;
        }
    }

    private void OnDestroy()
    {
        if(weaponManager)
        {
            weaponManager.OnWeaponSwitched -= GetNewWeaponAndUpdateUI;
        }

        if (currentWeaponData is GunData gunData && currentWeaponData != null)
        {
            gunData.OnBulletCountAdded -= AddBulletUI;
            gunData.OnBulletCountDecreased -= RecycleBulletUIInstances;
        }

        if(gameManager)
        {
            gameManager.GameSceneChangedAction -= ClearAllBulletUI;
        }
    }

    /// <summary>
    /// 获得新武器时更新子弹UI显示
    /// </summary>
    private void GetNewWeaponAndUpdateUI(WeaponData newWeaponData)
    {
        if (currentWeaponData is GunData oldGunData)
        {
            oldGunData.OnBulletCountAdded -= AddBulletUI;
            oldGunData.OnBulletCountDecreased -= RecycleBulletUIInstances;

            ClearAllBulletUI();
        }

        if(newWeaponData is GunData gunData)
        {
            gunData.OnBulletCountAdded += AddBulletUI;
            gunData.OnBulletCountDecreased += RecycleBulletUIInstances;
            if(gunData.WeaponBaseData is GunBaseData gunBaseData)
            {
                int bulletCount = weaponManager.GetFinalBulletCount(gunBaseData.MaxBulletCount);
                for (int i = 0; i < bulletCount; i++)
                {
                    Transform bulletUI = poolManager.Spawn(
                        prefab:bulletUIPrefab, 
                        pos: bulletUIParent.position,
                        rot: bulletUIParent.rotation,
                        defaultCapacity: bulletCount,
                        maxSize: 20,
                        setActive: true,
                        parent:bulletUIParent);
                    bulletUIInstances.Enqueue(bulletUI);
                }
            }
        }
    }

    /// <summary>
    /// 显示当前武器的子弹数量UI
    /// </summary>
    private void UpdateCurrentWeaponBulletUI()
    {
        if (currentWeaponData is GunData gunData)
        {
            int bulletCount = gunData.CurrentBulletCount;
            ClearAllBulletUI();
            for (int i = 0; i < bulletCount; i++)
            {
                Transform bulletUI = poolManager.Spawn(
                    prefab:bulletUIPrefab, 
                    pos: bulletUIParent.position,
                    rot: bulletUIParent.rotation,
                    defaultCapacity: bulletCount,
                    maxSize: 20,
                    setActive: true,
                    parent:bulletUIParent);
                bulletUIInstances.Enqueue(bulletUI);
            }
        }
    }

    /// <summary>
    /// 回收子弹UI实例
    /// </summary>
    private void RecycleBulletUIInstances(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if(bulletUIInstances.TryDequeue(out Transform bulletUI))
            {
                poolManager.Release(bulletUIPrefab, bulletUI);
            }
        }
    }

    /// <summary>
    /// 增加子弹UI显示
    /// </summary>
    public void AddBulletUI(int count)
    {
        if(currentWeaponData is GunData gunData)
        {
            if(gunData.WeaponBaseData is GunBaseData gunBaseData)
            {
                int bulletCount = weaponManager.GetFinalBulletCount(gunBaseData.MaxBulletCount);
                for (int i = 0; i < count; i++)
                {
                    Transform bulletUI = poolManager.Spawn(
                        prefab:bulletUIPrefab, 
                        pos: bulletUIParent.position,
                        rot: bulletUIParent.rotation,
                        defaultCapacity: bulletCount,
                        maxSize: 20,
                        setActive: true,
                        parent:bulletUIParent);
                    bulletUIInstances.Enqueue(bulletUI);
                }
            }
        }
    }

    private void ClearAllBulletUI()
    {
        while (bulletUIInstances!= null && bulletUIInstances.Count > 0)
        {
            var bulletUI = bulletUIInstances.Dequeue();
            if(bulletUI != null)
            {
                poolManager?.Release(bulletUIPrefab, bulletUI);
            }
        }
    }

    
}
