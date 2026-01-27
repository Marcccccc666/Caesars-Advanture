using UnityEngine;

[CreateAssetMenu(fileName = "GunBaseData", menuName = "Scriptable Objects/WeaponBaseData/GunBaseData")]
public class GunBaseData : WeaponBaseData
{
    [Header("子弹属性")]
    [SerializeField, ChineseLabel("子弹刚体预制体")] private Rigidbody2D bulletPrefab;
    /// <summary>
    /// 子弹刚体预制体
    /// </summary>
    public Rigidbody2D BulletPrefab => bulletPrefab;
    [SerializeField, ChineseLabel("子弹速度")] private float bulletSpeed;
    /// <summary>
    /// 子弹速度
    /// </summary>
    public float BulletSpeed => bulletSpeed;


}
