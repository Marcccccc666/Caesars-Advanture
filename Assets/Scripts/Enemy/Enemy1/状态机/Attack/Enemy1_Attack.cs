using UnityEngine;

public class Enemy1_Attack : BaseState<Enemy1HFSM.Enemy1StateID>
{
    private Enemy1HFSM M_enemy1HFSM;

    /// <summary>
    /// 冲撞距离
    /// </summary>
    private float collisionDistance;

    /// <summary>
    /// 起点位置
    /// </summary>
    private Vector2 startPosition;
    private readonly float collisionPrepareDuration;
    private float prepareTimer;
    private bool dashStarted;

    public Enemy1_Attack(Enemy1HFSM enemy1, float collisionDistance, float collisionPrepareDuration)
        : base()
    {
        M_enemy1HFSM = enemy1;
        this.collisionDistance = collisionDistance;
        this.collisionPrepareDuration = collisionPrepareDuration;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        startPosition = M_enemy1HFSM.transform.position;
        M_enemy1HFSM.SetAttackDirection(M_enemy1HFSM.GetDirectionToPlayer());
        M_enemy1HFSM.SetCanStartAttack(false);
        dashStarted = false;
        prepareTimer = Mathf.Max(0f, collisionPrepareDuration);
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (!dashStarted)
        {
            prepareTimer -= Time.deltaTime;
            if (prepareTimer > 0f)
            {
                return;
            }

            dashStarted = true;
            M_enemy1HFSM.SetCanStartAttack(true);
        }

        // 检查是否达到冲撞距离
        if (HasReachedCollisionDistance())
        {
            M_enemy1HFSM.SetCanStartAttack(false);
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        M_enemy1HFSM.SetCanStartAttack(false);
    }

    private bool HasReachedCollisionDistance()
    {
        float distanceTraveled = Vector2.Distance(startPosition, M_enemy1HFSM.transform.position);
        return distanceTraveled >= collisionDistance;
    }
}
