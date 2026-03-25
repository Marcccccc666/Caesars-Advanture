public class Enemy5_Attack : BaseState<Enemy5HFSM.Enemy5StateID>
{
    private readonly Enemy5HFSM enemy;

    public Enemy5_Attack(Enemy5HFSM enemy) : base()
    {
        this.enemy = enemy;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        enemy.EnterAttack();
    }
}
