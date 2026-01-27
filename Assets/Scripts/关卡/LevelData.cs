using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [SerializeField, ChineseListLabel("本关卡所有所有种类的敌人")]private EnemyData[] enemies;

    /// <summary>
    /// 本关卡所有所有种类的敌人
    /// </summary>
    public EnemyData[] Enemies => enemies;

    [SerializeField, ChineseLabel("关卡中出现多少敌人")] private int enemyCount;

    /// <summary>
    /// 关卡中出现多少敌人
    /// </summary>
    public int EnemyCount => enemyCount;


}
