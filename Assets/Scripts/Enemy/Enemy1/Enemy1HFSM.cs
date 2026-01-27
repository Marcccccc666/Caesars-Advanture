using UnityEngine;
using UnityHFSM;

public enum Enemy1{}

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyData))]
public class Enemy1HFSM : MonoBehaviour
{
    /// <summary>
    /// 旋转偏差角度
    /// </summary>
    [SerializeField,ChineseLabel("旋转偏差角度")] private float rotationOffsetAngle = 0f;

    /// <summary>
    /// 冲撞速度
    /// </summary>
    [SerializeField, ChineseLabel("冲撞速度")] private float collisionSpeed = 0f;

    /// <summary>
    /// 冲撞距离
    /// </summary>
    [SerializeField, ChineseLabel("冲撞距离")] private float collisionDistance = 0f;

    /// <summary>
    /// 角色数据
    /// </summary>
    private EnemyData M_chData;

    /// <summary>
    /// 刚体
    /// </summary>
    private Rigidbody2D M_rigidbody2D;

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

    private StateMachine<Enemy1StateID, Enemy1> M_stateMachine = new();
    
    void Awake()
    {
        M_chData = GetComponent<EnemyData>();
        M_chData.InitObjectData();
        

        M_rigidbody2D = GetComponent<Rigidbody2D>();
        AttackRange = GetComponentInChildren<AttackRangeGizmo>().GetAttackRange;

        Enemy1StateMachine();
    }

    void Start()
    {
        M_stateMachine.Init();
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
            ObjectMove.MoveObject(M_rigidbody2D, this.transform.right, collisionSpeed);
        }
    }

    void Update()
    {
        M_stateMachine.OnLogic();

        // 旋转头部朝向玩家
        if(M_stateMachine.ActiveStateName == Enemy1StateID.Move && NeedRotate())
        {
            ObjectRotation.RotateTowardsTarget(this.transform, playerTransform.position, M_chData.EnemyBaseData.rotationSpeed);
        }
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
                M_stateMachine.AddState(Enemy1StateID.Attack, new Enemy1_Attack(this, collisionDistance));
            // 死亡状态
                M_stateMachine.AddState(Enemy1StateID.Die, new Enemy1_Die());

        // 添加转换
            //待机 -> 移动
                M_stateMachine.AddTransition(Enemy1StateID.Idle, Enemy1StateID.Move, t => NeedMove());
            //待机 -> 攻击
                M_stateMachine.AddTransition(Enemy1StateID.Idle, Enemy1StateID.Attack, t => !NeedMove() && !NeedRotate());
            
            //移动 -> 攻击
                M_stateMachine.AddTransition(Enemy1StateID.Move, Enemy1StateID.Attack, t => !NeedMove() && !NeedRotate());

            //攻击 -> 移动
                M_stateMachine.AddTransition(Enemy1StateID.Attack, Enemy1StateID.Move, t => NeedMove());
            
        M_stateMachine.SetStartState(Enemy1StateID.Idle);
            
    }

    /// <summary>
    /// 是否需要旋转
    /// </summary>
    private bool NeedRotate()
    {
        // 计算头与玩家的角度差
        Vector2 directionToPlayer = ((Vector2)playerTransform.position - (Vector2)this.transform.position).normalized;
        float angleToPlayer = Vector2.SignedAngle(this.transform.right, directionToPlayer);

        // 判断是否需要旋转
        return Mathf.Abs(angleToPlayer) > rotationOffsetAngle;
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

}
