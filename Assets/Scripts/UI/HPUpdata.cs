using TMPro;
using UnityEngine;

public class HPUpdata : MonoBehaviour
{
    [SerializeField,ChineseLabel("文字前缀")] private string prefix = "HP: ";
    [SerializeField,ChineseLabel("HP文本")] private TextMeshProUGUI HPText;

    [SerializeField,ChineseLabel("角色数据")] private ObjectData characterData;

    private void Update()
    {
        HPText.text = $"{prefix}{characterData.CurrentHealth} / {characterData.MaxHealth}";
    }
}
