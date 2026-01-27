using UnityEngine;

public class GunController : WeaponBase
{
    [SerializeField, ChineseLabel("武器数据")]private GunBaseData GunData;

    [Header("动画相关")]
    [SerializeField, ChineseLabel("动画控制器")] private Animator gunAnimator;
    [SerializeField, ChineseLabel("射击动画名")] private string shootAnimationName = "Shoot";

    [Header("射击相关")]
    [SerializeField, ChineseLabel("子弹生成点")] private Transform bulletSpawnPoint;

    [SerializeField,ChineseLabel("攻击音效")]private AudioClip M_attackAudioClip;


    private WeaponManager weaponManager => WeaponManager.Instance;

    private void Awake()
    {
        weaponManager.SwitchWeapon(GunData);
    }

    public override void Attack()
    {
        base.Attack();
        gunAnimator.Play(shootAnimationName);
        // 实例化子弹
        Rigidbody2D bulletInstance = Instantiate(GunData.BulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        BulletAttack bulletAttack = bulletInstance.GetComponent<BulletAttack>();
        bulletAttack.SetBulletDamage(GunData.WeaponDamage);
        // 这里可以添加子弹的初速度等属性设置
        bulletInstance.AddForce(bulletSpawnPoint.right * GunData.BulletSpeed, ForceMode2D.Impulse);
        AudioSource.PlayClipAtPoint(M_attackAudioClip, Camera.main.transform.position);
    }

}
