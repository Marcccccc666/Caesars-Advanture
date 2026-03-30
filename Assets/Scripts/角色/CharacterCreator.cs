using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色创建器，负责在游戏开始时创建角色并初始化数据
/// </summary>
public class CharacterCreator : MonoBehaviour
{
    [SerializeField, ChineseLabel("角色预制体")] private CaesarData characterPrefab;

    [SerializeField, ChineseLabel("出生点")] private Transform spawnPoint;

    #if UNITY_EDITOR
    [SerializeField, ChineseLabel("测试武器数据")] private WeaponData testWeaponData;

    [SerializeField, ChineseLabel("测试Buff数据")] private List<BuffDefinition> testBuffData;
    #endif

    private PoolManager poolManager => PoolManager.Instance;
    private CharacterManager characterManager => CharacterManager.Instance;
    private WeaponManager weaponManager => WeaponManager.Instance;
    private GameManager gameManager => GameManager.Instance;

    private void Awake()
    {
        BuildCharacter();
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    private void OnEnable()
    {
        gameManager.GameSceneChangedAction += RecycleCharacter;
    }

    #if UNITY_EDITOR
    private void Start()
    {
            // 在编辑器模式下，如果当前没有玩家角色数据但有测试武器数据，则切换到测试武器，方便开发和调试
            var currentCharacterData = characterManager.GetCurrentPlayerCharacterData;
            var currentWeaponData = weaponManager.GetCurrentWeapon;
            if(currentCharacterData && testWeaponData != null && currentWeaponData == null)
            {
                weaponManager.SwitchWeapon(testWeaponData);
            }

            if(testBuffData != null && testBuffData.Count > 0)
            {
                foreach(var buff in testBuffData)
                {
                    buff.Apply();
                }
            }
    }
    #endif

    /// <summary>
    /// This function is called when the behaviour becomes disabled or inactive.
    /// </summary>
    private void OnDisable()
    {
        if(gameManager)
        {
            gameManager.GameSceneChangedAction -= RecycleCharacter;
        }
    }

    private void OnDestroy()
    {
        if(gameManager)
        {
            gameManager.GameSceneChangedAction -= RecycleCharacter;
        }
    }

    /// <summary>
    /// 构建角色，根据当前玩家控制的角色数据创建角色实例，如果没有则从对象池中生成一个新的角色实例，并设置为当前玩家控制的角色数据
    /// </summary>
    private void BuildCharacter()
    {
        var currentCharacterData = characterManager.GetCurrentPlayerCharacterData;
        bool IsCharacterOpen = true;
        if(currentCharacterData)
        {
            IsCharacterOpen = currentCharacterData.gameObject.activeInHierarchy;
            currentCharacterData.transform.SetPositionAndRotation(spawnPoint.position, Quaternion.identity);

            if (currentCharacterData.TryGetComponent<Rigidbody2D>(out var playerRigidbody))
            {
                playerRigidbody.linearVelocity = Vector2.zero;
                playerRigidbody.angularVelocity = 0f;
            }
        }

        if (!currentCharacterData || !IsCharacterOpen)
        {
            currentCharacterData = poolManager.Spawn(
                prefab: characterPrefab,
                pos: spawnPoint.position,
                rot: Quaternion.identity,
                parent: poolManager.transform,
                defaultCapacity: 1,
                maxSize: 1,
                setActive: true);
            characterManager.SetCurrentPlayerCharacterData(currentCharacterData);
        }
    }

    ///<summary>
    /// 回收角色实例，切换场景时调用
    /// </summary>
    public void RecycleCharacter()
    {
        var currentCharacterData = characterManager.GetCurrentPlayerCharacterData;
        if(currentCharacterData != null)
        {
        //    poolManager.Release(characterPrefab, currentCharacterData);
        }
    }


}
