using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MouseState
{
    None,
    Tap,
    Hold,
    Release
}


public class InputManager : Singleton<InputManager>
{   
#region WASD
    private Vector2 moveDirection;
    /// <summary>
    /// 移动方向
    /// </summary>
    public Vector2 MoveDirection
    {
        get { return moveDirection; }
        set { moveDirection = value; }
    }
#endregion


#region Mouse

    private MouseState currentMouseState = MouseState.None;

    /// <summary>
    /// 当前鼠标状态
    /// </summary>
    public MouseState CurrentMouseState=> currentMouseState;

    public void SetMouseState(MouseState newState)
    {
        currentMouseState = newState;

        switch (currentMouseState)
        {
            case MouseState.Tap:
                // 处理点击状态的逻辑
                OnMouseLeftTap?.Invoke();
                break;
            case MouseState.Hold:
                // 处理持续按住状态的逻辑
                OnMouseLeftHold?.Invoke();
                break;
            case MouseState.Release:
                // 处理释放状态的逻辑
                OnMouseLeftRelease?.Invoke();
                break;
        }
    }

    /// <summary>
    /// 鼠标世界位置
    /// </summary>
    public Vector2 MouseWorldPosition
    {
        get
        {
            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            return mouseWorldPosition;
        }
    }

    /// <summary>
    /// 鼠标左键点击事件
    /// </summary>
    public Action OnMouseLeftTap;

    /// <summary>
    /// 鼠标左键长按事件
    /// </summary>
    public Action OnMouseLeftHold;

    ///<summary>
    /// 左键取消事件
    /// </summary>
    public Action OnMouseLeftRelease;
#endregion

#region Interaction
    /// <summary>
    /// 交互按键事件
    /// </summary>
    public Action OnInteractionPressed;

#endregion
}
