using UnityEngine;
using UnityEngine.InputSystem;

public class InputData : Singleton<InputData>
{   
    private Vector2 moveDirection;
    /// <summary>
    /// 移动方向
    /// </summary>
    public Vector2 MoveDirection
    {
        get { return moveDirection; }
        set { moveDirection = value; }
    }

    private bool isAttack;

    /// <summary>
    /// 是否攻击
    /// </summary>
    public bool IsAttack
    {
        get { return isAttack; }
        set { isAttack = value; }
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
}
