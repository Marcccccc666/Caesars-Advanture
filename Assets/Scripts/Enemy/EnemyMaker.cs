using UnityEngine;
using System.Collections.Generic;

public class EnemyMaker : MonoBehaviour
{
    [SerializeField, ChineseLabel("生成最左高点")] private Transform topPoint;
    [SerializeField, ChineseLabel("生成最右低点")] private Transform bottomPoint;

    [SerializeField, ChineseLabel("生成敌人之间的距离")] private float spawnInterval = 3f;

    private EnemyManager enemyManager => EnemyManager.Instance;
    private LevelManager levelManager => LevelManager.Instance;

    void Awake()
    {
        levelManager.GotoNextLevelAction += MakeEnemy;
    }

    public void MakeEnemy(LevelData levelData)
    {
        List<EnemyData> enemyList = new List<EnemyData>(levelData.Enemies);

        for (int i = 0; i < levelData.EnemyCount; i++)
        {
            Vector2 spawnPosition = RandomSpawnPosition();
            EnemyData enemy =  Instantiate(enemyList[Random.Range(0, enemyList.Count)], spawnPosition, Quaternion.identity);
            enemyManager.AddEnemyData(enemy.gameObject.GetInstanceID(), enemy);
        }
    }

    private Vector2 RandomSpawnPosition()
    {
        float randomY = Random.Range(bottomPoint.position.y, topPoint.position.y);
        float randomX = Random.Range(topPoint.position.x, bottomPoint.position.x);
        
        var EnemyDict = enemyManager.GetEnemyDataDict;
        if(EnemyDict.Count > 0)
        {
            foreach (var enemy in EnemyDict)
            {
                if (Vector2.Distance(new Vector2(randomX, randomY), enemy.Value.gameObject.transform.position) < spawnInterval)
                {
                    return RandomSpawnPosition();
                }
            }
        }
        return new Vector2(randomX, randomY);
    }

}
