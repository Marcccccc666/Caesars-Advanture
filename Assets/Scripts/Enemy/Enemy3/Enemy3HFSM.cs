using UnityEngine;
using UnityHFSM;

public enum Enemy3
{
}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyData))]
public class Enemy3HFSM : MonoBehaviour
{
    [Header("视觉")]
    [SerializeField, ChineseLabel("模型根节点")] private Transform visualRoot;
    
    [Header("动画")]
    [SerializeField, ChineseLabel("动画控制器")] private Animator enemyAnimator;
    [SerializeField, ChineseLabel("待机动画状态名")] private string idleAnimationState = "idle";
    [SerializeField, ChineseLabel("追击动画状态名")] private string chaseAnimationState = "move";
    [SerializeField, ChineseLabel("攻击动画状态名")] private string attackAnimationState = "attack";
    [SerializeField, ChineseLabel("死亡动画状态名")] private string dieAnimationState = "die";

    [Header("攻击配置")]
    [SerializeField, ChineseLabel("持续开火时长")] private float attackDuration = 1.5f;
    [SerializeField, ChineseLabel("攻击冷却时长")] private float attackCooldown = 1.5f;
    [SerializeField, ChineseLabel("开火间隔")] private float fireInterval = 0.2f;

    [Header("待机游走")]
    [SerializeField, ChineseLabel("游走移速系数")] private float idleMoveSpeedMultiplier = 0.5f;
    [SerializeField, ChineseLabel("游走移动时长范围")]
    private Vector2 idleMoveDurationRange = new Vector2(0.35f, 0.8f);
    [SerializeField, ChineseLabel("游走停顿时长范围")]
    private Vector2 idleStopDurationRange = new Vector2(0.3f, 0.75f);

    private EnemyData enemyData;
    private Rigidbody2D rb2D;
    private Enemy3_RangedCombat rangedCombat;
    private EnemyVision2D vision;
    private EnemyAStarChase2D chasePathfinder;

    private float baseVisualScaleX = 1f;

    private float cooldownTimer;
    private float attackTimer;
    private float fireTimer;
    private bool attackFinished;

    private bool idleMoving;
    private float idlePhaseTimer;
    private Vector2 idleDirection;
    private Enemy3StateID? lastAnimationState = null;

    private readonly StateMachine<Enemy3StateID, Enemy3> stateMachine = new();

    public enum Enemy3StateID
    {
        Idle,
        Chase,
        Attack,
        Cooldown,
        Die
    }

    private Transform playerTransform
    {
        get => vision != null ? vision.PlayerTransform : null;
    }

    private EnemyManager enemyManager => EnemyManager.Instance;

    private void Awake()
    {
        enemyData = GetComponent<EnemyData>();
        enemyData.InitObjectData();
        enemyData.OnDamage += OnTakeDamage;

        rb2D = GetComponent<Rigidbody2D>();
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }
        rangedCombat = GetComponentInChildren<Enemy3_RangedCombat>(true);
        vision = GetComponent<EnemyVision2D>();
        if (vision == null)
        {
            vision = gameObject.AddComponent<EnemyVision2D>();
        }
        chasePathfinder = GetComponent<EnemyAStarChase2D>();
        if (chasePathfinder == null)
        {
            chasePathfinder = gameObject.AddComponent<EnemyAStarChase2D>();
        }

        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        baseVisualScaleX = Mathf.Abs(visualRoot.localScale.x);
        if (baseVisualScaleX <= 0f)
        {
            baseVisualScaleX = 1f;
        }

        if (vision != null)
        {
            if (rangedCombat != null && rangedCombat.FirePoint != null)
            {
                vision.SetAttackPoint(rangedCombat.FirePoint);
            }
            vision.RefreshRangesFromGizmos();
        }
        if (chasePathfinder != null)
        {
            chasePathfinder.BindVision(vision);
        }

        BuildStateMachine();
    }

    private void Start()
    {
        stateMachine.Init();
        UpdateAnimationByState(force: true);
    }

    private void Update()
    {
        if (!CanSwitchState())
            return;

        UpdateRuntimeTimers();
        stateMachine.OnLogic();
        UpdateFacing();
        UpdateAnimationByState();
    }

    private void FixedUpdate()
    {
        rb2D.angularVelocity = 0f;

        if (!CanSwitchState())
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        if (stateMachine.ActiveStateName != Enemy3StateID.Chase && chasePathfinder != null)
        {
            chasePathfinder.ResetPath();
        }

        if (stateMachine.ActiveStateName == Enemy3StateID.Chase && ShouldChase())
        {
            MoveWithPathfinding();
            return;
        }

        if (stateMachine.ActiveStateName == Enemy3StateID.Idle && idleMoving)
        {
            MoveByDirection(idleDirection, enemyData.CurrentMoveSpeed * idleMoveSpeedMultiplier);
            return;
        }

        rb2D.linearVelocity = Vector2.zero;
    }

    private void BuildStateMachine()
    {
        stateMachine.AddState(Enemy3StateID.Idle, new Enemy3_Idle(this));
        stateMachine.AddState(Enemy3StateID.Chase, new Enemy3_Chase(this));
        stateMachine.AddState(Enemy3StateID.Attack, new Enemy3_Attack(this));
        stateMachine.AddState(Enemy3StateID.Cooldown, new Enemy3_Cooldown(this));
        stateMachine.AddState(Enemy3StateID.Die, new Enemy3_Die(this));

        stateMachine.AddTransition(
            Enemy3StateID.Idle,
            Enemy3StateID.Chase,
            _ => CanSwitchState() && ShouldChase()
        );
        stateMachine.AddTransition(
            Enemy3StateID.Idle,
            Enemy3StateID.Attack,
            _ => CanSwitchState() && CanEnterAttack()
        );
        stateMachine.AddTransition(
            Enemy3StateID.Idle,
            Enemy3StateID.Cooldown,
            _ => CanSwitchState() && IsPlayerInAttackRange() && HasLineOfSightToPlayer() && cooldownTimer > 0f
        );

        stateMachine.AddTransition(
            Enemy3StateID.Chase,
            Enemy3StateID.Idle,
            _ => CanSwitchState() && ShouldIdle()
        );
        stateMachine.AddTransition(
            Enemy3StateID.Chase,
            Enemy3StateID.Attack,
            _ => CanSwitchState() && CanEnterAttack()
        );
        stateMachine.AddTransition(
            Enemy3StateID.Chase,
            Enemy3StateID.Cooldown,
            _ => CanSwitchState() && IsPlayerInAttackRange() && HasLineOfSightToPlayer() && cooldownTimer > 0f
        );

        // 攻击状态只在攻击时长结束后切出，不受玩家位置影响。
        stateMachine.AddTransition(
            Enemy3StateID.Attack,
            Enemy3StateID.Cooldown,
            _ => CanSwitchState() && attackFinished
        );

        stateMachine.AddTransition(
            Enemy3StateID.Cooldown,
            Enemy3StateID.Idle,
            _ => CanSwitchState() && ShouldIdle()
        );
        stateMachine.AddTransition(
            Enemy3StateID.Cooldown,
            Enemy3StateID.Chase,
            _ => CanSwitchState() && ShouldChase()
        );
        stateMachine.AddTransition(
            Enemy3StateID.Cooldown,
            Enemy3StateID.Attack,
            _ => CanSwitchState() && CanEnterAttack()
        );

        stateMachine.SetStartState(Enemy3StateID.Idle);
    }

    private void UpdateRuntimeTimers()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer < 0f)
            {
                cooldownTimer = 0f;
            }
        }

        if (stateMachine.ActiveStateName == Enemy3StateID.Idle)
        {
            TickIdleWander();
        }
        else if (stateMachine.ActiveStateName == Enemy3StateID.Attack)
        {
            TickAttack();
        }
    }

    private void TickIdleWander()
    {
        idlePhaseTimer -= Time.deltaTime;
        if (idlePhaseTimer > 0f)
        {
            return;
        }

        StartIdlePhase(!idleMoving);
    }

    private void StartIdlePhase(bool movePhase)
    {
        idleMoving = movePhase;
        if (idleMoving)
        {
            idleDirection = GetRandomDirection();
            idlePhaseTimer = GetRandomDuration(idleMoveDurationRange);
        }
        else
        {
            idlePhaseTimer = GetRandomDuration(idleStopDurationRange);
        }
    }

    private float GetRandomDuration(Vector2 range)
    {
        float min = Mathf.Max(0.05f, Mathf.Min(range.x, range.y));
        float max = Mathf.Max(min, Mathf.Max(range.x, range.y));
        return Random.Range(min, max);
    }

    private Vector2 GetRandomDirection()
    {
        Vector2 random = Random.insideUnitCircle;
        if (random.sqrMagnitude < 0.0001f)
        {
            return Vector2.right;
        }

        return random.normalized;
    }

    private bool HasPlayer()
    {
        return vision != null && vision.HasPlayer();
    }

    private void OnTakeDamage(int damage)
    {
        if (enemyData == null || enemyData.CurrentHealth > 0)
            return;

        enemyManager.RemoveEnemyData(gameObject.GetInstanceID());
        gameObject.SetActive(false);
    }

    private bool CanSwitchState()
    {
        return enemyData != null && enemyData.PlayerEnterRoom;
    }

    private bool IsPlayerInHateRange()
    {
        return vision != null && vision.IsPlayerInHateRange();
    }

    private bool IsPlayerInAttackRange()
    {
        return vision != null && vision.IsPlayerInAttackRange();
    }

    private bool HasLineOfSightToPlayer()
    {
        return vision != null && vision.HasLineOfSightToPlayer();
    }

    private bool CanEnterAttack()
    {
        return vision != null && vision.CanAttack() && cooldownTimer <= 0f;
    }

    private bool ShouldChase()
    {
        return vision != null && vision.ShouldChase();
    }

    private bool ShouldIdle()
    {
        return vision == null || vision.ShouldIdle();
    }

    private void MoveTowardsPlayer()
    {
        if (!HasPlayer())
        {
            return;
        }

        Vector2 direction =
            ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        MoveByDirection(direction, enemyData.CurrentMoveSpeed);
    }

    private void MoveWithPathfinding()
    {
        Vector2 direction = Vector2.zero;
        if (chasePathfinder != null)
        {
            direction = chasePathfinder.GetMoveDirectionToPlayer();
        }
        else
        {
            direction = vision != null ? vision.GetDirectionToPlayer() : Vector2.zero;
        }

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        MoveByDirection(direction, enemyData.CurrentMoveSpeed);
    }

    private void MoveByDirection(Vector2 direction, float speed)
    {
        ObjectMove.MoveObject(rb2D, direction, speed);
    }

    private void UpdateFacing()
    {
        float deltaX = 0f;
        if (stateMachine.ActiveStateName == Enemy3StateID.Idle && idleMoving)
        {
            deltaX = idleDirection.x;
        }
        else if (HasPlayer())
        {
            deltaX = playerTransform.position.x - transform.position.x;
        }

        if (Mathf.Abs(deltaX) < 0.001f)
        {
            return;
        }

        float targetScaleX = baseVisualScaleX * (deltaX >= 0f ? 1f : -1f);
        Vector3 scale = visualRoot.localScale;
        if (!Mathf.Approximately(scale.x, targetScaleX))
        {
            scale.x = targetScaleX;
            visualRoot.localScale = scale;
        }
    }

    private void TryFireAtPlayer()
    {
        if (rangedCombat == null || !HasPlayer())
        {
            return;
        }

        int damage = enemyData != null ? enemyData.CurrentAttack : 1;
        rangedCombat.FireTowards(playerTransform.position, damage);
    }

    private void TickAttack()
    {
        if (attackFinished)
        {
            return;
        }

        attackTimer -= Time.deltaTime;
        fireTimer -= Time.deltaTime;

        float validFireInterval = Mathf.Max(fireInterval, 0.05f);
        while (fireTimer <= 0f)
        {
            TryFireAtPlayer();
            fireTimer += validFireInterval;
        }

        if (attackTimer <= 0f)
        {
            attackTimer = 0f;
            attackFinished = true;
        }
    }

    public void EnterIdle()
    {
        StartIdlePhase(false);
    }

    public void EnterChase()
    {
    }

    public void EnterAttack()
    {
        rb2D.linearVelocity = Vector2.zero;

        attackFinished = false;
        attackTimer = Mathf.Max(attackDuration, 0.01f);
        fireTimer = 0f;
    }

    public void EnterCooldown()
    {
        rb2D.linearVelocity = Vector2.zero;

        if (attackFinished)
        {
            cooldownTimer = Mathf.Max(attackCooldown, 0.01f);
            attackFinished = false;
        }
    }

    public void EnterDie()
    {
        rb2D.linearVelocity = Vector2.zero;
    }

    private void UpdateAnimationByState(bool force = false)
    {
        Enemy3StateID currentState = stateMachine.ActiveStateName;
        if (!force && lastAnimationState.HasValue && lastAnimationState.Value == currentState)
        {
            return;
        }

        PlayAnimationForState(currentState);
        lastAnimationState = currentState;
    }

    private void PlayAnimationForState(Enemy3StateID state)
    {
        switch (state)
        {
            case Enemy3StateID.Idle:
                PlayFirstAvailableState(idleAnimationState, "Idle", "idle", "Move", "move");
                break;
            case Enemy3StateID.Chase:
                PlayFirstAvailableState(chaseAnimationState, "Move", "move", "Chase", "chase");
                break;
            case Enemy3StateID.Attack:
                PlayFirstAvailableState(attackAnimationState, "Attack", "attack");
                break;
            case Enemy3StateID.Cooldown:
                PlayFirstAvailableState(idleAnimationState, "Idle", "idle", "Move", "move");
                break;
            case Enemy3StateID.Die:
                PlayFirstAvailableState(dieAnimationState, "Die", "die");
                break;
        }
    }

    private void PlayFirstAvailableState(params string[] candidates)
    {
        if (enemyAnimator == null || candidates == null || candidates.Length == 0)
        {
            return;
        }

        for (int i = 0; i < candidates.Length; i++)
        {
            if (TryPlayAnimatorState(candidates[i]))
            {
                return;
            }
        }
    }

    private bool TryPlayAnimatorState(string stateName)
    {
        if (enemyAnimator == null || string.IsNullOrWhiteSpace(stateName))
        {
            return false;
        }

        int shortHash = Animator.StringToHash(stateName);
        if (enemyAnimator.HasState(0, shortHash))
        {
            enemyAnimator.Play(shortHash);
            return true;
        }

        int fullPathHash = Animator.StringToHash($"Base Layer.{stateName}");
        if (enemyAnimator.HasState(0, fullPathHash))
        {
            enemyAnimator.Play(fullPathHash);
            return true;
        }

        return false;
    }
}
