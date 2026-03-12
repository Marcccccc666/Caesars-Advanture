using UnityEngine;

public class BossRoomCleared : BaseState<RoomState>
{
    private BossRoomController roomController;
    private EnemyBulletAttack enemyBulletProfab;

    private PoolManager poolManager => PoolManager.Instance;
    private CameraManager cameraManager => CameraManager.Instance;
    public BossRoomCleared(BossRoomController roomController, EnemyBulletAttack enemyBulletProfab) : base()
    {
        this.roomController = roomController;
        this.enemyBulletProfab = enemyBulletProfab;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        // 回收房间内的敌人子弹
        if(enemyBulletProfab)
        {
            poolManager?.ReleasePool(enemyBulletProfab);
        }

        // 开门
        roomController.SetLockRoom(false);

        // 重置摄像机
        cameraManager?.ResetToDefaultCamera();

        
    }
}
