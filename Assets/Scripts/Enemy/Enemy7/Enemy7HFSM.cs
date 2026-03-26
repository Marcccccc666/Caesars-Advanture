using System.Collections;
using UnityEngine;
using UnityHFSM;

public enum Enemy7
{
}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyData))]
public class Enemy7HFSM : MonoBehaviour
{
    private enum AttackPhase
    {
        None,
        Prepare,
        Recover
    }

    [Header("Visual")]
    [SerializeField, ChineseLabel("模型根节点")] private Transform visualRoot;
    [SerializeField, ChineseLabel("默认朝向是否为右")] private bool visualFacesRight = false;

    [Header("动画")]
    [SerializeField, ChineseLabel("动画控制器")] private Animator enemyAnimator;
    [SerializeField, ChineseLabel("待机动画状态名")] private string idleAnimationState = "idle";
    [SerializeField, ChineseLabel("移动动画状态名")] private string moveAnimationState = "move";
    [SerializeField, ChineseLabel("攻击动画状态名")] private string attackAnimationState = "attack";
    [SerializeField, ChineseLabel("死亡动画状态名")] private string dieAnimationState = "die";
    [SerializeField, ChineseLabel("死亡动画时长")] private float dieAnimationDuration = 0.5f;

    [Header("攻击")]
    [SerializeField, ChineseLabel("砸地前摇时长")] private float slamPrepareDuration = 0.35f;
    [SerializeField, ChineseLabel("砸地后摇时长")] private float slamRecoverDuration = 0.35f;

    [SerializeField] private float attackCooldown = 1.5f;

    private EnemyData enemyData;
    private Rigidbody2D rb2D;
    private EnemyVision2D vision;
    private EnemyAStarChase2D chasePathfinder;
    private Enemy7_GroundSlamCombat groundSlamCombat;
    private Collider2D[] bodyColliders;

    private float baseVisualScaleX = 1f;
    private Vector2 lockedAttackDirection = Vector2.right;
    private AttackPhase attackPhase = AttackPhase.None;
    private bool attackFinished;
    private bool slamTriggered;
    private bool isDying;
    private Enemy7StateID? lastAnimationState;

    private string attackTimerName;
    private string attackCooldownTimerName;
    private DownTimer attackTimer;
    private DownTimer attackCooldownTimer;

    private readonly StateMachine<Enemy7StateID, Enemy7> stateMachine = new();

    public enum Enemy7StateID
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
        bodyColliders = GetComponentsInChildren<Collider2D>(true);
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }

        groundSlamCombat = GetComponentInChildren<Enemy7_GroundSlamCombat>(true);
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

        if (groundSlamCombat != null)
        {
            groundSlamCombat.RefreshDamageRadiusFromGizmo();
        }

        if (vision != null)
        {
            if (groundSlamCombat != null && groundSlamCombat.AttackPoint != null)
            {
                vision.SetAttackPoint(groundSlamCombat.AttackPoint);
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

        attackTimerName = $"Enemy7_Attack_{gameObject.GetInstanceID()}";
        attackCooldownTimerName = $"Enemy7_AttackCooldown_{gameObject.GetInstanceID()}";
        attackTimer = timerManager.Create_DownTimer(attackTimerName);
        attackCooldownTimer = timerManager.Create_DownTimer(attackCooldownTimerName);
        attackTimer.ResetTimer(0f);
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

        if (stateMachine.ActiveStateName == Enemy7StateID.Attack)
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

        if (stateMachine.ActiveStateName != Enemy7StateID.Move && chasePathfinder != null)
        {
            chasePathfinder.ResetPath();
        }

        if (stateMachine.ActiveStateName == Enemy7StateID.Move && ShouldMove())
        {
            MoveWithPathfinding();
            return;
        }

        rb2D.linearVelocity = Vector2.zero;
    }

    private void BuildStateMachine()
    {
        stateMachine.AddState(Enemy7StateID.Idle, new Enemy7_Idle(this));
        stateMachine.AddState(Enemy7StateID.Move, new Enemy7_Move(this));
        stateMachine.AddState(Enemy7StateID.Attack, new Enemy7_Attack(this));
        stateMachine.AddState(Enemy7StateID.Die, new Enemy7_Die(this));

        stateMachine.AddTransition(
            Enemy7StateID.Idle,
            Enemy7StateID.Attack,
            _ => CanSwitchState() && CanEnterAttack()
        );
        stateMachine.AddTransition(
            Enemy7StateID.Idle,
            Enemy7StateID.Move,
            _ => CanSwitchState() && ShouldMove()
        );

        stateMachine.AddTransition(
            Enemy7StateID.Move,
            Enemy7StateID.Attack,
            _ => CanSwitchState() && CanEnterAttack()
        );
        stateMachine.AddTransition(
            Enemy7StateID.Move,
            Enemy7StateID.Idle,
            _ => CanSwitchState() && ShouldIdle()
        );

        stateMachine.AddTransition(
            Enemy7StateID.Attack,
            Enemy7StateID.Move,
            _ => CanSwitchState() && attackFinished && ShouldMove()
        );
        stateMachine.AddTransition(
            Enemy7StateID.Attack,
            Enemy7StateID.Idle,
            _ => CanSwitchState() && attackFinished && !ShouldMove()
        );

        stateMachine.SetStartState(Enemy7StateID.Idle);
    }

    private void TickAttack()
    {
        if (attackTimer == null || !attackTimer.IsComplete() || attackFinished)
        {
            return;
        }

        switch (attackPhase)
        {
            case AttackPhase.Prepare:
                TriggerGroundSlam();
                attackPhase = AttackPhase.Recover;
                attackTimer.ResetTimer(Mathf.Max(0.01f, slamRecoverDuration));
                attackTimer.StartTimer();
                break;
            case AttackPhase.Recover:
                attackFinished = true;
                attackPhase = AttackPhase.None;
                attackTimer.ResetTimer(0f);
                StartAttackCooldown();
                break;
        }
    }

    private void TriggerGroundSlam()
    {
        if (slamTriggered || groundSlamCombat == null)
        {
            return;
        }

        groundSlamCombat.PerformGroundSlam();
        slamTriggered = true;
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
        slamTriggered = false;
        attackPhase = AttackPhase.Prepare;
        lockedAttackDirection = GetDirectionToPlayer();
        if (lockedAttackDirection.sqrMagnitude <= 0.0001f)
        {
            lockedAttackDirection = Vector2.right * Mathf.Sign(visualRoot.localScale.x);
            if (lockedAttackDirection.sqrMagnitude <= 0.0001f)
            {
                lockedAttackDirection = Vector2.right;
            }
        }

        if (groundSlamCombat != null)
        {
            groundSlamCombat.RefreshDamageRadiusFromGizmo();
        }

        attackTimer.ResetTimer(Mathf.Max(0.01f, slamPrepareDuration));
        attackTimer.StartTimer();
    }

    public void EnterDie()
    {
        rb2D.linearVelocity = Vector2.zero;
        ResetAttackRuntime();
    }

    private void ResetAttackRuntime()
    {
        attackFinished = false;
        slamTriggered = false;
        attackPhase = AttackPhase.None;

        if (attackTimer != null)
        {
            attackTimer.ResetTimer(0f);
        }
    }

    private bool CanSwitchState()
    {
        return !isDying && enemyData != null && enemyData.PlayerEnterRoom;
    }

    private bool CanEnterAttack()
    {
        return vision != null && vision.CanAttack() && !IsAttackCoolingDown();
    }

    private bool ShouldMove()
    {
        if (vision == null || !vision.IsPlayerInHateRange())
        {
            return false;
        }

        if (IsAttackCoolingDown())
        {
            return !IsTouchingPlayer();
        }

        return vision.ShouldChase();
    }

    private bool ShouldIdle()
    {
        return vision == null || vision.ShouldIdle();
    }

    private Vector2 GetDirectionToPlayer()
    {
        return vision != null ? vision.GetDirectionToPlayer() : Vector2.zero;
    }

    private bool IsAttackCoolingDown()
    {
        return attackCooldownTimer != null && !attackCooldownTimer.IsComplete();
    }

    private void StartAttackCooldown()
    {
        if (attackCooldownTimer == null)
        {
            return;
        }

        attackCooldownTimer.ResetTimer(Mathf.Max(0.01f, attackCooldown));
        attackCooldownTimer.StartTimer();
    }

    private bool IsTouchingPlayer()
    {
        CharacterDate playerData = CharacterManager.Instance?.GetCurrentPlayerCharacterData;
        if (playerData == null)
        {
            return false;
        }

        Collider2D[] playerColliders = playerData.GetComponentsInChildren<Collider2D>(true);
        if (playerColliders == null || playerColliders.Length == 0 || bodyColliders == null || bodyColliders.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < bodyColliders.Length; i++)
        {
            Collider2D bodyCollider = bodyColliders[i];
            if (bodyCollider == null || !bodyCollider.enabled || bodyCollider.isTrigger)
            {
                continue;
            }

            for (int j = 0; j < playerColliders.Length; j++)
            {
                Collider2D playerCollider = playerColliders[j];
                if (playerCollider == null || !playerCollider.enabled || playerCollider.isTrigger)
                {
                    continue;
                }

                if (bodyCollider.IsTouching(playerCollider))
                {
                    return true;
                }
            }
        }

        return false;
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
        if (stateMachine.ActiveStateName == Enemy7StateID.Attack)
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

        float facingSign = deltaX >= 0f ? 1f : -1f;
        if (!visualFacesRight)
        {
            facingSign *= -1f;
        }

        float targetScaleX = baseVisualScaleX * facingSign;
        Vector3 scale = visualRoot.localScale;
        if (!Mathf.Approximately(scale.x, targetScaleX))
        {
            scale.x = targetScaleX;
            visualRoot.localScale = scale;
        }
    }

    private void UpdateAnimationByState(bool force = false)
    {
        Enemy7StateID currentState = stateMachine.ActiveStateName;
        if (!force && lastAnimationState.HasValue && lastAnimationState.Value == currentState)
        {
            return;
        }

        PlayAnimationForState(currentState);
        lastAnimationState = currentState;
    }

    private void PlayAnimationForState(Enemy7StateID state)
    {
        switch (state)
        {
            case Enemy7StateID.Idle:
                PlayFirstAvailableState(idleAnimationState, "Idle", "idle");
                break;
            case Enemy7StateID.Move:
                PlayFirstAvailableState(moveAnimationState, "Move", "move");
                break;
            case Enemy7StateID.Attack:
                PlayFirstAvailableState(attackAnimationState, "Attack", "attack");
                break;
            case Enemy7StateID.Die:
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

        ReleaseTimer();
    }

    private void ReleaseTimer()
    {
        MultiTimerManager manager = FindAnyObjectByType<MultiTimerManager>();
        if (manager == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(attackTimerName))
        {
            manager.Delete_DownTimer(attackTimerName);
        }

        if (!string.IsNullOrEmpty(attackCooldownTimerName))
        {
            manager.Delete_DownTimer(attackCooldownTimerName);
        }
    }
}
