using UnityEngine;

public enum SwordState
{
    Idle,
    Attack,
}

public class SwordDate : WeaponData
{
    [SerializeField, ChineseLabel("当前剑状态")] private SwordState currentSwordState = SwordState.Idle;
    /// <summary>
    /// 当前剑状态
    /// </summary>
    public SwordState CurrentSwordState
    {
        get => currentSwordState;
        set => currentSwordState = value;
    }
}
