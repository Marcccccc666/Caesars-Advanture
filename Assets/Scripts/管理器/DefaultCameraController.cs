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
        defaultCamera.Follow = characterManager.GetCurrentPlayerCharacterData?.transform;
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
