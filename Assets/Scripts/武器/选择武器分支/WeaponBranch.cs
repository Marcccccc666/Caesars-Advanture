using TMPro;
using UnityEngine;

public class WeaponBranch : MonoBehaviour
{
    [SerializeField, ChineseLabel("武器名称")] private TextMeshProUGUI weaponNameText;

    [SerializeField,ChineseLabel("武器数据")] private WeaponData weaponData;

    [SerializeField,ChineseLabel("选中时的提示UI")] private GameObject selectedIndicator;

    void OnEnable()
    {
        SetSelectedIndicator(false);
    }

    /// <summary>
    /// 设置武器分支显示内容
    /// </summary>
    public void SetWeaponBranch(WeaponData weaponData, GunType gunType)
    {
        this.weaponData = weaponData;
        if (weaponData != null)
        {
            weaponNameText.text = gunType.ToString();
        }
        else
        {
            weaponNameText.text = "None";
        }
    }

    /// <summary>
    /// 设置选中状态的显示
    /// </summary> 
    /// <param name="isSelected">是否选中</param>
    public void SetSelectedIndicator(bool isSelected)
    {
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(isSelected);
        }
    }
}
