using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Enemy4_LineCombat : MonoBehaviour
{
    [Header("射线配置")]
    [SerializeField, ChineseLabel("发射点")] private Transform firePoint;
    [SerializeField, ChineseLabel("阻挡层")] private LayerMask obstacleMask;
    [SerializeField, ChineseLabel("伤害检测层")] private LayerMask damageMask;
    [SerializeField, ChineseLabel("射线宽度")] private float lineWidth = 0.08f;
    [SerializeField, Min(1), ChineseLabel("最小像素粗细")] private int minPixelWidth = 2;
    [SerializeField, ChineseLabel("射线颜色")] private Color lineColor = Color.red;
    [SerializeField, ChineseLabel("射线材质(留空自动创建)")] private Material lineMaterial;
    [SerializeField, ChineseLabel("发射瞬间粗细倍率")] private float fireWidthMultiplier = 2.4f;
    [SerializeField, ChineseLabel("发射瞬间持续时间")] private float fireFlashDuration = 0.08f;
    [SerializeField, ChineseLabel("射线渲染顺序")] private int lineSortingOrder = 50;
    [SerializeField, ChineseLabel("射线排序层(留空跟随本体)")] private string lineSortingLayerName;
    [SerializeField, ChineseLabel("攻击音效")] private AudioClip hitAudio;

    private LineRenderer lineRenderer;
    private Transform ownerTransform;
    private Vector2 currentStart;
    private Vector2 currentEnd;
    private bool isAiming;
    private float flashTimer;

    private static Material runtimeLineMaterial;

    public Transform FirePoint => firePoint;
    public LayerMask ObstacleMask => obstacleMask;

    private void Awake()
    {
        ownerTransform = GetComponentInParent<EnemyData>()?.transform ?? transform;
        lineRenderer = GetComponent<LineRenderer>();
        ConfigureLineRenderer();
        HideLine();
    }

    private void Update()
    {
        if (lineRenderer == null || flashTimer <= 0f) return;

        flashTimer -= Time.deltaTime;
        if (flashTimer > 0f) return;

        SetLineWidth(lineWidth);
        if (!isAiming) HideLine();
    }

    public void BeginAimLine(Vector2 targetPosition, float maxDistance)
    {
        if (lineRenderer == null) return;

        isAiming = true;
        flashTimer = 0f;
        SetLineWidth(lineWidth);
        ShowLine();
        UpdateAimLine(targetPosition, maxDistance);
    }

    public void UpdateAimLine(Vector2 targetPosition, float maxDistance)
    {
        if (lineRenderer == null) return;

        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 direction = targetPosition - origin;
        if (direction.sqrMagnitude < 0.0001f)
            direction = firePoint != null ? (Vector2)firePoint.right : (Vector2)transform.right;

        Vector2 end = ResolveBlockedEnd(origin, direction.normalized, Mathf.Max(0.1f, maxDistance));
        currentStart = origin;
        currentEnd = end;

        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, end);
    }

    public void FireLockedLine(int damage)
    {
        TriggerFireFlash();

        Vector2 segment = currentEnd - currentStart;
        float distance = segment.magnitude;
        if (distance <= 0.001f) return;

        RaycastHit2D[] hits = Physics2D.RaycastAll(currentStart, segment.normalized, distance);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D collider = hits[i].collider;
            if (collider == null || IsSelfCollider(collider)) continue;
            if (IsBlockingCollider(collider)) break;
            if (damageMask.value != 0 && !IsLayerInMask(collider.gameObject.layer, damageMask)) continue;

            if (!TryGetPlayerData(collider, out CharacterDate playerData)) continue;

            playerData.Damage(Mathf.Max(0, damage));
            break;
        }

        PlayHitAudio();
    }

    public void EndAimLine()
    {
        isAiming = false;
        if (flashTimer <= 0f) HideLine();
    }

    private void ConfigureLineRenderer()
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 0;
        lineRenderer.widthCurve = AnimationCurve.Constant(0f, 1f, 1f);

        ApplySortingSettings();
        SetLineWidth(lineWidth);

        Material material = ResolveLineMaterial();
        if (material != null) lineRenderer.sharedMaterial = material;
    }

    private void TriggerFireFlash()
    {
        if (lineRenderer == null) return;

        ShowLine();
        SetLineWidth(lineWidth * Mathf.Max(1f, fireWidthMultiplier));
        flashTimer = Mathf.Max(0.01f, fireFlashDuration);
    }

    private void SetLineWidth(float width)
    {
        if (lineRenderer == null) return;

        float targetWidth = Mathf.Max(0.005f, width);
        float worldUnitsPerPixel = GetWorldUnitsPerPixel();
        if (worldUnitsPerPixel > 0f)
        {
            float minWorldWidth = worldUnitsPerPixel * Mathf.Max(1, minPixelWidth);
            targetWidth = Mathf.Max(targetWidth, minWorldWidth);
        }

        lineRenderer.startWidth = targetWidth;
        lineRenderer.endWidth = targetWidth;
    }

    private void ApplySortingSettings()
    {
        if (lineRenderer == null) return;

        if (!string.IsNullOrWhiteSpace(lineSortingLayerName))
            lineRenderer.sortingLayerName = lineSortingLayerName;
        else
        {
            SpriteRenderer ownerSprite = ownerTransform != null
                ? ownerTransform.GetComponentInChildren<SpriteRenderer>(true)
                : GetComponentInParent<SpriteRenderer>(true);
            if (ownerSprite != null) lineRenderer.sortingLayerID = ownerSprite.sortingLayerID;
        }

        lineRenderer.sortingOrder = lineSortingOrder;
    }

    private Material ResolveLineMaterial()
    {
        if (lineMaterial != null) return lineMaterial;
        if (runtimeLineMaterial != null) return runtimeLineMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
            ?? Shader.Find("Sprites/Default");
        if (shader == null) return null;

        runtimeLineMaterial = new Material(shader) { name = "Enemy4_RuntimeLineMaterial" };
        return runtimeLineMaterial;
    }

    private Vector2 ResolveBlockedEnd(Vector2 origin, Vector2 direction, float maxDistance)
    {
        Vector2 fallbackEnd = origin + direction * maxDistance;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, maxDistance);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D collider = hits[i].collider;
            if (collider == null || IsSelfCollider(collider)) continue;
            if (IsBlockingCollider(collider)) return hits[i].point;
        }
        return fallbackEnd;
    }

    private bool IsSelfCollider(Collider2D collider)
    {
        if (collider == null || ownerTransform == null) return false;
        Transform hitTransform = collider.transform;
        return hitTransform == ownerTransform || hitTransform.IsChildOf(ownerTransform);
    }

    private bool IsBlockingCollider(Collider2D collider)
    {
        if (collider == null) return false;
        return collider.CompareTag("Wall") || IsLayerInMask(collider.gameObject.layer, obstacleMask);
    }

    private bool TryGetPlayerData(Collider2D collider, out CharacterDate playerData)
    {
        playerData = null;
        if (collider == null) return false;

        CharacterDate directData = collider.GetComponentInParent<CharacterDate>();
        CharacterDate currentPlayer = CharacterManager.Instance?.GetCurrentPlayerCharacterData;

        if (collider.CompareTag("Player"))
        {
            playerData = directData != null ? directData : currentPlayer;
            return playerData != null;
        }

        if (currentPlayer == null)
        {
            playerData = directData;
            return playerData != null;
        }

        Transform hitTransform = collider.transform;
        Transform playerRoot = currentPlayer.transform;
        bool samePlayer = hitTransform == playerRoot || hitTransform.IsChildOf(playerRoot) || directData == currentPlayer;
        if (!samePlayer) return false;

        playerData = currentPlayer;
        return true;
    }

    private void PlayHitAudio()
    {
        if (hitAudio == null || Camera.main == null) return;
        AudioSource.PlayClipAtPoint(hitAudio, Camera.main.transform.position);
    }

    private void ShowLine() { if (lineRenderer != null) lineRenderer.enabled = true; }
    private void HideLine() { if (lineRenderer != null) lineRenderer.enabled = false; }

    private static float GetWorldUnitsPerPixel()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic || Screen.height <= 0) return 0f;
        return (cam.orthographicSize * 2f) / Screen.height;
    }

    private static bool IsLayerInMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;
}
