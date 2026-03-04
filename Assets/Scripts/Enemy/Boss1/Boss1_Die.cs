public class Boss1_Die : BaseState<Boss1HFSM.Boss1StateID>
{
    private readonly Boss1HFSM boss;

    public Boss1_Die(Boss1HFSM boss) : base()
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        boss.EnterDie();
    }
}
