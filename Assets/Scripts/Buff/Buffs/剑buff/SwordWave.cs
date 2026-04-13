using UnityEngine;

[CreateAssetMenu(fileName = "剑波Buff", menuName = "Buffs/剑波Buff")]
public class SwordWave : BuffDefinition
{
    /// <summary>
    /// 剑波预制体，用于生成剑波攻击效果
    /// </summary>
    [SerializeField, ChineseLabel("剑波预制体")] private BulletAttack SwordWavePrefab;


    private PoolManager poolManager => PoolManager.Instance;
    private InputManager inputManager => InputManager.Instance;
    private BuffManager buffManager => BuffManager.Instance;
    private WeaponManager weaponManager => WeaponManager.Instance;
    public override void Apply()
    {

        poolManager.GetOrCreatePool(SwordWavePrefab);

        buffManager.AttackTriggered += InstantiateSwordWave;
    }

    public override void Remove()
    {

        poolManager.ReleasePool(SwordWavePrefab);
        buffManager.AttackTriggered -= InstantiateSwordWave;
    }

    private void InstantiateSwordWave(Transform transform)
    {
        Vector3 mousePosition = inputManager.MouseWorldPosition;
        Vector2 direction = mousePosition - transform.position;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            direction = transform.right;
        }

        direction.Normalize();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        int basedamage = weaponManager.GetCurrentWeapon.WeaponBaseData.WeaponDamage;
        int finalDamage = weaponManager.GetFinalDamage(basedamage);

        BulletAttack bulletAttack = poolManager.Spawn(
            SwordWavePrefab,
            transform.position,
            Quaternion.Euler(0f, 0f, angle),
            setActive: false);
        bulletAttack.Initialize(direction, 10f, finalDamage, 0, 0); // 设置剑波的移动方向和速度

        bulletAttack.gameObject.SetActive(true);
    }
}
