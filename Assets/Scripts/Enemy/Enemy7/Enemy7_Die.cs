public class Enemy7_Die : BaseState<Enemy7HFSM.Enemy7StateID>
{
    private readonly Enemy7HFSM enemy;

    public Enemy7_Die(Enemy7HFSM enemy) : base()
    {
        this.enemy = enemy;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        enemy.EnterDie();
    }
}
