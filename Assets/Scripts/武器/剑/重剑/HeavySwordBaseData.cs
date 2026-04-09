using UnityEngine;

[CreateAssetMenu(fileName = "HeavySwordBaseData", menuName = "Scriptable Objects/Sword/HeavySwordBaseData")]
public class HeavySwordBaseData : SwordBaseData
{
    [Header("蓄力相关")]
    [SerializeField, ChineseLabel("蓄力最大量")]private float maxCharge = 100f;

    /// <summary>
    /// 获取蓄力最大量
    /// </summary>
    public float MaxCharge => maxCharge;

    [SerializeField, ChineseLabel("蓄力增加速度")]private float chargeIncreaseRate = 20f;

    /// <summary>
    /// 获取蓄力增加速度
    /// </summary>
    public float ChargeIncreaseRate => chargeIncreaseRate;
}
