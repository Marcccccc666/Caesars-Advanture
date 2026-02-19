using UnityEngine;

[RequireComponent(typeof(ShotGunDate))]
public class ShotgunController : GunController
{
    private ShotGunDate M_gunData => WeaponData as ShotGunDate;

    private int shootAnimationHash = 0;

    protected override void Awake()
    {
        base.Awake();
        shootAnimationHash = Animator.StringToHash(shootAnimationName);

        weaponManager.SwitchWeapon(M_gunData);
    }

    protected override void Update()
    {
        if(!gameManager.IsPlayerControllable)
        {
            return;
        }
        base.Update();

        if(inputManager.CurrentMouseState == MouseState.Press || inputManager.CurrentMouseState == MouseState.Hold)
        {
            if(MultiTimerManager.IsDownTimerComplete("GunAttackCooldown") )
            {
                Attack();
                MultiTimerManager.Start_DownTimer("GunAttackCooldown", weaponManager.GetFinalAttackInterval(M_gunData.WeaponBaseData.AttackInterval));
            }
        }
    }

    protected override void Attack()
    {
        if(gunAnimator != null)
        {
            gunAnimator.Play(shootAnimationHash);
        }

        ShotGunBaseDate gunBaseData = M_gunData.WeaponBaseData as ShotGunBaseDate;

        int bulletCount = M_gunData.GetFinalBallisticsCount;
        int finalDamage = weaponManager.GetFinalDamage(gunBaseData.WeaponDamage);
        int finalPenetration = M_gunData.GetFinalPenetration;

        BulletAttack[] bullets = new BulletAttack[bulletCount];

        // 计算每个子弹的向量
        Vector3[] instancePositions = new Vector3[bulletCount];
        instancePositions = BulletMovement.BulletMoveTypes(
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
                speed: gunBaseData.BulletSpeed,
                damage: finalDamage,
                penetration: finalPenetration
            );
            poolManager.Activate(gunBaseData.BulletPrefab, bullets[i]);
        }

        if(M_attackAudioClip != null)
        {
            AudioSource.PlayClipAtPoint(M_attackAudioClip, Camera.main.transform.position);
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
