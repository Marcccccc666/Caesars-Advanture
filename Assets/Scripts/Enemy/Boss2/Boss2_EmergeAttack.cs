using UnityEngine;

public class Boss2_EmergeAttack : BaseState<Boss2HFSM.Boss2StateID>
{
    private const float EmergeDamageDelay = 0.3f;

    private readonly Boss2HFSM boss;
    private bool damageDealt = false;
    private bool pauseComplete = false;
    private float pauseStartTime;
    private float attackStartTime;
    private float preEmergeDelay;

    public Boss2_EmergeAttack(Boss2HFSM boss) : base(needsExitTime: true)
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        damageDealt = false;
        pauseComplete = false;
        pauseStartTime = Time.time;
        attackStartTime = 0f;
        preEmergeDelay = Mathf.Max(0f, boss.PreEmergePause - EmergeDamageDelay);
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (!pauseComplete)
        {
            if (Time.time - pauseStartTime >= preEmergeDelay)
            {
                pauseComplete = true;
                boss.EnterEmerge();
                attackStartTime = Time.time;
            }
            return;
        }

        // Timer fallback in case the animation event is missing or fails.
        float elapsed = Time.time - attackStartTime;
        if (!damageDealt && elapsed >= EmergeDamageDelay)
        {
            boss.TriggerEmergeDamage();
            damageDealt = true;
        }

        if (elapsed >= boss.EmergeDuration)
        {
            fsm.StateCanExit();
        }
    }

    public override void OnExitRequest()
    {
        // Only allow exit when our attack timer has completed
        if (pauseComplete && Time.time - attackStartTime >= boss.EmergeDuration)
        {
            fsm.StateCanExit();
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        boss.OnEmergeAttackExit();
    }
}
