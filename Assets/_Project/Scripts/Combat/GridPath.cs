using System.Collections.Generic;
using UnityEngine;

public static class GridPath
{
    private static readonly Vector2Int[] DIR4 =
    {
        new Vector2Int(1, 0), // right
        new Vector2Int(-1, 0), // left
        new Vector2Int(0, 1), // up
        new Vector2Int(0, -1) // down
    };

    public struct ReachResult
    {
        public Dictionary<Vector2Int, int> dist; // Distance from start to each reachable tile (coord, distance)
        public Dictionary<Vector2Int, Vector2Int> prev; // (coord, previous coord in path) for path reconstruction
    }

    public static ReachResult BFS_Reachable(GridManager gridManager, Vector2Int start, int maxStep)
    {
        var dist = new Dictionary<Vector2Int, int>();
        var prev = new Dictionary<Vector2Int, Vector2Int>();
        var queue = new Queue<Vector2Int>();

        dist[start] = 0;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            int curDist = dist[cur];

            foreach (var dir in DIR4)
            {
                Vector2Int next = cur + dir;
                if (dist.ContainsKey(next)) continue;

                Tile tile = gridManager.GetTile(next);
                if (tile == null || !tile.Walkable || tile.Occupied) continue;

                dist[next] = curDist + 1;
                prev[next] = cur;
                queue.Enqueue(next);
            }
        }
        return new ReachResult { dist = dist, prev = prev };
    }

    public static List<Vector2Int> ReconstructPath(ReachResult reach, Vector2Int start, Vector2Int end)
    {
        var path = new List<Vector2Int>();

        if (start == end) return path;
        if (!reach.dist.ContainsKey(end)) return path; // No path

        Vector2Int cur = end;
        path.Add(cur);

        while(cur != start)
        {
            cur = reach.prev[cur];
            path.Add(cur);
        }

        path.Reverse();
        return path;
    }

    public static int Manhattan(Vector2Int src, Vector2Int dst)
    {
        return Mathf.Abs(src.x - dst.x) + Mathf.Abs(src.y - dst.y);
    }
}
