using System.Collections;
using UnityEngine;

/// <summary>
/// 摄像头抖动工具。可挂载到任意物体上，也可通过静态方法 CameraShake.Shake() 直接调用。
/// 使用 WaitForSecondsRealtime，即使 Time.timeScale = 0 也能正常工作。
/// </summary>
[DefaultExecutionOrder(1000)]
public class CameraShake : MonoBehaviour
{
    [SerializeField, ChineseLabel("默认抖动强度")] private float defaultIntensity = 0.15f;
    [SerializeField, ChineseLabel("单次抖动时长")] private float shakeDuration = 0.06f;
    [SerializeField, ChineseLabel("抖动间隔")] private float shakeInterval = 0.04f;

    private static CameraShake instance;
    private Vector3 shakeOffset;
    private Coroutine activeShake;

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void LateUpdate()
    {
        if (shakeOffset.sqrMagnitude < 0.00001f)
            return;

        Camera cam = Camera.main;
        if (cam != null)
            cam.transform.position += shakeOffset;
    }

    /// <summary>
    /// 静态调用：抖动摄像头。
    /// 若场景中没有 CameraShake 实例，会自动在主摄像头上创建。
    /// </summary>
    /// <param name="count">抖动次数</param>
    /// <param name="intensity">抖动强度，传 -1 使用默认值</param>
    public static void Shake(int count = 1, float intensity = -1f)
    {
        EnsureInstance();
        if (instance == null)
            return;

        instance.StartShake(count, intensity);
    }

    public void StartShake(int count, float intensity = -1f)
    {
        if (intensity < 0f)
            intensity = defaultIntensity;

        if (activeShake != null)
            StopCoroutine(activeShake);

        activeShake = StartCoroutine(ShakeRoutine(count, intensity));
    }

    private IEnumerator ShakeRoutine(int count, float intensity)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 rand = Random.insideUnitCircle * intensity;
            shakeOffset = new Vector3(rand.x, rand.y, 0f);
            yield return new WaitForSecondsRealtime(shakeDuration);

            shakeOffset = Vector3.zero;
            if (i < count - 1)
                yield return new WaitForSecondsRealtime(shakeInterval);
        }

        shakeOffset = Vector3.zero;
        activeShake = null;
    }

    private static void EnsureInstance()
    {
        if (instance != null)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        instance = cam.gameObject.AddComponent<CameraShake>();
    }
}
