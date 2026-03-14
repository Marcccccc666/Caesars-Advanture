using UnityEngine;

public class InteractionObjectController : MonoBehaviour
{
    [SerializeField, ChineseLabel("提示交互")] private GameObject interactionHint;

    private InputManager inputManager => InputManager.Instance;
    protected void Awake()
    {
        interactionHint.SetActive(false);
        
        inputManager.OnInteractionPressed -= Interact;
    }

    /// <summary>
    /// 交互事件
    /// </summary>
    protected virtual void Interact()
    {
        // 交互逻辑
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inputManager.OnInteractionPressed -= Interact;
            inputManager.OnInteractionPressed += Interact;
            interactionHint.SetActive(true);
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            inputManager.OnInteractionPressed -= Interact;
            interactionHint.SetActive(false);
        }
    }

    protected void OnDisable()
    {
        if (inputManager)
            inputManager.OnInteractionPressed -= Interact;
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    protected void OnDestroy()
    {
        if (inputManager)
            inputManager.OnInteractionPressed -= Interact;
    }

}
