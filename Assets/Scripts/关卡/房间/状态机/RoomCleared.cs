using System.Collections.Generic;
using UnityEngine;

public class RoomCleared : BaseState<RoomState>
{
    private BattleRoomController battleRoomController;
    private bool isFirstRoom;
    private EnemyBulletAttack enemyBulletProfab;
    private bool isBossRoom;
    private GameObject 成功页面;

    private BuffManager buffManager => BuffManager.Instance;
    private CharacterManager characterManager => CharacterManager.Instance;
    protected WeaponManager weaponManager => WeaponManager.Instance;
    private PoolManager poolManager => PoolManager.Instance;
    private CameraManager cameraManager => CameraManager.Instance;


    public RoomCleared(BattleRoomController battleRoomController, bool isFirstRoom, EnemyBulletAttack enemyBullProfab, bool isBossRoom, GameObject 成功页面) : base()
    {
        this.battleRoomController = battleRoomController;
        this.isFirstRoom = isFirstRoom;
        this.enemyBulletProfab = enemyBullProfab;
        this.isBossRoom = isBossRoom;
        this.成功页面 = 成功页面;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        // 回收房间内的敌人子弹
        if(enemyBulletProfab)
        {
            poolManager.ReleasePool(enemyBulletProfab);
        }

        // 开门
        battleRoomController.SetLockRoom(false);

        // 重置摄像机
        cameraManager.ResetToDefaultCamera();

        int currentHealth = characterManager.GetCurrentPlayerCharacterData.CurrentHealth;

        if(isBossRoom)
        {
            成功页面.SetActive(true);
        }
        else if(isFirstRoom && currentHealth > 0)
        {
            weaponManager.UpgradeCurrentWeaponInvoke();
        }
        else if(currentHealth > 0)
        {
            // 房间清理后触发 Buff 选择界面
            buffManager.RequestBuffSelection();
        }
        
    }
}
