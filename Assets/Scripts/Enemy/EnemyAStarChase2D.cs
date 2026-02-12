using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAStarChase2D : MonoBehaviour
{
    [Header("2D寻路")]
    [SerializeField, ChineseLabel("障碍层")] private LayerMask obstacleMask;
    [SerializeField, ChineseLabel("网格大小")] private float pathCellSize = 0.5f;
    [SerializeField, ChineseLabel("节点检测半径")] private float pathNodeCheckRadius = 0.18f;
    [SerializeField, ChineseLabel("自动按碰撞体计算节点半径")] private bool autoPathNodeCheckRadius = true;
    [SerializeField, ChineseLabel("节点半径缩放")] private float pathNodeRadiusScale = 1f;
    [SerializeField, ChineseLabel("节点半径补偿")] private float pathNodeRadiusPadding = 0.02f;
    [SerializeField, ChineseLabel("节点半径最小值")] private float pathNodeRadiusMin = 0.12f;
    [SerializeField, ChineseLabel("重算路径间隔")] private float pathRepathInterval = 0.2f;
    [SerializeField, ChineseLabel("最大搜索节点")] private int pathMaxSearchNodes = 1200;
    [SerializeField, ChineseLabel("到点阈值")] private float pathWaypointReachDistance = 0.12f;

    private EnemyVision2D vision;
    private Rigidbody2D rb2D;
    private readonly List<Vector2> currentPath = new();
    private int currentPathIndex = 0;
    private float repathTimer = 0f;

    public void BindVision(EnemyVision2D enemyVision)
    {
        vision = enemyVision;
        SyncObstacleMaskFromVision();
    }

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        if (vision == null)
        {
            vision = GetComponent<EnemyVision2D>();
        }

        SyncObstacleMaskFromVision();
        ResolvePathNodeRadiusByCollider();
    }

    public Vector2 GetMoveDirectionToPlayer()
    {
        Transform player = vision != null ? vision.PlayerTransform : null;
        if (player == null)
        {
            ResetPath();
            return Vector2.zero;
        }

        repathTimer -= Time.fixedDeltaTime;
        if (repathTimer <= 0f)
        {
            RebuildPath(player.position);
            repathTimer = Mathf.Max(0.05f, pathRepathInterval);
        }

        Vector2 moveTarget = GetCurrentMoveTarget(player.position);
        Vector2 moveDirection = moveTarget - rb2D.position;
        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            return Vector2.zero;
        }

        return moveDirection.normalized;
    }

    public void ResetPath()
    {
        currentPath.Clear();
        currentPathIndex = 0;
        repathTimer = 0f;
    }

    public void SyncObstacleMaskFromVision()
    {
        if (vision != null && vision.ObstacleMask.value != 0)
        {
            obstacleMask = vision.ObstacleMask;
            return;
        }

        if (obstacleMask.value == 0)
        {
            int inferredMask = LayerMask.GetMask("Wall", "Obstacle");
            if (inferredMask != 0)
            {
                obstacleMask = inferredMask;
            }
        }
    }

    private void RebuildPath(Vector2 playerPosition)
    {
        Vector2 start = rb2D.position;
        Vector2 goal = playerPosition;

        if (obstacleMask.value == 0 || !Physics2D.Linecast(start, goal, obstacleMask))
        {
            currentPath.Clear();
            currentPathIndex = 0;
            return;
        }

        bool foundPath = Enemy1Pathfinding2D.TryBuildPath(
            start,
            goal,
            pathCellSize,
            pathNodeCheckRadius,
            obstacleMask,
            pathMaxSearchNodes,
            currentPath
        );

        if (foundPath)
        {
            currentPathIndex = 0;
            return;
        }

        currentPath.Clear();
        currentPathIndex = 0;
    }

    private Vector2 GetCurrentMoveTarget(Vector2 playerPosition)
    {
        if (currentPath.Count == 0)
        {
            return playerPosition;
        }

        Vector2 currentPosition = rb2D.position;
        while (currentPathIndex < currentPath.Count)
        {
            float distance = Vector2.Distance(currentPosition, currentPath[currentPathIndex]);
            if (distance > pathWaypointReachDistance)
            {
                break;
            }

            currentPathIndex++;
        }

        if (currentPathIndex >= currentPath.Count)
        {
            return playerPosition;
        }

        return currentPath[currentPathIndex];
    }

    private void ResolvePathNodeRadiusByCollider()
    {
        if (!autoPathNodeCheckRadius)
        {
            return;
        }

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
        float maxRadius = 0f;
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            if (collider == null || !collider.enabled || collider.isTrigger)
            {
                continue;
            }

            Bounds bounds = collider.bounds;
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.y);
            if (radius > maxRadius)
            {
                maxRadius = radius;
            }
        }

        if (maxRadius <= 0f)
        {
            return;
        }

        float computedRadius = maxRadius * Mathf.Max(0.01f, pathNodeRadiusScale)
            + Mathf.Max(0f, pathNodeRadiusPadding);
        pathNodeCheckRadius = Mathf.Max(pathNodeRadiusMin, computedRadius);
    }
}
