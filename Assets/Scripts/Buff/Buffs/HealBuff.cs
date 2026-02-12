using UnityEngine;

[CreateAssetMenu(fileName = "HealBuff", menuName = "Buffs/Heal Buff")]
public class HealBuff : BuffDefinition
{
    [SerializeField, ChineseLabel("生命上限增加")] private int maxHealthIncrease = 2;

    public override void Apply()
    {
        var characterDate = CharacterManager.Instance.GetCurrentPlayerCharacterData;
        if (characterDate == null)
        {
            return;
        }

        characterDate.MaxHealth = characterDate.MaxHealth + maxHealthIncrease;
        characterDate.CurrentHealth = characterDate.MaxHealth;
    }
}
