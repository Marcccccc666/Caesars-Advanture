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
        InitialWeaponBuffPool = buffPool ?? new List<BuffDefinition>();
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
        weaponSpecificBuffPool = buffPool ?? new List<BuffDefinition>();
    }

    public void ClearWeaponSpecificBuffPool()
    {
        weaponSpecificBuffPool = new List<BuffDefinition>();
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
        GameManager.Instance.SetGamePaused(true);
        OpenBuffSelectionUI?.Invoke();
    }

    /// <summary>
    /// 请求生成 3 个随机 Buff 供玩家选择
    /// </summary>
    public void RequestCreateRandomBuff()
    {
        List<BuffDefinition> combinedBuffs = new();
        AddValidBuffs(combinedBuffs, normalBuffPool?.Buffs);
        AddValidBuffs(combinedBuffs, InitialWeaponBuffPool);
        AddValidBuffs(combinedBuffs, weaponSpecificBuffPool);

        if (combinedBuffs.Count == 0)
        {
            for (int i = 0; i < currentSelection.Length; i++)
            {
                currentSelection[i] = null;
            }
            return;
        }

        Shuffle(combinedBuffs);

        for (int i = 0; i < currentSelection.Length; i++)
        {
            currentSelection[i] = combinedBuffs[i % combinedBuffs.Count];
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

    [SerializeField, Range(0f, 0.95f)] private float heavyChargeDamageReduction;

    public void AddHeavyChargeDamageReduction(float delta)
    {
        heavyChargeDamageReduction = Mathf.Clamp(heavyChargeDamageReduction + delta, 0f, 0.95f);
    }

    public int GetModifiedPlayerDamage(int damage)
    {
        if (damage <= 0)
        {
            return 0;
        }

        if (WeaponManager.Instance != null
            && WeaponManager.Instance.GetCurrentWeapon is HeavySwordData heavySwordData
            && heavySwordData.CurrentSwordState == SwordState.Charging
            && heavyChargeDamageReduction > 0f)
        {
            damage = Mathf.Max(1, Mathf.RoundToInt(damage * (1f - heavyChargeDamageReduction)));
        }

        return damage;
    }

    #endregion

    protected override void OnRest()
    {
        if(currentBuffs.Count == 0 || currentBuffs == null)
        {
            return;
        }
        
        for (int i = currentBuffs.Count - 1; i >= 0; i--)
        {
            currentBuffs[i].Remove();
        }
        currentBuffs.Clear();
    }

    private static void Shuffle(List<BuffDefinition> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private static void AddValidBuffs(List<BuffDefinition> target, IEnumerable<BuffDefinition> source)
    {
        if (source == null)
        {
            return;
        }

        foreach (BuffDefinition buff in source)
        {
            if (buff != null)
            {
                target.Add(buff);
            }
        }
    }
}
