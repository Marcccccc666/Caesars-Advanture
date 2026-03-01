using UnityEngine;
using UnityEngine.Pool;
using UnityHFSM;

public enum Character{}

[RequireComponent(typeof(Rigidbody2D), typeof(CaesarData))]
public class Caesar_Controller : MonoBehaviour
{
    /// <summary>
    /// 角色数据
    /// </summary>
    private CaesarData M_chData;

    [SerializeField, ChineseLabel("武器挂点")] private Transform weaponHoldPoint;

    [Header("动画设置")]
    [SerializeField,ChineseLabel("动画控制器")] private Animator M_animator;
    [SerializeField,ChineseLabel("移动动画名")] private string M_moveAnimaeName;

    [SerializeField, ChineseLabel("待机动画名")] private string M_idleAnimateName;

    /// <summary>
    /// 角色刚体
    /// </summary>
    private Rigidbody2D M_rigidbody2D;

    /// <summary>
    /// 角色状态机
    /// </summary>
    private StateMachine<Caesar_StateID, Character> Caesar_stateMachine = new StateMachine<Caesar_StateID, Character>();

    /// <summary>
    /// 输入数据
    /// </summary>
    private InputManager inputManager => InputManager.Instance;

    /// <summary>
    /// 角色管理器
    /// </summary>
    private CharacterManager characterManager => CharacterManager.Instance;

    private GameManager gameManager => GameManager.Instance;

    private BuffManager buffManager => BuffManager.Instance;

    private IObjectPool<Caesar_Controller> pool;
    public enum Caesar_StateID
    {
        Idle,
        Move,
        Die
    }

    public void SetPool(IObjectPool<Caesar_Controller> pool)
    {
       this.pool = pool;
    }

    protected void Awake()
    {
        M_chData = GetComponent<CaesarData>();
        
        M_chData.InitObjectData();
        M_chData.SetweaponHoldPoint(weaponHoldPoint);
        
        M_rigidbody2D = GetComponent<Rigidbody2D>();
        
        CharacterStateMachine();
    }

    private void OnEnable()
    {
        Caesar_stateMachine.Init();
    }
    
    private void FixedUpdate()
    {
        if(!gameManager.IsPlayerControllable)
        {
            return;
        }

        M_rigidbody2D.angularVelocity = 0f;
        // 移动角色
        if(inputManager.MoveDirection != Vector2.zero)
        {
            ObjectMove.MoveObject(M_rigidbody2D, inputManager.MoveDirection, M_chData.CurrentMoveSpeed);
        }
    }

    private void Update()
    {
        if(!gameManager.IsPlayerControllable)
        {
            return;
        }

        M_animator.SetFloat("Input_X", inputManager.MoveDirection.x);
        M_animator.SetFloat("Input_Y", inputManager.MoveDirection.y);

        Caesar_stateMachine.OnLogic();
    }

    private void OnDisable()
    {
        // 在对象被释放回池中时执行必要的清理和状态重置
        M_rigidbody2D.linearVelocity = Vector2.zero;
        M_rigidbody2D.angularVelocity = 0f;
        Caesar_stateMachine.OnExit();
    }

    public void Release()
    {
        pool.Release(this);
    }

    /// <summary>
    /// 角色状态机
    /// </summary>
    void CharacterStateMachine()
    {
        // 添加状态
            // 待机状态
                Caesar_stateMachine.AddState(Caesar_StateID.Idle, new Idle(M_animator, M_idleAnimateName));
            
            // 移动状态
                Caesar_stateMachine.AddState(Caesar_StateID.Move, new Move(M_animator, M_moveAnimaeName));

            // 死亡状态
                Caesar_stateMachine.AddState(Caesar_StateID.Die, new Die());

        // 转换条件
            // 待机 -> 移动
                Caesar_stateMachine.AddTwoWayTransition(Caesar_StateID.Idle, Caesar_StateID.Move, t => inputManager.MoveDirection != Vector2.zero);

        // 设置初始状态
        Caesar_stateMachine.SetStartState(Caesar_StateID.Idle);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (weaponHoldPoint == null)
        {
            weaponHoldPoint = transform.Find("武器");
        }
    }
#endif
}
