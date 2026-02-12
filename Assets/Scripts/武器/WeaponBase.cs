using UnityEngine;

[RequireComponent(typeof(WeaponData))]
public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField, ChineseLabel("武器数据")] protected WeaponData WeaponData;

    private InputData InputData => InputData.Instance;

    protected GameManager gameManager => GameManager.Instance;

    private BuffManager buffManager => BuffManager.Instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        if (WeaponData == null)
        {
            OnValidate();
        }
    }

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void Update()
    {
        // 使武器始终朝向鼠标位置
        Vector2 mouseWorldPosition = InputData.MouseWorldPosition;
        ObjectRotation.RotateTowardsTarget(this.transform, mouseWorldPosition, 1000f);
    }

    protected virtual void OnDisable()
    {
        
    }


    /// <summary>
    /// 武器攻击方法
    /// </summary>
    protected abstract void Attack();

#region UNITY_EDITOR
    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    protected virtual void OnValidate()
    {
        if (WeaponData == null)
        {
            WeaponData = GetComponent<WeaponData>();
            if (WeaponData == null)
            {
                Debug.LogError("WeaponData 未设置且在当前对象中未找到，请检查 " + gameObject.name);
            }
        }
    }
#endregion
}
