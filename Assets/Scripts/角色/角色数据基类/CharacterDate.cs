using System;
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


#region HP 变化
    /// <summary>
    /// 加血事件
    /// </summary>
    /// <param name="health">加血值</param>
    public new Action<int, int> OnHeal;

    /// <summary>
    /// 受伤事件
    /// </summary>
    /// <param name="damage">受伤值</param>
    public new Action<int, int> OnDamage;


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

    /// <summary>
    /// 加血
    /// </summary>
    /// <param name="health">加血值</param>
    public override void Heal(int health)
    {
        CurrentHealth += health;
        OnHeal?.Invoke(CurrentHealth, MaxHealth);
        base.OnHeal?.Invoke(health);
    }

    /// <summary>
    /// 受伤 
    /// </summary>
    /// <param name="damage">受伤值</param>
    public override void Damage(int damage)
    {
        CurrentHealth -= damage;
        OnDamage?.Invoke(CurrentHealth, MaxHealth);
        base.OnDamage?.Invoke(damage);
    }
#endregion

#region 武器
    private Transform weaponHoldPoint;

    public void SetweaponHoldPoint(Transform point)
    {
        weaponHoldPoint = point;
    }

    public Transform GetWeaponHoldPoint()
    {
        return weaponHoldPoint;
    }
#endregion
}
