using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class GunData : WeaponData
{
    private void OnEnable()
    {
        Initialize();
    }

    public override void Initialize()
    {
        if (weaponBaseData is GunBaseData gunBaseData)
        {
            CurrentBulletCount = weaponManager.GetFinalBulletCount(gunBaseData.MaxBulletCount);

            if (gunBaseData.BulletReplenishInterval > 0)
            {
                replenishCoroutine = StartCoroutine(ReplenishBullets());
            }
        }
    }

    #region 子弹数量管理
    /// <summary>
    /// 当前子弹数量
    /// </summary>
    [SerializeField, ChineseLabel("当前子弹数量")] private int currentBulletCount=0;

    /// <summary>
    /// 获取当前子弹数量
    /// </summary>
    public int CurrentBulletCount
    {
        set
        {
            if(currentBulletCount != value)
            {
                if(weaponBaseData is GunBaseData gunBaseData)
                {
                    value = Mathf.Clamp(value, 0, weaponManager.GetFinalBulletCount(gunBaseData.MaxBulletCount));
                }
                int delta = value - currentBulletCount;
                currentBulletCount = value;
                if (delta > 0)
                {
                    OnBulletCountAdded?.Invoke(delta);
                }
                else if (delta < 0)
                {
                    OnBulletCountDecreased?.Invoke(-delta);
                }
            }
        }
        get
        {
            return currentBulletCount;
        }
    }


    /// <summary>
    /// 增加子弹时调用(包括消耗和补充)
    /// <para>传输增加数</para>
    /// </summary>
    public Action<int> OnBulletCountAdded;

    /// <summary>
    /// 减少子弹时调用
    /// <para>传输减少子弹数</para>
    /// </summary>
    public Action<int> OnBulletCountDecreased;

    /// <summary>
    /// 回复子弹协程引用
     /// 以便在武器卸下时停止协程，避免继续补充子弹
    /// </summary>
    private Coroutine replenishCoroutine;

    private bool isConsumingBullet = true;
    /// <summary>
    /// 本次是否消耗子弹
    /// 用于抢buff
    /// </summary>
    /// <returns> true 消耗子弹 
    /// <para> false 不消耗</para>
    /// </returns>
    public bool IsConsumingBullet
    {
        get => isConsumingBullet;
        set => isConsumingBullet = value;
    }

    /// <summary>
    /// 补充子弹 +1
    /// </summary>
    private IEnumerator ReplenishBullets()
    {
        GunBaseData gunBaseData = weaponBaseData as GunBaseData;

        while (true)
        {
            yield return new WaitForSeconds(gunBaseData.BulletReplenishInterval);

            if (!GameManager.Instance.IsPlayerControllable)
                continue;

            int finalBulletCount = weaponManager.GetFinalBulletCount(gunBaseData.MaxBulletCount);

            if (currentBulletCount < finalBulletCount)
            {
                CurrentBulletCount++;
            }
        }
    }
    #endregion

    #region 对象池接口实现

    public override void Release()
    {
        if(replenishCoroutine != null)
        {
            StopCoroutine(replenishCoroutine);
        }
        pool?.Release(this);
    }
    #endregion

}
