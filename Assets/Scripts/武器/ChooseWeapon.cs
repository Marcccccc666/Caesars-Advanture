using UnityEngine;

public class ChooseWeapon : MonoBehaviour
{
    public WeaponData weapon;
    [SerializeField, ChineseLabel("提示交互")] private GameObject interactionHint;
    private InputManager inputManager => InputManager.Instance;
    private WeaponManager weaponManager => WeaponManager.Instance;

    private bool playerInRange = false;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !playerInRange)
        {
            playerInRange = true;
            inputManager.OnInteractionPressed -= GetWeapon;
            inputManager.OnInteractionPressed += GetWeapon;
            interactionHint.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && playerInRange)
        {
            playerInRange = false;
            inputManager.OnInteractionPressed -= GetWeapon;
            interactionHint.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (inputManager)
            inputManager.OnInteractionPressed -= GetWeapon;
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (inputManager)
            inputManager.OnInteractionPressed -= GetWeapon;
    }

    private void GetWeapon()
    {
        weaponManager.SwitchWeapon(weapon);
        gameObject.SetActive(false);
    }
}
