using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 战斗房间控制器
/// <para> 负责管理战斗房间的敌人和门的开关 </para>
/// </summary>
public abstract class BattleRoomController : RoomBase
{   
    /// <summary>
    /// 开门列表
    /// </summary>
    [SerializeField, ChineseListLabel("开门")] private GameObject OpenDoors;

    /// <summary>
    /// 关门列表
    /// </summary>
    [SerializeField, ChineseListLabel("关门")] private GameObject CloseDoors;

    /// <summary>
    /// 房间内敌人列表
    /// </summary>
    [SerializeField, ChineseListLabel("房间内敌人")] private EnemyData[] EnemiesInRoom;

    [SerializeField, ChineseLabel("房间清空后，要回收的子弹")] protected EnemyBulletAttack enemyBulletProfab;

    /// <summary>
    /// 是否锁门
    /// </summary>
    protected bool LockRoom = false;

    protected EnemyManager enemyManager => EnemyManager.Instance;

    protected override void Awake()
    {
        SetLockRoom(false);
        for (int i = 0; i < EnemiesInRoom.Length; i++)
        {
            EnemiesInRoom[i].PlayerEnterRoom = false;
        }
        
        base.Awake();
    }

    /// <summary>
    /// 玩家进入房间
    /// <para> 如果房间状态为未访问，则锁房间 </para>
    /// <para> TriggerEnter 调用 </para>
    /// </summary>
    public override void PlayerEnterRoom()
    {
        base.PlayerEnterRoom();
        if (M_StateMachine.ActiveStateName != RoomState.Unvisited)
            return;

        SetLockRoom(true);

        for (int i = 0; i < EnemiesInRoom.Length; i++)
        {
            int enemyID = EnemiesInRoom[i].gameObject.GetInstanceID();
            enemyManager.AddEnemyData(enemyID, EnemiesInRoom[i]);
            EnemiesInRoom[i].PlayerEnterRoom = true;
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

    ///<summary>
    /// 设置房间锁门
    /// </summary>
    public void SetLockRoom(bool lockRoom)
    {
        if(lockRoom)
        {
            OpenDoors.SetActive(false);
            CloseDoors.SetActive(true);
        }
        else
        {
            OpenDoors.SetActive(true);
            CloseDoors.SetActive(false);
        }
        this.LockRoom = lockRoom;
    }

#region UNITY_EDITOR
    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    protected override void OnValidate()
    {
        if (RoomCamera == null)
        {
            RoomCamera = GetComponentInChildren<CinemachineCamera>();
            if (RoomCamera == null)
            {
                Debug.LogError("RoomController脚本未找到CinemachineCamera组件，请检查！");
            }
        }

        if(OpenDoors == null)
        {
            OpenDoors = transform.Find("Doors")?.transform.Find("Open")?.gameObject;
            if (OpenDoors == null)
            {
                Debug.LogError("RoomController脚本未找到OpenDoors子对象，请检查！");
            }
        }

        if(CloseDoors == null)
        {
            CloseDoors = transform.Find("Doors")?.transform.Find("Close")?.gameObject;
            if (CloseDoors == null)
            {
                Debug.LogError("RoomController脚本未找到CloseDoors子对象，请检查！");
            }
        }
    }
#endregion
}
