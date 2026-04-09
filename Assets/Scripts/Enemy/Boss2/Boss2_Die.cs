using UnityEngine;
using UnityHFSM;

public class Boss2_Die : BaseState<Boss2HFSM.Boss2StateID>
{
    private readonly Boss2HFSM boss;

    public Boss2_Die(Boss2HFSM boss) : base(needsExitTime: false)
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        boss.EnterDie();
    }
}
