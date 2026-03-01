using Unity.Cinemachine;

public class CameraManager : Singleton<CameraManager>
{
    /// <summary>
    /// 当前相机
    /// </summary>
    private CinemachineCamera currentCamera;

    /// <summary>
    /// 默认相机
    /// </summary>
    private CinemachineCamera defaultCamera;

    /// <summary>
    /// 设置当前相机
    /// <para> 将当前相机优先级设置为最低，将新相机优先级设置为最高 </para>
    /// </summary>
    public void SetCurrentCamera(CinemachineCamera newCamera)
    {
        if (currentCamera != null)
        {
            currentCamera.Priority = 0; // 设置当前相机优先级为最低
        }

        currentCamera = newCamera;
        if (currentCamera != null)
        {
            currentCamera.Priority = 20; // 设置新相机优先级为最高
            if(CharacterManager.Instance.GetCurrentPlayerCharacterData != null)
            {
                currentCamera.Follow = CharacterManager.Instance.GetCurrentPlayerCharacterData.transform;
            }
        }
    }

    /// <summary>
    /// 设置默认相机
    /// </summary>
    public void SetDefaultCamera(CinemachineCamera defaultCam)
    {
        defaultCamera = defaultCam;
        if (currentCamera == null)
        {
            SetCurrentCamera(defaultCamera);
        }
    }

    /// <summary>
    /// 重置相机到默认相机
    /// </summary>
    public void ResetToDefaultCamera()
    {
        SetCurrentCamera(defaultCamera);
    }

}
