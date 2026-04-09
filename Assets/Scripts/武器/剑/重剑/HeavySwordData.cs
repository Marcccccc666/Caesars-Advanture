using UnityEngine;
using UnityEngine.Events;

public class HeavySwordData : SwordDate
{
    public HeavySwordBaseData M_WeaponBaseData => (HeavySwordBaseData)WeaponBaseData;

    [Header("重剑特有属性")]
    [SerializeField, ChineseLabel("当前蓄力值")] private float currentCharge = 0f;

    /// <summary>
    /// 当前蓄力值
    /// </summary>
    public float CurrentCharge
    {
        get => currentCharge;
        set
        {
            currentCharge = Mathf.Clamp(value, 0f, M_WeaponBaseData.MaxCharge);
            OnChargeChanged?.Invoke(currentCharge);
        }
    }

    /// <summary>
    /// 当蓄力值变化时的事件
    /// </summary>
    [ChineseLabel("当蓄力值变化时")] public UnityEvent<float> OnChargeChanged;
}
