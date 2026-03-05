using System.Collections.Generic;
using UnityEngine;

public class RoomFighting : BaseState<RoomState>
{

    private EnemyManager enemyManager = EnemyManager.Instance;
    public RoomFighting() : base()
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
    }
}
