using UnityEngine;

public class Boss1_Attack3 : BaseState<Boss1HFSM.Boss1StateID>
{
    private readonly Boss1HFSM boss;

    private float baseAngle;

    private DownTimer attackDurationTimer;
    private DownTimer laserDmgTimer;
    private DownTimer bulletFireTimer;
    private MultiTimerManager timerManager => MultiTimerManager.Instance;

    private LineRenderer[] laserRenderers;
    private bool lasersCreated;
    private static Material sharedLaserMaterial;

    public Boss1_Attack3(Boss1HFSM boss) : base()
    {
        this.boss = boss;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        boss.OnAttackStart();

        baseAngle = 0f;

        string id = boss.GetInstanceID().ToString();
        attackDurationTimer = timerManager.Create_DownTimer("Boss1_A3_Duration_" + id);
        attackDurationTimer.ResetTimer(boss.Attack3Duration);
        attackDurationTimer.StartTimer();
        laserDmgTimer = timerManager.Create_DownTimer("Boss1_A3_LaserDmg_" + id);
        laserDmgTimer.ResetTimer(0f);
        bulletFireTimer = timerManager.Create_DownTimer("Boss1_A3_Bullet_" + id);
        bulletFireTimer.ResetTimer(0f);

        if (boss.CurrentPhase == 1)
        {
            EnsureLasersCreated();
            ShowLasers();
            UpdateLaserPositions();
        }
    }

    public override void OnLogic()
    {
        base.OnLogic();

        float angleDelta = boss.Attack3RotateSpeed * Time.deltaTime;
        baseAngle += angleDelta;

        if (boss.CurrentPhase == 1)
        {
            UpdateLaserPositions();
            CheckLaserDamage();
        }
        else
        {
            TryFireRotatingBullets();
        }

        if (attackDurationTimer != null && attackDurationTimer.IsComplete())
        {
            boss.OnAttackComplete(2);
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        HideLasers();
        if (attackDurationTimer != null && attackDurationTimer.IsRunning)
            attackDurationTimer.PauseTimer();
        if (laserDmgTimer != null && laserDmgTimer.IsRunning)
            laserDmgTimer.PauseTimer();
        if (bulletFireTimer != null && bulletFireTimer.IsRunning)
            bulletFireTimer.PauseTimer();
    }

    #region Lasers

    private void EnsureLasersCreated()
    {
        if (lasersCreated)
            return;

        laserRenderers = new LineRenderer[4];
        Material mat = GetLaserMaterial();

        for (int i = 0; i < 4; i++)
        {
            GameObject obj = new GameObject($"Boss1_Laser_{i}");
            obj.transform.SetParent(boss.transform);
            obj.transform.localPosition = Vector3.zero;

            LineRenderer lr = obj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.startWidth = boss.LaserWidth;
            lr.endWidth = boss.LaserWidth;
            lr.startColor = boss.LaserColor;
            lr.endColor = boss.LaserColor;
            lr.numCapVertices = 4;
            lr.sortingOrder = boss.LaserSortingOrder;
            SpriteRenderer ownerSprite = boss.GetComponentInChildren<SpriteRenderer>(true);
            if (ownerSprite != null)
                lr.sortingLayerID = ownerSprite.sortingLayerID;
            lr.alignment = LineAlignment.View;
            lr.textureMode = LineTextureMode.Stretch;
            if (mat != null)
                lr.sharedMaterial = mat;
            lr.enabled = false;

            laserRenderers[i] = lr;
        }

        lasersCreated = true;
    }

    private static Material GetLaserMaterial()
    {
        if (sharedLaserMaterial != null)
            return sharedLaserMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
            ?? Shader.Find("Sprites/Default");
        if (shader == null)
            return null;

        sharedLaserMaterial = new Material(shader) { name = "Boss1_LaserMaterial" };
        return sharedLaserMaterial;
    }

    private void ShowLasers()
    {
        if (laserRenderers == null)
            return;
        for (int i = 0; i < laserRenderers.Length; i++)
            laserRenderers[i].enabled = true;
    }

    private void HideLasers()
    {
        if (laserRenderers == null)
            return;
        for (int i = 0; i < laserRenderers.Length; i++)
            laserRenderers[i].enabled = false;
    }

    private void UpdateLaserPositions()
    {
        Vector2 origin = boss.transform.position;
        float[] offsets = { 0f, 90f, 180f, 270f };

        for (int i = 0; i < 4; i++)
        {
            Vector2 dir = Boss1HFSM.DegreeToDirection(baseAngle + offsets[i]);
            Vector2 end = GetLaserEnd(origin, dir);
            laserRenderers[i].SetPosition(0, (Vector3)origin);
            laserRenderers[i].SetPosition(1, (Vector3)end);
        }
    }

    private Vector2 GetLaserEnd(Vector2 origin, Vector2 direction)
    {
        float length = boss.Attack3LaserLength;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, length, boss.WallMask);
        return hit.collider != null ? hit.point : origin + direction * length;
    }

    private void CheckLaserDamage()
    {
        if (laserDmgTimer == null || !laserDmgTimer.IsComplete())
            return;

        Vector2 origin = boss.transform.position;
        float[] offsets = { 0f, 90f, 180f, 270f };
        float length = boss.Attack3LaserLength;

        for (int i = 0; i < 4; i++)
        {
            Vector2 dir = Boss1HFSM.DegreeToDirection(baseAngle + offsets[i]);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, length);

            for (int j = 0; j < hits.Length; j++)
            {
                Collider2D col = hits[j].collider;
                if (col == null || boss.IsSelfCollider(col))
                    continue;
                if (col.CompareTag("Wall"))
                    break;

                CharacterDate playerData = boss.GetPlayerData(col);
                if (playerData != null)
                {
                    int damage = boss.EnemyDataRef != null ? boss.EnemyDataRef.CurrentAttack : 1;
                    playerData.Damage(damage);
                    laserDmgTimer.ResetTimer(boss.Attack3DamageInterval);
                    laserDmgTimer.StartTimer();
                    return;
                }
            }
        }
    }

    #endregion

    #region Rotating Bullets

    private void TryFireRotatingBullets()
    {
        if (bulletFireTimer == null || !bulletFireTimer.IsComplete())
            return;

        bulletFireTimer.ResetTimer(boss.Attack3BulletInterval);
        bulletFireTimer.StartTimer();

        Vector2 pos = boss.transform.position;
        float[] offsets = { 0f, 90f, 180f, 270f };
        for (int i = 0; i < 4; i++)
        {
            Vector2 dir = Boss1HFSM.DegreeToDirection(baseAngle + offsets[i]);
            boss.SpawnBullet(pos, dir);
        }
    }

    #endregion
}
