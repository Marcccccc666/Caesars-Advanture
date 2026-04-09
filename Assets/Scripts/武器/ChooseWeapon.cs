using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ChooseWeapon : InteractionObjectController
{
    public WeaponData weapon;
    public UnityEvent OnWeaponChosen;
    private WeaponManager weaponManager => WeaponManager.Instance;

    protected override void Interact()
    {
        weaponManager.SwitchWeapon(weapon);
        OnWeaponChosen?.Invoke();
        gameObject.SetActive(false);
    }
}
