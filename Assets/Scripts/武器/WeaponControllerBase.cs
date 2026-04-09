using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public abstract class WeaponControllerBase : MonoBehaviour
{
    [SerializeField, ChineseLabel("武器数据")] protected WeaponData WeaponData;

    [SerializeField,ChineseLabel("攻击音效")]protected AudioClip M_attackAudioClip;

    /// <summary>
    /// 能否操控
    /// </summary>
    protected bool CanControl = true;

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
        WeaponData.SetWeaponController(this);
    }

    protected virtual void OnEnable()
    {
        CanControl = true;
        inputManager.OnMouseLeftTap += HandleMouseClick;
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
        if (gameManager.IsGamePaused) return;
        if(inputManager.CurrentMouseState == MouseState.Hold)
        {
            HandleMouseHold();
        }
        else if(inputManager.CurrentMouseState == MouseState.Tap)
        {
            HandleMouseClick();
        }
    }

    protected virtual void LateUpdate()
    {
        if(gameManager.IsGamePaused)
        {
            return;
        }
        // 使武器始终朝向鼠标位置
        Vector2 mouseWorldPosition = inputManager.MouseWorldPosition;
        ObjectRotation.RotateTowardsTarget(this.transform, mouseWorldPosition, WeaponData.WeaponBaseData.WeaponRotationSpeed);
    }

    protected virtual void OnDisable()
    {
        if (inputManager)
        {
            inputManager.OnMouseLeftTap -= HandleMouseClick;
        }
    }

    /// <summary>
    /// 鼠标点击事件处理方法
    /// </summary>
    protected virtual void HandleMouseClick()
    {

    }

    /// <summary>
    /// 鼠标按住事件处理方法
    /// </summary>
    protected virtual void HandleMouseHold()
    {

    }

    /// <summary>
    /// 武器攻击方法
    /// </summary>
    public virtual void Attack()
    {
        
    }

    /// <summary>
    /// 游戏暂停事件
    /// </summary>
    public virtual void OnGamePaused()
    {
        CanControl = false;
        inputManager.OnMouseLeftTap -= HandleMouseClick;
    }

    /// <summary>
    /// 游戏继续事件
    /// </summary>
    public virtual void OnGameResumed()
    {
        CanControl = true;
        inputManager.OnMouseLeftTap += HandleMouseClick;
    }

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
