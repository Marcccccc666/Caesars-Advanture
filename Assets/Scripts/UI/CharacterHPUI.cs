using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHPUI : MonoBehaviour
{
    [SerializeField,ChineseLabel("HP文本")] private Slider HPSlider;

    private CharacterManager characterManager => CharacterManager.Instance;

    /// <summary>
    /// 当前订阅的角色
    /// </summary>
    private CharacterDate subscribedCharacter;

    private void OnEnable()
    {
        characterManager.OnCurrentPlayerCharacterDataChanged += OnCharacterChanged;

        if (characterManager.GetCurrentPlayerCharacterData != null)
        {
            Subscribe(characterManager.GetCurrentPlayerCharacterData);
        }
    }

    private void OnDisable()
    {
        if(!characterManager)
        {
            return;
        }
        characterManager.OnCurrentPlayerCharacterDataChanged -= OnCharacterChanged;
        Unsubscribe();
    }

    private void OnCharacterChanged(CharacterDate newCharacter)
    {
        Unsubscribe();
        Subscribe(newCharacter);
    }

    private void Subscribe(CharacterDate character)
    {
        if (character == null) return;

        subscribedCharacter = character;
        character.OnHeal += UpdateHPDisplay;
        character.OnDamage += UpdateHPDisplay;

        UpdateHPDisplay(character);
    }

    private void Unsubscribe()
    {
        if (subscribedCharacter == null) return;

        subscribedCharacter.OnHeal -= UpdateHPDisplay;
        subscribedCharacter.OnDamage -= UpdateHPDisplay;
        subscribedCharacter = null;
    }


    /// <summary>
    /// 更新HP显示
    /// </summary>
    private void UpdateHPDisplay(int currentHealth, int maxHealth)
    {
        if (HPSlider != null)
        {
            HPSlider.maxValue = maxHealth;
            HPSlider.value = currentHealth;
        }
    }

    private void UpdateHPDisplay(CharacterDate characterData)
    {
        if(characterData != null)
        {
            HPSlider.maxValue = characterData.MaxHealth;
            HPSlider.value = characterData.CurrentHealth;
        }
    }
}
