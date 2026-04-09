using UnityEngine;

[CreateAssetMenu(fileName = "InitialSwordBaseData", menuName = "Scriptable Objects/Sword/InitialSwordBaseData")]
public class InitialSwordBaseData : SwordBaseData, IInitialWeapon
{
    [SerializeField, ChineseLabel("武器分支")]private WeaponBranch[] weaponBranches;
    public WeaponBranch[] WeaponBrachs => weaponBranches;
}
