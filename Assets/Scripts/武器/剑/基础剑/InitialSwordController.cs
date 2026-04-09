using System.Collections;
using UnityEngine;

public class InitialSwordController : WeaponControllerBase
{
    [SerializeField, ChineseLabel("旋转轴")] protected Transform rotationPivot;
    [SerializeField, ChineseLabel("动画控制器")] protected Animator animator;

    [SerializeField, ChineseLabel("默认动画片段名称")] protected string defaultAnimationName;
    protected int defaultAnimationHash;
    [SerializeField, ChineseLabel("攻击左动画名称")] protected string attackLeftAnimationName;
    protected int attackLeftAnimationHash;

    [SerializeField, ChineseLabel("攻击右动画名称")] protected string attackRightAnimationName;
    protected int attackRightAnimationHash;

    [SerializeField, ChineseLabel("攻击碰撞箱")] protected Collider2D attackCollider;
    private SwordDate M_swordData => WeaponData as SwordDate; 

    protected override void Awake()
    {
        base.Awake();
        if (animator)
        {
            defaultAnimationHash = Animator.StringToHash(defaultAnimationName);
            attackLeftAnimationHash = Animator.StringToHash(attackLeftAnimationName);
            attackRightAnimationHash = Animator.StringToHash(attackRightAnimationName);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        MultiTimerManager.Create_DownTimer("SwordAttackCooldown", 0f);

        animator.Play(defaultAnimationHash);
        attackCollider.enabled = false;

        M_swordData.CurrentSwordState = SwordState.Idle;
        
    }

    protected override void Update()
    {
        if(inputManager.CurrentMouseState == MouseState.Hold)
        {
            HandleMouseHold();
        }
    }

    protected override void LateUpdate()
    {
        if(!CanControl || M_swordData.CurrentSwordState == SwordState.Attack)
        {
            return;
        }

        Vector2 mouseWorldPosition = inputManager.MouseWorldPosition;

        ObjectRotation.RotateTowardsTarget(rotationPivot.transform, mouseWorldPosition, M_swordData.WeaponBaseData.WeaponRotationSpeed, -90f);
    }

    

    protected override void HandleMouseClick()
    {
        Attack();
    }

    protected override void HandleMouseHold()
    {
        Attack();
    }

    public override void Attack()
    {
        if (!MultiTimerManager.IsDownTimerComplete("SwordAttackCooldown") || 
            M_swordData.CurrentSwordState == SwordState.Attack)
        {
            return;
        }

        M_swordData.CurrentSwordState = SwordState.Attack;

        StartCoroutine(AttackAnimationFinishCheck());
    }

    protected virtual IEnumerator AttackAnimationFinishCheck()
    {
        int currentAttackAnimationHash = IsSwordFacingLeft() ? attackLeftAnimationHash : attackRightAnimationHash;
        
        animator.Play(currentAttackAnimationHash);
        attackCollider.enabled = true;
        
        yield return null; // 等待一帧，确保动画状态机更新
        
        yield return new WaitUntil(() => AnimatorTool.IsAnimationFinished(animator, currentAttackAnimationHash));

        animator.transform.localRotation = new Quaternion(0, 0, 0, 1); // 重置旋转
        animator.Play(defaultAnimationHash);
        yield return null; // 等待一帧，确保动画状态机更新

        attackCollider.enabled = false;

        M_swordData.CurrentSwordState = SwordState.Idle;
        MultiTimerManager.Start_DownTimer("SwordAttackCooldown", weaponManager.GetFinalAttackInterval(M_swordData.WeaponBaseData.AttackInterval));
    }

    /// <summary>
    /// 剑是否朝向左边
    /// </summary>
    protected virtual bool IsSwordFacingLeft()
    {
        float zRotation = rotationPivot.transform.eulerAngles.z;
        return zRotation >= 0f && zRotation < 180f;
    }
}