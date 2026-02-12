using System;
using UnityEngine;

/// <summary>
/// 管理游戏状态
/// </summary>
public class GameManager : Singleton<GameManager>
{

#region 游戏暂停
    /// <summary>
    /// 暂停游戏
    /// </summary>
    private bool isGamePaused = false;

    /// <summary>
    /// 游戏是否暂停
    /// </summary>
    public bool IsGamePaused => isGamePaused;

    public Action GamePausedAction;

    /// <summary>
    /// 设置游戏是否暂停
    /// </summary>
    public void SetGamePaused(bool paused)
    {
        isGamePaused = paused;
        if (isGamePaused)
        {
            GamePausedAction?.Invoke();
        }
    }
#endregion

#region 玩家可操作
    /// <summary>
    /// 玩家是否可操作
    /// <para> 玩家不可操作的情况包括：游戏暂停、正在选择 Buff </para>
    /// <para> Ture 表示玩家可操作, False 表示玩家不可操作 </para>
    /// </summary>
    public bool IsPlayerControllable => !isGamePaused && !(BuffManager.Instance.IsBuffSelectionOpen);
#endregion
}
