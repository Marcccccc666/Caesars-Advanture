using UnityEngine;

public enum WeaponType
{
    /// <summary>
    /// 霰弹枪
    /// </summary>
    ShotGun,

    /// <summary>
    /// 狙击枪
    /// </summary>
    Sniper,

    /// <summary>
    /// 轻剑
    /// </summary>
    LightSword,

    /// <summary>
    /// 重剑
    /// </summary>
    HeavySword,
}

[System.Serializable]
public class WeaponBranch
{
    [ChineseLabel("武器类型")]public WeaponType Type;
    [ChineseLabel("武器数据")]public WeaponData Data;
    [ChineseLabel("武器升级UI图片")]public Sprite WeaponSprite;
}


/// <summary>
/// 初始武器接口，定义了武器升级分支的属性
/// </summary>
public interface IInitialWeapon
{
    /// <summary>
    /// 武器升级分支
    /// </summary>
    WeaponBranch[] WeaponBrachs { get; }
}
