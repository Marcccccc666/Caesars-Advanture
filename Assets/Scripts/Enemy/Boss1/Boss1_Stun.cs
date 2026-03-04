public class Boss1_Stun : BaseState<Boss1HFSM.Boss1StateID>
{
    private readonly Boss1HFSM boss;
    private DownTimer stunTimer;
    private MultiTimerManager timerManager => MultiTimerManager.Instance;

    public Boss1_Stun(Boss1HFSM boss) : base()
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        boss.EnterStun();

        float duration = boss.CurrentStunDuration;
        string key = "Boss1_Stun_" + boss.GetInstanceID();
        stunTimer = timerManager.Create_DownTimer(key, duration);
        stunTimer.SetDuration(duration);
        stunTimer.StartTimer();
    }

    public override void OnLogic()
    {
        base.OnLogic();
        if (stunTimer != null && stunTimer.IsComplete())
        {
            boss.SetStunComplete();
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        if (stunTimer != null && stunTimer.IsRunning)
            stunTimer.PauseTimer();
    }
}
