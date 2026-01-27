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
    /// 获取敌人角色数据
    /// </summary>
    /// <param name="id">敌人ID</param>
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
            Object.Destroy(EnemyDataDict[id].gameObject);
            EnemyDataDict.Remove(id);

            if(EnemyDataDict.Count == 0)
            {
                Debug.Log("所有敌人已被消灭");
                GameManager.Instance.GameCheckout?.Invoke();
            }
        }
    }
}
