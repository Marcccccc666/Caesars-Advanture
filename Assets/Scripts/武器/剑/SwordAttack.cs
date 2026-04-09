using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    [SerializeField, ChineseLabel("剑数据")] private SwordDate swordData;

    private WeaponManager weaponManager => WeaponManager.Instance;
    private EnemyManager enemyManager => EnemyManager.Instance;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            int BaseDamage = swordData.WeaponBaseData.WeaponDamage;
            int finalDamage = weaponManager.GetFinalDamage(BaseDamage);

            int enemyId = collision.gameObject.GetInstanceID();
            EnemyData enemyData = enemyManager.GetEnemyData(enemyId);
            if (enemyData != null)
            {
                enemyData.Damage(finalDamage);
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (swordData == null)
        {
            swordData = GetComponentInParent<SwordDate>();
        }
    }
#endif
}
