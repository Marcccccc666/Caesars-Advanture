using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class DefaultCameraController : MonoBehaviour
{
    [SerializeField, ChineseLabel("默认相机")] private CinemachineCamera defaultCamera;

    private CameraManager cameraManager => CameraManager.Instance;
    private CharacterManager characterManager => CharacterManager.Instance;
    void Awake()
    {
        cameraManager.SetDefaultCamera(defaultCamera);
    }

    void OnEnable()
    {
        characterManager.OnCurrentPlayerCharacterDataChanged += SetDefaultCameraFollowTarget;
    }

    void OnDisable()
    {
        if(characterManager != null)
        {
            characterManager.OnCurrentPlayerCharacterDataChanged -= SetDefaultCameraFollowTarget;
        }
    }

    /// <summary>
    /// 设置默认相机的跟随目标为当前玩家控制的角色数据，如果当前玩家控制的角色数据为null，则取消跟随目标
     /// 当当前玩家控制的角色数据发生变化时调用
     /// 订阅于CharacterManager.OnCurrentPlayerCharacterDataChanged事件
    /// </summary>
    /// <param name="newCharacter"></param>
    private void SetDefaultCameraFollowTarget(CharacterDate newCharacter)
    {
        if (newCharacter != null)
        {
            defaultCamera.Follow = newCharacter.transform;
        }
        else
        {
            defaultCamera.Follow = null;
        }
    }

    #region UNITY_EDITOR
    /// <summary>
    /// Called when the script is loaded or a value is changed in the
    /// inspector (Called in the editor only).
    /// </summary>
    private void OnValidate()
    {
        if (defaultCamera == null)
        {
            defaultCamera = GetComponent<CinemachineCamera>();
            if (defaultCamera == null)
            {
                Debug.LogError("默认相机未设置且在子对象中未找到，请检查 " + gameObject.name);
            }
        }
    }
#endregion
}
