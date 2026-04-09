using System.Collections.Generic;
using UnityEngine;

public class BulletCountController : MonoBehaviour
{
    [SerializeField, ChineseLabel("子弹UI预制体")] private Transform bulletUIPrefab;
    [SerializeField, ChineseLabel("子弹UI父物体")] private Transform bulletUIParent;
    [SerializeField, ChineseLabel("订阅的枪数据")] private GunData subscribedGunData;

    private readonly Queue<Transform> bulletUIInstances = new();

    private WeaponManager weaponManager => WeaponManager.Instance;
    private PoolManager poolManager => PoolManager.Instance;
    private GameManager gameManager => GameManager.Instance;

    

    private void Awake()
    {
        ClearAllBulletUI();
    }

    private void OnEnable()
    {
        if (weaponManager != null)
        {
            weaponManager.OnWeaponSwitched += GetNewWeaponAndUpdateUI;
        }

        if (gameManager != null)
        {
            gameManager.GameSceneChangedAction += ClearAllBulletUI;
        }

        BindCurrentWeapon();
    }

    private void OnDisable()
    {
        UnbindCurrentWeapon();

        if (weaponManager != null)
        {
            weaponManager.OnWeaponSwitched -= GetNewWeaponAndUpdateUI;
        }

        if (gameManager != null)
        {
            gameManager.GameSceneChangedAction -= ClearAllBulletUI;
        }

        ClearAllBulletUI();
    }

    private void OnDestroy()
    {
        UnbindCurrentWeapon();

        if (weaponManager != null)
        {
            weaponManager.OnWeaponSwitched -= GetNewWeaponAndUpdateUI;
        }

        if (gameManager != null)
        {
            gameManager.GameSceneChangedAction -= ClearAllBulletUI;
        }
    }

    private void BindCurrentWeapon()
    {
        if (weaponManager == null)
        {
            return;
        }

        GetNewWeaponAndUpdateUI(weaponManager.GetCurrentWeapon);
    }

    private void UnbindCurrentWeapon()
    {
        if (subscribedGunData != null)
        {
            subscribedGunData.OnBulletCountAdded -= AddBulletUI;
            subscribedGunData.OnBulletCountDecreased -= RecycleBulletUIInstances;
            subscribedGunData = null;
        }
    }

    private void GetNewWeaponAndUpdateUI(WeaponData newWeaponData)
    {
        UnbindCurrentWeapon();
        ClearAllBulletUI();

        if (newWeaponData is not GunData gunData)
        {
            return;
        }

        subscribedGunData = gunData;
        subscribedGunData.OnBulletCountAdded += AddBulletUI;
        subscribedGunData.OnBulletCountDecreased += RecycleBulletUIInstances;

        if (!bulletUIParent || !bulletUIPrefab || poolManager == null || weaponManager == null)
        {
            return;
        }

        if (gunData.WeaponBaseData is GunBaseData gunBaseData)
        {
            int bulletCount = weaponManager.GetFinalBulletCount(gunBaseData.MaxBulletCount);
            for (int i = 0; i < gunData.CurrentBulletCount; i++)
            {
                Transform bulletUI = poolManager.Spawn(
                    prefab: bulletUIPrefab,
                    pos: bulletUIParent.position,
                    rot: bulletUIParent.rotation,
                    defaultCapacity: bulletCount,
                    maxSize: 20,
                    setActive: true,
                    parent: bulletUIParent
                );
                bulletUIInstances.Enqueue(bulletUI);
            }
        }
    }

    private void AddBulletUI(int count)
    {
        if (!bulletUIParent || !bulletUIPrefab || poolManager == null || weaponManager == null)
        {
            Debug.LogWarning("BulletCountController: UI 父物体或对象池已失效。");
            return;
        }

        if (subscribedGunData == null || subscribedGunData.WeaponBaseData is not GunBaseData gunBaseData)
        {
            return;
        }

        int bulletCount = weaponManager.GetFinalBulletCount(gunBaseData.MaxBulletCount);
        for (int i = 0; i < count; i++)
        {
            Transform bulletUI = poolManager.Spawn(
                prefab: bulletUIPrefab,
                pos: bulletUIParent.position,
                rot: bulletUIParent.rotation,
                defaultCapacity: bulletCount,
                maxSize: 20,
                setActive: true,
                parent: bulletUIParent
            );
            bulletUIInstances.Enqueue(bulletUI);
        }
    }

    private void RecycleBulletUIInstances(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (bulletUIInstances.TryDequeue(out Transform bulletUI))
            {
                poolManager?.Release(bulletUIPrefab, bulletUI);
            }
        }
    }

    private void ClearAllBulletUI()
    {
        while (bulletUIInstances.Count > 0)
        {
            var bulletUI = bulletUIInstances.Dequeue();
            if (bulletUI != null && poolManager != null)
            {
                poolManager.Release(bulletUIPrefab, bulletUI);
            }
        }
    }
}