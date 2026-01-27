using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    // 游戏是否结束
    private bool isGameOver = false;

    public bool IsGameOver
    {
        get { return isGameOver; }
        set { isGameOver = value; }
    }

    /// <summary>
    /// 游戏结算动作
    /// </summary>
    public Action GameCheckout;
}
