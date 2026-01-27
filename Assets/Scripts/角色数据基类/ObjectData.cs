using UnityEngine;

public class ObjectData : MonoBehaviour
{
    /// <summary>
    /// 初始化数据
    /// </summary>
    public virtual void InitObjectData()
    {
        
    }

#region 生命值
    protected int maxHealth;
    /// <summary>
    /// 最大生命值
    /// </summary>
    public int MaxHealth
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
            }
            else
            {
                currentHealth = value;
            }
        }
    }

#endregion

#region 移动速度
    private float currentMoveSpeed;
    /// <summary>
    /// 当前移动速度
    /// </summary>
    public float CurrentMoveSpeed
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
