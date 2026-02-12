using UnityEngine;

public class Enemy1_Idle : BaseState<Enemy1HFSM.Enemy1StateID>
{
    public Enemy1_Idle() : base()
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("敌人1进入待机状态");
    }
}
