using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ExpedController : MonoBehaviour
{
    [Header("Expedition Object")]
    [SerializeField] private GridManager    gridManager;
    [SerializeField] private ExpedPlayer    player;
    [SerializeField] private Camera         cam;
    [SerializeField] private List<CombatUnit> units;

    [Header("Raycast")]
    [SerializeField] private LayerMask tileMask;
    [SerializeField] private LayerMask unitMask;

    [Header("World Turn")]
    [SerializeField] private WorldTurnRunner    worldTurnRunner;
    [SerializeField] private CombatUnit         playerUnit;

    [Header("UI")]
    [SerializeField] private ActionMenuUI   actionMenuUI;
    [SerializeField] private TalkUI         talkUI;
    [SerializeField] private Vector2        actionMenuOffset = new Vector2(80f, 40f);
    [SerializeField] private bool           blockMoveWhileAction = true;

    [Header("LLM")]
    [SerializeField] private TalkDirector talkDirector;
    private CombatUnit currentTalkTarget;

    private Tile selectedTile;
    private Tile hoveredTile;
    private bool isMoving = false;
    private bool moveThisTurn = false;
    private bool actionThisTurn = false;

    private enum CommandType { None, SelectMove, SelectAttack }
    private CommandType commandType = CommandType.None;

    private readonly HashSet<Vector2Int> moveReachable = new HashSet<Vector2Int>();
    private readonly Dictionary<Vector2Int, Vector2Int> moveParent = new Dictionary<Vector2Int, Vector2Int>();
    private readonly HashSet<Vector2Int> attackHighlight = new HashSet<Vector2Int>();

    private static readonly Vector2Int[] DIR4 =
    {
        new Vector2Int(1, 0), // right
        new Vector2Int(-1, 0), // left
        new Vector2Int(0, 1), // up
        new Vector2Int(0, -1) // down
    };

    private void Awake()
    {
        if (cam == null)    cam = Camera.main;
        if (player == null) player = FindFirstObjectByType<ExpedPlayer>();
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (worldTurnRunner == null) worldTurnRunner = FindFirstObjectByType<WorldTurnRunner>();
        if (playerUnit == null && player != null) playerUnit = player.GetComponent<CombatUnit>();
        if (units == null || units.Count == 0)
        {
            units = new List<CombatUnit>();
        }
        if(talkDirector == null) talkDirector = FindFirstObjectByType<TalkDirector>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2Int startCoord = new Vector2Int(0, 0);
        Tile startTile = gridManager.GetTile(startCoord);

        if (startTile != null)
        {
            //playerUnit.SyncCoord(startCoord);
            startTile.Occupied = true;
        }
        else
        {
            Debug.LogError("[ExpedController] Start tile not found at coordinate: " + startCoord);
        }

        foreach(var unit in units)
        {
            if (unit != null && unit.UnitData != null)
            {
                Faction faction = unit.UnitData.faction;
                Vector3 wpos = unit.transform.position;
                Vector2Int gpos = gridManager.WorldToGridCoord(wpos);

                unit.Init(unit.UnitData, faction, gpos, wpos);

                Tile tile = gridManager.GetTile(gpos);
                tile.Occupied = true;
            }
        }

        var state = GameManager.gameManager?.state;
        if(state != null)
        {
            state.BeginExped(startCoord);
            gridManager.RefreshOpportunityCount(state);
        }

        BeginPlayerTurn();
    }

    // Update is called once per frame
    void Update()
    {
        if(worldTurnRunner != null && worldTurnRunner.busy)
        {
            return;
        }

        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if(talkUI != null && talkUI.isOpen)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !isMoving)
        {
            OnClick_EndTurn();
            return;
        }

        if (isMoving) return;

        // click
        if(Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (commandType == CommandType.SelectMove)
            {
                if(TryRaycastTile(out Tile t) && moveReachable.Contains(t.Coord))
                {
                    StartCoroutine(MovePlayerPath(t));
                }
                return;
            }
            if(commandType == CommandType.SelectAttack)
            {
                if(TryRaycastUnit(out CombatUnit target) && target != null && !target.isDead)
                {
                    StartCoroutine(PlayerAttack(target));
                }
                return;
            }

            bool isClickPlayer = false;
            if (TryRaycastUnit(out CombatUnit hit) && hit == playerUnit)
            {
                isClickPlayer = true;
            }
            else if (TryRaycastTile(out Tile tile) && tile.Coord == player.Coord)
            {
                isClickPlayer = true;
            }

            if(isClickPlayer)
            {
                if(actionMenuUI != null && actionMenuUI.isOpen) CloseActionMenu();
                else OpenActionMenu();
                return;
            }

            if(actionMenuUI != null && actionMenuUI.isOpen)
            {
                CloseActionMenu();
                return;
            }
        }

    }

    void BeginPlayerTurn()
    {
        moveThisTurn = false;
        actionThisTurn = false;

        UpdateActionMenuButton();
    }

    bool TryRaycastTile(out Tile hitTile)
    {
        hitTile = null;
        if(cam == null || Mouse.current == null)
        {
            Debug.LogError("[ExpedController] reference is missing.");
            return false;
        }

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if(Physics.Raycast(ray, out RaycastHit hit, 100.0f, tileMask))
        {
            hitTile = hit.collider.GetComponentInParent<Tile>();
            if(hitTile == null)
            {
                Debug.LogError("[ExpedController] Raycast hit an object without a Tile component.");
                return false;
            }
        }
        else
        {
            //Debug.Log("[ExpedController] Raycast did not hit any tile.");
            return false;
        }
        return true;
    }

    bool TryRaycastUnit(out CombatUnit unit)
    {
        unit = null;
        if (cam == null || Mouse.current == null) return false;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, unitMask)) return false;

        unit = hit.collider.GetComponentInParent<CombatUnit>();
        return unit != null;
    }


    // space to wait and pass turn
    IEnumerator EndPlayerTurn()
    {
        if(isMoving) yield break;
        if(worldTurnRunner != null && worldTurnRunner.busy) yield break;

        isMoving = true;
        var state = GameManager.gameManager?.state;
        if(state != null)
        {
            state.expeditionTurn++;
            Debug.Log($"[Player End] Expedition turn: {state.expeditionTurn}");
        }

        CloseActionMenu();

        if (worldTurnRunner != null)
        {
            yield return worldTurnRunner.RunWorldTurn();
        }

        BeginPlayerTurn();

        isMoving = false;
    }

    IEnumerator PlayerAttack(CombatUnit target)
    {
        isMoving = true;

        if (playerUnit == null || playerUnit.UnitData == null)
        {
            isMoving = false;
            yield break;
        }

        if(target == null || target.isDead)
        {
            isMoving = false;
            yield break;
        }

        int dist = GridPath.Manhattan(playerUnit.coord, target.coord);
        int range = playerUnit.UnitData.attackRange;

        if(dist > range)
        {
            Debug.Log("[ExpedController] Target out of range. Attack cancelled.");
            isMoving = false;
            yield break;
        }

        int dmg = playerUnit.DamageTo(target);
        bool isKilled = target.TakeDamage(dmg);
        actionThisTurn = true;
        Debug.Log($"[Attack] {playerUnit.UnitData.unitName} attacked {target.UnitData.unitName} for {dmg} damage. Target HP: {target.HP}");
    
        var state = GameManager.gameManager?.state;

        if(state != null)
        {
            if(isKilled)
            {
                state.optionalKillCount++;
            }
        }

        if(isKilled)
        {
            Tile tile = gridManager.GetTile(target.coord);
            if (tile != null) tile.Occupied = false;

            target.gameObject.SetActive(false);
        }

        ExitAttackType();
        UpdateActionMenuButton();

        isMoving = false;
    }

    void ShowAttackHighlight(bool show)
    {
        attackHighlight.Clear();

        if (!show) return;
        if (playerUnit == null || playerUnit.UnitData == null) return;

        int range = playerUnit.UnitData.attackRange;

        var enemies = FindObjectsByType<CombatUnit>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (enemy == null || enemy.isDead) continue;
            int dist = GridPath.Manhattan(playerUnit.coord, enemy.coord);
            if (dist <= range)
            {
                attackHighlight.Add(enemy.coord);
                Tile tile = gridManager.GetTile(enemy.coord);
                if(tile != null) tile.SetHighlight(true);
            }
        }
    }

    void ExitAttackType()
    {
        foreach(var coord in attackHighlight)
        {
            Tile tile = gridManager.GetTile(coord);
            if(tile != null) tile.SetHighlight(false);
        }
        attackHighlight.Clear();

        if(commandType == CommandType.SelectAttack)
        {
            commandType = CommandType.None;
        }
    }

    //void DoTalk(string targetName)
    //{
    //    var state = GameManager.gameManager?.state;
    //    if(state != null)
    //    {
    //        state.talkCount++;
    //        actionThisTurn = true;
    //        Debug.Log($"[Talk] talked with {targetName}. talkCount={state.talkCount}");
    //    }
    //}

    public void OnClick_EndTurn()
    {
        if(isMoving) return;
        if(worldTurnRunner != null && worldTurnRunner.busy) return;
        StartCoroutine(EndPlayerTurn());
    }

    public void OnClick_ActionMove()
    {
        if (moveThisTurn) return;
        if (blockMoveWhileAction && actionThisTurn) return;

        ExitMoveType();
        commandType = CommandType.SelectMove;
        BuildMoveReachable();
        ShowMoveHighlight(true);
    }

    public void OnClick_ActionAttack()
    {
        if (actionThisTurn) return;

        ExitAttackType();
        commandType = CommandType.SelectAttack;
        ShowAttackHighlight(true);
    }

    public void OnClick_ActionTalk()
    {
        if (actionThisTurn) return;

        StartTalk();
    }

    public void OnClick_ActionCancel()
    {
        CloseActionMenu();
    }

    void OpenActionMenu()
    {
        if (actionMenuUI == null || cam == null || player == null) return;
        actionMenuUI.Open(player.transform.position, cam, actionMenuOffset);

        UpdateActionMenuButton();
    }

    public void BuildMoveReachable()
    {
        moveReachable.Clear();
        moveParent.Clear();

        Vector2Int start = player.Coord;
        int range = playerUnit.UnitData.moveRange;

        var q = new Queue<Vector2Int>();
        var dist = new Dictionary<Vector2Int, int>();

        q.Enqueue(start);
        dist[start] = 0;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            int d = dist[cur];
            if (d >= range) continue;

            foreach (var dir in DIR4)
            {
                Vector2Int next = cur + dir;

                if (dist.ContainsKey(next)) continue;

                Tile t = gridManager.GetTile(next);
                if (t == null) continue;
                if (!t.Walkable) continue;
                if (t.Occupied) continue;

                dist[next] = d + 1;
                moveParent[next] = cur;
                moveReachable.Add(next);
                q.Enqueue(next);
            }
        }
    }

    void CloseActionMenu()
    {
        ExitMoveType();
        ExitAttackType();
        commandType = CommandType.None;

        if(actionMenuUI != null)
        {
            actionMenuUI.Close();
        }
    }

    void UpdateActionMenuButton()
    {
        if (actionMenuUI == null) return;

        bool canMove = !moveThisTurn && !(blockMoveWhileAction && actionThisTurn);
        bool canTalk = !actionThisTurn;
        bool canAttack = !actionThisTurn;

        actionMenuUI.SetButton(canMove, canTalk, canAttack);
    }

    void ShowMoveHighlight(bool show)
    {
        foreach(var coord in moveReachable)
        {
            Tile t = gridManager.GetTile(coord);
            if (t != null) t.SetHighlight(show);
        }
    }

    void ExitMoveType()
    {
        if(moveReachable.Count > 0) ShowMoveHighlight(false);

        moveReachable.Clear();
        moveParent.Clear();

        if(commandType == CommandType.SelectMove)
        {
            commandType = CommandType.None;
        }
    }

    List<Vector2Int> ReconstructPath(Vector2Int src, Vector2Int dst)
    {
        var path = new List<Vector2Int>();
        Vector2Int cur = dst;
        while (cur != src)
        {
            path.Add(cur);
            if (!moveParent.ContainsKey(cur))
            {
                Debug.LogError("[ExpedController] Failed to reconstruct path: no parent for " + cur);
                return new List<Vector2Int>();
            }
            cur = moveParent[cur];
        }
        path.Reverse();
        return path;
    }

    IEnumerator MovePlayerPath(Tile destTile)
    {
        isMoving = true;

        Vector2Int src = player.Coord;
        Vector2Int dst = destTile.Coord;

        var path = ReconstructPath(src, dst);

        Tile srcTile = gridManager.GetTile(src);
        if(srcTile != null) srcTile.Occupied = false;
        destTile.Occupied = true;

        foreach(var coord in path)
        {
            Tile curTile = gridManager.GetTile(coord);
            if (curTile == null) continue;

            yield return player.MoveTo(coord, curTile.transform.position);
        }

        if(playerUnit != null)
            playerUnit.SyncCoord(dst);

        moveThisTurn = true;
        ExitMoveType();
        UpdateActionMenuButton();
        if(actionMenuUI != null && actionMenuUI.isOpen)
        {
            actionMenuUI.Open(player.transform.position, cam, actionMenuOffset);
        }

        HandleEnterTile(destTile);

        isMoving = false;
    }

    bool TryFindTalkTarget(out CombatUnit target)
    {
        target = null;
        if (playerUnit == null) return false;

        var units = FindObjectsByType<CombatUnit>(FindObjectsSortMode.None);
        foreach(var unit in units)
        {
            if (unit == null || unit.isDead) continue;
            if (unit.UnitData.faction == Faction.Player)
            {
                continue;
            }
            if(GridPath.Manhattan(playerUnit.coord, unit.coord) == 1)
            {
                target = unit;
                return true;
            }
        }
        return false;
    }

    void StartTalk()
    {
        if (talkUI == null) return;
        if (talkDirector == null) return;

        if(!TryFindTalkTarget(out var target))
        {
            Debug.Log("[ExpedController] No talk target found.");
            return;
        }

        currentTalkTarget = target;
        actionThisTurn = true;
        string name = target.UnitData.unitName;

        var state = GameManager.gameManager?.state;
        if(state != null)
        {
            state.talkCount++;
            Debug.Log($"[Talk] talked with {name}. talkCount={state.talkCount}");
        }

        CloseActionMenu();

        talkUI.OpenAsync(name,
            userSend: (userText) =>
            {
                StartCoroutine(Coroutine_SendTalk(userText));
            },
            onClose: () => 
            {
                OpenActionMenu();
            }
        );
    }

    IEnumerator Coroutine_SendTalk(string userText)
    {
        var state = GameManager.gameManager?.state;
        var target = currentTalkTarget;

        if(target == null || target.UnitData == null)
        {
            talkUI.AppendLine("[Error] Talk target is missing.");
            talkUI.SetBusy(false);
            yield break;
        }

        yield return talkDirector.TalkToUnit(
            target, state, userText, 
            onOk: (response) =>
            {
                talkUI.AppendNPC(response.reply);
                talkUI.AppendLine($"[Affinity] {response.affinityDelta}  (Total: {target.affinityToPlayer})");
                talkUI.SetBusy(false);
            },
            onError: (error) =>
            {
                talkUI.AppendLine("[Error] " + error);
                talkUI.SetBusy(false);
            }
        );
    }

    bool HandleEnterTile(Tile tile)
    {
        if (tile == null) return true;
        if (GameManager.gameManager == null || GameManager.gameManager.state == null) return true;

        var state = GameManager.gameManager?.state;

        switch(tile.tileContent)
        {
            case TileContentType.Farming:
                state.farmingCount++;
                actionThisTurn = true;
                Debug.Log($"[Farming] ({tile.Coord.x},{tile.Coord.y})  farmingCount={state.farmingCount}");
                tile.SetTileContent(TileContentType.None); // allow only once per tile
                return true;

            case TileContentType.NPC:
                //state.talkCount++;
                //actionThisTurn = true;
                //Debug.Log($"[Talk] ({tile.Coord.x},{tile.Coord.y})  talkCount={state.talkCount}");
                return true;

            case TileContentType.Goal:
                Debug.Log($"[Goal] ({tile.Coord.x},{tile.Coord.y})  Expedition Completed in {state.expeditionTurn} turns!");
                state.day++;
                state.EndExped(tile.Coord, ExpedEndType.GoalReached);
                GameManager.gameManager.LoadScene("Hub");
                return false;

            case TileContentType.Radiation:
                state.radiationCount++;
                Debug.Log($"[Radiation] ({tile.Coord.x},{tile.Coord.y})  radiation={state.radiation}");
                //tile.SetTileContent(TileContentType.None);
                return true;
        }

        return true;
    }
}
