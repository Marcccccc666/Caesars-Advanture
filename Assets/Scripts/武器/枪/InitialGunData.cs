using UnityEngine;

public enum GunType
{
    /// <summary>
    /// 霰弹枪
    /// </summary>
    ShotGun,
    /// <summary>
    /// 狙击枪
    /// </summary>
    Sniper
}

[System.Serializable]
public class GunBrach
{
    public GunType Type;
    public GunData Data;
}

[CreateAssetMenu(fileName = "InitialGunData", menuName = "Scriptable Objects/Gun/InitialGunData")]
public class InitialGunData : GunBaseData
{
    [Space(10)]

    [SerializeField, ChineseLabel("枪升级分支")] private GunBrach[] gunBrachs;
    /// <summary>
    /// 枪升级分支
    /// </summary>
    public GunBrach[] GunBrachs => gunBrachs;
}
