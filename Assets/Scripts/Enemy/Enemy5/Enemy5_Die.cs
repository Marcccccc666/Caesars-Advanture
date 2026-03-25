public class Enemy5_Die : BaseState<Enemy5HFSM.Enemy5StateID>
{
    private readonly Enemy5HFSM enemy;

    public Enemy5_Die(Enemy5HFSM enemy) : base()
    {
        this.enemy = enemy;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        enemy.EnterDie();
    }
}
