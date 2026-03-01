using System.Collections.Generic;
using UnityEngine;


public class MultiTimerManager : Singleton<MultiTimerManager>
{
    #region 计时器管理
    /// <summary>
    /// 存储所有前进计时器的字典
    /// </summary>
    private Dictionary<string, UpTimer> M_UpTimers = new Dictionary<string, UpTimer>();

    /// <summary>
    /// 存储所有后退计时器的字典
    /// </summary>
    private Dictionary<string, DownTimer> M_DownTimers = new Dictionary<string, DownTimer>();

#region 注册计时器
    /// <summary>
    /// 创建一个前进计时器
    /// <para>如果计时器名称已存在，则重置该计时器</para>
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    /// <returns>新创建的前进计时器</returns>
    public UpTimer Create_UpTimer(string timerName)
    {
        if (M_UpTimers.ContainsKey(timerName))
        {
            M_UpTimers[timerName].ResetTimer();
            return M_UpTimers[timerName];
        }
        UpTimer newTimer = new UpTimer();
        M_UpTimers.Add(timerName, newTimer);
        return newTimer;
    }

    /// <summary>
    /// 创建一个前进计时器
    /// <para>如果计时器名称已存在，则重置该计时器</para>
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    public void Create_UpTime(string timerName)
    {
        if (M_UpTimers.ContainsKey(timerName))
        {
            M_UpTimers[timerName].ResetTimer();
            return;
        }
        UpTimer newTimer = new UpTimer();
        M_UpTimers.Add(timerName, newTimer);
    }

    /// <summary>
    /// 创建一个后退计时器
    /// <para>如果计时器名称已存在，则重置该计时器</para>
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    /// <param name="duration">倒计时开始时间(秒)</param>
    /// <returns>新创建的后退计时器</returns>
    public DownTimer Create_DownTimer(string timerName, float duration = 0f)
    {
        if (M_DownTimers.ContainsKey(timerName))
        {
            M_DownTimers[timerName].ResetTimer(duration);
            return M_DownTimers[timerName];
        }
        DownTimer newTimer = new();
        M_DownTimers.Add(timerName, newTimer);
        return newTimer;
    }

    /// <summary>
    /// 创建一个后退计时器
    /// <para>如果计时器名称已存在，则重置该计时器</para>
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    /// <param name="duration">倒计时开始时间(秒)</param>
    public void Create_DownTime(string timerName, float duration = 0f)
    {
        if (M_DownTimers.ContainsKey(timerName))
        {
            M_DownTimers[timerName].ResetTimer(duration);
            return;
        }
        DownTimer newTimer = new();
        M_DownTimers.Add(timerName, newTimer);
    }

    #endregion 注册计时器

    #region 删除计时器

    /// <summary>
    /// 删除指定名称的前进计时器
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    public void Delete_UpTimer(string timerName)
    {
        if (M_UpTimers.ContainsKey(timerName))
        {
            M_UpTimers.Remove(timerName);
        }
        else
        {
            Debug.LogError($"计时器名称 {timerName} 不存在");
        }
    }

    /// <summary>
    /// 删除指定名称的后退计时器
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    public void Delete_DownTimer(string timerName)
    {
        if (M_DownTimers.ContainsKey(timerName))
        {
            M_DownTimers.Remove(timerName);
        }
        else
        {
            Debug.LogError($"计时器名称 {timerName} 不存在");
        }
    }
    #endregion 删除计时器

    #region 启动计时器
    /// <summary>
    /// 启动指定名称的前进计时器
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    public void Start_UpTimer(string timerName)
    {
        if (M_UpTimers.ContainsKey(timerName))
        {
            M_UpTimers[timerName].StartTimer();
        }
        else
        {
            Debug.LogError($"计时器名称 {timerName} 不存在");
        }
    }

    /// <summary>
    /// 启动指定名称的后退计时器
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    /// <param name="duration">倒计时开始时间(秒)</param>
    public void Start_DownTimer(string timerName, float duration = 0f)
    {
        if (M_DownTimers.ContainsKey(timerName))
        {
            if (duration > 0f)
            {
                M_DownTimers[timerName].SetDuration(duration);
            }
            M_DownTimers[timerName].StartTimer();
        }
        else
        {
            Debug.LogError($"计时器名称 {timerName} 不存在");
        }
    }

    #endregion 启动计时器

    #region 暂停计时器
    /// <summary>
    /// 暂停指定名称的前进计时器
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    public void Pause_UpTimer(string timerName)
    {
        if (M_UpTimers.ContainsKey(timerName))
        {
            M_UpTimers[timerName].PauseTimer();
        }
        else
        {
            Debug.LogError($"计时器名称 {timerName} 不存在");
        }
    }

    /// <summary>
    /// 暂停前进计时器
    /// </summary>
    public void Pause_UpTimer(UpTimer timer)
    {
        timer.PauseTimer();
    }

    /// <summary>
    /// 暂停指定名称的后退计时器
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    public void Pause_DownTimer(string timerName)
    {
        if (M_DownTimers.ContainsKey(timerName))
        {
            M_DownTimers[timerName].PauseTimer();
        }
        else
        {
            Debug.LogError($"计时器名称 {timerName} 不存在");
        }
    }

    /// <summary>
    /// 暂停倒计时器
    /// </summary>
    /// <param name="timer">倒计时器</param>
    public void Pause_DownTimer(DownTimer timer)
    {
        timer.PauseTimer();
    }

    #endregion 暂停计时器

    #region 重置计时器
    /// <summary>
    /// 重置指定名称的前进计时器
    /// <para>如果未指定倒计时时间，则重置为0</para>
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    public void Reset_UpTimer(string timerName)
    {
        if (M_UpTimers.ContainsKey(timerName))
        {
            M_UpTimers[timerName].ResetTimer();
        }
        else
        {
            Debug.LogError($"计时器名称 {timerName} 不存在");
        }
    }

    public void Reset_DownTimer(string timerName, float duration = 0f)
    {
        if (M_DownTimers.ContainsKey(timerName))
        {
            M_DownTimers[timerName].ResetTimer(duration);
        }
        else
        {
            Debug.LogError($"计时器名称 {timerName} 不存在");
        }
    }

    #endregion 重置计时器

    #region 获取计时器时间
    /// <summary>
    /// 获取指定名称的前进计时器的已运行时间
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    /// <returns>已运行时间(秒)</returns>
    public float GetElapsed_UpTime(string timerName)
    {
        if (M_UpTimers.ContainsKey(timerName))
        {
            return M_UpTimers[timerName].Elapsed;
        }
        else
        {
            Debug.LogError($"计时器名称 {timerName} 不存在");
            return 0f;
        }
    }

    /// <summary>
    /// 获取指定名称的后退计时器的剩余时间
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    /// <returns>剩余时间(秒)</returns>
    public float GetRemainingTime_DownTime(string timerName)
    {
        if (M_DownTimers.ContainsKey(timerName))
        {
            return M_DownTimers[timerName].GetRemainingTime();
        }
        else
        {
            Debug.LogError($"计时器名称 {timerName} 不存在");
            return 0f;
        }
    }
    #endregion 获取计时器时间

    #region 倒计时器是否完成
    /// <summary>
    /// 判断指定名称的倒计时器是否完成
    /// </summary>
    /// <param name="timerName">计时器名称</param>
    /// <returns>是否完成</returns>
    public bool IsDownTimerComplete(string timerName)
    {
        if (M_DownTimers.ContainsKey(timerName))
        {
            return M_DownTimers[timerName].IsComplete();
        }
        else
        {
            Debug.LogError($"计时器名称 {timerName} 不存在");
            return false;
        }
    }

    public bool IsDownTimerComplete(DownTimer timer)
    {
        return timer.IsComplete();
    }
    #endregion 倒计时器是否完成


    #endregion 计时器管理

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        foreach (var timer in M_UpTimers)
        {
            UpTimer t = timer.Value;
            t.UpdateTimer(deltaTime);
        }
        foreach (var timer in M_DownTimers)
        {
            DownTimer t = timer.Value;
            t.UpdateTimer(deltaTime);

        }
    }
}
