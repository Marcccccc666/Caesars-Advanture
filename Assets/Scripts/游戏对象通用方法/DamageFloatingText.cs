using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class DamageFloatingText : MonoBehaviour
{
    private TextMeshPro text;
    private float duration;
    private float riseDistance;
    private float elapsed;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private Color startColor;
    private bool initialized;

    public void Initialize(TextMeshPro textMesh, float lifeDuration, float rise)
    {
        text = textMesh;
        duration = Mathf.Max(0.05f, lifeDuration);
        riseDistance = rise;

        elapsed = 0f;
        startPosition = transform.position;
        endPosition = startPosition + Vector3.up * riseDistance;
        startColor = text != null ? text.color : Color.white;
        initialized = true;
    }

    private void Awake()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshPro>();
        }
    }

    private void OnEnable()
    {
        if (initialized || text == null)
        {
            return;
        }

        // 兜底参数，避免遗漏 Initialize 时飘字永久停留。
        Initialize(text, 0.45f, 0.8f);
    }

    private void Update()
    {
        if (text == null)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        transform.position = Vector3.Lerp(startPosition, endPosition, t);
        transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, t);

        Color fadeColor = startColor;
        fadeColor.a = Mathf.Lerp(startColor.a, 0f, t);
        text.color = fadeColor;

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
