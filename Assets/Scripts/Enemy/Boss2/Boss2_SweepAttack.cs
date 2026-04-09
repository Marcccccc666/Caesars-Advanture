using UnityEngine;

public class Boss2_SweepAttack : BaseState<Boss2HFSM.Boss2StateID>
{
    private readonly Boss2HFSM boss;
    private bool damageDealt = false;

    private DownTimer attackTimer;
    private MultiTimerManager timerManager => MultiTimerManager.Instance;

    public Boss2_SweepAttack(Boss2HFSM boss) : base(needsExitTime: true)
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        boss.EnterSweep();
        damageDealt = false;

        string key = "Boss2_SweepAttack_" + boss.GetInstanceID();
        attackTimer = timerManager.Create_DownTimer(key);
        attackTimer.ResetTimer(boss.SweepDuration);
        attackTimer.StartTimer();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (attackTimer == null) return;

        // Timer fallback in case the animation event is missing or fails.
        float elapsed = boss.SweepDuration - attackTimer.GetRemainingTime();
        if (!damageDealt && elapsed >= 0.5f)
        {
            boss.TriggerSweepDamage();
            damageDealt = true;
        }

        if (attackTimer.IsComplete())
        {
            fsm.StateCanExit();
        }
    }

    public override void OnExitRequest()
    {
        // Only allow exit when our attack timer has completed
        if (attackTimer != null && attackTimer.IsComplete())
        {
            fsm.StateCanExit();
        }
    }

    public override void OnExit()
    {
        base.OnExit();

        if (attackTimer != null && attackTimer.IsRunning)
            attackTimer.PauseTimer();
    }
}
