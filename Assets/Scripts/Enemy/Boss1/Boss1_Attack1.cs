using UnityEngine;

public class Boss1_Attack1 : BaseState<Boss1HFSM.Boss1StateID>
{
    private readonly Boss1HFSM boss;

    private Vector2 chargeDirection;
    private int bounceCount;
    private bool isCharging;

    private DownTimer contactDmgTimer;
    private MultiTimerManager timerManager => MultiTimerManager.Instance;

    public Boss1_Attack1(Boss1HFSM boss) : base()
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        boss.OnAttackStart();

        chargeDirection = boss.GetDirectionToPlayer();
        bounceCount = 0;
        isCharging = true;

        string key = "Boss1_A1_Contact_" + boss.GetInstanceID();
        contactDmgTimer = timerManager.Create_DownTimer(key);
        contactDmgTimer.ResetTimer(0f);
    }

    public override void OnLogic()
    {
        base.OnLogic();

        if (bounceCount >= boss.Attack1TargetBounces)
        {
            isCharging = false;
            boss.OnAttackComplete(0);
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        isCharging = false;
        boss.Rb2D.linearVelocity = Vector2.zero;
        if (contactDmgTimer != null && contactDmgTimer.IsRunning)
            contactDmgTimer.PauseTimer();
    }

    public void FixedTick()
    {
        if (!isCharging)
        {
            boss.Rb2D.linearVelocity = Vector2.zero;
            return;
        }

        MoveWithBounce();
        CheckContactDamage();
    }

    private void MoveWithBounce()
    {
        Rigidbody2D rb = boss.Rb2D;
        float speed = boss.Attack1Speed;
        float remaining = speed * Time.fixedDeltaTime;
        float radius = boss.ColliderRadius;
        int maxBouncePerFrame = 3;

        for (int i = 0; i < maxBouncePerFrame && remaining > 0.001f; i++)
        {
            if (bounceCount >= boss.Attack1TargetBounces)
                break;

            RaycastHit2D hit = Physics2D.CircleCast(
                rb.position, radius, chargeDirection, remaining, boss.WallMask);

            if (hit.collider == null)
            {
                rb.MovePosition(rb.position + chargeDirection * remaining);
                break;
            }

            float safeDistance = Mathf.Max(0f, hit.distance - 0.01f);
            rb.MovePosition(rb.position + chargeDirection * safeDistance);
            remaining -= safeDistance;

            chargeDirection = Vector2.Reflect(chargeDirection, hit.normal).normalized;
            bounceCount++;

            if (boss.CurrentPhase >= 2)
                CameraShake.Shake(1);
        }
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
