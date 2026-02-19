using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class DamageHitFeedback : MonoBehaviour
{
    [Header("数据源")]
    [SerializeField, ChineseLabel("数据组件(自动查找)")] private ObjectData objectData;

    [Header("闪白配置")]
    [SerializeField, ChineseLabel("受击渲染器(留空自动查找)")] private SpriteRenderer[] flashRenderers;
    [SerializeField, ChineseLabel("闪白颜色")] private Color flashColor = Color.white;
    [SerializeField, ChineseLabel("闪白持续时间")] private float flashDuration = 0.08f;
    [SerializeField, ChineseLabel("优先使用材质高亮")] private bool useMaterialIntensity = true;
    [SerializeField, ChineseLabel("材质白闪强度倍率")] [Min(1f)] private float flashIntensity = 2.2f;

    [Header("伤害飘字配置")]
    [SerializeField, ChineseLabel("飘字颜色")] private Color damageTextColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField, ChineseLabel("字体大小")] private float damageTextFontSize = 3f;
    [SerializeField, ChineseLabel("飘字持续时间")] private float damageTextDuration = 0.45f;
    [SerializeField, ChineseLabel("飘字上升距离")] private float damageTextRiseDistance = 0.8f;
    [SerializeField, ChineseLabel("飘字位置偏移")] private Vector3 damageTextOffset = new Vector3(0f, 0.3f, 0f);
    [SerializeField, ChineseLabel("飘字水平随机偏移")] private float damageTextRandomX = 0.2f;
    [SerializeField, ChineseLabel("飘字渲染层级")] private int damageTextSortingOrder = 200;

    private Coroutine flashCoroutine;
    private Color[] preFlashColors;
    private Material[] flashMaterials;
    private Color[] preMaterialColors;
    private string[] materialColorProperties;
    private bool isSubscribed;
    private bool warnedMissingRenderer;

    private void Awake()
    {
        AutoAssignReferences();
        EnsureColorBuffer();
        EnsureMaterialBuffer();
    }

    private void OnEnable()
    {
        SubscribeDamageEvent();
    }

    private void OnDisable()
    {
        UnsubscribeDamageEvent();

        // 仅在闪白过程中被禁用时恢复颜色，避免未初始化颜色覆盖精灵颜色。
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
            RestoreColors();
        }
    }

    private void SubscribeDamageEvent()
    {
        if (isSubscribed || objectData == null)
        {
            return;
        }

        objectData.OnDamage += HandleDamage;
        isSubscribed = true;
    }

    private void UnsubscribeDamageEvent()
    {
        if (!isSubscribed || objectData == null)
        {
            return;
        }

        objectData.OnDamage -= HandleDamage;
        isSubscribed = false;
    }

    private void HandleDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        PlayFlash();
        SpawnDamageText(damage);
    }

    private void PlayFlash()
    {
        if (flashDuration <= 0f || flashRenderers == null || flashRenderers.Length == 0)
        {
            if (!warnedMissingRenderer)
            {
                warnedMissingRenderer = true;
                Debug.LogWarning($"{name} 未找到可闪白的 SpriteRenderer，请检查挂载层级或手动指定 flashRenderers。", this);
            }
            return;
        }

        CaptureCurrentColors();

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        ApplyFlashColor();
        yield return new WaitForSeconds(flashDuration);
        RestoreColors();
        flashCoroutine = null;
    }

    private void CaptureCurrentColors()
    {
        EnsureColorBuffer();
        EnsureMaterialBuffer();

        for (int i = 0; i < flashRenderers.Length; i++)
        {
            if (flashRenderers[i] == null)
            {
                continue;
            }

            preFlashColors[i] = flashRenderers[i].color;

            if (flashMaterials == null || preMaterialColors == null || materialColorProperties == null)
            {
                continue;
            }

            Material material = flashMaterials[i];
            string colorProperty = materialColorProperties[i];
            if (material != null && !string.IsNullOrEmpty(colorProperty))
            {
                preMaterialColors[i] = material.GetColor(colorProperty);
            }
        }
    }

    private void ApplyFlashColor()
    {
        if (flashRenderers == null)
        {
            return;
        }

        for (int i = 0; i < flashRenderers.Length; i++)
        {
            SpriteRenderer renderer = flashRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            bool useMaterialFlash = TryApplyMaterialFlash(i);
            if (!useMaterialFlash)
            {
                Color targetColor = flashColor;
                targetColor.a = preFlashColors != null && i < preFlashColors.Length
                    ? preFlashColors[i].a
                    : renderer.color.a;
                renderer.color = targetColor;
            }
        }
    }

    private void RestoreColors()
    {
        if (flashRenderers == null || preFlashColors == null)
        {
            return;
        }

        int count = Mathf.Min(flashRenderers.Length, preFlashColors.Length);
        for (int i = 0; i < count; i++)
        {
            if (flashRenderers[i] == null)
            {
                continue;
            }

            flashRenderers[i].color = preFlashColors[i];

            if (flashMaterials == null || preMaterialColors == null || materialColorProperties == null)
            {
                continue;
            }

            Material material = flashMaterials[i];
            string colorProperty = materialColorProperties[i];
            if (material != null && !string.IsNullOrEmpty(colorProperty))
            {
                material.SetColor(colorProperty, preMaterialColors[i]);
            }
        }
    }

    private void SpawnDamageText(int damage)
    {
        Vector3 spawnPosition = GetDamageTextSpawnPosition();
        spawnPosition.x += Random.Range(-damageTextRandomX, damageTextRandomX);

        GameObject textObject = new GameObject($"DamageText_{damage}");
        textObject.transform.position = spawnPosition;

        TextMeshPro text = textObject.AddComponent<TextMeshPro>();
        text.text = damage.ToString();
        text.fontSize = Mathf.Max(0.1f, damageTextFontSize);
        text.alignment = TextAlignmentOptions.Center;
        text.color = damageTextColor;
        text.enableWordWrapping = false;

        if (TMP_Settings.defaultFontAsset != null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }

        MeshRenderer textRenderer = text.GetComponent<MeshRenderer>();
        SpriteRenderer referenceRenderer = GetReferenceRenderer();
        if (textRenderer != null && referenceRenderer != null)
        {
            textRenderer.sortingLayerID = referenceRenderer.sortingLayerID;
            textRenderer.sortingOrder = damageTextSortingOrder;
        }

        DamageFloatingText floatingText = textObject.AddComponent<DamageFloatingText>();
        floatingText.Initialize(text, Mathf.Max(0.05f, damageTextDuration), damageTextRiseDistance);
    }

    private Vector3 GetDamageTextSpawnPosition()
    {
        SpriteRenderer referenceRenderer = GetReferenceRenderer();
        if (referenceRenderer == null)
        {
            return transform.position + damageTextOffset;
        }

        Bounds bounds = referenceRenderer.bounds;
        for (int i = 1; i < flashRenderers.Length; i++)
        {
            if (flashRenderers[i] == null)
            {
                continue;
            }

            bounds.Encapsulate(flashRenderers[i].bounds);
        }

        return new Vector3(bounds.center.x, bounds.max.y, transform.position.z) + damageTextOffset;
    }

    private SpriteRenderer GetReferenceRenderer()
    {
        if (flashRenderers == null)
        {
            return null;
        }

        for (int i = 0; i < flashRenderers.Length; i++)
        {
            if (flashRenderers[i] != null)
            {
                return flashRenderers[i];
            }
        }

        return null;
    }

    private void AutoAssignReferences()
    {
        if (objectData == null)
        {
            objectData = GetComponent<ObjectData>();
        }

        if (objectData == null)
        {
            objectData = GetComponentInParent<ObjectData>();
        }

        if (flashRenderers == null || flashRenderers.Length == 0)
        {
            flashRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        if ((flashRenderers == null || flashRenderers.Length == 0) && objectData != null)
        {
            flashRenderers = objectData.GetComponentsInChildren<SpriteRenderer>(true);
        }

        if ((flashRenderers == null || flashRenderers.Length == 0) && transform.parent != null)
        {
            flashRenderers = transform.parent.GetComponentsInChildren<SpriteRenderer>(true);
        }
    }

    private void EnsureColorBuffer()
    {
        if (flashRenderers == null)
        {
            preFlashColors = null;
            return;
        }

        if (preFlashColors == null || preFlashColors.Length != flashRenderers.Length)
        {
            preFlashColors = new Color[flashRenderers.Length];
        }
    }

    private void EnsureMaterialBuffer()
    {
        if (flashRenderers == null)
        {
            flashMaterials = null;
            preMaterialColors = null;
            materialColorProperties = null;
            return;
        }

        int count = flashRenderers.Length;
        if (flashMaterials == null || flashMaterials.Length != count)
        {
            flashMaterials = new Material[count];
            preMaterialColors = new Color[count];
            materialColorProperties = new string[count];
        }

        for (int i = 0; i < count; i++)
        {
            SpriteRenderer renderer = flashRenderers[i];
            if (renderer == null)
            {
                flashMaterials[i] = null;
                materialColorProperties[i] = null;
                continue;
            }

            Material material = renderer.material;
            flashMaterials[i] = material;
            materialColorProperties[i] = ResolveColorPropertyName(material);
        }
    }

    private bool TryApplyMaterialFlash(int index)
    {
        if (!useMaterialIntensity
            || flashMaterials == null
            || preMaterialColors == null
            || materialColorProperties == null
            || index < 0
            || index >= flashMaterials.Length
            || index >= preMaterialColors.Length
            || index >= materialColorProperties.Length)
        {
            return false;
        }

        Material material = flashMaterials[index];
        string colorProperty = materialColorProperties[index];
        if (material == null || string.IsNullOrEmpty(colorProperty))
        {
            return false;
        }

        Color boostedColor = flashColor * Mathf.Max(1f, flashIntensity);
        boostedColor.a = preMaterialColors[index].a;
        material.SetColor(colorProperty, boostedColor);
        return true;
    }

    private static string ResolveColorPropertyName(Material material)
    {
        if (material == null)
        {
            return null;
        }

        if (material.HasProperty("_Color"))
        {
            return "_Color";
        }

        if (material.HasProperty("_BaseColor"))
        {
            return "_BaseColor";
        }

        return null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        AutoAssignReferences();
        EnsureColorBuffer();
    }
#endif
}
