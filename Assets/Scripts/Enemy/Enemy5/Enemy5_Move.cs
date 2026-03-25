public class Enemy5_Move : BaseState<Enemy5HFSM.Enemy5StateID>
{
    private readonly Enemy5HFSM enemy;

    public Enemy5_Move(Enemy5HFSM enemy) : base()
    {
        this.enemy = enemy;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        enemy.EnterMove();
    }
}
