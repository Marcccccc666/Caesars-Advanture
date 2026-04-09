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
                if(swordData is HeavySwordData heavySwordData)
                {
                    // 根据蓄力值增加伤害
                    float chargeMultiplier = 1f + (heavySwordData.CurrentCharge / heavySwordData.M_WeaponBaseData.MaxCharge);
                    finalDamage = Mathf.RoundToInt(finalDamage * chargeMultiplier);
                }
                
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
