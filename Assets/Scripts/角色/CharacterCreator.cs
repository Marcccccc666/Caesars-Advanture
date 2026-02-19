using UnityEngine;

/// <summary>
/// 角色创建器，负责在游戏开始时创建角色并初始化数据
/// 
/// </summary>
public class CharacterCreator : MonoBehaviour
{
    [SerializeField, ChineseLabel("角色预制体")] private Caesar_Controller characterPrefab;

    [SerializeField, ChineseLabel("出生点")] private Transform spawnPoint;

    private PoolManager poolManager => PoolManager.Instance;
    private CharacterManager characterManager => CharacterManager.Instance;
    private WeaponManager weaponManager => WeaponManager.Instance;

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        var currentCharacterData = characterManager.GetCurrentPlayerCharacterData;
        if(!currentCharacterData)
            CreateCharacter();
        
        if (currentCharacterData)
            currentCharacterData.transform.position = spawnPoint.position;
    }

    /// <summary>
    /// 创建角色并初始化数据
    /// </summary>
    private void CreateCharacter()
    {
        var currentCharacter = poolManager.Spawn(
            prefab: characterPrefab,
            position: spawnPoint.position,
            rotation: Quaternion.identity,
            defaultCapacity: 1,
            maxSize: 1,
            autoActive: true);
    }
}
