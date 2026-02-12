using UnityEngine;

public class WeaponData : MonoBehaviour
{
    [SerializeField, ChineseLabel("武器基础数据")] protected WeaponBaseData weaponBaseData;
    /// <summary>
    /// 武器基础数据
    /// </summary>
    public WeaponBaseData WeaponBaseData => weaponBaseData;
}
