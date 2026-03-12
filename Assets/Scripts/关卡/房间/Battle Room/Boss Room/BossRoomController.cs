using UnityEngine;
using UnityHFSM;

public class BossRoomController : BattleRoomController
{
    protected override void RoomStateMachineInit()
    {
        M_StateMachine.AddState(RoomState.Unvisited, new RoomUnvisited());
        M_StateMachine.AddState(RoomState.Fighting, new RoomFighting());
        M_StateMachine.AddState(RoomState.Cleared, new BossRoomCleared(this, enemyBulletProfab));

        M_StateMachine.AddTransition(RoomState.Unvisited, RoomState.Fighting, t => LockRoom == true);

        M_StateMachine.AddTransition(RoomState.Fighting, RoomState.Cleared, t => enemyManager.EnemyCount <= 0);
    }
}
