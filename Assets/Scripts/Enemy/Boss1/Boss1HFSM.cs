using System.Collections;
using UnityEngine;
using UnityHFSM;

public enum Boss1 { }

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyData))]
public class Boss1HFSM : MonoBehaviour
{
    [Header("阶段一 - 攻击1: 反弹冲撞")]
    [SerializeField, ChineseLabel("冲撞速度")] private float p1Attack1Speed = 12f;
    [SerializeField, ChineseLabel("反弹次数")] private int p1Attack1BounceCount = 4;
    [SerializeField, ChineseLabel("眩晕时长")] private float p1Attack1StunDuration = 2f;

    [Header("阶段一 - 攻击2: 冲刺射击")]
    [SerializeField, ChineseLabel("蓄力时长")] private float p1Attack2ChargeUpDuration = 0.8f;
    [SerializeField, ChineseLabel("冲刺速度")] private float p1Attack2DashSpeed = 10f;
    [SerializeField, ChineseLabel("冲刺时长")] private float p1Attack2DashDuration = 1f;
    [SerializeField, ChineseLabel("冲刺间隔")] private float p1Attack2DashInterval = 0.8f;
    [SerializeField, ChineseLabel("冲刺期间子弹发射间隔")] private float p1Attack2SideBulletInterval = 0.15f;
    [SerializeField, ChineseLabel("冲刺次数")] private int p1Attack2DashCount = 3;
    [SerializeField, ChineseLabel("眩晕时长(攻击2后)")] private float p1Attack2StunDuration = 2f;

    [Header("阶段一 - 攻击3: 射线旋转")]
    [SerializeField, ChineseLabel("攻击持续时间")] private float p1Attack3Duration = 4f;
    [SerializeField, ChineseLabel("旋转速度(度/秒)")] private float p1Attack3RotateSpeed = 90f;
    [SerializeField, ChineseLabel("射线长度")] private float p1Attack3LaserLength = 15f;
    [SerializeField, ChineseLabel("射线伤害间隔")] private float p1Attack3DamageInterval = 0.5f;

    [Header("阶段二 - 攻击1: 反弹冲撞(强化)")]
    [SerializeField, ChineseLabel("冲撞速度")] private float p2Attack1Speed = 16f;
    [SerializeField, ChineseLabel("反弹次数")] private int p2Attack1BounceCount = 5;
    [SerializeField, ChineseLabel("眩晕时长")] private float p2Attack1StunDuration = 1.5f;

    [Header("阶段二 - 攻击2: 冲刺射击(强化)")]
    [SerializeField, ChineseLabel("蓄力时长")] private float p2Attack2ChargeUpDuration = 0.6f;
    [SerializeField, ChineseLabel("冲刺速度")] private float p2Attack2DashSpeed = 12f;
    [SerializeField, ChineseLabel("冲刺时长")] private float p2Attack2DashDuration = 1f;
    [SerializeField, ChineseLabel("冲刺间隔")] private float p2Attack2DashInterval = 0.5f;
    [SerializeField, ChineseLabel("冲刺期间子弹发射间隔")] private float p2Attack2SideBulletInterval = 0.12f;
    [SerializeField, ChineseLabel("冲刺次数")] private int p2Attack2DashCount = 5;
    [SerializeField, ChineseLabel("眩晕时长(攻击2后)")] private float p2Attack2StunDuration = 1.5f;

    [Header("阶段二 - 攻击3: 弹幕旋转")]
    [SerializeField, ChineseLabel("攻击持续时间")] private float p2Attack3Duration = 3f;
    [SerializeField, ChineseLabel("旋转速度(度/秒)")] private float p2Attack3RotateSpeed = 120f;
    [SerializeField, ChineseLabel("子弹发射间隔")] private float p2Attack3BulletInterval = 0.1f;

    [Header("子弹配置")]
    [SerializeField, ChineseLabel("子弹预制体")] private EnemyBulletAttack bulletPrefab;
    [SerializeField, ChineseLabel("子弹速度")] private float bulletSpeed = 8f;
    [SerializeField, ChineseLabel("子弹存活时长")] private float bulletLifetime = 3f;

    [Header("射线配置")]
    [SerializeField, ChineseLabel("射线宽度")] private float laserWidth = 0.12f;
    [SerializeField, ChineseLabel("射线颜色")] private Color laserColor = Color.red;
    [SerializeField, ChineseLabel("射线排序层级")] private int laserSortingOrder = 50;

    [Header("碰撞检测")]
    [SerializeField, ChineseLabel("墙壁层")] private LayerMask wallMask;
    [SerializeField, ChineseLabel("伤害检测层")] private LayerMask damageMask;

    [Header("接触伤害")]
    [SerializeField, ChineseLabel("冲撞伤害冷却")] private float contactDamageCooldown = 0.5f;

    [Header("动画")]
    [SerializeField, ChineseLabel("动画控制器")] private Animator bossAnimator;
    [SerializeField, ChineseLabel("待机动画")] private string idleAnimState = "idle";
    [SerializeField, ChineseLabel("攻击动画")] private string attackAnimState = "attack";
    [SerializeField, ChineseLabel("眩晕动画")] private string stunAnimState = "stun";
    [SerializeField, ChineseLabel("阶段转换动画")] private string phaseTransitionAnimState = "phase2";
    [SerializeField, ChineseLabel("死亡动画")] private string dieAnimState = "die";

    [Header("阶段转换")]
    [SerializeField, ChineseLabel("暂停时长")] private float phaseTransitionPauseDuration = 3f;
    [SerializeField, ChineseLabel("转换动画时长")] private float phaseTransitionAnimDuration = 1f;

    private EnemyData enemyData;
    private Rigidbody2D rb2D;
    private float colliderRadius = 0.5f;

    private int currentPhase = 1;
    private int attackCycleIndex;
    private bool attackComplete;
    private float currentStunDuration;
    private bool stunComplete;
    private bool phaseTransitionComplete;

    private Boss1_Attack1 attack1State;
    private Boss1_Attack2 attack2State;

    private Boss1StateID? lastAnimState;
    private readonly StateMachine<Boss1StateID, Boss1> stateMachine = new();

    public enum Boss1StateID
    {
        Idle, Attack1, Attack2, Attack3,
        Stun, PhaseTransition, Die
    }

    #region Public References

    public Rigidbody2D Rb2D => rb2D;
    public EnemyData EnemyDataRef => enemyData;
    public float ColliderRadius => colliderRadius;
    public LayerMask WallMask => wallMask;
    public LayerMask DamageMask => damageMask;
    public float ContactDamageCooldown => contactDamageCooldown;
    public int CurrentPhase => currentPhase;
    public float CurrentStunDuration => currentStunDuration;

    public float Attack1Speed => currentPhase == 1 ? p1Attack1Speed : p2Attack1Speed;
    public int Attack1TargetBounces => currentPhase == 1 ? p1Attack1BounceCount : p2Attack1BounceCount;

    public float Attack2ChargeUpDuration => currentPhase == 1 ? p1Attack2ChargeUpDuration : p2Attack2ChargeUpDuration;
    public float Attack2DashSpeed => currentPhase == 1 ? p1Attack2DashSpeed : p2Attack2DashSpeed;
    public float Attack2DashDuration => currentPhase == 1 ? p1Attack2DashDuration : p2Attack2DashDuration;
    public float Attack2DashInterval => currentPhase == 1 ? p1Attack2DashInterval : p2Attack2DashInterval;
    public float Attack2SideBulletInterval => currentPhase == 1 ? p1Attack2SideBulletInterval : p2Attack2SideBulletInterval;
    public int Attack2TargetDashCount => currentPhase == 1 ? p1Attack2DashCount : p2Attack2DashCount;

    public float Attack3Duration => currentPhase == 1 ? p1Attack3Duration : p2Attack3Duration;
    public float Attack3RotateSpeed => currentPhase == 1 ? p1Attack3RotateSpeed : p2Attack3RotateSpeed;
    public float Attack3LaserLength => p1Attack3LaserLength;
    public float Attack3DamageInterval => p1Attack3DamageInterval;
    public float Attack3BulletInterval => p2Attack3BulletInterval;
    public float LaserWidth => laserWidth;
    public Color LaserColor => laserColor;
    public int LaserSortingOrder => laserSortingOrder;
    public float BulletSpeed => bulletSpeed;
    public float BulletLifetime => bulletLifetime;

    #endregion

    private Transform playerTransform
    {
        get
        {
            CharacterManager cm = CharacterManager.Instance;
            if (cm == null || cm.GetCurrentPlayerCharacterData == null)
                return null;
            return cm.GetCurrentPlayerCharacterData.transform;
        }
    }

    private PoolManager poolManager => PoolManager.Instance;

    private EnemyManager enemyManager => EnemyManager.Instance;

    private void Awake()
    {
        enemyData = GetComponent<EnemyData>();
        enemyData.InitObjectData();
        enemyData.OnDamage += OnTakeDamage;

        rb2D = GetComponent<Rigidbody2D>();
        if (bossAnimator == null)
            bossAnimator = GetComponent<Animator>();

        CircleCollider2D cc = GetComponent<CircleCollider2D>();
        if (cc != null)
            colliderRadius = cc.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);

        BuildStateMachine();
    }

    private void Start()
    {
        stateMachine.Init();
        UpdateAnimation(force: true);
    }

    private void Update()
    {
        if (!CanSwitchState())
            return;

        stateMachine.OnLogic();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        rb2D.angularVelocity = 0f;

        if (!CanSwitchState())
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        Boss1StateID active = stateMachine.ActiveStateName;
        if (active == Boss1StateID.Attack1)
            attack1State.FixedTick();
        else if (active == Boss1StateID.Attack2)
            attack2State.FixedTick();
        else
            rb2D.linearVelocity = Vector2.zero;
    }

    private void BuildStateMachine()
    {
        attack1State = new Boss1_Attack1(this);
        attack2State = new Boss1_Attack2(this);

        stateMachine.AddState(Boss1StateID.Idle, new Boss1_Idle(this));
        stateMachine.AddState(Boss1StateID.Attack1, attack1State);
        stateMachine.AddState(Boss1StateID.Attack2, attack2State);
        stateMachine.AddState(Boss1StateID.Attack3, new Boss1_Attack3(this));
        stateMachine.AddState(Boss1StateID.Stun, new Boss1_Stun(this));
        stateMachine.AddState(Boss1StateID.PhaseTransition, new Boss1_PhaseTransition(this));
        stateMachine.AddState(Boss1StateID.Die, new Boss1_Die(this));

        stateMachine.AddTransition(
            Boss1StateID.Idle, Boss1StateID.Attack1,
            _ => CanSwitchState());

        stateMachine.AddTransition(
            Boss1StateID.Attack1, Boss1StateID.PhaseTransition,
            _ => ShouldTransitionToPhase2());
        stateMachine.AddTransition(
            Boss1StateID.Attack1, Boss1StateID.Stun,
            _ => attackComplete);

        stateMachine.AddTransition(
            Boss1StateID.Stun, Boss1StateID.PhaseTransition,
            _ => ShouldTransitionToPhase2());
        stateMachine.AddTransition(
            Boss1StateID.Stun, Boss1StateID.Attack2,
            _ => stunComplete && attackCycleIndex == 0);
        stateMachine.AddTransition(
            Boss1StateID.Stun, Boss1StateID.Attack3,
            _ => stunComplete && attackCycleIndex == 1);

        stateMachine.AddTransition(
            Boss1StateID.Attack2, Boss1StateID.PhaseTransition,
            _ => ShouldTransitionToPhase2());
        stateMachine.AddTransition(
            Boss1StateID.Attack2, Boss1StateID.Stun,
            _ => attackComplete);

        stateMachine.AddTransition(
            Boss1StateID.Attack3, Boss1StateID.PhaseTransition,
            _ => ShouldTransitionToPhase2());
        stateMachine.AddTransition(
            Boss1StateID.Attack3, Boss1StateID.Attack1,
            _ => attackComplete);

        stateMachine.AddTransition(
            Boss1StateID.PhaseTransition, Boss1StateID.Attack1,
            _ => phaseTransitionComplete);

        stateMachine.SetStartState(Boss1StateID.Idle);
    }

    private void OnTakeDamage(int damage)
    {
        if (enemyData == null || enemyData.CurrentHealth > 0)
            return;

        BuffManager.Instance?.EnemyKilledTriggered?.Invoke(transform);
        enemyManager.RemoveEnemyData(gameObject.GetInstanceID());
        gameObject.SetActive(false);
    }

    private bool CanSwitchState()
    {
        return enemyData != null && enemyData.PlayerEnterRoom;
    }

    private bool ShouldTransitionToPhase2()
    {
        return currentPhase == 1
            && enemyData != null
            && enemyData.CurrentHealth <= enemyData.MaxHealth / 2;
    }

    #region State Notifications

    public void OnAttackStart()
    {
        attackComplete = false;
        rb2D.linearVelocity = Vector2.zero;
    }

    public void OnAttackComplete(int cycleIndex)
    {
        attackComplete = true;
        attackCycleIndex = cycleIndex;

        if (cycleIndex == 0)
            currentStunDuration = currentPhase == 1 ? p1Attack1StunDuration : p2Attack1StunDuration;
        else if (cycleIndex == 1)
            currentStunDuration = currentPhase == 1 ? p1Attack2StunDuration : p2Attack2StunDuration;
    }

    public void EnterIdle()
    {
        rb2D.linearVelocity = Vector2.zero;
    }

    public void EnterStun()
    {
        rb2D.linearVelocity = Vector2.zero;
        stunComplete = false;
    }

    public void SetStunComplete()
    {
        stunComplete = true;
    }

    public void EnterPhaseTransition()
    {
        phaseTransitionComplete = false;
        rb2D.linearVelocity = Vector2.zero;
        StartCoroutine(PhaseTransitionRoutine());
    }

    public void EnterDie()
    {
        rb2D.linearVelocity = Vector2.zero;
    }

    #endregion

    #region Shared Utilities

    public Vector2 GetDirectionToPlayer()
    {
        if (playerTransform == null)
            return Vector2.right;

        Vector2 dir = (Vector2)playerTransform.position - rb2D.position;
        return dir.sqrMagnitude < 0.0001f ? Vector2.right : dir.normalized;
    }

    public void SpawnBullet(Vector2 position, Vector2 direction)
    {
        if (bulletPrefab == null)
            return;

        Vector2 dir = direction.sqrMagnitude < 0.0001f ? Vector2.right : direction.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        int damage = enemyData != null ? enemyData.CurrentAttack : 1;

        EnemyBulletAttack bullet = null;
        if (poolManager != null)
        {
            bullet = poolManager.Spawn(
                prefab: bulletPrefab,
                pos: position,
                rot: Quaternion.Euler(0f, 0f, angle),
                defaultCapacity: 40,
                maxSize: 200,
                setActive: true);
        }
        else
        {
            bullet = Instantiate(bulletPrefab, position, Quaternion.Euler(0f, 0f, angle));
        }

        if (bullet != null)
        {
            bullet.SetBulletDamage(damage);
            bullet.Launch(dir, bulletSpeed, bulletLifetime, transform);
        }
    }

    public static Vector2 DegreeToDirection(float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    public bool IsSelfCollider(Collider2D col)
    {
        if (col == null)
            return false;
        Transform t = col.transform;
        return t == transform || t.IsChildOf(transform);
    }

    public CharacterDate GetPlayerData(Collider2D col)
    {
        if (col == null)
            return null;

        if (col.CompareTag("Player"))
        {
            CharacterDate direct = col.GetComponentInParent<CharacterDate>();
            if (direct != null)
                return direct;
        }

        CharacterManager cm = CharacterManager.Instance;
        if (cm == null)
            return null;

        CharacterDate current = cm.GetCurrentPlayerCharacterData;
        if (current == null)
            return null;

        Transform hitT = col.transform;
        Transform playerRoot = current.transform;
        if (hitT == playerRoot || hitT.IsChildOf(playerRoot))
            return current;

        return col.GetComponentInParent<CharacterDate>();
    }

    #endregion

    #region Phase Transition

    private IEnumerator PhaseTransitionRoutine()
    {
        Time.timeScale = 0f;
        CameraShake.Shake(7);
        yield return new WaitForSecondsRealtime(phaseTransitionPauseDuration);

        Time.timeScale = 1f;
        currentPhase = 2;
        attackCycleIndex = 0;

        TryPlayAnimatorState(phaseTransitionAnimState);

        if (phaseTransitionAnimDuration > 0f)
            yield return new WaitForSeconds(phaseTransitionAnimDuration);

        phaseTransitionComplete = true;
    }

    #endregion

    #region Animation

    private void UpdateAnimation(bool force = false)
    {
        Boss1StateID current = stateMachine.ActiveStateName;
        if (!force && lastAnimState.HasValue && lastAnimState.Value == current)
            return;

        PlayAnimationForState(current);
        lastAnimState = current;
    }

    private void PlayAnimationForState(Boss1StateID state)
    {
        switch (state)
        {
            case Boss1StateID.Idle:
                TryPlayAnimatorState(idleAnimState);
                break;
            case Boss1StateID.Attack1:
            case Boss1StateID.Attack2:
            case Boss1StateID.Attack3:
                TryPlayAnimatorState(attackAnimState);
                break;
            case Boss1StateID.Stun:
                TryPlayAnimatorState(stunAnimState);
                break;
            case Boss1StateID.PhaseTransition:
                TryPlayAnimatorState(phaseTransitionAnimState);
                break;
            case Boss1StateID.Die:
                TryPlayAnimatorState(dieAnimState);
                break;
        }
    }

    private bool TryPlayAnimatorState(string stateName)
    {
        if (bossAnimator == null || string.IsNullOrWhiteSpace(stateName))
            return false;

        int hash = Animator.StringToHash(stateName);
        if (bossAnimator.HasState(0, hash))
        {
            bossAnimator.Play(hash);
            return true;
        }

        int fullHash = Animator.StringToHash($"Base Layer.{stateName}");
        if (bossAnimator.HasState(0, fullHash))
        {
            bossAnimator.Play(fullHash);
            return true;
        }

        return false;
    }

    #endregion
}
