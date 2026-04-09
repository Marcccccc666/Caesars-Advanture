using UnityEngine;

public class ChooseWeaponBranch : MonoBehaviour
{
    [SerializeField, ChineseLabel("分支选择界面")] private GameObject branchSelectionUI;

    [SerializeField, ChineseLabel("分支卡")] private WeaponBranchCard[] weaponBranchCards;
    private WeaponManager weaponManager => WeaponManager.Instance;
    private BuffManager buffManager => BuffManager.Instance;

    /// <summary>
    /// 选中的武器分支索引
    /// </summary>
    private int selectedBranchIndex = -1;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        if (branchSelectionUI != null)
        {
            branchSelectionUI.SetActive(false);
        }

        var currentWeaponData = weaponManager.GetCurrentWeapon;
        if(currentWeaponData != null)
        {
            UpdateSelectionUI(currentWeaponData);
        }
    }

    private void OnEnable()
    {
        weaponManager.UpgradeCurrentWeapon += OpenBranchSelectionUI;
        weaponManager.OnWeaponSwitched += UpdateSelectionUI;
    }

    private void OnDisable()
    {
        if(weaponManager != null)
        {
            weaponManager.UpgradeCurrentWeapon -= OpenBranchSelectionUI;
            weaponManager.OnWeaponSwitched -= UpdateSelectionUI;
        }
    }

    private void OpenBranchSelectionUI()
    {
        if (branchSelectionUI != null)
        {
            branchSelectionUI.SetActive(true);
        }
    }

    private void UpdateSelectionUI(WeaponData newWeapon)
    {
        if(newWeapon.WeaponBaseData is IInitialWeapon initialWeaponData)
        {
            WeaponBranch[] weaponBranches = initialWeaponData.WeaponBrachs;
            for (int i = 0; i < weaponBranches.Length; i++)
            {
                if (i < weaponBranches.Length)
                {
                    weaponBranchCards[i].SetWeaponBranch(weaponBranches[i].Data, weaponBranches[i].Type, weaponBranches[i].WeaponSprite);
                }
                else
                {
                    weaponBranchCards[i].SetWeaponBranch(null, default, null);
                }
            }
        }
    }

    /// <summary>
    /// 选择武器分支
    /// </summary>
    /// <param name="index">武器分支索引</param>
    public void SelectWeaponBranch(int index)
    {
        selectedBranchIndex = index;
        for (int i = 0; i < weaponBranchCards.Length; i++)
        {
            weaponBranchCards[i].SetSelectedIndicator(i == index);
        }
    }

    public void ConfirmSelection()
    {

        // 切换到选中的武器分支
        if (selectedBranchIndex >= 0 && weaponManager.GetCurrentWeapon.WeaponBaseData is IInitialWeapon initialData)
        {
            Debug.Log($"Selected branch index: {selectedBranchIndex}");
            WeaponBranch selectedBranch = initialData.WeaponBrachs[selectedBranchIndex];
            weaponManager.SwitchWeapon(selectedBranch.Data);
        }
        // 结束升级状态，关闭分支选择界面
        weaponManager.SetIsUpgradeInProgress(false);
        branchSelectionUI.SetActive(false);

        // 触发 Buff 选择
        buffManager.RequestBuffSelection();
    }
}
