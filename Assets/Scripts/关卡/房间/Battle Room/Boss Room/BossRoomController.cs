using UnityEngine;
using UnityEngine.Serialization;
using UnityHFSM;

public class BossRoomController : BattleRoomController
{
    [Header("Boss 设置")]
    [SerializeField, ChineseLabel("Boss 数据")] protected EnemyData bossEnemy;
    [SerializeField, ChineseLabel("Boss 根对象")] protected GameObject bossRoot;
    [SerializeField, FormerlySerializedAs("triggerWeaponUpgradeOnBossDie"), ChineseLabel("Boss 死亡后触发奖励选择")] protected bool enableRewardSelectionOnBossDeath = true;

    private bool rewardSelectionTriggered = false;

    protected WeaponManager weaponManager => WeaponManager.Instance;

    public EnemyData BossEnemy => bossEnemy;
    public GameObject BossRoot => bossRoot;

    protected override void Awake()
    {
        base.Awake();
        ResolveBossReferences();
    }

    protected virtual void OnEnable()
    {
        SubscribeBossDeath();
    }

    protected virtual void OnDisable()
    {
        UnsubscribeBossDeath();
    }

    protected override void RoomStateMachineInit()
    {
        M_StateMachine.AddState(RoomState.Unvisited, new RoomUnvisited());
        M_StateMachine.AddState(RoomState.Fighting, new RoomFighting());
        M_StateMachine.AddState(RoomState.Cleared, new BossRoomCleared(this, enemyBulletProfab));

        M_StateMachine.AddTransition(RoomState.Unvisited, RoomState.Fighting, t => LockRoom == true);

        M_StateMachine.AddTransition(RoomState.Fighting, RoomState.Cleared, t => enemyManager.EnemyCount <= 0);

        M_StateMachine.SetStartState(RoomState.Unvisited);
    }

    protected virtual void OnBossDie()
    {
        if (rewardSelectionTriggered || !enableRewardSelectionOnBossDeath)
            return;

        rewardSelectionTriggered = true;
        TriggerBossDeathRewardSelection();
    }

    protected virtual void TriggerBossDeathRewardSelection()
    {
        weaponManager?.UpgradeCurrentWeaponInvoke();
    }

    private void SubscribeBossDeath()
    {
        if (bossEnemy != null)
        {
            bossEnemy.OnDie += OnBossDie;
        }
    }

    private void UnsubscribeBossDeath()
    {
        if (bossEnemy != null)
        {
            bossEnemy.OnDie -= OnBossDie;
        }
    }

    protected void SetBossVisible(bool visible)
    {
        GameObject targetBoss = bossRoot != null ? bossRoot : bossEnemy != null ? bossEnemy.gameObject : null;
        if (targetBoss != null)
        {
            targetBoss.SetActive(visible);
            return;
        }

        if (bossEnemy == null)
            return;

        Renderer[] renderers = bossEnemy.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = visible;
        }

        Collider2D[] colliders2D = bossEnemy.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders2D.Length; i++)
        {
            colliders2D[i].enabled = visible;
        }

        Collider[] colliders3D = bossEnemy.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders3D.Length; i++)
        {
            colliders3D[i].enabled = visible;
        }
    }

    protected void ResolveBossReferences()
    {
        if (bossEnemy == null && bossRoot != null)
        {
            bossEnemy = bossRoot.GetComponentInChildren<EnemyData>(true);
        }

        if (bossRoot == null && bossEnemy != null)
        {
            bossRoot = bossEnemy.gameObject;
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        ResolveBossReferences();
    }
}
