using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityHFSM;

public enum Boss2 { }

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyData))]
public class Boss2HFSM : MonoBehaviour
{
    [Header("一阶段参数")]
    [SerializeField, ChineseLabel("一阶段移动速度")] private float p1MoveSpeed = 5f;
    [SerializeField, ChineseLabel("破土前停顿(s)")] private float p1PreEmergePause = 1f;
    [SerializeField] private float p1EmergeDuration = 0.7f;
    [SerializeField] private float p1SweepDuration = 1.2f;
    [SerializeField, ChineseLabel("攻击后停顿(s)")] private float p1PostAttackPause = 2f;

    [Header("二阶段参数")]
    [SerializeField, ChineseLabel("二阶段移动速度")] private float p2MoveSpeed = 8f;
    [SerializeField, ChineseLabel("破土前停顿(s)")] private float p2PreEmergePause = 0.5f;
    [SerializeField] private float p2EmergeDuration = 0.6f;
    [SerializeField] private float p2SweepDuration = 1f;
    [SerializeField, ChineseLabel("攻击后停顿(s)")] private float p2PostAttackPause = 1f;
    [SerializeField, ChineseLabel("二阶段开场连击次数")] private int p2ComboCount = 3;

    [Header("攻击设置")]
    [SerializeField, ChineseLabel("破土伤害")] private int emergeDamage = 2;
    [SerializeField, ChineseLabel("破土范围")] private float emergeRadius = 1.5f;
    [SerializeField, ChineseLabel("甩身体伤害")] private int sweepDamage = 1;
    [SerializeField, ChineseLabel("甩身体范围")] private float sweepRadius = 2.5f;

    [Header("碰撞检测")]
    [SerializeField, ChineseLabel("伤害检测层")] private LayerMask damageMask;

    [Header("动画")]
    [SerializeField, ChineseLabel("动画控制器")] private Animator bossAnimator;
    [SerializeField, ChineseLabel("待机动画")] private string idleAnim = "idle";
    [SerializeField, ChineseLabel("钻地动画")] private string moveAnim = "move";
    [SerializeField, ChineseLabel("破土动画")] private string emergeAnim = "emerge";
    [SerializeField, ChineseLabel("攻击动画")] private string sweepAnim = "sweep";
    [SerializeField, ChineseLabel("死亡动画")] private string dieAnim = "die";
    [SerializeField, ChineseLabel("死亡动画时长(s)")] private float dieAnimationDuration = 1f;

    [Header("休眠")]
    [SerializeField, ChineseLabel("生成休眠时长(s)")] private float spawnSleepDuration = 2f;

    public UnityEvent BossDie;
    private EnemyData enemyData;
    private Rigidbody2D rb2D;
    private Collider2D bodyCollider;

    private int currentPhase = 1;
    private bool isDead = false;
    private bool phaseTransitionComplete = false;
    private int p2ComboDone = 0;
    private bool emergeDamageArmed = false;
    private bool sweepDamageArmed = false;
    private bool isSleeping = false;
    private float sleepTimer = 0f;
    private Coroutine dieRoutine;

    private readonly StateMachine<Boss2StateID, Boss2> stateMachine = new();

    public enum Boss2StateID
    {
        Idle, Move, EmergeAttack, SweepAttack, PhaseTransition, Die
    }

    #region Properties
    public Rigidbody2D Rb2D => rb2D;
    public EnemyData EnemyDataRef => enemyData;
    public LayerMask DamageMask => damageMask;
    public int CurrentPhase => currentPhase;
    public float MoveSpeed => currentPhase == 1 ? p1MoveSpeed : p2MoveSpeed;
    public float PreEmergePause => currentPhase == 1 ? p1PreEmergePause : p2PreEmergePause;
    public float EmergeDuration => currentPhase == 1 ? p1EmergeDuration : p2EmergeDuration;
    public float SweepDuration => currentPhase == 1 ? p1SweepDuration : p2SweepDuration;
    public float PostAttackPause => currentPhase == 1 ? p1PostAttackPause : p2PostAttackPause;
    public int P2ComboCount => p2ComboCount;
    public int P2ComboDone { get => p2ComboDone; set => p2ComboDone = value; }

    public int EmergeDamage => emergeDamage;
    public float EmergeRadius => emergeRadius;
    public int SweepDamage => sweepDamage;
    public float SweepRadius => sweepRadius;
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

    private void Awake()
    {
        enemyData = GetComponent<EnemyData>();
        enemyData.InitObjectData();
        enemyData.OnDamage += OnTakeDamage;
        enemyData.OnDie += OnDieAction;

        rb2D = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        if (bossAnimator == null)
            bossAnimator = GetComponent<Animator>();

        AttackRangeGizmo attackRangeGizmo = GetComponentInChildren<AttackRangeGizmo>();
        if (attackRangeGizmo != null)
        {
            sweepRadius = attackRangeGizmo.GetAttackRange;
        }

        BuildStateMachine();
    }

    private void Start()
    {
        stateMachine.Init();
    }

    private void OnEnable()
    {
        BeginSpawnSleep();
    }

    private void OnDestroy()
    {
        if (enemyData == null)
            return;

        enemyData.OnDamage -= OnTakeDamage;
        enemyData.OnDie -= OnDieAction;
    }

    private void Update()
    {
        if (isDead) return;

        if (UpdateSleepState())
            return;

        stateMachine.OnLogic();
    }

    private void FixedUpdate()
    {
        if (isDead || isSleeping)
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        Boss2StateID active = stateMachine.ActiveStateName;
        if (active == Boss2StateID.Move)
        {
            moveState.FixedTick();
        }
        else
        {
            rb2D.linearVelocity = Vector2.zero;
        }
    }

    private Boss2_Move moveState;

    private void BuildStateMachine()
    {
        moveState = new Boss2_Move(this);

        stateMachine.AddState(Boss2StateID.Idle, new Boss2_Idle(this));
        stateMachine.AddState(Boss2StateID.Move, moveState);
        stateMachine.AddState(Boss2StateID.EmergeAttack, new Boss2_EmergeAttack(this));
        stateMachine.AddState(Boss2StateID.SweepAttack, new Boss2_SweepAttack(this));
        stateMachine.AddState(Boss2StateID.PhaseTransition, new Boss2_PhaseTransition(this));
        stateMachine.AddState(Boss2StateID.Die, new Boss2_Die(this));

        stateMachine.AddTransition(Boss2StateID.Idle, Boss2StateID.Move, _ => true);
        stateMachine.AddTransition(Boss2StateID.Move, Boss2StateID.EmergeAttack, _ => true);

        stateMachine.AddTransition(Boss2StateID.EmergeAttack, Boss2StateID.Move, _ =>
        {
            return currentPhase == 2 && p2ComboDone + 1 < p2ComboCount;
        });

        stateMachine.AddTransition(Boss2StateID.EmergeAttack, Boss2StateID.SweepAttack, _ =>
        {
            return !(currentPhase == 2 && p2ComboDone + 1 < p2ComboCount);
        });

        stateMachine.AddTransition(Boss2StateID.SweepAttack, Boss2StateID.Idle, _ => true);
        stateMachine.AddTransition(Boss2StateID.PhaseTransition, Boss2StateID.Move, _ => phaseTransitionComplete);

        stateMachine.AddTransition(new Transition<Boss2StateID>(Boss2StateID.Idle, Boss2StateID.Die, _ => isDead));
        stateMachine.AddTransition(new Transition<Boss2StateID>(Boss2StateID.Move, Boss2StateID.Die, _ => isDead));
        stateMachine.AddTransition(new Transition<Boss2StateID>(Boss2StateID.EmergeAttack, Boss2StateID.Die, _ => isDead));
        stateMachine.AddTransition(new Transition<Boss2StateID>(Boss2StateID.SweepAttack, Boss2StateID.Die, _ => isDead));

        stateMachine.SetStartState(Boss2StateID.Idle);
    }

    private void OnTakeDamage(int damage)
    {
        if (enemyData == null || isDead)
            return;

        if (enemyData.CurrentHealth <= 0)
        {
            OnDieAction();
            return;
        }

        if (currentPhase == 1 && enemyData.CurrentHealth <= enemyData.MaxHealth * 0.5f)
        {
            currentPhase = 2;
            stateMachine.RequestStateChange(Boss2StateID.PhaseTransition);
        }
    }

    private void OnDieAction()
    {
        if (isDead)
            return;

        isDead = true;
        isSleeping = false;
        sleepTimer = 0f;
        if (enemyData != null)
        {
            enemyData.PlayerEnterRoom = false;
        }

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
        }

        DisableAllColliders();
        EnterDie();
        stateMachine.RequestStateChange(Boss2StateID.Die);

        if (dieRoutine == null)
        {
            dieRoutine = StartCoroutine(DieAndDestroyRoutine());
        }

        if (BossDie != null)
        {
            BossDie.Invoke();
        }
    }

    private IEnumerator DieAndDestroyRoutine()
    {
        yield return null;

        float waitDuration = GetCurrentAnimationLength();
        if (waitDuration > 0f)
        {
            yield return new WaitForSeconds(waitDuration);
        }

        Destroy(gameObject);
    }

    private float GetCurrentAnimationLength()
    {
        if (bossAnimator == null)
            return Mathf.Max(0f, dieAnimationDuration);

        AnimatorStateInfo stateInfo = bossAnimator.GetCurrentAnimatorStateInfo(0);
        if ((stateInfo.IsName(dieAnim) || stateInfo.IsName($"Base Layer.{dieAnim}")) && stateInfo.length > 0f)
        {
            return stateInfo.length;
        }

        return Mathf.Max(0f, dieAnimationDuration);
    }

    private void DisableAllColliders()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
    }

    private void BeginSpawnSleep()
    {
        if (enemyData == null)
            return;

        if (spawnSleepDuration <= 0f)
        {
            isSleeping = false;
            sleepTimer = 0f;
            enemyData.PlayerEnterRoom = true;
            RefreshInvulnerableState();
            return;
        }

        isSleeping = true;
        sleepTimer = spawnSleepDuration;
        enemyData.PlayerEnterRoom = false;

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
        }

        SetInvulnerable(true);
    }

    private bool UpdateSleepState()
    {
        if (!isSleeping)
            return false;

        sleepTimer -= Time.deltaTime;
        if (sleepTimer > 0f)
        {
            if (rb2D != null)
            {
                rb2D.linearVelocity = Vector2.zero;
            }

            return true;
        }

        EndSpawnSleep();
        return false;
    }

    private void EndSpawnSleep()
    {
        isSleeping = false;
        sleepTimer = 0f;

        if (enemyData != null)
        {
            enemyData.PlayerEnterRoom = true;
        }

        RefreshInvulnerableState();
    }

    private void RefreshInvulnerableState()
    {
        Boss2StateID activeState = stateMachine.ActiveStateName;
        bool shouldBeInvulnerable = activeState == Boss2StateID.Move || activeState == Boss2StateID.Die;
        SetInvulnerable(shouldBeInvulnerable);
    }

    #region Helper Methods for States
    public Vector2 GetPlayerPosition()
    {
        return playerTransform != null ? (Vector2)playerTransform.position : (Vector2)transform.position;
    }

    public void SetInvulnerable(bool invulnerable)
    {
        if (bodyCollider != null) bodyCollider.enabled = !invulnerable;
    }

    public void PlayAnimation(string animName)
    {
        if (bossAnimator != null && !string.IsNullOrEmpty(animName))
            bossAnimator.Play(animName);
    }

    public void OnPhaseTransitionEnd()
    {
        phaseTransitionComplete = true;
        p2ComboDone = 0;
    }

    public void OnEmergeAttackExit()
    {
        if (currentPhase == 2)
        {
            p2ComboDone++;
        }
    }

    public void ResetPhaseTransition()
    {
        phaseTransitionComplete = false;
    }
    #endregion

    #region State Enter Methods
    public void EnterIdle()
    {
        PlayAnimation(idleAnim);
    }

    public void EnterMove()
    {
        PlayAnimation(moveAnim);
        SetInvulnerable(true);
    }

    public void EnterEmerge()
    {
        emergeDamageArmed = true;
        PlayAnimation(emergeAnim);
        SetInvulnerable(false);
    }

    public void EnterSweep()
    {
        sweepDamageArmed = true;
        PlayAnimation(sweepAnim);
    }

    public void EnterDie()
    {
        PlayAnimation(dieAnim);
        SetInvulnerable(true);
    }
    #endregion

    #region Animation Event Triggers
    public void TriggerEmergeDamage()
    {
        if (stateMachine.ActiveStateName != Boss2StateID.EmergeAttack || !emergeDamageArmed)
            return;

        emergeDamageArmed = false;
        DealDamageInRadius(emergeRadius, emergeDamage);
    }

    public void TriggerSweepDamage()
    {
        if (stateMachine.ActiveStateName != Boss2StateID.SweepAttack || !sweepDamageArmed)
            return;

        sweepDamageArmed = false;
        DealDamageInRadius(sweepRadius, sweepDamage);
    }

    private void DealDamageInRadius(float radius, int damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, damageMask);
        foreach (var hit in hits)
        {
            CharacterDate playerData = hit.GetComponentInParent<CharacterDate>();
            if (playerData != null)
            {
                playerData.Damage(damage);
            }
        }
    }
    #endregion
}
