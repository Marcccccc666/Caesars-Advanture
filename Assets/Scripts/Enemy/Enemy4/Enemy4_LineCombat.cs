using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Enemy4_LineCombat : MonoBehaviour
{
    [Header("射线配置")]
    [SerializeField, ChineseLabel("发射点")] private Transform firePoint;
    [SerializeField, ChineseLabel("阻挡层")] private LayerMask obstacleMask;
    [SerializeField, ChineseLabel("伤害检测层")] private LayerMask damageMask;
    [SerializeField, ChineseLabel("射线宽度")] private float lineWidth = 0.08f;
    [SerializeField, ChineseLabel("射线颜色")] private Color lineColor = Color.red;
    [SerializeField, ChineseLabel("射线渲染顺序")] private int lineSortingOrder = 50;
    [SerializeField, ChineseLabel("攻击音效")] private AudioClip hitAudio;

    private LineRenderer lineRenderer;
    private Vector2 currentStart;
    private Vector2 currentEnd;
    private Transform ownerTransform;

    public Transform FirePoint => firePoint;

    private void Awake()
    {
        EnemyData ownerData = GetComponentInParent<EnemyData>();
        ownerTransform = ownerData != null ? ownerData.transform : transform;
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
        EndAimLine();
    }

    private void SetupLineRenderer()
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.numCapVertices = 4;
        lineRenderer.sortingOrder = lineSortingOrder;

        if (lineRenderer.sharedMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                lineRenderer.sharedMaterial = new Material(shader);
            }
        }
    }

    public void BeginAimLine(Vector2 targetPosition, float maxDistance)
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.enabled = true;
        UpdateAimLine(targetPosition, maxDistance);
    }

    public void UpdateAimLine(Vector2 targetPosition, float maxDistance)
    {
        if (lineRenderer == null)
        {
            return;
        }

        Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 direction = targetPosition - origin;
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = firePoint != null ? (Vector2)firePoint.right : (Vector2)transform.right;
        }

        Vector2 normalizedDirection = direction.normalized;
        float distance = Mathf.Max(0.1f, maxDistance);
        Vector2 end = ResolveBlockedEnd(origin, normalizedDirection, distance);

        currentStart = origin;
        currentEnd = end;

        lineRenderer.SetPosition(0, currentStart);
        lineRenderer.SetPosition(1, currentEnd);
    }

    public void FireLockedLine(int damage)
    {
        Vector2 segment = currentEnd - currentStart;
        float distance = segment.magnitude;
        if (distance <= 0.001f)
        {
            return;
        }

        RaycastHit2D[] hits = Physics2D.RaycastAll(currentStart, segment.normalized, distance);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D collider = hits[i].collider;
            if (collider == null)
            {
                continue;
            }

            if (IsSelfCollider(collider))
            {
                continue;
            }

            if (IsBlockingCollider(collider))
            {
                break;
            }

            if (!IsPlayerCollider(collider))
            {
                continue;
            }

            if (damageMask.value != 0 && !IsLayerInMask(collider.gameObject.layer, damageMask))
            {
                continue;
            }

            CharacterDate playerData = GetPlayerData(collider);
            if (playerData != null)
            {
                playerData.Damage(Mathf.Max(0, damage));
            }
            break;
        }

        if (hitAudio != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(hitAudio, Camera.main.transform.position);
        }
    }

    public void EndAimLine()
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.enabled = false;
    }

    private Vector2 ResolveBlockedEnd(Vector2 origin, Vector2 direction, float maxDistance)
    {
        Vector2 fallbackEnd = origin + direction * maxDistance;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, maxDistance);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D collider = hits[i].collider;
            if (collider == null || IsSelfCollider(collider))
            {
                continue;
            }

            if (!IsBlockingCollider(collider))
            {
                continue;
            }

            return hits[i].point;
        }

        return fallbackEnd;
    }

    private bool IsSelfCollider(Collider2D collider)
    {
        if (collider == null || ownerTransform == null)
        {
            return false;
        }

        Transform hitTransform = collider.transform;
        return hitTransform == ownerTransform || hitTransform.IsChildOf(ownerTransform);
    }

    private bool IsBlockingCollider(Collider2D collider)
    {
        if (collider == null)
        {
            return false;
        }

        if (collider.CompareTag("Wall"))
        {
            return true;
        }

        return IsLayerInMask(collider.gameObject.layer, obstacleMask);
    }

    private bool IsPlayerCollider(Collider2D collider)
    {
        if (collider == null)
        {
            return false;
        }

        if (collider.CompareTag("Player"))
        {
            return true;
        }

        CharacterDate data = collider.GetComponentInParent<CharacterDate>();
        if (data != null)
        {
            return true;
        }

        CharacterManager manager = CharacterManager.Instance;
        if (manager == null || manager.GetCurrentPlayerCharacterData == null)
        {
            return false;
        }

        Transform playerRoot = manager.GetCurrentPlayerCharacterData.transform;
        Transform hitTransform = collider.transform;
        return hitTransform == playerRoot || hitTransform.IsChildOf(playerRoot);
    }

    private CharacterDate GetPlayerData(Collider2D collider)
    {
        if (collider == null)
        {
            return null;
        }

        CharacterDate directData = collider.GetComponentInParent<CharacterDate>();
        if (directData != null)
        {
            return directData;
        }

        CharacterManager manager = CharacterManager.Instance;
        return manager != null ? manager.GetCurrentPlayerCharacterData : null;
    }

    private static bool IsLayerInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}
