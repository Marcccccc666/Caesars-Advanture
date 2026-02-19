using UnityEngine;

public class ChooseWeapon : MonoBehaviour
{
    public WeaponData weapon;
    [SerializeField, ChineseLabel("提示交互")] private GameObject interactionHint;
    private InputManager inputManager => InputManager.Instance;
    private CharacterManager characterManager => CharacterManager.Instance;

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
        if (inputManager != null)
            inputManager.OnInteractionPressed -= GetWeapon;
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (inputManager != null)
            inputManager.OnInteractionPressed -= GetWeapon;
    }

    private void GetWeapon()
    {
        Transform weaponHoldPoint = characterManager.GetCurrentPlayerCharacterData.GetWeaponHoldPoint();
        GameObject weaponObj = Instantiate(weapon.gameObject, weaponHoldPoint.position, Quaternion.identity, weaponHoldPoint);
        gameObject.SetActive(false);
    }
}
