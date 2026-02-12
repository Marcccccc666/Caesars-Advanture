using UnityEngine;

[CreateAssetMenu(fileName = "AddPenetration", menuName = "Buffs/Add Penetration")]
public class AddPenetration : BuffDefinition
{
    [SerializeField, ChineseLabel("添加穿透力")] private int penetrationBonus = 1;
    
    public override void Apply()
    {
        WeaponManager weaponManager = WeaponManager.Instance;
        if (weaponManager == null)
        {
            return;
        }

        if (weaponManager.GetCurrentWeapon is GunData gunData)
        {
            gunData.AddPenetrationBonus(penetrationBonus);
        }
    }
}
