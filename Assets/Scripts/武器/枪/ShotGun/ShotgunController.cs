using UnityEngine;

[RequireComponent(typeof(ShotGunDate))]
public class ShotgunController : GunController
{
    private ShotGunDate M_gunData => WeaponData as ShotGunDate;

    protected override void Awake()
    {
        base.Awake();
    }


    public override void Attack()
    {
        if(gunAnimator != null)
        {
            gunAnimator.Play(shootAnimationHash);
        }

        ShotGunBaseDate gunBaseData = M_gunData.WeaponBaseData as ShotGunBaseDate;

        int bulletCount = weaponManager.GetFinalBallisticsCount(gunBaseData.InitialBallisticsCount);
        int finalDamage = weaponManager.GetFinalDamage(gunBaseData.WeaponDamage);
        int finalPenetration = weaponManager.GetFinalPenetration(gunBaseData.BulletPenetration);
        float finalBulletSpeed = weaponManager.GetFinalBulletSpeed(gunBaseData.BulletSpeed);
        int finalBounce = weaponManager.GetFinalBulletBounce(gunBaseData.BulletBounce);

        BulletAttack[] bullets = new BulletAttack[bulletCount];

        // 计算每个子弹的向量
        Vector3[] instancePositions = BulletMovement.BulletMoveTypes(
            bulletSpawnPoint: bulletSpawnPoint,
            bulletType: gunBaseData.BallisticsType,
            intervalDistance: gunBaseData.BulletIntervalDistance,
            fanAngle: gunBaseData.FanShapedAngle,
            bulletCount: bulletCount
        );



        // 设置子弹
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = Mathf.Atan2(instancePositions[i].y, instancePositions[i].x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            // 实例化子弹并设置其伤害
            bullets[i] = poolManager.Spawn(
                prefab:gunBaseData.BulletPrefab, 
                position:bulletSpawnPoint.position, 
                rotation:rotation, 
                autoActive:false);
            bullets[i].Initialize(
                direction: instancePositions[i].normalized,
                speed: finalBulletSpeed,
                damage: finalDamage,
                penetration: finalPenetration,
                bounce: finalBounce
            );
            poolManager.Activate(gunBaseData.BulletPrefab, bullets[i]);
        }

        buffManager.AttackTriggered?.Invoke(transform);

        if(M_attackAudioClip != null)
        {
            audioManager.PlaySFX(M_attackAudioClip);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (WeaponData != null)
        {
            if (!(WeaponData is ShotGunDate))
            {
                Debug.LogError("WeaponData 必须是 ShotGunDate 类型，请检查 " + gameObject.name);
                WeaponData = null;
            }
            else
            {
                base.OnValidate();
            }
        }
    }
#endif
}
