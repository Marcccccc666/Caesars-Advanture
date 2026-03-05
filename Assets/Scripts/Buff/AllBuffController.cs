using System.Collections.Generic;
using UnityEngine;

public class AllBuffController : MonoBehaviour
{
    /// <summary>
    /// Buff 面板
    /// </summary>
    [SerializeField, ChineseLabel("Buff页面")] private GameObject buffPanel;

    /// <summary>
    /// 3 个 Buff UI 更新脚本
    /// </summary>
    [SerializeField, ChineseLabel("3个BuffUI更新脚本")] private BuffUIUpdata[] buffUIUpdatas;

    private BuffManager buffManager => BuffManager.Instance;
    private WeaponManager weaponManager => WeaponManager.Instance;
    private CharacterManager characterManager => CharacterManager.Instance;
    private GameManager gameManager => GameManager.Instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        if (buffPanel != null)
        {
            buffPanel.SetActive(false);
        }
        OnNewBuffListReceived();
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        weaponManager.OnWeaponSwitched += SetBuffPollData;
        buffManager.OpenBuffSelectionUI += OpenBuffSelectionUI;
    }

    void OnDisable()
    {
        if(weaponManager)
        {
            weaponManager.OnWeaponSwitched -= SetBuffPollData;
        }
        if(buffManager)
        {
            buffManager.OpenBuffSelectionUI -= OpenBuffSelectionUI;
        }
    }

    /// <summary>
    /// 打开 Buff 选择界面
    /// </summary>
    private void OpenBuffSelectionUI()
    {
        if (buffPanel != null)
        {
            buffPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("buffPanel 未设置！");
        }
    }

#region 更新 Buff UI
    /// <summary>
    /// 得到 新的 Buff 列表后 更新 UI
    /// </summary>
    public void OnNewBuffListReceived()
    {
        // 请求生成 3 个随机 Buff
        buffManager.RequestCreateRandomBuff();

        // 更新 Buff UI
        UpdateBuffUI();
    }

    /// <summary>
    /// 更新所有 Buff UI
    /// </summary>
    private void UpdateBuffUI()
    {
        for (int i = 0; i < buffUIUpdatas.Length; i++)
        {
            BuffDefinition buffDefinition = buffManager.CurrentSelection[i];
            buffUIUpdatas[i].UpdateBuffUI(buffDefinition);
        }
    }
#endregion

#region 更新Buff池数据
    /// <summary>
    /// 切换武器时更新 Buff 池数据
    /// </summary> <param name="weaponProfab"> 武器预设 </param> <param name="weaponData"> 武器数据 </param>
    public void SetBuffPollData(WeaponData weaponProfab,WeaponData weaponData)
    {
        WeaponBaseData weaponBaseData = weaponData.WeaponBaseData;
        bool IsInitialWeapon = weaponBaseData is IInitialWeapon;
        if(IsInitialWeapon)
        {
            if(weaponBaseData  is IWeaponSpecificBuff weaponSpecificBuffData)
            {
                buffManager.SetInitialWeaponBuffPool(new List<BuffDefinition>(weaponSpecificBuffData.GetWeaponSpecificBuffs.Buffs));
            }
        }
        else
        {
            if(weaponBaseData is IWeaponSpecificBuff weaponSpecificBuffData)
            {
                buffManager.SetWeaponSpecificBuffPool(new List<BuffDefinition>(weaponSpecificBuffData.GetWeaponSpecificBuffs.Buffs));
            }
        }
        OnNewBuffListReceived();
    }
#endregion

#region 按钮点击事件
    /// <summary>
    /// 选择 Buff, 并更新选中状态的显示
    /// <para> unity UI 按钮事件 </para>
    /// </summary>
    /// <param name="index"> 选择的 Buff 索引 </param>
    public void ChooseBuff(int index)
    {
        buffManager.SelectedBuffIndex = index;

        // 更新选中状态的显示
        for (int i = 0; i < buffUIUpdatas.Length; i++)
        {
            buffUIUpdatas[i].SetSelectedIndicator(i == index);
        }
    }

    /// <summary>
    /// 应用所选择的 Buff
    /// <para> unity UI 按钮事件 </para>
    /// </summary>
    public void ApplySelectedBuff()
    {
        int selectedIndex = buffManager.SelectedBuffIndex;
        if (selectedIndex >= 0 && selectedIndex < buffUIUpdatas.Length)
        {
            BuffDefinition selectedBuff = buffManager.CurrentSelection[selectedIndex];
            if (selectedBuff != null)
            {
                buffManager.AddBuff(selectedBuff);
                // 应用 Buff 效果
                selectedBuff.Apply();
            }
        }
    }

    public void CloseBuffPanel()
    {
        // 关闭 Buff 面板
        if (buffPanel != null)
        {
            buffPanel.SetActive(false);
        }

        // 通知buff管理器 Buff 选择结束
        buffManager.SetIsBuffSelectionOpen(false);

        // 重置选中状态显示
        for (int i = 0; i < buffUIUpdatas.Length; i++)
        {
            buffUIUpdatas[i].SetSelectedIndicator(false);
        }

        // 重置选择索引
        buffManager.SelectedBuffIndex = -1;

        // 请求新的 Buff 列表以备下次选择
        OnNewBuffListReceived();
    }
#endregion
}
