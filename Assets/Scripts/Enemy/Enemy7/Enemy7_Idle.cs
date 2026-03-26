public class Enemy7_Idle : BaseState<Enemy7HFSM.Enemy7StateID>
{
    private readonly Enemy7HFSM enemy;

    public Enemy7_Idle(Enemy7HFSM enemy) : base()
    {
        this.enemy = enemy;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        enemy.EnterIdle();
    }
}
