using UnityEngine;
using UnityHFSM;

public class Boss2_Idle : BaseState<Boss2HFSM.Boss2StateID>
{
    private readonly Boss2HFSM boss;

    public Boss2_Idle(Boss2HFSM boss) : base(needsExitTime: true)
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        boss.EnterIdle();
    }

    public override void OnLogic()
    {
        base.OnLogic();
        if (timer.Elapsed >= 0.5f)
        {
            fsm.StateCanExit();
        }
    }
}
