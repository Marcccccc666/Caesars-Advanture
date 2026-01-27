using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    private LevelManager levelManager => LevelManager.Instance;

    public ThemeData currentThemeData;

    private void Awake()
    {
        // 设置当前关卡数据
        levelManager.SetCurrentLevel(currentThemeData);
    }

    /// <summary>
    /// 前往下一关
    /// </summary>
    public void NextLevel()
    {
        if(levelManager.IsLastLevel())
        {
            Debug.Log("已经是最后一关了！");
            return;
        }
        else
        {
            List<LevelData> levelDataList = levelManager.GetLevelDataList();
            levelManager.GotoNextLevelAction?.Invoke(levelDataList[levelManager.CurrentLevelIndex]);
            levelManager.CurrentLevelIndex++;
        }
    }
}
