using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private InputManager inputManager => InputManager.Instance;
    private GameManager gameManager => GameManager.Instance;

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 moveDirection = context.ReadValue<Vector2>();
        inputManager.MoveDirection = moveDirection;
    }

    public void OnTapAttack(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            inputManager.SetMouseState(MouseState.Tap);
        }
        else if(context.canceled)
        {
            inputManager.SetMouseState(MouseState.Release);
        }
    }

     public void OnHoldAttack(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            inputManager.SetMouseState(MouseState.Hold);
        }
        else if(context.canceled)
        {
            inputManager.SetMouseState(MouseState.Release);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inputManager.OnInteractionPressed?.Invoke();
        }
    }
}


