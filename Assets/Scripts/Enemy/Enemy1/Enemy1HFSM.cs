using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityHFSM;

public enum Enemy1{}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyData))]
public class Enemy1HFSM : MonoBehaviour
{
    /// <summary>
    /// 冲撞速度
    /// </summary>
    [SerializeField, ChineseLabel("冲撞速度")] private float collisionSpeed = 0f;

    /// <summary>
    /// 冲撞距离
    /// </summary>
    [SerializeField, ChineseLabel("冲撞距离")] private float collisionDistance = 0f;
    
    /// <summary>
    /// 冲撞前摇时长
    /// </summary>
    [SerializeField, ChineseLabel("冲撞前摇时长")] private float collisionPrepareDuration = 0.2f;

    /// <summary>
    /// 角色数据
    /// </summary>
    private EnemyData M_chData;

    /// <summary>
    /// 刚体
    /// </summary>
    private Rigidbody2D M_rigidbody2D;

    [Header("动画")]
    [SerializeField, ChineseLabel("动画控制器")] private Animator enemyAnimator;
    [SerializeField, ChineseLabel("待机动画状态名")] private string idleAnimationState = "idle";
    [SerializeField, ChineseLabel("移动动画状态名")] private string moveAnimationState = "move";
    [SerializeField, ChineseLabel("攻击动画状态名")] private string attackAnimationState = "attack";
    [SerializeField, ChineseLabel("死亡动画状态名")] private string dieAnimationState = "die";
    [SerializeField, ChineseLabel("死亡动画时长")] private float dieAnimationDuration = 0.5f;

    /// <summary>
    /// 攻击范围
    /// </summary>
    private float AttackRange = 0f;
    [SerializeField, ChineseLabel("默认仇恨范围")] private float defaultHateRange = 8f;
    private float hateRange = 0f;

    [Header("2D寻路")]
    [SerializeField, ChineseLabel("障碍层")] private LayerMask obstacleMask;
    [SerializeField, ChineseLabel("网格大小")] private float pathCellSize = 0.5f;
    [SerializeField, ChineseLabel("节点检测半径")] private float pathNodeCheckRadius = 0.18f;
    [SerializeField, ChineseLabel("自动按碰撞体计算节点半径")] private bool autoPathNodeCheckRadius = true;
    [SerializeField, ChineseLabel("节点半径缩放")] private float pathNodeRadiusScale = 1f;
    [SerializeField, ChineseLabel("节点半径补偿")] private float pathNodeRadiusPadding = 0.02f;
    [SerializeField, ChineseLabel("节点半径最小值")] private float pathNodeRadiusMin = 0.12f;
    [SerializeField, ChineseLabel("重算路径间隔")] private float pathRepathInterval = 0.2f;
    [SerializeField, ChineseLabel("最大搜索节点")] private int pathMaxSearchNodes = 1200;
    [SerializeField, ChineseLabel("到点阈值")] private float pathWaypointReachDistance = 0.12f;

    /// <summary>
    /// 玩家位置
    /// </summary>
    private Transform playerTransform
    {
        get
        {
            if (characterManager == null || characterManager.GetCurrentPlayerCharacterData == null)
            {
                return null;
            }

            return characterManager.GetCurrentPlayerCharacterData.transform;
        }
    }
    private CharacterManager characterManager => CharacterManager.Instance;
    
    /// <summary>
    /// 可以开始攻击
    /// </summary>
    private bool CanStartAttack = false;
    private Vector2 attackDirection = Vector2.right;
    private Enemy1StateID? lastAnimationState = null;
    private bool isDying;
    private float pathRepathTimer = 0f;
    private readonly List<Vector2> currentPath = new();
    private int currentPathIndex = 0;

    private StateMachine<Enemy1StateID, Enemy1> M_stateMachine = new();
    
    private EnemyManager enemyManager => EnemyManager.Instance;

    void Awake()
    {
        M_chData = GetComponent<EnemyData>();
        M_chData.InitObjectData();
        M_chData.OnDamage += OnTakeDamage;

        M_rigidbody2D = GetComponent<Rigidbody2D>();
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }
        AttackRangeGizmo attackRangeGizmo = GetComponentInChildren<AttackRangeGizmo>();
        if (attackRangeGizmo != null)
        {
            AttackRange = attackRangeGizmo.GetAttackRange;
        }

        HateRangeGizmo hateRangeGizmo = GetComponentInChildren<HateRangeGizmo>();
        hateRange = hateRangeGizmo != null ? hateRangeGizmo.GetHateRange : defaultHateRange;
        
        ResolvePathNodeRadiusByCollider();

        Enemy1StateMachine();
    }

    void Start()
    {
        M_stateMachine.Init();
        UpdateAnimationByState(force: true);
    }

    void FixedUpdate()
    {
        M_rigidbody2D.angularVelocity = 0f;

        if (!CanSwitchState())
            return;

        if (M_stateMachine.ActiveStateName != Enemy1StateID.Move)
        {
            pathRepathTimer = 0f;
            ClearPath();
        }

        if (M_stateMachine.ActiveStateName == Enemy1StateID.Move && ShouldChase())
        {
            MoveWithPathfinding();
        }
        else if (M_stateMachine.ActiveStateName == Enemy1StateID.Attack && CanStartAttack)
        {
            ObjectMove.MoveObject(M_rigidbody2D, attackDirection, collisionSpeed);
        }
    }

    void Update()
    {
        if (!CanSwitchState())
            return;

        M_stateMachine.OnLogic();
        UpdateAnimationByState();
    }

    public enum Enemy1StateID
    {
        Idle,
        Move,
        Attack,
        Die
    }

    /// <summary>
    /// 敌人1状态机
    /// <summary>
    private void Enemy1StateMachine()
    {
        // 添加状态
            //待机状态
                M_stateMachine.AddState(Enemy1StateID.Idle, new Enemy1_Idle());

            //移动状态
                M_stateMachine.AddState(Enemy1StateID.Move, new Enemy1_Move());
            
            //攻击状态
                M_stateMachine.AddState(
                    Enemy1StateID.Attack,
                    new Enemy1_Attack(this, collisionDistance, collisionPrepareDuration)
                );
            // 死亡状态
                M_stateMachine.AddState(Enemy1StateID.Die, new Enemy1_Die());

        // 添加转换
            //待机 -> 移动
                M_stateMachine.AddTransition(
                    Enemy1StateID.Idle,
                    Enemy1StateID.Move,
                    t => CanSwitchState() && ShouldChase()
                );
            //待机 -> 攻击
                M_stateMachine.AddTransition(
                    Enemy1StateID.Idle,
                    Enemy1StateID.Attack,
                    t => CanSwitchState() && ShouldAttack()
                );
            
            //移动 -> 攻击
                M_stateMachine.AddTransition(
                    Enemy1StateID.Move,
                    Enemy1StateID.Attack,
                    t => CanSwitchState() && ShouldAttack()
                );
            //移动 -> 待机
                M_stateMachine.AddTransition(
                    Enemy1StateID.Move,
                    Enemy1StateID.Idle,
                    t => CanSwitchState() && ShouldIdle()
                );

            //攻击 -> 移动
                M_stateMachine.AddTransition(
                    Enemy1StateID.Attack,
                    Enemy1StateID.Move,
                    t => CanSwitchState() && ShouldChase()
                );
            //攻击 -> 待机
                M_stateMachine.AddTransition(
                    Enemy1StateID.Attack,
                    Enemy1StateID.Idle,
                    t => CanSwitchState() && ShouldIdle()
                );
            
        M_stateMachine.SetStartState(Enemy1StateID.Idle);
            
    }

    private void OnTakeDamage(int damage)
    {
        if (M_chData == null || M_chData.CurrentHealth > 0 || isDying)
            return;

        isDying = true;
        BuffManager.Instance?.EnemyKilledTriggered?.Invoke(transform);
        enemyManager.RemoveEnemyData(gameObject.GetInstanceID());
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        M_rigidbody2D.linearVelocity = Vector2.zero;

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
            colliders[i].enabled = false;

        PlayFirstAvailableState(dieAnimationState, "Die", "die");

        yield return new WaitForSeconds(dieAnimationDuration);

        gameObject.SetActive(false);
    }

    private bool CanSwitchState()
    {
        return !isDying && M_chData != null && M_chData.PlayerEnterRoom;
    }

    private bool NeedMove()
    {
        if (playerTransform == null)
        {
            return false;
        }

        // 获取玩家位置
        Vector2 playerPosition = playerTransform.position;

        // 计算敌人与玩家的距离
        float distanceToPlayer = Vector2.Distance(transform.position, playerPosition);

        // 判断是否需要移动
        return distanceToPlayer > AttackRange;
    }

    private bool IsPlayerInAttackRange()
    {
        if (playerTransform == null)
        {
            return false;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= AttackRange;
    }

    private bool HasLineOfSightToPlayer()
    {
        if (playerTransform == null)
        {
            return false;
        }

        if (obstacleMask.value == 0)
        {
            return true;
        }

        Vector2 origin = transform.position;
        Vector2 target = playerTransform.position;
        RaycastHit2D obstacleHit = Physics2D.Linecast(origin, target, obstacleMask);
        return obstacleHit.collider == null;
    }

    private bool IsPlayerInHateRange()
    {
        if (playerTransform == null)
        {
            return false;
        }

        if (hateRange <= 0f)
        {
            return true;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= hateRange;
    }

    private bool ShouldChase()
    {
        if (!IsPlayerInHateRange())
        {
            return false;
        }

        // 在攻击范围内但被障碍挡住时，继续追击找角度，不允许隔墙开打。
        return !IsPlayerInAttackRange() || !HasLineOfSightToPlayer();
    }

    private bool ShouldAttack()
    {
        return IsPlayerInHateRange() && IsPlayerInAttackRange() && HasLineOfSightToPlayer();
    }

    private bool ShouldIdle()
    {
        return !IsPlayerInHateRange();
    }

    private void MoveWithPathfinding()
    {
        if (playerTransform == null)
        {
            return;
        }

        pathRepathTimer -= Time.fixedDeltaTime;
        if (pathRepathTimer <= 0f)
        {
            RebuildPathToPlayer();
            pathRepathTimer = Mathf.Max(0.05f, pathRepathInterval);
        }

        Vector2 moveTarget = GetCurrentMoveTarget();
        Vector2 moveDirection = moveTarget - (Vector2)transform.position;
        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        ObjectMove.MoveObject(M_rigidbody2D, moveDirection.normalized, M_chData.CurrentMoveSpeed);
    }

    private void RebuildPathToPlayer()
    {
        if (playerTransform == null)
        {
            ClearPath();
            return;
        }

        Vector2 start = transform.position;
        Vector2 goal = playerTransform.position;

        if (obstacleMask.value == 0 || !Physics2D.Linecast(start, goal, obstacleMask))
        {
            ClearPath();
            return;
        }

        bool foundPath = Enemy1Pathfinding2D.TryBuildPath(
            start,
            goal,
            pathCellSize,
            pathNodeCheckRadius,
            obstacleMask,
            pathMaxSearchNodes,
            currentPath
        );

        if (foundPath)
        {
            currentPathIndex = 0;
            return;
        }

        ClearPath();
    }

    private Vector2 GetCurrentMoveTarget()
    {
        if (currentPath.Count == 0)
        {
            return playerTransform != null ? (Vector2)playerTransform.position : (Vector2)transform.position;
        }

        Vector2 currentPosition = transform.position;
        while (currentPathIndex < currentPath.Count)
        {
            float distance = Vector2.Distance(currentPosition, currentPath[currentPathIndex]);
            if (distance > pathWaypointReachDistance)
            {
                break;
            }

            currentPathIndex++;
        }

        if (currentPathIndex >= currentPath.Count)
        {
            return playerTransform != null ? (Vector2)playerTransform.position : currentPosition;
        }

        return currentPath[currentPathIndex];
    }

    private void ClearPath()
    {
        currentPath.Clear();
        currentPathIndex = 0;
    }

    private void ResolvePathNodeRadiusByCollider()
    {
        if (!autoPathNodeCheckRadius)
        {
            return;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        float maxRadius = 0f;
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            if (collider == null || !collider.enabled || collider.isTrigger)
            {
                continue;
            }

            Bounds bounds = collider.bounds;
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.y);
            if (radius > maxRadius)
            {
                maxRadius = radius;
            }
        }

        if (maxRadius <= 0f)
        {
            return;
        }

        float computedRadius = maxRadius * Mathf.Max(0.01f, pathNodeRadiusScale)
            + Mathf.Max(0f, pathNodeRadiusPadding);
        pathNodeCheckRadius = Mathf.Max(pathNodeRadiusMin, computedRadius);
    }
    
    /// <summary>
    /// 能否开始攻击
    /// </summary>
    public void SetCanStartAttack(bool canStart)
    {
        CanStartAttack = canStart;
    }

    /// <summary>
    /// 设置冲撞方向
    /// </summary>
    public void SetAttackDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        attackDirection = direction.normalized;
    }

    /// <summary>
    /// 获取朝向玩家的方向
    /// </summary>
    public Vector2 GetDirectionToPlayer()
    {
        if (playerTransform == null)
        {
            return attackDirection;
        }

        Vector2 direction = (Vector2)playerTransform.position - (Vector2)transform.position;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return attackDirection;
        }

        return direction.normalized;
    }

    private void UpdateAnimationByState(bool force = false)
    {
        Enemy1StateID currentState = M_stateMachine.ActiveStateName;
        if (!force && lastAnimationState.HasValue && lastAnimationState.Value == currentState)
        {
            return;
        }

        PlayAnimationForState(currentState);
        lastAnimationState = currentState;
    }

    private void PlayAnimationForState(Enemy1StateID state)
    {
        switch (state)
        {
            case Enemy1StateID.Idle:
                PlayFirstAvailableState(idleAnimationState, "Idle", "idle", "敌人1_Idle");
                break;
            case Enemy1StateID.Move:
                PlayFirstAvailableState(moveAnimationState, "Move", "move", "敌人1_Move");
                break;
            case Enemy1StateID.Attack:
                PlayFirstAvailableState(attackAnimationState, "Attack", "attack", "敌人1_attack");
                break;
            case Enemy1StateID.Die:
                //PlayFirstAvailableState(dieAnimationState, "Die", "die");
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
