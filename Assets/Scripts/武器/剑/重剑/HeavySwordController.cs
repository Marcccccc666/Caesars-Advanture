using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeavySwordController : InitialSwordController
{
    [SerializeField, ChineseLabel("蓄力条")] private Slider chargeBar;
    private HeavySwordData M_swordData => (HeavySwordData)WeaponData;

    protected override void OnEnable()
    {
        base.OnEnable();
        chargeBar.value = 0f;
        chargeBar.maxValue = M_swordData.M_WeaponBaseData.MaxCharge;

        chargeBar.gameObject.SetActive(false);
        inputManager.OnMouseLeftRelease += HandleMouseRelease;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if(inputManager != null)
        {
            inputManager.OnMouseLeftRelease -= HandleMouseRelease;
        }
    }

    protected override void HandleMouseHold()
    {
        M_swordData.CurrentSwordState = SwordState.Charging;
        chargeBar.gameObject.SetActive(true);

        // 更新蓄力值
        if(M_swordData.CurrentCharge <= M_swordData.M_WeaponBaseData.MaxCharge)
        {
            M_swordData.CurrentCharge += Time.deltaTime * M_swordData.M_WeaponBaseData.ChargeIncreaseRate;
        }
    }

    private void HandleMouseRelease()
    {
        if (M_swordData.CurrentSwordState == SwordState.Charging)
        {
            chargeBar.gameObject.SetActive(false);

            Attack();

            
        }
    }

    public override void Attack()
    {
        if(M_swordData.CurrentSwordState == SwordState.Attack)
        {
            return;
        }

        if(M_swordData.CurrentCharge <= 0f && 
            !MultiTimerManager.IsDownTimerComplete("SwordAttackCooldown"))
        {
            return;
        }

        if(M_attackAudioClip != null)
        {
            audioManager.PlaySFX(M_attackAudioClip);
        }

        M_swordData.CurrentSwordState = SwordState.Attack;
        StartCoroutine(AttackAnimationFinishCheck());
        buffManager.AttackTriggered?.Invoke(transform);
    }

    protected override IEnumerator AttackAnimationFinishCheck()
    {
        yield return base.AttackAnimationFinishCheck();
        // 重置蓄力值
        M_swordData.CurrentCharge = 0f;
    }


}
