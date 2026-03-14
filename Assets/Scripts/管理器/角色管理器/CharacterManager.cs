using System;
using UnityEngine;

/// <summary>
/// 角色管理器
/// </summary>
public class CharacterManager: Singleton<CharacterManager>
{
#region 玩家
    /// <summary>
    /// 当前玩家控制的角色数据
    /// </summary>
    [SerializeField] private CharacterDate CurrentPlayerCharacterData;

    /// <summary>
    /// 获取当前玩家控制的角色数据
    /// </summary>
    public CharacterDate GetCurrentPlayerCharacterData
    {
        get=> CurrentPlayerCharacterData;
    }

    /// <summary>
    /// 当前玩家控制的角色数据改变事件
    /// </summary>
    public Action<CharacterDate> OnCurrentPlayerCharacterDataChanged;

    /// <summary>
    /// 设置当前玩家控制的角色数据
    /// </summary>
    /// <param name="characterData">角色数据</param>
    public void SetCurrentPlayerCharacterData(CharacterDate characterData)
    {
        if(CurrentPlayerCharacterData == null)
        {
            characterData.InitObjectData();
            CurrentPlayerCharacterData = characterData;
        }
        
        OnCurrentPlayerCharacterDataChanged?.Invoke(characterData);
    }
    #endregion
}
