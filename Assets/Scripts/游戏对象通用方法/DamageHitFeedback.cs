using System.Collections;
using System.Collections.Generic;
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
    [SerializeField, ChineseLabel("材质白闪强度倍率"), Min(1f)] private float flashIntensity = 2.2f;

    [Header("伤害飘字配置")]
    [SerializeField, ChineseLabel("飘字颜色")] private Color damageTextColor = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField, ChineseLabel("字体大小")] private float damageTextFontSize = 3f;
    [SerializeField, ChineseLabel("飘字持续时间")] private float damageTextDuration = 0.45f;
    [SerializeField, ChineseLabel("飘字上升距离")] private float damageTextRiseDistance = 0.8f;
    [SerializeField, ChineseLabel("飘字位置偏移")] private Vector3 damageTextOffset = new Vector3(0f, 0.3f, 0f);
    [SerializeField, ChineseLabel("飘字水平随机偏移")] private float damageTextRandomX = 0.2f;
    [SerializeField, ChineseLabel("飘字渲染层级")] private int damageTextSortingOrder = 200;

    private readonly List<FlashSnapshot> activeFlashSnapshots = new();
    private Coroutine flashCoroutine;
    private bool isSubscribed;
    private bool warnedMissingRenderer;

    private struct FlashSnapshot
    {
        public SpriteRenderer renderer;
        public Color spriteColor;
        public Material material;
        public string materialColorProperty;
        public Color materialColor;
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeDamageEvent();
    }

    private void OnDisable()
    {
        UnsubscribeDamageEvent();
        StopFlashAndRestore();
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
        if (flashDuration <= 0f)
        {
            return;
        }

        ResolveReferences();
        CaptureFlashSnapshots(activeFlashSnapshots);
        if (activeFlashSnapshots.Count == 0)
        {
            if (!warnedMissingRenderer)
            {
                warnedMissingRenderer = true;
                Debug.LogWarning($"{name} 未找到可闪白的 SpriteRenderer，请检查挂载层级或手动指定 flashRenderers。", this);
            }
            return;
        }

        if (flashCoroutine != null)
        {
            StopFlashAndRestore();
        }

        ApplyFlash(activeFlashSnapshots);
        flashCoroutine = StartCoroutine(FlashRoutine(Mathf.Max(0.01f, flashDuration)));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        RestoreFlash(activeFlashSnapshots);
        activeFlashSnapshots.Clear();
        flashCoroutine = null;
    }

    private void StopFlashAndRestore()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        if (activeFlashSnapshots.Count > 0)
        {
            RestoreFlash(activeFlashSnapshots);
            activeFlashSnapshots.Clear();
        }
    }

    private void CaptureFlashSnapshots(List<FlashSnapshot> snapshots)
    {
        snapshots.Clear();

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

            FlashSnapshot snapshot = new FlashSnapshot
            {
                renderer = renderer,
                spriteColor = renderer.color,
                material = renderer.material
            };

            snapshot.materialColorProperty = ResolveColorPropertyName(snapshot.material);
            if (!string.IsNullOrEmpty(snapshot.materialColorProperty))
            {
                snapshot.materialColor = snapshot.material.GetColor(snapshot.materialColorProperty);
            }

            snapshots.Add(snapshot);
        }
    }

    private void ApplyFlash(List<FlashSnapshot> snapshots)
    {
        for (int i = 0; i < snapshots.Count; i++)
        {
            FlashSnapshot snapshot = snapshots[i];
            if (snapshot.renderer == null)
            {
                continue;
            }

            Color targetSpriteColor = flashColor;
            targetSpriteColor.a = snapshot.spriteColor.a;
            snapshot.renderer.color = targetSpriteColor;

            if (!useMaterialIntensity
                || snapshot.material == null
                || string.IsNullOrEmpty(snapshot.materialColorProperty))
            {
                continue;
            }

            Color boostedColor = flashColor * Mathf.Max(1f, flashIntensity);
            boostedColor.a = snapshot.materialColor.a;
            snapshot.material.SetColor(snapshot.materialColorProperty, boostedColor);
        }
    }

    private void RestoreFlash(List<FlashSnapshot> snapshots)
    {
        for (int i = 0; i < snapshots.Count; i++)
        {
            FlashSnapshot snapshot = snapshots[i];
            if (snapshot.renderer != null)
            {
                snapshot.renderer.color = snapshot.spriteColor;
            }

            if (snapshot.material != null && !string.IsNullOrEmpty(snapshot.materialColorProperty))
            {
                snapshot.material.SetColor(snapshot.materialColorProperty, snapshot.materialColor);
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

        SpriteRenderer referenceRenderer = GetFirstValidRenderer();
        MeshRenderer textRenderer = text.GetComponent<MeshRenderer>();
        if (referenceRenderer != null && textRenderer != null)
        {
            textRenderer.sortingLayerID = referenceRenderer.sortingLayerID;
            textRenderer.sortingOrder = damageTextSortingOrder;
        }

        DamageFloatingText floatingText = textObject.AddComponent<DamageFloatingText>();
        floatingText.Initialize(text, Mathf.Max(0.05f, damageTextDuration), damageTextRiseDistance);
    }

    private Vector3 GetDamageTextSpawnPosition()
    {
        SpriteRenderer firstRenderer = GetFirstValidRenderer();
        if (firstRenderer == null)
        {
            return transform.position + damageTextOffset;
        }

        Bounds bounds = firstRenderer.bounds;
        for (int i = 0; i < flashRenderers.Length; i++)
        {
            if (flashRenderers[i] == null || flashRenderers[i] == firstRenderer)
            {
                continue;
            }

            bounds.Encapsulate(flashRenderers[i].bounds);
        }

        return new Vector3(bounds.center.x, bounds.max.y, transform.position.z) + damageTextOffset;
    }

    private SpriteRenderer GetFirstValidRenderer()
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

    private void ResolveReferences()
    {
        if (objectData == null)
        {
            objectData = GetComponent<ObjectData>();
        }

        if (objectData == null)
        {
            objectData = GetComponentInParent<ObjectData>();
        }

        if (flashRenderers != null && flashRenderers.Length > 0)
        {
            return;
        }

        flashRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (flashRenderers != null && flashRenderers.Length > 0)
        {
            return;
        }

        if (objectData != null)
        {
            flashRenderers = objectData.GetComponentsInChildren<SpriteRenderer>(true);
            if (flashRenderers != null && flashRenderers.Length > 0)
            {
                return;
            }
        }

        if (transform.parent != null)
        {
            flashRenderers = transform.parent.GetComponentsInChildren<SpriteRenderer>(true);
        }
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
        ResolveReferences();
    }
#endif
}
