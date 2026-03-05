using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : Singleton<BuffManager>
{
    #region Buff池
    /// <summary>
    /// 通用Buff池
    /// </summary>
    [SerializeField] private BuffPool normalBuffPool;

    
    [SerializeField] private List<BuffDefinition> InitialWeaponBuffPool;
    /// <summary>
    /// 基础武器Buff池
    /// </summary>
    public List<BuffDefinition> GetInitialWeaponBuffPool => InitialWeaponBuffPool;

    /// <summary>
    /// 设置基础武器Buff池数据
    /// </summary>
    /// <param name="buffPool"> 基础武器Buff池 </param>
    public void SetInitialWeaponBuffPool(List<BuffDefinition> buffPool)
    {
        if(InitialWeaponBuffPool == null)
        {
            InitialWeaponBuffPool = buffPool;
        }
        else
        {
            InitialWeaponBuffPool?.AddRange(buffPool);
        }
    }

    [SerializeField] private List<BuffDefinition> weaponSpecificBuffPool;
    /// <summary>
    /// 武器特定Buff池
    /// </summary>
    public List<BuffDefinition> GetWeaponSpecificBuffPool => weaponSpecificBuffPool;

    /// <summary>
    /// 设置武器特定Buff池数据
    /// </summary>
    /// <param name="buffPool"> 武器特定Buff池 </param>
    public void SetWeaponSpecificBuffPool(List<BuffDefinition> buffPool)
    {
        if(weaponSpecificBuffPool == null)
        {
            weaponSpecificBuffPool = buffPool;
        }
        else
        {
            weaponSpecificBuffPool?.AddRange(buffPool);
        }
    }

    [SerializeField, ChineseLabel("当前选择的Buff")] private List<BuffDefinition> currentBuffs = new List<BuffDefinition>();
    /// <summary>
    /// 当前选择的Buff </summary>
    public IReadOnlyList<BuffDefinition> CurrentBuffs => currentBuffs;

    public void AddBuff(BuffDefinition buff)
    {
        currentBuffs.Add(buff);
    }


    #endregion

    #region 选择buff相关
    [SerializeField, ChineseLabel("玩家选择第几个 Buff")] private int selectedBuffIndex = -1;
    /// <summary>
    /// 玩家选择第几个 Buff
    /// </summary>
    public int SelectedBuffIndex
    {
        get => selectedBuffIndex;
        set => selectedBuffIndex = value;
    }

    /// <summary>
    /// 当前随机 3 个 Buff
    /// </summary>
    private readonly BuffDefinition[] currentSelection = new BuffDefinition[3];

    public Action OpenBuffSelectionUI;

    public IReadOnlyList<BuffDefinition> CurrentSelection => currentSelection;

    private bool isBuffSelectionOpen = false;
    /// <summary> 
    /// 是否正在选择 Buff 
    /// </summary>
    public bool IsBuffSelectionOpen => isBuffSelectionOpen;

    /// <summary>
    /// 设置是否正在选择 Buff
    /// </summary>
    public void SetIsBuffSelectionOpen(bool isOpen)
    {
        isBuffSelectionOpen = isOpen;
    }

    /// <summary>
    /// 触发 Buff 选择请求事件
    /// </summary>
    public void RequestBuffSelection()
    {
        SetIsBuffSelectionOpen(true);
        OpenBuffSelectionUI?.Invoke();
    }

    /// <summary>
    /// 请求生成 3 个随机 Buff 供玩家选择
    /// </summary>
    public void RequestCreateRandomBuff()
    {
        int totalCount =
                (normalBuffPool?.Buffs?.Count ?? 0) +
                (InitialWeaponBuffPool?.Count ?? 0) +
                (weaponSpecificBuffPool?.Count ?? 0);
        List<BuffDefinition> combinedBuffs = new(totalCount);
        combinedBuffs?.AddRange(normalBuffPool.Buffs);
        combinedBuffs?.AddRange(InitialWeaponBuffPool);
        combinedBuffs?.AddRange(weaponSpecificBuffPool);

        Shuffle(combinedBuffs);

        for (int i = 0; i < currentSelection.Length; i++)
        {
            currentSelection[i] = combinedBuffs[i];
        }
    }
    #endregion

    #region Buff触发相关

    /// <summary>
    /// 攻击前触发
    /// </summary>
    public Action<WeaponData> BeforeAttackTriggered;

    /// <summary>
    /// 攻击时触发 Buff 效果
    /// </summary>
    public Action<Transform> AttackTriggered;

    /// <summary>
    /// 攻击后触发 Buff 效果
    /// </summary>
    public Action<WeaponData> AfterAttackTriggered;

    /// <summary>
    /// 攻击命中时触发 Buff 效果
    /// </summary>
    public Action<Transform> AttackHitTriggered;

    /// <summary>
    /// 玩家受伤时触发 Buff 效果
    /// </summary>
    public Action<Transform> PlayerDamagedTriggered;

    /// <summary>
    /// 敌人死亡时触发 Buff 效果
    /// </summary>
    public Action<Transform> EnemyKilledTriggered;

    #endregion

    private static void Shuffle(List<BuffDefinition> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
