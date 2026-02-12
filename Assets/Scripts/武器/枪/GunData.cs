using UnityEngine;

public class GunData : WeaponData
{
#region 弹道
    [SerializeField, ChineseLabel("武器弹道数加成")] private int ballisticsBonus = 0;
    /// <summary>
    /// 武器弹道数加成
    /// </summary>
    public int BallisticsBonus => ballisticsBonus;

    /// <summary>
    /// 增加武器弹道数加成
     /// <para> 最终弹道数 = 基础弹道数 + 弹道数加成 </para>
    /// </summary>
    /// <param name="bonus"></param>
    public void AddBallisticsBonus(int bonus)
    {
        ballisticsBonus += bonus;
    }

    
    ///<summary>
    /// 获取武器弹道数
    /// </summary>
    public int GetFinalBallisticsCount
    {
        get
        {
            if (weaponBaseData is GunBaseData gunData)
            {
                return gunData.InitialBallisticsCount + ballisticsBonus;
            }
            return 1; // 默认弹道数量为1
        }
    }
#endregion

#region 穿透力
    [SerializeField, ChineseLabel("武器穿透力加成")] private int penetrationBonus = 0;
    /// <summary>
    /// 武器穿透力加成
    /// </summary>
    public int PenetrationBonus => penetrationBonus;

    /// <summary>
    /// 增加武器穿透力加成
     /// <para> 最终穿透力 = 基础穿透力 + 穿透力加成 </para>
    /// </summary>
    /// <param name="bonus"></param>
    public void AddPenetrationBonus(int bonus)
    {
        penetrationBonus += bonus;
    }

    public int GetFinalPenetration
    {
        get
        {
            if (weaponBaseData is GunBaseData gunData)
            {
                return gunData.BulletPenetration + penetrationBonus;
            }
            return 0; // 默认穿透力为0
        }
    }
#endregion

}
