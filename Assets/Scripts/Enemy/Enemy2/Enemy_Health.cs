using UnityEngine;

[RequireComponent(typeof(EnemyData))]
public class Enemy_Health : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private Enemy_HitEffect1 enemyHitEffect;
    [SerializeField] private float knockbackForce = 5f;

    private EnemyManager enemyManager => EnemyManager.Instance;
    private CharacterManager characterManager => CharacterManager.Instance;

    private void Awake()
    {
        if (enemyData == null)
        {
            enemyData = GetComponent<EnemyData>();
        }

        if (enemyHitEffect == null)
        {
            enemyHitEffect = GetComponent<Enemy_HitEffect1>();
        }
    }

    public void TakeDamage(int damage)
    {
        if (enemyData == null)
        {
            return;
        }

        enemyData.Damage(damage);

        Transform hitSource = characterManager.GetCurrentPlayerCharacterData != null
            ? characterManager.GetCurrentPlayerCharacterData.transform
            : null;
        if (enemyHitEffect != null && hitSource != null)
        {
            enemyHitEffect.Knockback(hitSource, knockbackForce);
        }

        if (enemyData.CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        int enemyId = gameObject.GetInstanceID();
        if (enemyManager.GetEnemyDataDict.ContainsKey(enemyId))
        {
            enemyManager.RemoveEnemyData(enemyId);
            return;
        }

        Destroy(gameObject);
    }
}
