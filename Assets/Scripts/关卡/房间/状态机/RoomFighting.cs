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

        // 放狗
        foreach(var enemy in enemyManager.GetEnemyDataDict.Values)
        {
            enemy.gameObject.SetActive(true);
        }
    }
}
