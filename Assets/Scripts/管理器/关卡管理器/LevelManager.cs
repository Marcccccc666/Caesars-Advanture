using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    [SerializeField, ChineseLabel("关卡数据列表")] private List<LevelData> levelDataList;

    /// <summary>
    /// 获取关卡数据列表
    /// </summary>
    public List<LevelData> GetLevelDataList()
    {
        return levelDataList;
    }

    public void SetCurrentLevel(ThemeData themeData)
    {
        levelDataList = new List<LevelData>(themeData.Levels);
        currentLevel = 0;
    }

    private int currentLevel = 0;
    /// <summary>
    /// 获取当前关卡索引
    /// </summary>
    public int CurrentLevelIndex
    {
        get => currentLevel;
        set => currentLevel = value;
    }

    /// <summary>
    /// 前往下一关的动作
    /// </summary>
    public Action<LevelData> GotoNextLevelAction;

    /// <summary>
    /// 是否是最后一关
    /// </summary>
    public bool IsLastLevel()
    {
        return currentLevel > levelDataList.Count - 1;
    }
}
