using UnityEngine;

[CreateAssetMenu(fileName = "GunBaseData", menuName = "Scriptable Objects/WeaponBaseData/GunBaseData")]
public class GunBaseData : WeaponBaseData
{
    [Header("射击属性")]
    [SerializeField, ChineseLabel("初试弹道数量")] private int initialBallisticsCount = 1;
    /// <summary>
    /// 初始弹道数量
    /// </summary>
    public int InitialBallisticsCount => initialBallisticsCount;

    [SerializeField, ChineseLabel("弹道类型")] private BulletType ballisticsType;
    /// <summary>
    /// 弹道类型
    /// </summary>
    public BulletType BallisticsType => ballisticsType;

    [SerializeField, ChineseLabel("弹道间隔距离")] private float bulletIntervalDistance;
    /// <summary>
    /// 弹道间隔距离
    /// </summary>
    public float BulletIntervalDistance => bulletIntervalDistance;

    [Header("子弹属性")]
    [SerializeField, ChineseLabel("子弹刚体预制体")] private BulletAttack bulletPrefab;
    /// <summary>
    /// 子弹刚体预制体
    /// </summary>
    public BulletAttack BulletPrefab => bulletPrefab;
    [SerializeField, ChineseLabel("子弹速度")] private float bulletSpeed;
    /// <summary>
    /// 子弹速度
    /// </summary>
    public float BulletSpeed => bulletSpeed;

    [SerializeField, ChineseLabel("子弹穿透力")] private int bulletPenetration;
    /// <summary>
    /// 子弹穿透力
    /// </summary>
    public int BulletPenetration => bulletPenetration;


}
