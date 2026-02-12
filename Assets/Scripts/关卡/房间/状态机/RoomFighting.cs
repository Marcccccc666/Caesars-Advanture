using System.Collections.Generic;
using UnityEngine;

public class RoomFighting : BaseState<RoomController.RoomState>
{
    private GameObject Doors;

    private EnemyManager enemyManager = EnemyManager.Instance;
    public RoomFighting(GameObject Doors) : base()
    {
        this.Doors = Doors;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        // 锁门
        Doors.SetActive(true);

        // 放狗
        foreach(var enemy in enemyManager.GetEnemyDataDict.Values)
        {
            enemy.gameObject.SetActive(true);
        }
    }
}
