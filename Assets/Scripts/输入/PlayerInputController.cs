using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField, ChineseLabel("攻击键按多久才算是Hold")] private float holdThreshold = 0.5f;
    private InputManager inputManager => InputManager.Instance;
    private GameManager gameManager => GameManager.Instance;

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!gameManager.IsPlayerControllable)
        {
            return;
        }

        Vector2 moveDirection = context.ReadValue<Vector2>();
        inputManager.MoveDirection = moveDirection;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            inputManager.SetMouseState(MouseState.Press);
            StartCoroutine(HoldAttackCoroutine());
        }
        else if(context.canceled)
        {
            inputManager.SetMouseState(MouseState.Release);
            StopCoroutine(HoldAttackCoroutine());
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inputManager.OnInteractionPressed?.Invoke();
        }
    }

    IEnumerator HoldAttackCoroutine()
    {
        yield return new WaitForSeconds(holdThreshold);
        if(inputManager.CurrentMouseState == MouseState.Press)
        {
            inputManager.SetMouseState(MouseState.Hold);
        }
    }
}


