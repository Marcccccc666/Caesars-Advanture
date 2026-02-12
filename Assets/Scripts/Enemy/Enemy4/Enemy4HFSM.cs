using UnityEngine;
using UnityHFSM;

public enum Enemy4
{
}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyData))]
public class Enemy4HFSM : MonoBehaviour
{
    [Header("检测范围")]
    [SerializeField, ChineseLabel("仇恨判定点")] private Transform detectionPoint;
    [SerializeField, ChineseLabel("默认仇恨范围")] private float defaultHateRange = 8f;
    [SerializeField, ChineseLabel("默认攻击范围")] private float defaultAttackRange = 4f;

    [Header("视觉")]
    [SerializeField, ChineseLabel("模型根节点")] private Transform visualRoot;
    
    [Header("动画")]
    [SerializeField, ChineseLabel("动画控制器")] private Animator enemyAnimator;
    [SerializeField, ChineseLabel("待机动画状态名")] private string idleAnimationState = "idle";
    [SerializeField, ChineseLabel("追击动画状态名")] private string chaseAnimationState = "move";
    [SerializeField, ChineseLabel("攻击动画状态名")] private string attackAnimationState = "attack";
    [SerializeField, ChineseLabel("死亡动画状态名")] private string dieAnimationState = "die";

    [Header("攻击配置")]
    [SerializeField, ChineseLabel("瞄准时长")] private float aimDuration = 2f;
    [SerializeField, ChineseLabel("攻击冷却时长")] private float attackCooldown = 1.5f;

    [Header("待机游走")]
    [SerializeField, ChineseLabel("游走移速系数")] private float idleMoveSpeedMultiplier = 0.5f;
    [SerializeField, ChineseLabel("游走移动时长范围")]
    private Vector2 idleMoveDurationRange = new Vector2(0.35f, 0.8f);
    [SerializeField, ChineseLabel("游走停顿时长范围")]
    private Vector2 idleStopDurationRange = new Vector2(0.3f, 0.75f);

    private EnemyData enemyData;
    private Rigidbody2D rb2D;
    private Enemy4_LineCombat lineCombat;

    private float baseVisualScaleX = 1f;
    private float hateRange;
    private float attackRange;

    private float cooldownTimer;
    private float attackTimer;
    private bool attackFinished;

    private bool idleMoving;
    private float idlePhaseTimer;
    private Vector2 idleDirection;
    private Vector2 lastKnownAimTarget;
    private Enemy4StateID? lastAnimationState = null;

    private readonly StateMachine<Enemy4StateID, Enemy4> stateMachine = new();

    public enum Enemy4StateID
    {
        Idle,
        Chase,
        Attack,
        Cooldown,
        Die
    }

    private Transform playerTransform
    {
        get
        {
            var playerData = CharacterManager.Instance.GetCurrentPlayerCharacterData;
            return playerData != null ? playerData.transform : null;
        }
    }

    private Vector2 DetectionPosition =>
        detectionPoint != null ? (Vector2)detectionPoint.position : (Vector2)transform.position;

    private void Awake()
    {
        enemyData = GetComponent<EnemyData>();
        enemyData.InitObjectData();

        rb2D = GetComponent<Rigidbody2D>();
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }
        lineCombat = GetComponentInChildren<Enemy4_LineCombat>(true);

        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        baseVisualScaleX = Mathf.Abs(visualRoot.localScale.x);
        if (baseVisualScaleX <= 0f)
        {
            baseVisualScaleX = 1f;
        }

        var attackGizmo = GetComponentInChildren<AttackRangeGizmo>();
        attackRange = attackGizmo != null ? attackGizmo.GetAttackRange : defaultAttackRange;

        var hateGizmo = GetComponentInChildren<HateRangeGizmo>();
        hateRange = hateGizmo != null ? hateGizmo.GetHateRange : defaultHateRange;

        if (enemyData != null && enemyData.CurrentAttackInterval > 0f)
        {
            attackCooldown = enemyData.CurrentAttackInterval;
        }

        if (playerTransform != null)
        {
            lastKnownAimTarget = playerTransform.position;
        }
        else
        {
            lastKnownAimTarget = transform.position + transform.right;
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
        UpdateRuntimeTimers();
        stateMachine.OnLogic();
        UpdateFacing();
        UpdateAnimationByState();
    }

    private void FixedUpdate()
    {
        rb2D.angularVelocity = 0f;

        if (stateMachine.ActiveStateName == Enemy4StateID.Chase && ShouldChase())
        {
            MoveTowardsPlayer();
            return;
        }

        if (stateMachine.ActiveStateName == Enemy4StateID.Idle && idleMoving)
        {
            MoveByDirection(idleDirection, enemyData.CurrentMoveSpeed * idleMoveSpeedMultiplier);
            return;
        }

        rb2D.linearVelocity = Vector2.zero;
    }

    private void BuildStateMachine()
    {
        stateMachine.AddState(Enemy4StateID.Idle, new Enemy4_Idle(this));
        stateMachine.AddState(Enemy4StateID.Chase, new Enemy4_Chase(this));
        stateMachine.AddState(Enemy4StateID.Attack, new Enemy4_Attack(this));
        stateMachine.AddState(Enemy4StateID.Cooldown, new Enemy4_Cooldown(this));
        stateMachine.AddState(Enemy4StateID.Die, new Enemy4_Die(this));

        stateMachine.AddTransition(Enemy4StateID.Idle, Enemy4StateID.Chase, _ => ShouldChase());
        stateMachine.AddTransition(Enemy4StateID.Idle, Enemy4StateID.Attack, _ => CanEnterAttack());
        stateMachine.AddTransition(
            Enemy4StateID.Idle,
            Enemy4StateID.Cooldown,
            _ => IsPlayerInAttackRange() && cooldownTimer > 0f
        );

        stateMachine.AddTransition(Enemy4StateID.Chase, Enemy4StateID.Idle, _ => ShouldIdle());
        stateMachine.AddTransition(Enemy4StateID.Chase, Enemy4StateID.Attack, _ => CanEnterAttack());
        stateMachine.AddTransition(
            Enemy4StateID.Chase,
            Enemy4StateID.Cooldown,
            _ => IsPlayerInAttackRange() && cooldownTimer > 0f
        );

        // 攻击状态在瞄准时长结束前不会切出。
        stateMachine.AddTransition(Enemy4StateID.Attack, Enemy4StateID.Cooldown, _ => attackFinished);

        stateMachine.AddTransition(Enemy4StateID.Cooldown, Enemy4StateID.Idle, _ => ShouldIdle());
        stateMachine.AddTransition(Enemy4StateID.Cooldown, Enemy4StateID.Chase, _ => ShouldChase());
        stateMachine.AddTransition(Enemy4StateID.Cooldown, Enemy4StateID.Attack, _ => CanEnterAttack());

        stateMachine.SetStartState(Enemy4StateID.Idle);
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

        if (stateMachine.ActiveStateName == Enemy4StateID.Idle)
        {
            TickIdleWander();
        }
        else if (stateMachine.ActiveStateName == Enemy4StateID.Attack)
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
        return playerTransform != null;
    }

    private bool IsPlayerInHateRange()
    {
        if (!HasPlayer())
        {
            return false;
        }

        if (hateRange <= 0f)
        {
            return true;
        }

        return Vector2.Distance(DetectionPosition, playerTransform.position) <= hateRange;
    }

    private bool IsPlayerInAttackRange()
    {
        if (!HasPlayer())
        {
            return false;
        }

        if (attackRange <= 0f)
        {
            return false;
        }

        Vector2 origin = transform.position;
        if (lineCombat != null && lineCombat.FirePoint != null)
        {
            origin = lineCombat.FirePoint.position;
        }

        return Vector2.Distance(origin, playerTransform.position) <= attackRange;
    }

    private bool CanEnterAttack()
    {
        return IsPlayerInAttackRange() && cooldownTimer <= 0f;
    }

    private bool ShouldChase()
    {
        return IsPlayerInHateRange() && !IsPlayerInAttackRange();
    }

    private bool ShouldIdle()
    {
        return !IsPlayerInHateRange();
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

    private void MoveByDirection(Vector2 direction, float speed)
    {
        ObjectMove.MoveObject(rb2D, direction, speed);
    }

    private void UpdateFacing()
    {
        float deltaX = 0f;
        if (stateMachine.ActiveStateName == Enemy4StateID.Idle && idleMoving)
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

    private void TickAttack()
    {
        if (HasPlayer())
        {
            lastKnownAimTarget = playerTransform.position;
        }

        if (lineCombat != null)
        {
            lineCombat.UpdateAimLine(lastKnownAimTarget, attackRange);
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f || attackFinished)
        {
            return;
        }

        attackTimer = 0f;
        attackFinished = true;

        if (lineCombat != null)
        {
            int damage = enemyData != null ? enemyData.CurrentAttack : 1;
            lineCombat.FireLockedLine(damage);
        }
    }

    public void EnterIdle()
    {
        StartIdlePhase(false);
        if (lineCombat != null)
        {
            lineCombat.EndAimLine();
        }
    }

    public void EnterChase()
    {
        if (lineCombat != null)
        {
            lineCombat.EndAimLine();
        }
    }

    public void EnterAttack()
    {
        rb2D.linearVelocity = Vector2.zero;

        attackFinished = false;
        attackTimer = Mathf.Max(aimDuration, 0.01f);

        if (HasPlayer())
        {
            lastKnownAimTarget = playerTransform.position;
        }
        else
        {
            lastKnownAimTarget = transform.position + transform.right * Mathf.Max(attackRange, 0.5f);
        }

        if (lineCombat != null)
        {
            lineCombat.BeginAimLine(lastKnownAimTarget, attackRange);
        }
    }

    public void EnterCooldown()
    {
        rb2D.linearVelocity = Vector2.zero;

        if (attackFinished)
        {
            cooldownTimer = Mathf.Max(attackCooldown, 0.01f);
            attackFinished = false;
        }

        if (lineCombat != null)
        {
            lineCombat.EndAimLine();
        }
    }

    public void EnterDie()
    {
        rb2D.linearVelocity = Vector2.zero;
        if (lineCombat != null)
        {
            lineCombat.EndAimLine();
        }
    }

    private void UpdateAnimationByState(bool force = false)
    {
        Enemy4StateID currentState = stateMachine.ActiveStateName;
        if (!force && lastAnimationState.HasValue && lastAnimationState.Value == currentState)
        {
            return;
        }

        PlayAnimationForState(currentState);
        lastAnimationState = currentState;
    }

    private void PlayAnimationForState(Enemy4StateID state)
    {
        switch (state)
        {
            case Enemy4StateID.Idle:
                PlayFirstAvailableState(idleAnimationState, "Idle", "idle", "Move", "move");
                break;
            case Enemy4StateID.Chase:
                PlayFirstAvailableState(chaseAnimationState, "Move", "move", "Chase", "chase");
                break;
            case Enemy4StateID.Attack:
                PlayFirstAvailableState(attackAnimationState, "Attack", "attack");
                break;
            case Enemy4StateID.Cooldown:
                PlayFirstAvailableState(idleAnimationState, "Idle", "idle", "Move", "move");
                break;
            case Enemy4StateID.Die:
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
