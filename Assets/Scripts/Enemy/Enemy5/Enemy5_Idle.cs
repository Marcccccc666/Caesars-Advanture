public class Enemy5_Idle : BaseState<Enemy5HFSM.Enemy5StateID>
{
    private readonly Enemy5HFSM enemy;

    public Enemy5_Idle(Enemy5HFSM enemy) : base()
    {
        this.enemy = enemy;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        enemy.EnterIdle();
    }
}
