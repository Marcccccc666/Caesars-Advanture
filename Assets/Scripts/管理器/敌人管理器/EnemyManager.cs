using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    /// <summary>
    /// 敌人角色数据字典
    /// </summary>
    private Dictionary<int, EnemyData> EnemyDataDict = new Dictionary<int, EnemyData>();

    /// <summary>
    /// 敌人角色数据字典
    /// </summary>
    public Dictionary<int, EnemyData> GetEnemyDataDict
    {
        get => EnemyDataDict;
    }

    /// <summary>
    /// 敌人数量
    /// </summary>
    public int EnemyCount
    {
        get => EnemyDataDict.Count;
    }

    /// <summary>
    /// 添加敌人角色数据
    /// </summary>
    /// <param name="id">敌人ID</param>
    /// <param name="characterData">角色数据</param>
    public void AddEnemyData(int id, EnemyData characterData)
    {
        if (!EnemyDataDict.ContainsKey(id))
        {
            EnemyDataDict.Add(id, characterData);
        }
    }

    /// <summary>
    /// 批量添加敌人角色数据到房间
    /// </summary>
    /// <param name="enemiesInRoom">房间内敌人数据字典</param>
    public void AddEnemyData(Dictionary<int, EnemyData> enemiesInRoom)
    {
        foreach(var enemy in enemiesInRoom)
        {
            AddEnemyData(enemy.Key, enemy.Value);
        }
    }

    /// <summary>
    /// 获取敌人角色数据
    /// </summary>
    /// <param name="id">敌人 游戏对象ID</param>
    /// <returns>角色数据</returns>
    public EnemyData GetEnemyData(int id)
    {
        if (EnemyDataDict.TryGetValue(id, out EnemyData characterData))
        {
            return characterData;
        }
        else
        {
            Debug.LogError($"敌人ID为{id}的角色数据不存在");
            return null;
        }
    }

    /// <summary>
    /// 清除所有敌人角色数据
    /// </summary>
    public void ClearEnemyData()
    {
        EnemyDataDict.Clear();
    }

    /// <summary>
    /// 移除敌人角色数据
    /// </summary>
    public void RemoveEnemyData(int id)
    {
        if (EnemyDataDict.ContainsKey(id))
        {
            EnemyDataDict.Remove(id);
            foreach (var i in EnemyDataDict) {
                Debug.Log($"[EnemyManager] NAME={i.Value.gameObject.name}");
            }
            Debug.Log($"[EnemyManager] 敌人已移除 ID={id}, 剩余数量={EnemyDataDict.Count}");
        }
    }
}
