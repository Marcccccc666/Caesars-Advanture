using UnityEngine;

public class GunController : WeaponBase
{
    [Header("动画相关")]
    [SerializeField, ChineseLabel("动画控制器")] protected Animator gunAnimator;
    [SerializeField, ChineseLabel("射击动画名")] protected string shootAnimationName = "Shoot";

    [Header("射击相关")]
    [SerializeField, ChineseLabel("子弹生成点")] protected Transform bulletSpawnPoint;

    /// <summary>
    /// 枪械数据
    /// </summary>
    private GunData M_gunData => WeaponData as GunData;

    protected override void Awake()
    {
        base.Awake();
        
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
                Attack();
                MultiTimerManager.Start_DownTimer("GunAttackCooldown", weaponManager.GetFinalAttackInterval(M_gunData.WeaponBaseData.AttackInterval));
            }
        }
    }



    protected override void Attack()
    {
        if(gunAnimator != null)
        {
            gunAnimator.Play(Animator.StringToHash(shootAnimationName));
        }

        GunBaseData gunBaseData = M_gunData.WeaponBaseData as GunBaseData;

        int bulletCount = M_gunData.GetFinalBallisticsCount;
        int finalDamage = weaponManager.GetFinalDamage(gunBaseData.WeaponDamage);
        int finalPenetration = M_gunData.GetFinalPenetration;
        
        BulletAttack[] bullets = new BulletAttack[bulletCount];
        

        // 计算每个子弹的生成位置
        Vector3[] instancePositions = new Vector3[bulletCount];
        instancePositions = BulletMovement.BulletMoveTypes(
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
            bullets[i].Initialize(bulletSpawnPoint.right, gunBaseData.BulletSpeed, finalDamage, finalPenetration);
            poolManager.Activate(gunBaseData.BulletPrefab, bullets[i]);
        }

        AudioSource.PlayClipAtPoint(M_attackAudioClip, Camera.main.transform.position);
    }

}
