using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GridMoveCoroutine
{
    public static IEnumerator Move(GridManager gridManager, CombatUnit unit, List<Vector2Int> path, float stepDuration)
    {
        if(gridManager == null || unit == null || path == null || path.Count == 0)
        {
            yield break;
        }

        int srcIdx = (path[0] == unit.coord) ? 1 : 0;
        if (srcIdx >= path.Count) yield break;

        Vector2Int _dst = path[path.Count - 1];

        Tile src = gridManager.GetTile(unit.coord);
        Tile dst = gridManager.GetTile(_dst);

        if (dst == null || dst.Occupied) yield break;
        dst.Occupied = true;

        float height = unit.UnitData.unitHeight;
        int len = path.Count;

        for (int i = srcIdx; i < len; i++)
        {
            Vector2Int coord = path[i];
            Tile tile = gridManager.GetTile(coord);
            if (tile == null) continue;

            Vector3 startPos = unit.transform.position;
            Vector3 endPos = tile.transform.position;
            endPos.y += height;

            float elapsed = 0f;
            float duration = Mathf.Max(0.001f, stepDuration);
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float u = Mathf.Clamp01(elapsed / duration);
                u = u * u * (3f - 2f * u);

                unit.transform.position = Vector3.Lerp(startPos, endPos, u);
                yield return null;
            }

            unit.SetCoord(coord, tile.transform.position);
        }
    }
}
