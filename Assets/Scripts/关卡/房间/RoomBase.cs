using Unity.Cinemachine;
using UnityEngine;
using UnityHFSM;


public enum RoomType{}

/// <summary>
/// 房间基类
/// <para> 所有房间都必须继承自该类 </para>
/// <para> 负责房间状态机的初始化，相机初始化 </para>
/// </summary>
public abstract class RoomBase : MonoBehaviour
{
    [SerializeField, ChineseLabel("房间相机")] protected CinemachineCamera RoomCamera;

    private CameraManager cameraManager => CameraManager.Instance;

    protected StateMachine<RoomState, RoomType> M_StateMachine = new();

    protected virtual void Awake()
    {
        RoomCamera.Priority = 0; // 设置房间相机优先级为最低
        RoomStateMachineInit();
        M_StateMachine.Init();
    }

    protected virtual void Update()
    {
        M_StateMachine.OnLogic();
    }

    /// <summary>
    /// 房间状态机初始化
    /// <para> 由子类实现，添加状态和转换 </para>
    /// </summary>
    protected abstract void RoomStateMachineInit();

    /// <summary>
    /// 玩家进入房间
    /// <para> TriggerEnter 调用 </para>
    /// </summary>
    public virtual void PlayerEnterRoom()
    {
        cameraManager.SetCurrentCamera(RoomCamera);
    }

#region UNITY_EDITOR
    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    protected virtual void OnValidate()
    {
        if (RoomCamera == null)
        {
            RoomCamera = GetComponentInChildren<CinemachineCamera>();
            if (RoomCamera == null)
            {
                Debug.LogError("房间相机未设置且在子对象中未找到，请检查 " + gameObject.name);
            }
        }
    }
#endregion
}
