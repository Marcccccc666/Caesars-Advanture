using UnityEngine;
using UnityHFSM;

public class Boss2_Move : BaseState<Boss2HFSM.Boss2StateID>
{
    private readonly Boss2HFSM boss;
    private Vector2 targetPos;
    private bool reachedTarget;

    public Boss2_Move(Boss2HFSM boss) : base(needsExitTime: true)
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        boss.EnterMove();
        reachedTarget = false;
        UpdateTarget();
    }

    private void UpdateTarget()
    {
        targetPos = boss.GetPlayerPosition();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (reachedTarget)
            return;

        UpdateTarget();

        float dist = Vector2.Distance(boss.Rb2D.position, targetPos);
        if (dist < 0.1f)
        {
            reachedTarget = true;
            boss.Rb2D.linearVelocity = Vector2.zero;
            fsm.StateCanExit();
        }
    }

    public void FixedTick()
    {
        if (reachedTarget)
        {
            boss.Rb2D.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = (targetPos - boss.Rb2D.position).normalized;
        boss.Rb2D.linearVelocity = dir * boss.MoveSpeed;
    }

    public override void OnExitRequest()
    {
        if (reachedTarget)
        {
            fsm.StateCanExit();
        }
    }
}
