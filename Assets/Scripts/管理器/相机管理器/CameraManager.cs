using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    /// <summary>
    /// 当前相机
    /// </summary>
    public CinemachineCamera currentCamera;

    /// <summary>
    /// 默认相机
    /// </summary>
    private CinemachineCamera defaultCamera;

    private CinemachineBrain cameraBrain;
    private Coroutine temporaryCameraCoroutine;

    protected override void Awake()
    {
        base.Awake();
        cameraBrain = FindFirstObjectByType<CinemachineBrain>();
    }

    /// <summary>
    /// 设置当前相机
    /// <para> 将当前相机优先级设置为最低，将新相机优先级设置为最高 </para>
    /// </summary>
    public void SetCurrentCamera(CinemachineCamera newCamera, bool followPlayer = true)
    {
        if (currentCamera != null)
        {
            currentCamera.Priority = 0; // 设置当前相机优先级为最低
        }

        currentCamera = newCamera;
        if (currentCamera != null)
        {
            currentCamera.Priority = 20; // 设置新相机优先级为最高
            if(followPlayer && CharacterManager.Instance.GetCurrentPlayerCharacterData != null)
            {
                currentCamera.Follow = CharacterManager.Instance.GetCurrentPlayerCharacterData.transform;
            }
        }
    }

    public void PlayTemporaryCamera(CinemachineCamera temporaryCamera, float blendDuration, float holdDuration, bool followPlayer, Action onComplete)
    {
        PlayTemporaryCamera(temporaryCamera, blendDuration, holdDuration, followPlayer, null, onComplete);
    }

    public void PlayTemporaryCamera(CinemachineCamera temporaryCamera, float blendDuration, float holdDuration, bool followPlayer, Action onReachedTemporaryCamera, Action onComplete)
    {
        if (temporaryCamera == null)
        {
            onReachedTemporaryCamera?.Invoke();
            onComplete?.Invoke();
            return;
        }

        if (temporaryCameraCoroutine != null)
        {
            StopCoroutine(temporaryCameraCoroutine);
        }

        temporaryCameraCoroutine = StartCoroutine(PlayTemporaryCameraCoroutine(temporaryCamera, blendDuration, holdDuration, followPlayer, onReachedTemporaryCamera, onComplete));
    }

    private IEnumerator PlayTemporaryCameraCoroutine(CinemachineCamera temporaryCamera, float blendDuration, float holdDuration, bool followPlayer, Action onReachedTemporaryCamera, Action onComplete)
    {
        CinemachineCamera previousCamera = currentCamera;
        CinemachineBlendDefinition previousBlend = default;
        bool hasCameraBrain = TryGetCameraBrain(out CinemachineBrain brain);

        if (hasCameraBrain)
        {
            previousBlend = brain.DefaultBlend;
            brain.DefaultBlend = new CinemachineBlendDefinition(previousBlend.Style, Mathf.Max(0f, blendDuration));
        }

        SetCurrentCamera(temporaryCamera, followPlayer);

        float safeBlendDuration = Mathf.Max(0f, blendDuration);
        float safeHoldDuration = Mathf.Max(0f, holdDuration);

        if (safeBlendDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(safeBlendDuration);
        }

        onReachedTemporaryCamera?.Invoke();

        if (safeHoldDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(safeHoldDuration);
        }

        if (previousCamera != null)
        {
            SetCurrentCamera(previousCamera);
        }
        else
        {
            ResetToDefaultCamera();
        }

        if (safeBlendDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(safeBlendDuration);
        }

        if (hasCameraBrain)
        {
            brain.DefaultBlend = previousBlend;
        }

        temporaryCameraCoroutine = null;
        onComplete?.Invoke();
    }

    private bool TryGetCameraBrain(out CinemachineBrain brain)
    {
        if (cameraBrain == null)
        {
            cameraBrain = FindFirstObjectByType<CinemachineBrain>();
        }

        brain = cameraBrain;
        return brain != null;
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
