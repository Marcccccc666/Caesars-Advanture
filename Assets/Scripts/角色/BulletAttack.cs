using UnityEngine;

public class BulletAttack : MonoBehaviour
{
    private int bulletDamage = 10;
    private GameManager GameManager => GameManager.Instance;
    private WeaponManager weaponManager => WeaponManager.Instance;
    private EnemyManager enemyManager => EnemyManager.Instance;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            int enemyID = collision.gameObject.GetInstanceID();
            EnemyData enemyData = enemyManager.GetEnemyDataDict[enemyID];
            enemyData.CurrentHealth -= bulletDamage;

            if(enemyData.CurrentHealth <= 0)
            {
                enemyManager.RemoveEnemyData(enemyID);

                // 检查是否所有敌人都已被消灭
                int numberOfEnemies = enemyManager.GetEnemyDataDict.Count;
                if(numberOfEnemies <= 0)
                {
                    GameManager.IsGameOver = true;
                }
            }
            Destroy(gameObject);
        }
        else if(collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }

    public void SetBulletDamage(int damage)
    {
        bulletDamage = damage;
    }
}
