using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MouseState
{
    None,
    Press,
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
#endregion

#region Interaction
    /// <summary>
    /// 交互按键事件
    /// </summary>
    public Action OnInteractionPressed;

#endregion
}
