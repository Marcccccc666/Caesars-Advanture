public class Boss1_PhaseTransition : BaseState<Boss1HFSM.Boss1StateID>
{
    private readonly Boss1HFSM boss;

    public Boss1_PhaseTransition(Boss1HFSM boss) : base()
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        boss.EnterPhaseTransition();
    }
}
