using UnityEngine;
using UnityHFSM;

public class Boss2_Idle : BaseState<Boss2HFSM.Boss2StateID>
{
    private readonly Boss2HFSM boss;
    private float enterTime;

    public Boss2_Idle(Boss2HFSM boss) : base(needsExitTime: true)
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        boss.EnterIdle();
        enterTime = Time.time;
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (Time.time - enterTime >= boss.PostAttackPause)
        {
            fsm.StateCanExit();
        }
    }

    public override void OnExitRequest()
    {
        if (Time.time - enterTime >= boss.PostAttackPause)
        {
            fsm.StateCanExit();
        }
    }
}
