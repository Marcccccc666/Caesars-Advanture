using System.Collections.Generic;
using UnityEngine;

public class RoomCleared : BaseState<RoomState>
{
    private BuffManager buffManager => BuffManager.Instance;
    private CharacterManager characterManager => CharacterManager.Instance;
    private BattleRoomController battleRoomController;

    public RoomCleared(BattleRoomController battleRoomController) : base()
    {
        this.battleRoomController = battleRoomController;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        battleRoomController.SetLockRoom(false);

        int currentHealth = characterManager.GetCurrentPlayerCharacterData.CurrentHealth;
        if(currentHealth > 0)
        {
            // 房间清理后触发 Buff 选择界面
        buffManager.RequestBuffSelection();
        }
        
    }
}
