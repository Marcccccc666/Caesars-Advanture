using UnityEngine;

public class GunController : WeaponBase
{
    [Header("动画相关")]
    [SerializeField, ChineseLabel("动画控制器")] protected Animator gunAnimator;
    [SerializeField, ChineseLabel("射击动画名")] protected string shootAnimationName = "Shoot";
    protected int shootAnimationHash = 0;

    [Header("射击相关")]
    [SerializeField, ChineseLabel("子弹生成点")] protected Transform bulletSpawnPoint;

    /// <summary>
    /// 枪械数据
    /// </summary>
    private GunData M_gunData => WeaponData as GunData;

    protected override void Awake()
    {
        base.Awake();
        if(!gunAnimator)
        {
            shootAnimationHash = Animator.StringToHash(shootAnimationName);
        }
        
        MultiTimerManager.Create_DownTimer("GunAttackCooldown");
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
                int currentBulletCount = M_gunData.CurrentBulletCount;
                if (currentBulletCount <= 0)
                {
                    return; // 没有子弹，无法攻击
                }

                buffManager.BeforeAttackTriggered?.Invoke(M_gunData);
                if(M_gunData.IsConsumingBullet)
                {
                    M_gunData.CurrentBulletCount = currentBulletCount - 1; // 消耗一发子弹
                }
                
                buffManager.AttackTriggered?.Invoke(bulletSpawnPoint);
                Attack();
                buffManager.AfterAttackTriggered?.Invoke(M_gunData);

                if(M_attackAudioClip != null)
                {
                    audioManager.PlaySFX(M_attackAudioClip);
                }
                MultiTimerManager.Start_DownTimer("GunAttackCooldown", weaponManager.GetFinalAttackInterval(M_gunData.WeaponBaseData.AttackInterval));
            }
        }
    }



    public override void Attack()
    {
        base.Attack();
        if(gunAnimator != null)
        {
            gunAnimator.Play(shootAnimationHash);
        }

        GunBaseData gunBaseData = M_gunData.WeaponBaseData as GunBaseData;
        int bulletCount = weaponManager.GetFinalBallisticsCount(gunBaseData.InitialBallisticsCount);
        int finalDamage = weaponManager.GetFinalDamage(gunBaseData.WeaponDamage);
        int finalPenetration = weaponManager.GetFinalPenetration(gunBaseData.BulletPenetration);
        float finalBulletSpeed = weaponManager.GetFinalBulletSpeed(gunBaseData.BulletSpeed);
        int finalBounce = weaponManager.GetFinalBulletBounce(gunBaseData.BulletBounce);
        
        BulletAttack[] bullets = new BulletAttack[bulletCount];
        

        // 计算每个子弹的生成位置
        Vector3[] instancePositions = BulletMovement.BulletMoveTypes(
            bulletSpawnPoint: bulletSpawnPoint,
            bulletType: gunBaseData.BallisticsType,
            intervalDistance: gunBaseData.BulletIntervalDistance,
            bulletCount: bulletCount
        );

        // 实例化子弹
        for (int i = 0; i < bulletCount; i++)
        {
            // 实例化子弹并设置其伤害
            bullets[i] = poolManager.Spawn(
                prefab:gunBaseData.BulletPrefab, 
                position:instancePositions[i], 
                rotation:bulletSpawnPoint.rotation, 
                autoActive:false);
            bullets[i].Initialize(bulletSpawnPoint.right, finalBulletSpeed, finalDamage, finalPenetration, finalBounce);
            poolManager.Activate(gunBaseData.BulletPrefab, bullets[i]);
        }

    }

}
