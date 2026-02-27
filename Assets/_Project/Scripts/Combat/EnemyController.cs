using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Patrol")]
    [SerializeField] private float  idleRate = 0.6f;

    [Header("Move Animation")]
    [SerializeField] private float  stepDuration = 0.05f;
    [SerializeField] private float  actionDelay = 0.1f;

    [Header("Sight Tuning")]
    [SerializeField] private int    maxAggressiveSightBonus = 2;

    private CombatUnit self;
    private Vector2Int home;
    private bool isSetHome = false;

    private static readonly Vector2Int[] DIR4 =
    {
        new Vector2Int(1, 0), // right
        new Vector2Int(-1, 0), // left
        new Vector2Int(0, 1), // up
        new Vector2Int(0, -1) // down
    };

    void Awake()
    {
        self = GetComponent<CombatUnit>();
        if(gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
    }

    // world turn 1에 행동하는 enemy action
    public IEnumerator TakeTurn(GameStateSO state, List<CombatUnit> playerUnits)
    {
        if (self == null || self.isDead) yield break;
        if(gridManager == null) yield break;
        if(!isSetHome)
        {
            home = self.coord;
            isSetHome = true;
        }

        CombatUnit target = FindNearest(playerUnits);
        if (target == null || target.isDead) yield break;

        int sight = CalcSight(state);
        int distToTarget = GridPath.Manhattan(self.coord, target.coord);

        // If the target is within attack range, attack it and end turn
        if (distToTarget <= self.UnitData.attackRange)
        {
            Attack(target);
            yield return new WaitForSeconds(actionDelay);
            yield break;
        }

        // If the target is out of sight, patrol around home and end turn
        if (distToTarget > sight)
        {
            yield return Patrol();
            yield break;
        }

        yield return ChaseUsingUtility(playerUnits);
        yield return new WaitForSeconds(actionDelay);
    }

    int CalcSight(GameStateSO state)
    {
        int baseSight = Mathf.Max(1, self.UnitData.sightRange);

        int bonusSight = 0;
        if(state != null)
        {
            float tmp = Mathf.Clamp01(state.enemyAgressive / 100f);
            bonusSight = Mathf.RoundToInt(Mathf.Lerp(0f, maxAggressiveSightBonus, tmp));
        }
        return Mathf.Clamp(baseSight + bonusSight, 0, 20);
    }

    CombatUnit FindNearest(List<CombatUnit> playerUnits)
    {
        if (playerUnits == null || playerUnits.Count == 0) return null;
        CombatUnit nearest = null;
        int minDist = int.MaxValue;

        foreach(var unit in playerUnits)
        {
            if (unit.isDead) continue;
            int dist = GridPath.Manhattan(self.coord, unit.coord);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = unit;
            }
        }
        return nearest;
    }

    IEnumerator Patrol()
    {
        if(Random.value < idleRate)
        {
            yield return new WaitForSeconds(actionDelay);
            yield break; // idle
        }

        var candidates = new List<Vector2Int>();
        foreach(var dir in DIR4)
        {
            Vector2Int next = self.coord + dir;
            Tile tile = gridManager.GetTile(next);
            if (tile == null) continue;
            if (!tile.Walkable) continue;
            if (tile.Occupied) continue;

            if (GridPath.Manhattan(next, home) >= self.UnitData.patrolRange) continue;

            candidates.Add(next);
        }

        if(candidates.Count == 0)
        {
            yield return new WaitForSeconds(actionDelay);
            yield break;
        }

        Vector2Int dst = candidates[Random.Range(0, candidates.Count)];
        List<Vector2Int> path = new List<Vector2Int>() { dst };
        yield return Move(path);
    }

    IEnumerator ChaseUsingUtility(List<CombatUnit> playerUnits)
    {
        var action = EnemyUtilityAI.Select(gridManager, self, playerUnits);

        // move
        if(action.moveTo != self.coord)
        {
            var dst = GridPath.BFS_Reachable(gridManager, self.coord, self.UnitData.moveRange);
            var path = GridPath.ReconstructPath(dst, self.coord, action.moveTo);

            if(path != null && path.Count > 0)
            {
                yield return Move(path);
            }
        }    

        yield return new WaitForSeconds(actionDelay);

        if(action.target != null && !action.target.isDead)
        {
            int dst = GridPath.Manhattan(self.coord, action.target.coord);
            if(dst <= self.UnitData.attackRange)
            {
                Attack(action.target);
            }
        }
    }

    IEnumerator Move(List<Vector2Int> path)
    {
        if(path == null || path.Count == 0) yield break;

        int srcIdx = (path[0] == self.coord) ? 1 : 0;
        if (srcIdx >= path.Count) yield break;

        Vector2Int _dst = path[path.Count - 1];

        Tile src = gridManager.GetTile(self.coord);
        Tile dst = gridManager.GetTile(_dst);

        if (dst == null || dst.Occupied) yield break;
        dst.Occupied = true;

        float height = self.UnitData.unitHeight;
        int len = path.Count;
        for(int i = srcIdx; i < len; i++)
        {
            Vector2Int coord = path[i];
            Tile tile = gridManager.GetTile(coord);
            if (tile == null) continue;

            Vector3 startPos = self.transform.position;
            Vector3 endPos = tile.transform.position;
            endPos.y += height;

            float elapsed = 0f;
            float duration = Mathf.Max(0.001f, stepDuration);
            while(elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float u = Mathf.Clamp01(elapsed / duration);
                u = u * u * (3f - 2f * u);

                self.transform.position = Vector3.Lerp(startPos, endPos, u);
                yield return null;
            }

            self.SetCoord(coord, tile.transform.position);
        }
    }

    void Attack(CombatUnit target)
    {
        int damage = self.DamageTo(target);
        bool isKilled = target.TakeDamage(damage);

        Debug.Log($"[Attack] {self.UnitData.unitName} attacked {target.UnitData.unitName} for {damage} damage. Target HP: {target.HP}");
        if(isKilled)
        {
            Tile tile = gridManager.GetTile(target.coord);
            if(tile != null) tile.Occupied = false;

            target.gameObject.SetActive(false);
        }
    }
}
