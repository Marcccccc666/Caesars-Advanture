using System.Collections.Generic;
using UnityEngine;

public class RoomCleared : BaseState<RoomController.RoomState>
{
    private GameObject Doors;

    private BuffManager buffManager => BuffManager.Instance;

    public RoomCleared(GameObject Doors) : base()
    {
        this.Doors = Doors;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Doors.SetActive(false);

        // 房间清理后触发 Buff 选择界面
        buffManager.RequestBuffSelection();
    }
}
