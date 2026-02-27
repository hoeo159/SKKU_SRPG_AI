using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;

    [Header("Patrol")]
    [SerializeField] private bool   canPatrol = true;
    [SerializeField] private float  idleRate = 0.7f;
    [SerializeField] private int    interactionRange = 3;

    [Header("Move Animation")]
    [SerializeField] private float stepDuration = 0.05f;
    [SerializeField] private float actionDelay = 0.1f;

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

    private void Awake()
    {
        self = GetComponent<CombatUnit>();
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
    }

    public IEnumerator TakeTurn(GameStateSO state, CombatUnit playerUnit)
    {
        if (self == null || self.isDead || gridManager == null) yield break;
        if (!isSetHome)
        {
            home = self.coord;
            isSetHome = true;
        }
        if(!canPatrol)
        {
            yield return new WaitForSeconds(actionDelay);
            yield break;
        }

        if (playerUnit != null && !playerUnit.isDead)
        {
            int distToPlayer = GridPath.Manhattan(self.coord, playerUnit.coord);
            if (distToPlayer <= interactionRange)
            {
                yield return new WaitForSeconds(actionDelay);
                yield break;
            }
        }

        if(Random.value < idleRate)
        {
            yield return new WaitForSeconds(actionDelay);
            yield break;
        }

        var candidates = new List<Vector2Int>();
        foreach(var dir in DIR4)
        {
            Vector2Int coord = self.coord + dir;
            Tile tile = gridManager.GetTile(coord);
            if (tile == null || !tile.Walkable || tile.Occupied) continue;
            if(GridPath.Manhattan(coord, home) > self.UnitData.patrolRange) continue;

            candidates.Add(coord);
        }

        if(candidates.Count == 0)
        {
            yield return new WaitForSeconds(actionDelay);
            yield break;
        }

        Vector2Int dst = candidates[Random.Range(0, candidates.Count)];
        List<Vector2Int> path = new List<Vector2Int> { dst };
        yield return GridMoveCoroutine.Move(gridManager, self, path, stepDuration);
        yield return new WaitForSeconds(actionDelay);
    }

}
