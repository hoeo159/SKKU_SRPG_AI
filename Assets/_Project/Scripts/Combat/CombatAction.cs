using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public static class CombatAction
{
    public static IEnumerator Move(GridManager gridManager, CombatUnit unit, List<Vector2Int> path, float stepDuration = 0.05f)
    {
        if (gridManager == null || unit == null || path == null || path.Count == 0)
        {
            yield break;
        }

        int srcIdx = (path[0] == unit.coord) ? 1 : 0;
        if (srcIdx >= path.Count) yield break;

        Vector2Int _dst = path[path.Count - 1];

        Tile src = gridManager.GetTile(unit.coord);
        Tile dst = gridManager.GetTile(_dst);

        if (dst == null || dst.Occupied) yield break;

        if(src != null) src.Occupied = false;
        dst.Occupied = true;

        float height = unit.UnitData.unitHeight;
        int len = path.Count;

        Vector2Int prev = unit.coord;
        unit.SetMoving(true);

        for (int i = srcIdx; i < len; i++)
        {
            Vector2Int coord = path[i];
            Tile tile = gridManager.GetTile(coord);
            if (tile == null) continue;

            Vector2Int delta = coord - prev;
            Vector3 lookDir;
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                lookDir = new Vector3(delta.x, 0, 0);
            }
            else
            {
                lookDir = new Vector3(0, 0, delta.y);
            }

            unit.transform.rotation = Quaternion.LookRotation(lookDir);

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

            prev = coord;
        }
        unit.SetMoving(false);
    }

    public static void Attack(GridManager gridManager, CombatUnit attacker, CombatUnit target)
    {
        if (attacker == null || target == null || attacker.isDead || target.isDead) return;

        attacker.FaceTowards(target.coord);
        target.FaceTowards(attacker.coord);
        attacker.PlayAttack();

        int damage = attacker.DamageTo(target);
        bool isKilled = target.TakeDamage(damage);

        Debug.Log($"[Attack] {attacker.UnitData.unitName} attacked {target.UnitData.unitName} for {damage} damage. Target HP: {target.HP}");

        if(isKilled)
        {
            if(gridManager != null)
            {
                Tile tile = gridManager.GetTile(target.coord);
                if(tile != null)
                {
                    tile.Occupied = false;
                }
            }
            //target.gameObject.SetActive(false);
            target.DisableAfter(3.0f);
        }
    }


}
