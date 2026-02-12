using System.Collections.Generic;
using UnityEngine;

public static class Enemy1Pathfinding2D
{
    private const int StraightCost = 10;
    private const int DiagonalCost = 14;
    private static readonly Vector2Int[] NeighborOffsets =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1)
    };

    private class PathNode
    {
        public readonly Vector2Int Cell;
        public int G = int.MaxValue;
        public int H;
        public int F => G + H;
        public PathNode Parent;

        public PathNode(Vector2Int cell)
        {
            Cell = cell;
        }
    }

    public static bool TryBuildPath(
        Vector2 startWorld,
        Vector2 goalWorld,
        float cellSize,
        float checkRadius,
        LayerMask obstacleMask,
        int maxSearchNodes,
        List<Vector2> outPath
    )
    {
        if (outPath == null)
        {
            return false;
        }

        outPath.Clear();
        cellSize = Mathf.Max(0.1f, cellSize);
        checkRadius = Mathf.Max(0.01f, checkRadius);
        maxSearchNodes = Mathf.Max(128, maxSearchNodes);

        Vector2Int startCell = WorldToCell(startWorld, cellSize);
        Vector2Int goalCell = WorldToCell(goalWorld, cellSize);

        if (IsCellBlocked(startCell, cellSize, checkRadius, obstacleMask)
            && !TryFindNearbyWalkableCell(startCell, cellSize, checkRadius, obstacleMask, out startCell))
        {
            return false;
        }

        if (IsCellBlocked(goalCell, cellSize, checkRadius, obstacleMask)
            && !TryFindNearbyWalkableCell(goalCell, cellSize, checkRadius, obstacleMask, out goalCell))
        {
            return false;
        }

        var openList = new List<PathNode>(128);
        var closedSet = new HashSet<Vector2Int>();
        var allNodes = new Dictionary<Vector2Int, PathNode>(256);

        PathNode startNode = GetOrCreateNode(startCell, allNodes);
        startNode.G = 0;
        startNode.H = Heuristic(startCell, goalCell);
        startNode.Parent = null;
        openList.Add(startNode);

        int searchedNodes = 0;
        while (openList.Count > 0 && searchedNodes < maxSearchNodes)
        {
            PathNode current = GetBestNode(openList);
            openList.Remove(current);
            closedSet.Add(current.Cell);
            searchedNodes++;

            if (current.Cell == goalCell)
            {
                BuildWorldPath(current, goalWorld, cellSize, outPath);
                return outPath.Count > 0;
            }

            for (int i = 0; i < NeighborOffsets.Length; i++)
            {
                Vector2Int offset = NeighborOffsets[i];
                Vector2Int neighborCell = current.Cell + offset;

                if (closedSet.Contains(neighborCell))
                {
                    continue;
                }

                if (IsCellBlocked(neighborCell, cellSize, checkRadius, obstacleMask))
                {
                    continue;
                }

                bool diagonal = offset.x != 0 && offset.y != 0;
                if (diagonal)
                {
                    Vector2Int sideX = current.Cell + new Vector2Int(offset.x, 0);
                    Vector2Int sideY = current.Cell + new Vector2Int(0, offset.y);
                    if (IsCellBlocked(sideX, cellSize, checkRadius, obstacleMask)
                        || IsCellBlocked(sideY, cellSize, checkRadius, obstacleMask))
                    {
                        continue;
                    }
                }

                PathNode neighborNode = GetOrCreateNode(neighborCell, allNodes);
                int tentativeG = current.G + (diagonal ? DiagonalCost : StraightCost);
                if (tentativeG >= neighborNode.G)
                {
                    continue;
                }

                neighborNode.Parent = current;
                neighborNode.G = tentativeG;
                neighborNode.H = Heuristic(neighborCell, goalCell);

                if (!openList.Contains(neighborNode))
                {
                    openList.Add(neighborNode);
                }
            }
        }

        return false;
    }

    private static PathNode GetBestNode(List<PathNode> nodes)
    {
        PathNode best = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            PathNode candidate = nodes[i];
            if (candidate.F < best.F || (candidate.F == best.F && candidate.H < best.H))
            {
                best = candidate;
            }
        }

        return best;
    }

    private static PathNode GetOrCreateNode(Vector2Int cell, Dictionary<Vector2Int, PathNode> map)
    {
        if (map.TryGetValue(cell, out PathNode node))
        {
            return node;
        }

        node = new PathNode(cell);
        map.Add(cell, node);
        return node;
    }

    private static int Heuristic(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);
        int diagonal = Mathf.Min(dx, dy);
        int straight = Mathf.Abs(dx - dy);
        return diagonal * DiagonalCost + straight * StraightCost;
    }

    private static void BuildWorldPath(
        PathNode endNode,
        Vector2 goalWorld,
        float cellSize,
        List<Vector2> outPath
    )
    {
        var reversedCells = new List<Vector2Int>(32);
        PathNode node = endNode;
        while (node != null)
        {
            reversedCells.Add(node.Cell);
            node = node.Parent;
        }

        reversedCells.Reverse();
        for (int i = 1; i < reversedCells.Count; i++)
        {
            outPath.Add(CellToWorld(reversedCells[i], cellSize));
        }

        if (outPath.Count == 0 || Vector2.Distance(outPath[outPath.Count - 1], goalWorld) > cellSize * 0.2f)
        {
            outPath.Add(goalWorld);
        }
    }

    private static bool TryFindNearbyWalkableCell(
        Vector2Int center,
        float cellSize,
        float checkRadius,
        LayerMask obstacleMask,
        out Vector2Int result
    )
    {
        if (!IsCellBlocked(center, cellSize, checkRadius, obstacleMask))
        {
            result = center;
            return true;
        }

        const int maxRadius = 4;
        float bestDistance = float.MaxValue;
        bool found = false;
        result = center;

        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius)
                    {
                        continue;
                    }

                    Vector2Int candidate = center + new Vector2Int(x, y);
                    if (IsCellBlocked(candidate, cellSize, checkRadius, obstacleMask))
                    {
                        continue;
                    }

                    float distance = (candidate - center).sqrMagnitude;
                    if (distance >= bestDistance)
                    {
                        continue;
                    }

                    found = true;
                    bestDistance = distance;
                    result = candidate;
                }
            }

            if (found)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsCellBlocked(
        Vector2Int cell,
        float cellSize,
        float checkRadius,
        LayerMask obstacleMask
    )
    {
        if (obstacleMask.value == 0)
        {
            return false;
        }

        Vector2 center = CellToWorld(cell, cellSize);
        Collider2D hit = Physics2D.OverlapCircle(center, checkRadius, obstacleMask);
        return hit != null;
    }

    private static Vector2Int WorldToCell(Vector2 worldPosition, float cellSize)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPosition.x / cellSize),
            Mathf.RoundToInt(worldPosition.y / cellSize)
        );
    }

    private static Vector2 CellToWorld(Vector2Int cell, float cellSize)
    {
        return new Vector2(cell.x * cellSize, cell.y * cellSize);
    }
}
