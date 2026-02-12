using UnityEngine;

[CreateAssetMenu(fileName = "MoveSpeedBuff", menuName = "Buffs/Move Speed Buff")]
public class MoveSpeedBuff : BuffDefinition
{
    [SerializeField, ChineseLabel("移速数值加成")] private float speedBonus = 2f;

    public override void Apply()
    {
        var target = CharacterManager.Instance.GetCurrentPlayerCharacterData;
        if (target == null)
        {
            return;
        }

        float newSpeed = target.CurrentMoveSpeed + speedBonus;
        target.CurrentMoveSpeed = Mathf.Max(0f, newSpeed);
    }
}
