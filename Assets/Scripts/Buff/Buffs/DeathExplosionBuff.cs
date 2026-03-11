using UnityEngine;

[CreateAssetMenu(fileName = "死亡发生爆炸", menuName = "Buffs/死亡爆炸")]
public class DeathExplosionBuff : BuffDefinition
{
    [SerializeField, ChineseLabel("爆炸伤害")] private int explosionDamage;
    [SerializeField,ChineseLabel("爆炸Profab")] private ExplosionController explosionPrefab;
    
    public override void Apply()
    {
        Transform PoolTransform = PoolManager.Instance.transform;
        PoolManager.Instance.GetOrCreatePool(explosionPrefab);
        WeaponManager.Instance.SetExplosionDamage(explosionDamage);

        BuffManager.Instance.EnemyKilledTriggered += InstanceExplosion;
    }

    public override void Remove()
    {
        BuffManager.Instance.EnemyKilledTriggered -= InstanceExplosion;
    }

    private void InstanceExplosion(Transform position)
    {
        PoolManager poolManager = PoolManager.Instance;
        WeaponManager weaponManager = WeaponManager.Instance;

        ExplosionController explosion = poolManager.Spawn(
                                            prefab: explosionPrefab,
                                            pos: position.position,
                                            rot: Quaternion.identity,
                                            setActive: false
                                        );
        if (explosion)
        {
            int finalDamage = weaponManager.GetExplosionDamage;
            explosion.Initialize(finalDamage);
            explosion.gameObject.SetActive(true);
        }

    }

}
