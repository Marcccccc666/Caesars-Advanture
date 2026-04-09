using UnityEngine;
using UnityHFSM;

public class Boss2_PhaseTransition : BaseState<Boss2HFSM.Boss2StateID>
{
    private readonly Boss2HFSM boss;

    public Boss2_PhaseTransition(Boss2HFSM boss) : base(true)
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        boss.ResetPhaseTransition();
    }

    public override void OnLogic()
    {
        base.OnLogic();
        
        boss.OnPhaseTransitionEnd();
        fsm.StateCanExit();
    }
}
