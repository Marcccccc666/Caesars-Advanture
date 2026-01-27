using UnityEngine;


public class Attack : CharacterState<Caesar_Controller.Caesar_StateID>
{
    /// <summary>
    /// 子弹预制体
    /// </summary>
    private Rigidbody2D M_bulletPrefab;

    /// <summary>
    /// 枪口位置
    /// </summary>
    private Transform M_gunMuzzle;

    /// <summary>
    /// 子弹移动速度
    /// </summary>
    private float M_bulletSpeed;

    ///<summary>
    /// 攻击音效
    /// </summary>
    private AudioClip M_attackAudioClip;

    public Attack(AudioClip attackAudioClip, Rigidbody2D bulletPrefab, Transform gunMuzzle, float bulletSpeed) : base(needsExitTime:true)
    {
        this.M_bulletPrefab = bulletPrefab;
        this.M_gunMuzzle = gunMuzzle;
        this.M_bulletSpeed = bulletSpeed;
        this.M_attackAudioClip = attackAudioClip;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        InputData.IsAttack = false;
        ExecuteAttack();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        OnExitRequest();
    }

    /// <summary>
    /// 攻击
    /// </summary>
    private void ExecuteAttack()
    {
        if (M_bulletPrefab == null)
        {
            Debug.LogError("子弹预制体未赋值");
            return;
        }
        Rigidbody2D bullet = Object.Instantiate(M_bulletPrefab, M_gunMuzzle.position, M_gunMuzzle.rotation);
        Vector2 force = M_gunMuzzle.right * M_bulletSpeed;
        bullet.AddForce(force, ForceMode2D.Impulse);
        AudioSource.PlayClipAtPoint(M_attackAudioClip, Camera.main.transform.position);
    }
}
