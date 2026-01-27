using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    private InputData InputData => InputData.Instance;

    protected virtual void Update()
    {
        Vector2 mouseWorldPosition = InputData.MouseWorldPosition;
        ObjectRotation.RotateTowardsTarget(this.transform, mouseWorldPosition, 1000f);

        if(InputData.IsAttack)
        {
            Attack();
            InputData.IsAttack = false;
        }
    }

    /// <summary>
    /// 武器攻击方法
    /// </summary>
    public virtual void Attack()
    {
        Debug.Log("WeaponBase Attack");
    }
}
