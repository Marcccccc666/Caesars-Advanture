using UnityEngine;
using UnityHFSM;

public enum Character{}

[RequireComponent(typeof(Rigidbody2D))]
public class Caesar_Controller : MonoBehaviour
{
    /// <summary>
    /// 角色数据
    /// </summary>
    [SerializeField,ChineseLabel("角色数据")]private CaesarData M_chData;

    [Header("攻击设置")]
    [SerializeField,ChineseLabel("枪口位置")]private Transform M_gunMuzzle;

    [SerializeField,ChineseLabel("子弹预制体")]private Rigidbody2D M_bulletPrefab;

    [Header("动画设置")]
    [SerializeField,ChineseLabel("动画控制器")] private Animator M_animator;
    [SerializeField,ChineseLabel("移动动画名")] private string M_moveAnimaeName;

    [Header("音频设置")]
    [SerializeField,ChineseLabel("攻击音效")]private AudioClip M_attackAudioClip;

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
    private InputData InputData => InputData.Instance;

    /// <summary>
    /// 角色管理器
    /// </summary>
    private CharacterManager characterManager => CharacterManager.Instance;

    public enum Caesar_StateID
    {
        Idle,
        Move,
        Die
    }

    private void Awake()
    {
        characterManager.SetCurrentPlayerCharacterData(M_chData);
        M_rigidbody2D = GetComponent<Rigidbody2D>();

        M_chData.InitObjectData();
        
        CharacterStateMachine();

        Caesar_stateMachine.Init();
    }
    
    private void FixedUpdate()
    {
        M_rigidbody2D.angularVelocity = 0f;
        // 移动角色
        if(InputData.MoveDirection != Vector2.zero)
        {
            ObjectMove.MoveObject(M_rigidbody2D, InputData.MoveDirection, M_chData.CurrentMoveSpeed);
        }
    }

    private void Update()
    {
        Caesar_stateMachine.OnLogic();

        M_animator.SetFloat("Input_X", InputData.MoveDirection.x);
        M_animator.SetFloat("Input_Y", InputData.MoveDirection.y);
    }

    /// <summary>
    /// 角色状态机
    /// </summary>
    void CharacterStateMachine()
    {
        // 添加状态
            // 待机状态
                Caesar_stateMachine.AddState(Caesar_StateID.Idle, new Idle());
            
            // 移动状态
                Caesar_stateMachine.AddState(Caesar_StateID.Move, new Move(M_animator, Animator.StringToHash(M_moveAnimaeName)));

            // 死亡状态
                Caesar_stateMachine.AddState(Caesar_StateID.Die, new Die());

        // 转换条件
            // 待机 -> 移动
                Caesar_stateMachine.AddTransition(Caesar_StateID.Idle, Caesar_StateID.Move, t => InputData.MoveDirection != Vector2.zero);

            // 移动 -> 待机
                Caesar_stateMachine.AddTransition(Caesar_StateID.Move, Caesar_StateID.Idle, t => InputData.MoveDirection == Vector2.zero);

        // 设置初始状态
        Caesar_stateMachine.SetStartState(Caesar_StateID.Idle);
    }
}
