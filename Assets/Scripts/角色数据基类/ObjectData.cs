using System;
using UnityEngine;

public class ObjectData : MonoBehaviour
{
    /// <summary>
    /// 初始化数据
    /// </summary>
    public virtual void InitObjectData()
    {
        
    }

    /// <summary>
    /// 加血事件
    /// </summary>
    /// <param name="health">加血值</param>
    public Action<int> OnHeal;

    /// <summary>
    /// 受伤事件
    /// </summary>
    /// <param name="damage">受伤值</param>
    public Action<int> OnDamage;

    /// <summary>
    /// 死亡事件
    /// </summary>
    public Action OnDie;

#region 生命值
    protected int maxHealth;
    /// <summary>
    /// 最大生命值
    /// </summary>
    public virtual int MaxHealth
    {
        get
        {
            return maxHealth;
        }
        set
        {
            if(value > 0)
            {
                maxHealth = value;
            }
            else
            {
                Debug.LogError("最大生命值必须大于0！");
            }
        }
    }
    protected int currentHealth;

    /// <summary>
    /// 当前生命值
    /// </summary>
    public virtual int CurrentHealth
    {
        get
        {
            return currentHealth;
        }
        set
        {
            if(value >= maxHealth)
            {
                currentHealth = maxHealth;
            }
            else if(value < 0)
            {
                currentHealth = 0;
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
    public virtual void Heal(int health)
    {
        CurrentHealth += health;
        OnHeal?.Invoke(health);
    }

    /// <summary>
    /// 受伤
    /// </summary>
    /// <param name="damage">受伤值</param>
    public virtual void Damage(int damage)
    {
        CurrentHealth -= damage;
        OnDamage?.Invoke(damage);
    }

#endregion

#region 移动速度
    private float currentMoveSpeed;
    /// <summary>
    /// 当前移动速度
    /// </summary>
    public virtual float CurrentMoveSpeed
    {
        get
        {
            return currentMoveSpeed;
        }
        set
        {
            currentMoveSpeed = value;
        }
    }
#endregion
}
