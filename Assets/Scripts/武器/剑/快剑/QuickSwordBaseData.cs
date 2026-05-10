using UnityEngine;

[CreateAssetMenu(fileName = "QuickSwordBaseData", menuName = "Scriptable Objects/Sword/QuickSwordBaseData")]
public class QuickSwordBaseData : SwordBaseData, IWeaponSpecificBuff
{
    [SerializeField] private BuffPool weaponSpecificBuffs;

    public BuffPool GetWeaponSpecificBuffs => weaponSpecificBuffs;
}
