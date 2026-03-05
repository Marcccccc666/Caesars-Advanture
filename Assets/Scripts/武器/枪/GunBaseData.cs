using UnityEngine;

[CreateAssetMenu(fileName = "GunBaseData", menuName = "Scriptable Objects/WeaponBaseData/GunBaseData")]
public class GunBaseData : WeaponBaseData
{
    [Header("射击属性")]
    [SerializeField, ChineseLabel("子弹最大数量")] private int maxBulletCount;

    /// <summary>
    /// 子弹最大数量
    /// </summary>
    public int MaxBulletCount => maxBulletCount;

    [SerializeField, ChineseLabel("弹药补充间隔")] private float bulletReplenishInterval;
    /// <summary>
    /// 弹药补充间隔
    /// </summary>
    public float BulletReplenishInterval => bulletReplenishInterval;

    [SerializeField, ChineseLabel("子弹反弹次数")] private int bulletBounce;
    /// <summary>
    /// 子弹反弹次数
    /// </summary>
    public int BulletBounce => bulletBounce;

}
