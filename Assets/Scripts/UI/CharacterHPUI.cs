using TMPro;
using UnityEngine;

public class CharacterHPUI : MonoBehaviour
{
    [SerializeField,ChineseLabel("文字前缀")] private string prefix = "HP: ";
    [SerializeField,ChineseLabel("HP文本")] private TextMeshProUGUI HPText;

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
        if (HPText != null)
            HPText.text = $"{prefix}{currentHealth}/{maxHealth}";
    }

    private void UpdateHPDisplay(CharacterDate characterData)
    {
        if(characterData != null)
        {
            Debug.Log("角色数据已更新，刷新HP显示");
            HPText.text = $"{prefix}{characterData.CurrentHealth}/{characterData.MaxHealth}";
        }
    }
}
