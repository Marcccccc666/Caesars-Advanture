using UnityEngine;

public class ObjectBaseData : ScriptableObject
{
    [ChineseLabel("移动速度")]public float moveSpeed;

    [ChineseLabel("基础攻击力")]public int baseAttack;

    [ChineseLabel("最大生命值")]public int maxHealth;

    [ChineseLabel("攻击间隔")] public float attackInterval;  

    [ChineseLabel("角色旋转速度")] public float rotationSpeed;
}
