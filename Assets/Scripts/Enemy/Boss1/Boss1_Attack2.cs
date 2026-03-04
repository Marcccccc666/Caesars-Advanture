using UnityEngine;

public class Boss1_Attack2 : BaseState<Boss1HFSM.Boss1StateID>
{
    private readonly Boss1HFSM boss;

    private enum SubState { ChargeUp, Dash, Pause }

    private SubState subState;
    private int dashesCompleted;

    private Vector2 dashDirection;
    private Vector2 dashStartPosition;
    private bool isDashing;

    private DownTimer subStateTimer;
    private DownTimer contactDmgTimer;
    private MultiTimerManager timerManager => MultiTimerManager.Instance;

    public Boss1_Attack2(Boss1HFSM boss) : base()
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        boss.OnAttackStart();

        isDashing = false;
        dashesCompleted = 0;

        string id = boss.GetInstanceID().ToString();
        subStateTimer = timerManager.Create_DownTimer("Boss1_A2_Sub_" + id);
        contactDmgTimer = timerManager.Create_DownTimer("Boss1_A2_Contact_" + id);
        contactDmgTimer.ResetTimer(0f);

        BeginChargeUp();
    }

    public override void OnLogic()
    {
        base.OnLogic();

        switch (subState)
        {
            case SubState.ChargeUp:
                if (subStateTimer.IsComplete())
                {
                    StartDash();
                    FireSideBullets();
                    subState = SubState.Dash;
                }
                break;

            case SubState.Dash:
                if (HasDashReachedDistance())
                {
                    StopDash();
                    dashesCompleted++;
                    if (dashesCompleted >= boss.Attack2TargetDashCount)
                    {
                        boss.OnAttackComplete(1);
                    }
                    else
                    {
                        subState = SubState.Pause;
                        subStateTimer.ResetTimer(boss.Attack2DashInterval);
                        subStateTimer.StartTimer();
                    }
                }
                break;

            case SubState.Pause:
                if (subStateTimer.IsComplete())
                    BeginChargeUp();
                break;
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        StopDash();
        if (subStateTimer != null && subStateTimer.IsRunning)
            subStateTimer.PauseTimer();
        if (contactDmgTimer != null && contactDmgTimer.IsRunning)
            contactDmgTimer.PauseTimer();
    }

    public void FixedTick()
    {
        if (!isDashing)
        {
            boss.Rb2D.linearVelocity = Vector2.zero;
            return;
        }

        ObjectMove.MoveObject(boss.Rb2D, dashDirection, boss.Attack2DashSpeed);
        CheckContactDamage();
    }

    private void BeginChargeUp()
    {
        subState = SubState.ChargeUp;
        subStateTimer.ResetTimer(boss.Attack2ChargeUpDuration);
        subStateTimer.StartTimer();
    }

    private void StartDash()
    {
        dashDirection = boss.GetDirectionToPlayer();
        dashStartPosition = boss.Rb2D.position;
        isDashing = true;
    }

    private void StopDash()
    {
        isDashing = false;
        boss.Rb2D.linearVelocity = Vector2.zero;
    }

    private bool HasDashReachedDistance()
    {
        return Vector2.Distance(dashStartPosition, boss.Rb2D.position) >= boss.Attack2DashDistance;
    }

    private void FireSideBullets()
    {
        Vector2 pos = boss.transform.position;
        Vector2 left = new Vector2(-dashDirection.y, dashDirection.x);
        Vector2 right = new Vector2(dashDirection.y, -dashDirection.x);
        boss.SpawnBullet(pos, left);
        boss.SpawnBullet(pos, right);
    }

    private void CheckContactDamage()
    {
        if (contactDmgTimer == null || !contactDmgTimer.IsComplete())
            return;

        Rigidbody2D rb = boss.Rb2D;
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            rb.position, boss.ColliderRadius + 0.1f, boss.DamageMask);

        for (int i = 0; i < hits.Length; i++)
        {
            if (boss.IsSelfCollider(hits[i]))
                continue;

            CharacterDate playerData = boss.GetPlayerData(hits[i]);
            if (playerData != null)
            {
                int damage = boss.EnemyDataRef != null ? boss.EnemyDataRef.CurrentAttack : 1;
                playerData.Damage(damage);
                contactDmgTimer.ResetTimer(boss.ContactDamageCooldown);
                contactDmgTimer.StartTimer();
                return;
            }
        }
    }
}
