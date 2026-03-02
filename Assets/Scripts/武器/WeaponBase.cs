using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField, ChineseLabel("武器数据")] protected WeaponData WeaponData;

    [SerializeField,ChineseLabel("攻击音效")]protected AudioClip M_attackAudioClip;

    protected InputManager inputManager => InputManager.Instance;

    protected GameManager gameManager => GameManager.Instance;

    protected BuffManager buffManager => BuffManager.Instance;
    protected WeaponManager weaponManager => WeaponManager.Instance;
    protected PoolManager poolManager => PoolManager.Instance;
    protected MultiTimerManager MultiTimerManager => MultiTimerManager.Instance;
    protected AudioManager audioManager => AudioManager.Instance;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    protected virtual void Awake()
    {
        if (WeaponData == null)
        {
            OnValidate();
        }
    }

    protected virtual void OnEnable()
    {
        
    }

    protected virtual void Start()
    {
        if(M_attackAudioClip != null)
        {
            audioManager.CreateSFXPool(M_attackAudioClip, 5);
        }
    }

    protected virtual void Update()
    {
        // 使武器始终朝向鼠标位置
        Vector2 mouseWorldPosition = inputManager.MouseWorldPosition;
        ObjectRotation.RotateTowardsTarget(this.transform, mouseWorldPosition, WeaponData.WeaponBaseData.WeaponRotationSpeed);
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
