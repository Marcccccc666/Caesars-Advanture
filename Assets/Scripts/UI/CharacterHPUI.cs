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
    private CharacterDate currentCharacter;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void OnEnable()
    {
        if(characterManager.GetCurrentPlayerCharacterData != null)
        {
            BindCharacter(characterManager.GetCurrentPlayerCharacterData);
        }
        else
        {
            characterManager.OnCurrentPlayerCharacterDataChanged += BindCharacter;
        }
    }

    void OnDisable()
    {
        characterManager.OnCurrentPlayerCharacterDataChanged -= BindCharacter;
        UnbindCharacter(characterManager.GetCurrentPlayerCharacterData);
    }

    private void BindCharacter(CharacterDate character)
    {
        if(currentCharacter != null)
        {
            UnbindCharacter(currentCharacter);
        }
        currentCharacter = character;
        
        character.OnDamage += (damage) => UpdateHPDisplay(character);
        character.OnHeal += (heal) => UpdateHPDisplay(character);
        character.OnDie += () => UpdateHPDisplay(character);
        UpdateHPDisplay(character);
    }

    private void UnbindCharacter(CharacterDate character)
    {
        character.OnDamage -= (damage) => UpdateHPDisplay(character);
        character.OnHeal -= (heal) => UpdateHPDisplay(character);
        character.OnDie -= () => UpdateHPDisplay(character);
    }

    /// <summary>
    /// 更新HP显示
    /// </summary>
    private void UpdateHPDisplay(CharacterDate character)
    {
        int currentHP = character.CurrentHealth;
        int maxHP = character.MaxHealth;
        Debug.Log($"当前HP: {currentHP}, 最大HP: {maxHP}");
        HPText.text = $"{prefix}{currentHP} / {maxHP}";
    }
}
