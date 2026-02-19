using System;
using UnityEngine;

public class EnemyData : ObjectData
{
    [SerializeField, ChineseLabel("敌人基础数据")]private EnemyBaseData enemyBaseData;
    public EnemyBaseData EnemyBaseData
    {
        get => enemyBaseData;
        set => enemyBaseData = value;
    }

    public override void InitObjectData()
    {
        if(EnemyBaseData != null)
        {
            MaxHealth = EnemyBaseData.maxHealth;
            //初始化生命值
            CurrentHealth = EnemyBaseData.maxHealth;
            //初始化攻击力
            CurrentAttack = EnemyBaseData.baseAttack;
            //初始化攻击间隔
            CurrentAttackInterval = EnemyBaseData.attackInterval;
            //初始化移动速度
            CurrentMoveSpeed = EnemyBaseData.moveSpeed;
        }
        else
        {
            Debug.LogError("角色数据未设置！");
        }
    }

    #region 攻击
    private int currentAttack;
    /// <summary>
    /// 当前攻击力
    /// </summary>
    public int CurrentAttack
    {
        get
        {
            return currentAttack;
        }
        set
        {
            currentAttack = value;
        }
    }

    private float currentAttackInterval;
    /// <summary>
    /// 当前攻击间隔
    /// </summary>
    public float CurrentAttackInterval
    {
        get
        {
            return currentAttackInterval;
        }
        set
        {
            currentAttackInterval = value;
        }
    }
#endregion

#region 血量变化
    public new Action<int> OnHeal;

    public new Action<int> OnDamage;

    /// <summary>
    /// 受伤事件
    /// </summary>
    /// <param name="damage">受伤值</param>
    public override void Damage(int damage)
    {
        base.Damage(damage);
        OnDamage?.Invoke(damage);
    }

    /// <summary>
    /// 加血事件
    /// </summary>
    /// <param name="health">加血值</param>
    public override void Heal(int health)
    {
        base.Heal(health);
        OnHeal?.Invoke(health);
    }
#endregion
}
