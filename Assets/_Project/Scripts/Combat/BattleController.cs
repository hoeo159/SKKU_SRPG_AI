using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BattleController : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GridManager    gridManager;
    [SerializeField] private Camera         cam;

    [Header("Raycast")]
    [SerializeField] private LayerMask tileMask;
    [SerializeField] private LayerMask unitMask;

    [Header("Prefab")]
    [SerializeField] private CombatUnit unitPrefab;
    [SerializeField] private Transform  unitParent;

    [Header("Data")]
    [SerializeField] private UnitDataSO playerData;
    [SerializeField] private UnitDataSO enemyData;

    [Header("Spawn")]
    [SerializeField] private Vector2Int playerSpawnPos = new Vector2Int(1, 1);
    [SerializeField] private Vector2Int enemySpawnPos = new Vector2Int(8, 8);

    private readonly List<CombatUnit> players   = new();
    private readonly List<CombatUnit> enemies   = new();
    private readonly List<CombatUnit> turnOrder = new();

    private int turnIndex = 0;
    private bool playerMoved = false;

    private CombatUnit curUnit => (turnOrder.Count > 0) ? turnOrder[turnIndex] : null;

    private void Awake()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (cam != null) cam = Camera.main;
        if (unitParent == null) unitParent = this.transform;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnTest();
        BuildTurnOrder();
        BeginTurn();
    }

    void SpawnTest()
    {
        SpawnUnit(playerData, Faction.Player, playerSpawnPos);
        SpawnUnit(enemyData, Faction.Enemy, enemySpawnPos);
    }

    CombatUnit SpawnUnit(UnitDataSO data, Faction fac, Vector2Int coord)
    {
        Tile tile = gridManager.GetTile(coord);
        if (tile == null)
        {
            Debug.LogError($"[SpawnUnit] Invalid spawn position: {coord}");
            return null;
        }
        var unit = Instantiate(unitPrefab, unitParent);
        //unit.Init(data, fac, coord, tile.transform.position);
        unit.Init(data, fac, coord, tile.transform.position);

        tile.Occupied = true;

        if(fac == Faction.Player) players.Add(unit);
        else if(fac == Faction.Enemy) enemies.Add(unit);

        return unit;
    }    

    void BuildTurnOrder()
    {
        turnOrder.Clear();
        turnOrder.AddRange(players);
        turnOrder.AddRange(enemies);

        turnIndex = 0;
    }

    void BeginTurn()
    {
        playerMoved = false;
        //Debug.Log($"[BeginTurn] Turn Start: {curUnit.Faction}  coord={curUnit.coord}  hp={curUnit.HP}");
    }

    // Update is called once per frame
    void Update()
    {
        if(turnOrder.Count == 0)
        {
            Debug.Log("[BattleController] No units in battle.");
            return;
        }

        if (IsBattleEnd()) return;

        var cur = curUnit;
        if(cur == null || cur.isDead)
        {
            NextTurn();
            return;
        }

        if (cur.Faction == Faction.Player)
        {
            PlayerTurn(cur);
        }
        else if(cur.Faction == Faction.Enemy)
        {
            EnemyTurn(cur);
            NextTurn();
        }
    }

    void NextTurn()
    {
        int limit = 0;
        do
        {
            turnIndex = (turnIndex + 1) % turnOrder.Count;
            limit++;
        }
        while ((curUnit == null || curUnit.isDead) || limit > 100);

        BeginTurn();
    }

    void PlayerTurn(CombatUnit unit)
    {
        if(Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("[PlayerTurn] Player ended turn.");
            NextTurn();
            return;
        }

        if(Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if(TryRaycastUnit(out CombatUnit target))
            {
                if(target.Faction != Faction.Player && !target.isDead)
                {
                    int dist = GridPath.Manhattan(unit.coord, target.coord);
                    if(dist <= unit.UnitData.attackRange)
                    {
                        Attack(unit, target);
                        NextTurn();
                        return;
                    }
                }
            }

            if(!playerMoved && TryRaycastTile(out Tile tile))
            {
                var reach = GridPath.BFS_Reachable(gridManager, unit.coord, unit.UnitData.moveRange);
                if(reach.dist.ContainsKey(tile.Coord))
                {
                    Move(unit, tile.Coord);
                    playerMoved = true;
                }
            }
        }
    }

    bool TryRaycastUnit(out CombatUnit target)
    {
        target = null;
        if(cam == null || Mouse.current == null) return false;
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, unitMask)) return false;

        target = hit.collider.GetComponent<CombatUnit>();
        return target != null;
    }

    bool TryRaycastTile(out Tile tile)
    {
        tile = null;
        if (cam == null || Mouse.current == null) return false;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, tileMask)) return false;

        tile = hit.collider.GetComponent<Tile>();
        return tile != null;
    }

    void Attack(CombatUnit attacker, CombatUnit target)
    {
        int damage = attacker.DamageTo(target);
        bool killed = target.TakeDamage(damage);

        Debug.Log($"[Attack] {attacker.UnitData.unitName} attacked {target.UnitData.unitName} for {damage} damage. Target HP: {target.HP}");

        if(killed)
        {
            Tile tile = gridManager.GetTile(target.coord);
            if(tile != null) tile.Occupied = false;

            Debug.Log($"[Attack] {target.UnitData.unitName} died at {target.coord}");
            target.gameObject.SetActive(false);
        }
    }

    void Move(CombatUnit unit, Vector2Int dest)
    {
        Tile src = gridManager.GetTile(unit.coord);
        Tile dst = gridManager.GetTile(dest);
        if (dst == null || dst.Occupied) return;
        if (src != null) src.Occupied = false;

        unit.SetCoord(dest, dst.transform.position);
        dst.Occupied = true;
    }

    void EnemyTurn(CombatUnit enemy)
    {
        var action = EnemyUtilityAI.Select(gridManager, enemy, players);

        Debug.Log($"[EnemyTurn] Enemy selected action: moveTo={action.moveTo} target={action.target?.UnitData.unitName} score={action.score}");

        if (action.moveTo != enemy.coord)
        {
            Move(enemy, action.moveTo);
        }

        if (action.target != null && !action.target.isDead)
        {
            int dist = GridPath.Manhattan(enemy.coord, action.target.coord);
            if (dist <= enemy.UnitData.attackRange)
            {
                Attack(enemy, action.target);
            }
        }
    }

    bool IsBattleEnd()
    {
        bool playersAlive = players.Exists(p => !p.isDead);
        bool enemiesAlive = enemies.Exists(e => !e.isDead);

        if (!playersAlive)
        {
            Debug.Log("Defeat");
            return true;
        }
        else if (!enemiesAlive)
        {
            Debug.Log("Victory");
            return true;
        }
        return false;
    }
}
