public class Enemy7_Attack : BaseState<Enemy7HFSM.Enemy7StateID>
{
    private readonly Enemy7HFSM enemy;

    public Enemy7_Attack(Enemy7HFSM enemy) : base()
    {
        this.enemy = enemy;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        enemy.EnterAttack();
    }
}
