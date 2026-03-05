using UnityEngine;

[CreateAssetMenu(fileName = "AddBulletBounce", menuName = "Buffs/Add Bullet Bounce")]
public class AddBulletBounce : BuffDefinition
{
    [SerializeField, ChineseLabel("增加反弹次数")] private int bounceBonus = 1;

    public override void Apply()
    {
        WeaponManager weaponManager = WeaponManager.Instance;
        if (weaponManager == null)
        {
            return;
        }

        weaponManager.AddBulletBounceBonus(bounceBonus);
    }
}
