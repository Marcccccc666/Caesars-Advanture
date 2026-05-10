using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "疾步", menuName = "Buffs/剑/疾步")]
public class SwiftStepBuff : BuffDefinition
{
    [SerializeField, Min(0.1f)] private float moveSpeedBonus = 1.5f;
    [SerializeField, Min(0.1f)] private float duration = 1.2f;

    private Coroutine activeCoroutine;
    private bool isApplied;

    public override void Apply()
    {
        BuffManager.Instance.AfterAttackTriggered += OnAfterAttack;
    }

    public override void Remove()
    {
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.AfterAttackTriggered -= OnAfterAttack;
            if (activeCoroutine != null)
            {
                BuffManager.Instance.StopCoroutine(activeCoroutine);
            }
        }

        ClearMoveSpeedBonus();
    }

    private void OnAfterAttack(WeaponData weaponData)
    {
        if (!SwordBuffUtility.IsCurrentQuickSword())
        {
            return;
        }

        CharacterDate playerData = CharacterManager.Instance.GetCurrentPlayerCharacterData;
        if (playerData == null)
        {
            return;
        }

        if (!isApplied)
        {
            playerData.CurrentMoveSpeed += moveSpeedBonus;
            isApplied = true;
        }

        if (activeCoroutine != null)
        {
            BuffManager.Instance.StopCoroutine(activeCoroutine);
        }

        activeCoroutine = BuffManager.Instance.StartCoroutine(RemoveLater(playerData));
    }

    private IEnumerator RemoveLater(CharacterDate playerData)
    {
        yield return new WaitForSeconds(duration);

        if (playerData != null)
        {
            playerData.CurrentMoveSpeed = Mathf.Max(1f, playerData.CurrentMoveSpeed - moveSpeedBonus);
        }

        isApplied = false;
        activeCoroutine = null;
    }

    private void ClearMoveSpeedBonus()
    {
        CharacterDate playerData = CharacterManager.Instance.GetCurrentPlayerCharacterData;
        if (isApplied && playerData != null)
        {
            playerData.CurrentMoveSpeed = Mathf.Max(1f, playerData.CurrentMoveSpeed - moveSpeedBonus);
        }

        isApplied = false;
        activeCoroutine = null;
    }
}
