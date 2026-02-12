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

    /// <summary>
    /// 攻击范围
    /// </summary>
    private float AttackRange = 0f;

    /// <summary>
    /// 玩家位置
    /// </summary>
    private Transform playerTransform => characterManager.GetCurrentPlayerCharacterData.transform;
    private CharacterManager characterManager => CharacterManager.Instance;
    
    /// <summary>
    /// 可以开始攻击
    /// </summary>
    private bool CanStartAttack = false;
    private Vector2 attackDirection = Vector2.right;
    private Enemy1StateID? lastAnimationState = null;

    private StateMachine<Enemy1StateID, Enemy1> M_stateMachine = new();
    
    void Awake()
    {
        M_chData = GetComponent<EnemyData>();
        M_chData.InitObjectData();
        
        M_rigidbody2D = GetComponent<Rigidbody2D>();
        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponent<Animator>();
        }
        AttackRange = GetComponentInChildren<AttackRangeGizmo>().GetAttackRange;

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
        // 移动游戏对象
        if(NeedMove()&& M_stateMachine.ActiveStateName == Enemy1StateID.Move)
        {
            Vector2 directionToPlayer = ((Vector2)playerTransform.position - (Vector2)this.transform.position).normalized;
            ObjectMove.MoveObject(M_rigidbody2D, directionToPlayer, M_chData.CurrentMoveSpeed);
        }
        else if(M_stateMachine.ActiveStateName == Enemy1StateID.Attack && CanStartAttack)
        {
            ObjectMove.MoveObject(M_rigidbody2D, attackDirection, collisionSpeed);
        }
    }

    void Update()
    {
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
                M_stateMachine.AddTransition(Enemy1StateID.Idle, Enemy1StateID.Move, t => NeedMove());
            //待机 -> 攻击
                M_stateMachine.AddTransition(Enemy1StateID.Idle, Enemy1StateID.Attack, t => !NeedMove());
            
            //移动 -> 攻击
                M_stateMachine.AddTransition(Enemy1StateID.Move, Enemy1StateID.Attack, t => !NeedMove());

            //攻击 -> 移动
                M_stateMachine.AddTransition(Enemy1StateID.Attack, Enemy1StateID.Move, t => NeedMove());
            
        M_stateMachine.SetStartState(Enemy1StateID.Idle);
            
    }

    private bool NeedMove()
    {
        // 获取玩家位置
        Vector2 playerPosition = playerTransform.position;

        // 计算敌人与玩家的距离
        float distanceToPlayer = Vector2.Distance(this.transform.position, playerPosition);

        // 判断是否需要移动
        return distanceToPlayer > AttackRange;
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
                // 当前未配置死亡动画，保持现状。
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
