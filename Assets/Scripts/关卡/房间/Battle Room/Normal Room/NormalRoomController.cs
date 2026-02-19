using UnityEngine;
using UnityHFSM;

/// <summary>
/// 通常房间控制器
/// <para> 状态机的具体实现 </para>
/// </summary>
public class NormalRoomController : BattleRoomController
{
    public bool isFirstRoom = false;
    protected override void RoomStateMachineInit()
    {
        //增加状态
            //未访问状态
            M_StateMachine.AddState(RoomState.Unvisited, new RoomUnvisited());

            //战斗中状态
            M_StateMachine.AddState(RoomState.Fighting, new RoomFighting());

            //已清除状态
            M_StateMachine.AddState(RoomState.Cleared, new RoomCleared(this, isFirstRoom));

        // 转换条件
            M_StateMachine.AddTransition(RoomState.Unvisited, RoomState.Fighting, t => LockRoom == true);

            M_StateMachine.AddTransition(RoomState.Fighting, RoomState.Cleared, t => enemyManager.EnemyCount <= 0);

        //设置初始状态
        M_StateMachine.SetStartState(RoomState.Unvisited);
    }
}
