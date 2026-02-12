using UnityEngine;

public class BulletAttack : MonoBehaviour
{
    private int bulletDamage = 10;
    private EnemyManager enemyManager => EnemyManager.Instance;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyData enemyData = collision.GetComponentInParent<EnemyData>();
            if (enemyData == null)
            {
                Destroy(gameObject);
                return;
            }

            int enemyID = enemyData.gameObject.GetInstanceID();
            if (enemyManager != null && enemyManager.GetEnemyDataDict.TryGetValue(enemyID, out EnemyData managedData))
            {
                enemyData = managedData;
            }

            enemyData.CurrentHealth -= bulletDamage;
            if (enemyData.CurrentHealth <= 0)
            {
                if (enemyManager != null && enemyManager.GetEnemyDataDict.ContainsKey(enemyID))
                {
                    enemyManager.RemoveEnemyData(enemyID);
                }
                else
                {
                    Destroy(enemyData.gameObject);
                }
            }

            Destroy(gameObject);
        }
        else if(collision.CompareTag("Wall") || !collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }

    public void SetBulletDamage(int damage)
    {
        bulletDamage = damage;
    }
}
