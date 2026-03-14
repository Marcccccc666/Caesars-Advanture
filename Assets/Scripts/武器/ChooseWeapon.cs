using UnityEngine;

public class ChooseWeapon : InteractionObjectController
{
    public WeaponData weapon;
    private WeaponManager weaponManager => WeaponManager.Instance;

    protected override void Interact()
    {
        weaponManager.SwitchWeapon(weapon);
        gameObject.SetActive(false);
    }
}
