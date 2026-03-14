using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    /// <summary>
    /// 游戏暂停事件
    /// </summary>
    public Action GamePausedAction;

    /// <summary>
    /// 游戏恢复事件    
    /// </summary>
    public Action GameResumedAction;

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
        else
        {
            GameResumedAction?.Invoke();
        }
    }
    #endregion

    #region 通关房间
    /// <summary>
    /// 通关房间事件
    /// </summary>
    public Action PassedRoomAction;
    #endregion

    #region 游戏重置

    /// <summary>
    /// 游戏重置事件
    /// </summary>
    public Action GameResetAction;

    #endregion

    #region 游戏切换场景
    public Action GameSceneChangedAction;

    private GameObject BlackScreen;

    public void SetBlackScreen(GameObject blackScreen)
    {
        BlackScreen = blackScreen;
    }

    /// <summary>
    /// 切换场景
    /// </summary>
    /// <param name="SceneName">场景名称</param>
    /// <param name="NeedRetset">是否需要重置游戏</param>
    public void ChangeScene(string SceneName, bool NeedRetset = false)
    {
        if(BlackScreen)
        {
            BlackScreen.SetActive(true);
        }
        
        GameSceneChangedAction?.Invoke();
        if(NeedRetset)
        {
            GameResetAction?.Invoke();
        }
        
        SceneManager.LoadScene(SceneName);
    }
    #endregion

    #region 玩家可操作
    /// <summary>
    /// 玩家是否可操作
    /// <para> 玩家不可操作的情况包括：游戏暂停、正在选择 Buff </para>
    /// <para> Ture 表示玩家可操作, False 表示玩家不可操作 </para>
    /// </summary>
    public bool IsPlayerControllable => !isGamePaused && !BuffManager.Instance.IsBuffSelectionOpen && !WeaponManager.Instance.IsUpgradeInProgress;
    #endregion

    protected override void OnRest()
    {
        isGamePaused = false;
    }
}
