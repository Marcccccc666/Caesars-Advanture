using System.Collections;
using UnityEngine;
using UnityHFSM;

public enum Enemy5
{
}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyData))]
public class Enemy5HFSM : MonoBehaviour
{
    private enum AttackPhase
    {
        None,
        Windup,
        Active,
        Recover
    }

    [Header("Visual")]
    [SerializeField, ChineseLabel("模型根节点")] private Transform visualRoot;

    [Header("动画")]
    [SerializeField, ChineseLabel("动画控制器")] private Animator enemyAnimator;
    [SerializeField, ChineseLabel("待机动画状态名")] private string idleAnimationState = "idle";
    [SerializeField, ChineseLabel("移动动画状态名")] private string moveAnimationState = "move";
    [SerializeField, ChineseLabel("攻击动画状态名")] private string attackAnimationState = "attack";
    [SerializeField, ChineseLabel("死亡动画状态名")] private string dieAnimationState = "die";
    [SerializeField, ChineseLabel("死亡动画时长")] private float dieAnimationDuration = 0.5f;

    [Header("攻击")]
    [SerializeField, ChineseLabel("攻击前摇时长")] private float attackWindupDuration = 0.2f;
    [SerializeField, ChineseLabel("攻击有效时长")] private float attackActiveDuration = 0.1f;
    [SerializeField, ChineseLabel("攻击后摇时长")] private float attackRecoverDuration = 0.2f;
    [SerializeField, ChineseLabel("攻击冷却时长")] private float attackCooldown = 1.5f;

    private EnemyData enemyData;
    private Rigidbody2D rb2D;
    private EnemyVision2D vision;
    private EnemyAStarChase2D chasePathfinder;
    private Enemy5_MeleeCombat meleeCombat;

    private float baseVisualScaleX = 1f;
    private Vector2 lockedAttackDirection = Vector2.right;
    private AttackPhase attackPhase = AttackPhase.None;
    private bool attackDamageApplied;
    private bool attackFinished;
    private bool isDying;
    private Enemy5StateID? lastAnimationState;

    private string attackPhaseTimerName;
    private string attackCooldownTimerName;
    private DownTimer attackPhaseTimer;
    private DownTimer attackCooldownTimer;

    private readonly StateMachine<Enemy5StateID, Enemy5> stateMachine = new();

    public enum Enemy5StateID
    {
        Idle,
        Move,
        Attack,
        Die
    }

    private Transform playerTransform => vision != null ? vision.PlayerTransform : null;
    private EnemyManager enemyManager => EnemyManager.Instance;
    private MultiTimerManager timerManager => MultiTimerManager.Instance;

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

        meleeCombat = GetComponentInChildren<Enemy5_MeleeCombat>(true);
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
            if (meleeCombat != null && meleeCombat.AttackPoint != null)
            {
                vision.SetAttackPoint(meleeCombat.AttackPoint);
            }

            vision.RefreshRangesFromGizmos();
        }

        if (chasePathfinder != null)
        {
            chasePathfinder.BindVision(vision);
        }

        if (enemyData != null && enemyData.CurrentAttackInterval > 0f)
        {
            attackCooldown = enemyData.CurrentAttackInterval;
        }

        attackPhaseTimerName = $"Enemy5_AttackPhase_{gameObject.GetInstanceID()}";
        attackCooldownTimerName = $"Enemy5_AttackCooldown_{gameObject.GetInstanceID()}";
        attackPhaseTimer = timerManager.Create_DownTimer(attackPhaseTimerName);
        attackCooldownTimer = timerManager.Create_DownTimer(attackCooldownTimerName);
        attackPhaseTimer.ResetTimer(0f);
        attackCooldownTimer.ResetTimer(0f);

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
        {
            return;
        }

        if (stateMachine.ActiveStateName == Enemy5StateID.Attack)
        {
            TickAttack();
        }

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

        if (stateMachine.ActiveStateName != Enemy5StateID.Move && chasePathfinder != null)
        {
            chasePathfinder.ResetPath();
        }

        if (stateMachine.ActiveStateName == Enemy5StateID.Move && ShouldMove())
        {
            MoveWithPathfinding();
            return;
        }

        rb2D.linearVelocity = Vector2.zero;
    }

    private void BuildStateMachine()
    {
        stateMachine.AddState(Enemy5StateID.Idle, new Enemy5_Idle(this));
        stateMachine.AddState(Enemy5StateID.Move, new Enemy5_Move(this));
        stateMachine.AddState(Enemy5StateID.Attack, new Enemy5_Attack(this));
        stateMachine.AddState(Enemy5StateID.Die, new Enemy5_Die(this));

        stateMachine.AddTransition(
            Enemy5StateID.Idle,
            Enemy5StateID.Attack,
            _ => CanSwitchState() && CanEnterAttack()
        );
        stateMachine.AddTransition(
            Enemy5StateID.Idle,
            Enemy5StateID.Move,
            _ => CanSwitchState() && ShouldMove()
        );

        stateMachine.AddTransition(
            Enemy5StateID.Move,
            Enemy5StateID.Attack,
            _ => CanSwitchState() && CanEnterAttack()
        );
        stateMachine.AddTransition(
            Enemy5StateID.Move,
            Enemy5StateID.Idle,
            _ => CanSwitchState() && ShouldIdle()
        );

        stateMachine.AddTransition(
            Enemy5StateID.Attack,
            Enemy5StateID.Move,
            _ => CanSwitchState() && attackFinished && IsPlayerInHateRange()
        );
        stateMachine.AddTransition(
            Enemy5StateID.Attack,
            Enemy5StateID.Idle,
            _ => CanSwitchState() && attackFinished && !IsPlayerInHateRange()
        );

        stateMachine.SetStartState(Enemy5StateID.Idle);
    }

    private void TickAttack()
    {
        if (attackPhaseTimer == null || !attackPhaseTimer.IsComplete() || attackFinished)
        {
            return;
        }

        switch (attackPhase)
        {
            case AttackPhase.Windup:
                EnterAttackActivePhase();
                break;
            case AttackPhase.Active:
                StartAttackPhase(AttackPhase.Recover, attackRecoverDuration);
                break;
            case AttackPhase.Recover:
                FinishAttack();
                break;
        }
    }

    private void EnterAttackActivePhase()
    {
        if (!attackDamageApplied && meleeCombat != null)
        {
            meleeCombat.TryHitPlayer();
            attackDamageApplied = true;
        }

        StartAttackPhase(AttackPhase.Active, attackActiveDuration);
    }

    private void FinishAttack()
    {
        attackFinished = true;
        attackPhase = AttackPhase.None;

        if (attackPhaseTimer != null)
        {
            attackPhaseTimer.ResetTimer(0f);
        }

        if (attackCooldownTimer != null)
        {
            attackCooldownTimer.ResetTimer(Mathf.Max(0.01f, attackCooldown));
            attackCooldownTimer.StartTimer();
        }
    }

    private void StartAttackPhase(AttackPhase phase, float duration)
    {
        attackPhase = phase;

        if (attackPhaseTimer == null)
        {
            return;
        }

        attackPhaseTimer.ResetTimer(Mathf.Max(0.01f, duration));
        attackPhaseTimer.StartTimer();
    }

    public void EnterIdle()
    {
        rb2D.linearVelocity = Vector2.zero;
        ResetAttackRuntime();
    }

    public void EnterMove()
    {
        ResetAttackRuntime();
    }

    public void EnterAttack()
    {
        rb2D.linearVelocity = Vector2.zero;

        attackFinished = false;
        attackDamageApplied = false;
        lockedAttackDirection = GetDirectionToPlayer();
        if (lockedAttackDirection.sqrMagnitude <= 0.0001f)
        {
            lockedAttackDirection = Vector2.right * Mathf.Sign(visualRoot.localScale.x);
            if (lockedAttackDirection.sqrMagnitude <= 0.0001f)
            {
                lockedAttackDirection = Vector2.right;
            }
        }

        StartAttackPhase(AttackPhase.Windup, attackWindupDuration);
    }

    public void EnterDie()
    {
        rb2D.linearVelocity = Vector2.zero;
        ResetAttackRuntime();
    }

    private void ResetAttackRuntime()
    {
        attackFinished = false;
        attackDamageApplied = false;
        attackPhase = AttackPhase.None;

        if (attackPhaseTimer != null)
        {
            attackPhaseTimer.ResetTimer(0f);
        }
    }

    private bool CanSwitchState()
    {
        return !isDying && enemyData != null && enemyData.PlayerEnterRoom;
    }

    private bool IsPlayerInHateRange()
    {
        return vision != null && vision.IsPlayerInHateRange();
    }

    private bool CanEnterAttack()
    {
        return vision != null && vision.CanAttack() && !IsAttackCoolingDown();
    }

    private bool ShouldMove()
    {
        if (!IsPlayerInHateRange())
        {
            return false;
        }

        if (IsAttackCoolingDown())
        {
            return true;
        }

        return vision != null && vision.ShouldChase();
    }

    private bool ShouldIdle()
    {
        return !IsPlayerInHateRange();
    }

    private bool IsAttackCoolingDown()
    {
        return attackCooldownTimer != null && !attackCooldownTimer.IsComplete();
    }

    private Vector2 GetDirectionToPlayer()
    {
        return vision != null ? vision.GetDirectionToPlayer() : Vector2.zero;
    }

    private void MoveWithPathfinding()
    {
        Vector2 direction = Vector2.zero;
        if (chasePathfinder != null)
        {
            direction = chasePathfinder.GetMoveDirectionToPlayer();
        }
        else if (vision != null)
        {
            direction = vision.GetDirectionToPlayer();
        }

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        ObjectMove.MoveObject(rb2D, direction, enemyData.CurrentMoveSpeed);
    }

    private void UpdateFacing()
    {
        if (visualRoot == null)
        {
            return;
        }

        float deltaX = 0f;
        if (stateMachine.ActiveStateName == Enemy5StateID.Attack)
        {
            deltaX = lockedAttackDirection.x;
        }
        else if (playerTransform != null)
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

    private void UpdateAnimationByState(bool force = false)
    {
        Enemy5StateID currentState = stateMachine.ActiveStateName;
        if (!force && lastAnimationState.HasValue && lastAnimationState.Value == currentState)
        {
            return;
        }

        PlayAnimationForState(currentState);
        lastAnimationState = currentState;
    }

    private void PlayAnimationForState(Enemy5StateID state)
    {
        switch (state)
        {
            case Enemy5StateID.Idle:
                PlayFirstAvailableState(idleAnimationState, "Idle", "idle");
                break;
            case Enemy5StateID.Move:
                PlayFirstAvailableState(moveAnimationState, "Move", "move");
                break;
            case Enemy5StateID.Attack:
                PlayFirstAvailableState(attackAnimationState, "Attack", "attack");
                break;
            case Enemy5StateID.Die:
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

    private void OnTakeDamage(int damage)
    {
        if (enemyData == null || enemyData.CurrentHealth > 0 || isDying)
        {
            return;
        }

        isDying = true;
        BuffManager.Instance?.EnemyKilledTriggered?.Invoke(transform);
        enemyManager.RemoveEnemyData(gameObject.GetInstanceID());
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        EnterDie();

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        PlayFirstAvailableState(dieAnimationState, "Die", "die");

        yield return new WaitForSeconds(dieAnimationDuration);

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (enemyData != null)
        {
            enemyData.OnDamage -= OnTakeDamage;
        }

        ReleaseTimers();
    }

    private void ReleaseTimers()
    {
        MultiTimerManager manager = FindAnyObjectByType<MultiTimerManager>();
        if (manager == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(attackPhaseTimerName))
        {
            manager.Delete_DownTimer(attackPhaseTimerName);
        }

        if (!string.IsNullOrEmpty(attackCooldownTimerName))
        {
            manager.Delete_DownTimer(attackCooldownTimerName);
        }
    }
}
