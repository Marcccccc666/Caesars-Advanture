using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Base Data", menuName = "Enemy/Enemy Base Data")]
public class EnemyBaseData : ObjectBaseData
{
    [Header("元素抗性")]
    [SerializeField, ChineseLabel("火元素抗性")] private float fireResistance;

    /// <summary>
    /// 火元素抗性
    /// </summary>
    public float FireResistance => fireResistance;

    [SerializeField, ChineseLabel("冰元素抗性")] private float iceResistance;

    /// <summary>
    /// 冰元素抗性
    /// </summary>
    public float IceResistance => iceResistance;

    [SerializeField, ChineseLabel("雷元素抗性")] private float thunderResistance;

    /// <summary>
    /// 雷元素抗性
    /// </summary>
    public float ThunderResistance => thunderResistance;

}
