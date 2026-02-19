using UnityEngine;

public class ChooseWeaponBranch : MonoBehaviour
{
    [SerializeField, ChineseLabel("分支选择界面")] private GameObject branchSelectionUI;

    [SerializeField, ChineseLabel("分支卡")] private WeaponBranch[] weaponBranches;
    private WeaponData weaponData => WeaponManager.Instance.GetCurrentWeapon;
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

        if(weaponData.WeaponBaseData is InitialGunData initialGunData)
        {
            GunBrach[] gunBranches = initialGunData.GunBrachs;
            for (int i = 0; i < weaponBranches.Length; i++)
            {
                if (i < gunBranches.Length)
                {
                    weaponBranches[i].SetWeaponBranch(gunBranches[i].Data, gunBranches[i].Type);
                }
            }
        }
    }

    private void OnEnable()
    {
        WeaponManager.Instance.UpgradeCurrentWeapon += OpenBranchSelectionUI;
    }

    private void OnDisable()
    {
        WeaponManager.Instance.UpgradeCurrentWeapon -= OpenBranchSelectionUI;
    }

    private void OpenBranchSelectionUI()
    {
        if (branchSelectionUI != null)
        {
            branchSelectionUI.SetActive(true);
        }
    }

    /// <summary>
    /// 选择武器分支
    /// </summary>
    /// <param name="index">武器分支索引</param>
    public void SelectWeaponBranch(int index)
    {
        selectedBranchIndex = index;
        for (int i = 0; i < weaponBranches.Length; i++)
        {
            weaponBranches[i].SetSelectedIndicator(i == index);
        }
    }

    public void ConfirmSelection()
    {
        if (selectedBranchIndex >= 0 && weaponData.WeaponBaseData is InitialGunData initialGunData)
        {
            Destroy(weaponData.gameObject);
            GunBrach selectedBranch = initialGunData.GunBrachs[selectedBranchIndex];
            Transform weaponHoldPoint = CharacterManager.Instance.GetCurrentPlayerCharacterData.GetWeaponHoldPoint();
            WeaponData newWeaponData = Instantiate(selectedBranch.Data, weaponHoldPoint.position, weaponHoldPoint.rotation, weaponHoldPoint);
        }
    }
}
