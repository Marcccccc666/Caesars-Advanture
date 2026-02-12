using UnityEngine;

public class CharacterDate : ObjectData
{
    [SerializeField, ChineseLabel("角色数据")]private CharacterBaseData characterBaseData;

    private GameManager gameManager => GameManager.Instance;

    /// <summary>
    /// 角色数据容器
    /// </summary>
    public CharacterBaseData CharacterBaseData
    {
        get { return characterBaseData; }
        set { characterBaseData = value; }
    }

    #region 初始化
    public override void InitObjectData()
    {
        if(CharacterBaseData != null)
        {
            MaxHealth = CharacterBaseData.maxHealth;
            //初始化生命值
            CurrentHealth = CharacterBaseData.maxHealth;
            //初始化移动速度
            CurrentMoveSpeed = CharacterBaseData.moveSpeed;
        }
        else
        {
            Debug.LogError("角色数据未设置！");
        }
    }
#endregion


    public override int CurrentHealth
    {
        get => currentHealth;
        set
        {
            if (value >= MaxHealth)
            {
                currentHealth = MaxHealth;
            }
            else if (value <= 0)
            {
                currentHealth = 0;
                gameManager.SetGamePaused(true);
                OnDie?.Invoke();
            }
            else
            {
                currentHealth = value;
            }
        }
    }
}
