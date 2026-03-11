using UnityEngine;

/// <summary>
/// 武器特定 Buff 接口
/// </summary>
public interface IWeaponSpecificBuff
{
    /// <summary>
    /// 获取武器特定 Buff 列表
    /// </summary>
    /// <returns>武器特定 Buff 列表</returns>
    BuffPool GetWeaponSpecificBuffs { get; }
}
