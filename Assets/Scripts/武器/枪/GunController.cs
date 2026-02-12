using UnityEngine;

public class GunController : WeaponBase
{
    [Header("动画相关")]
    [SerializeField, ChineseLabel("动画控制器")] private Animator gunAnimator;
    [SerializeField, ChineseLabel("射击动画名")] private string shootAnimationName = "Shoot";

    [Header("射击相关")]
    [SerializeField, ChineseLabel("子弹生成点")] private Transform bulletSpawnPoint;

    [SerializeField,ChineseLabel("攻击音效")]private AudioClip M_attackAudioClip;

    /// <summary>
    /// 枪械数据
    /// </summary>
    private GunData M_gunData => WeaponData as GunData;
    
    private InputData inputData => InputData.Instance;
    private WeaponManager weaponManager => WeaponManager.Instance;
    private MultiTimerManager MultiTimerManager => MultiTimerManager.Instance;

    private void Awake()
    {
        weaponManager.SwitchWeapon(WeaponData);
        MultiTimerManager.Create_DownTimer("GunAttackCooldown");
    }

    protected override void Update()
    {
        if(!gameManager.IsPlayerControllable)
        {
            return;
        }
        base.Update();

        if(inputData.CurrentMouseState == MouseState.Press || inputData.CurrentMouseState == MouseState.Hold)
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
        gunAnimator.Play(Animator.StringToHash(shootAnimationName));

        GunBaseData gunBaseData = M_gunData.WeaponBaseData as GunBaseData;

        int bulletCount = M_gunData.GetFinalBallisticsCount;
        int finalDamage = weaponManager.GetFinalDamage(gunBaseData.WeaponDamage);
        int finalPenetration = M_gunData.GetFinalPenetration;
        
        BulletAttack[] bullets = new BulletAttack[bulletCount];
        

        // 计算每个子弹的生成位置
        Vector3[] instancePositions = new Vector3[bulletCount];
        instancePositions = BulletMovement.MoveBullet(bulletSpawnPoint, gunBaseData.BulletIntervalDistance, bulletCount);

        // 实例化子弹
        for (int i = 0; i < bulletCount; i++)
        {
            // 实例化子弹并设置其伤害
            bullets[i] = Instantiate(gunBaseData.BulletPrefab, instancePositions[i], bulletSpawnPoint.rotation);
            bullets[i].SetBulletDamage(finalDamage);
            bullets[i].SetBulletPenetration(finalPenetration);
        }

        // 发射子弹
        foreach (var bullet in bullets)
        {
            bullet.GetRG2D.AddForce(bulletSpawnPoint.right * gunBaseData.BulletSpeed, ForceMode2D.Impulse);
        }

        AudioSource.PlayClipAtPoint(M_attackAudioClip, Camera.main.transform.position);
    }

}
