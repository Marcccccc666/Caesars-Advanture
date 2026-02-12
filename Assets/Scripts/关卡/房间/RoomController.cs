using UnityEngine;
using UnityHFSM;

/// <summary>
/// f房间类型
/// </summary>
public enum RoomType
{
    /// <summary>
    /// 怪物房间
    /// </summary>
    Normal,
    
    /// <summary>
    /// 精英房间 
    /// </summary>
    Boss,
}

public class RoomController : MonoBehaviour
{
    /// <summary>
    /// 房间类型
    /// </summary>
    [SerializeField, ChineseListLabel("房间类型")] private RoomType M_RoomType;
    
    /// <summary>
    /// 门列表
    /// </summary>
    [SerializeField, ChineseListLabel("门")] private GameObject Doors;

    /// <summary>
    /// 房间内敌人列表
    /// </summary>
    [SerializeField, ChineseListLabel("房间内敌人")] private EnemyData[] EnemiesInRoom;

    /// <summary>
    /// 房间状态机
    /// </summary>
    private StateMachine<RoomState,RoomType> M_StateMachine = new();

    /// <summary>
    /// 是否锁门
    /// </summary>
    private bool LockRoom = false;

    private EnemyManager enemyManager => EnemyManager.Instance;

    private void Awake()
    {
        Doors.SetActive(false);
        for (int i = 0; i < EnemiesInRoom.Length; i++)
        {
            EnemiesInRoom[i].gameObject.SetActive(false);
        }

        RoomStateMachineInit();
        M_StateMachine.Init();
    }

    void Update()
    {
        M_StateMachine.OnLogic();
    }

    public enum RoomState
    {
        /// <summary>
        /// 未访问
        /// </summary>
        Unvisited,
        /// <summary>
        /// 战斗中
        /// </summary>
        Fighting,
        /// <summary>
        /// 已清除
        /// </summary>
        Cleared
    }

    private void RoomStateMachineInit()
    {
        //增加状态
            //未访问状态
            M_StateMachine.AddState(RoomState.Unvisited, new RoomUnvisited());

            //战斗中状态
            M_StateMachine.AddState(RoomState.Fighting, new RoomFighting(Doors));

            //已清除状态
            M_StateMachine.AddState(RoomState.Cleared, new RoomCleared(Doors));

        // 转换条件
            M_StateMachine.AddTransition(RoomState.Unvisited, RoomState.Fighting, t => LockRoom == true);

            M_StateMachine.AddTransition(RoomState.Fighting, RoomState.Cleared, t => enemyManager.EnemyCount <= 0);

        //设置初始状态
        M_StateMachine.SetStartState(RoomState.Unvisited);
    }

    /// <summary>
    /// 玩家进入房间
    /// <para> 如果房间状态为未访问，则锁房间 </para>
    /// <para> TriggerEnter 调用 </para>
    /// </summary>
    public void PlayerEnterRoom()
    {
        if(M_StateMachine.ActiveStateName != RoomState.Unvisited)
        {
            return;
        }
        else
        {
            for (int i = 0; i < EnemiesInRoom.Length; i++)
            {
                int enemyID = EnemiesInRoom[i].gameObject.GetInstanceID();
                enemyManager.AddEnemyData(enemyID, EnemiesInRoom[i]);
            }

            LockRoom = true;
        }
    }

    /// <summary>
    /// 获取房间内所有敌人数据
    /// <para> unity界面按钮调用 </para>
    /// </summary>
    public void GetAllEnemies()
    {
        EnemyData[] enemies = GetComponentsInChildren<EnemyData>();
        EnemiesInRoom = new EnemyData[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            EnemiesInRoom[i] = enemies[i];
        }
    }
}
