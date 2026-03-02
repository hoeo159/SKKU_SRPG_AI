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
    [SerializeField] private List<CombatUnit> playerUnits;

    [Header("Raycast")]
    [SerializeField] private LayerMask tileMask;
    [SerializeField] private LayerMask unitMask;

    [Header("World Turn")]
    [SerializeField] private WorldTurnRunner    worldTurnRunner;
    [SerializeField] private CombatUnit         playerUnit;

    private Tile selectedTile;
    private Tile hoveredTile;
    private bool isMoving = false;
    private bool moveThisTurn = false;
    private bool actionThisTurn = false;

    private void Awake()
    {
        if (cam == null)    cam = Camera.main;
        if (player == null) player = FindFirstObjectByType<ExpedPlayer>();
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        if (worldTurnRunner == null) worldTurnRunner = FindFirstObjectByType<WorldTurnRunner>();
        if (playerUnit == null && player != null) playerUnit = player.GetComponent<CombatUnit>();
        if (playerUnits == null || playerUnits.Count == 0)
        {
            playerUnits = new List<CombatUnit>();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2Int startCoord = new Vector2Int(0, 0);
        Tile startTile = gridManager.GetTile(startCoord);

        if (startTile != null)
        {
            //player.Place(startCoord, startTile.transform.position);
            startTile.Occupied = true;
        }
        else
        {
            Debug.LogError("[ExpedController] Start tile not found at coordinate: " + startCoord);
        }

        foreach(var unit in playerUnits)
        {
            if (unit != null && unit.UnitData != null)
            {
                Faction faction = unit.UnitData.faction;
                Vector3 wpos = unit.transform.position;
                Vector2Int gpos = gridManager.WorldToGridCoord(wpos);
                unit.Init(unit.UnitData, faction, gpos, wpos);
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

        if(Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !isMoving)
        {
            OnClick_EndTurn();
            return;
        }

        if (isMoving) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearHover();
            return;
        }

        UpdateHover();

        if(Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleLeftClick();
        }
    }

    void BeginPlayerTurn()
    {
        moveThisTurn = false;
        actionThisTurn = false;
    }

    bool TryRaycast(out RaycastHit hit)
    {
        hit = default;
        if (cam == null || Mouse.current == null) return false;
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        int mask = tileMask.value | unitMask.value;
        return Physics.Raycast(ray, out hit, 100.0f, mask);
    }

    private static bool LayerInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private void HandleLeftClick()
    {
        if (!TryRaycast(out var hit)) return;

        int layer = hit.collider.gameObject.layer;

        // unit click 우선
        if (LayerInMask(layer, unitMask))
        {
            var unit = hit.collider.GetComponentInParent<CombatUnit>();
            if (unit != null && unit != playerUnit && !unit.isDead)
            {
                if (unit.UnitData.faction == Faction.Enemy)
                {
                    StartCoroutine(PlayerAttack(unit));
                    return;
                }
                if (unit.UnitData.faction == Faction.Neutral)
                {
                    if(GridPath.Manhattan(playerUnit.coord, unit.coord) == 1)
                    {
                        DoTalk(unit.UnitData.unitName);
                    }
                    return;
                }
            }
            return;
        }

        if (LayerInMask(layer, tileMask))
        {
            var tile = hit.collider.GetComponentInParent<Tile>();
            if (tile != null)
            {
                OnClickedTile(tile);
            }
            return;
        }
    }

    void ClearHover()
    {
        SetHover(hoveredTile, false);
        hoveredTile = null;
    }

    void SetHover(Tile tile, bool isHover)
    {
        if (tile != null)
        {
            //hover issue : 마우스 클릭해도 hover가 계속 남아 있음
            if (tile == selectedTile)
            {
                return;
            }
            tile.SetHighlight(isHover);
        }
    }

    void UpdateHover()
    {
        if (TryRaycastTile(out Tile hitTile))
        {
            if (hitTile != hoveredTile)
            {
                SetHover(hoveredTile, false);
                hoveredTile = hitTile;
                SetHover(hoveredTile, true);
            }
        }
        else
        {
            ClearHover();
        }
    }
    bool TryRaycastTile(out Tile hitTile)
    {
        hitTile = null;
        if(cam == null)
        {
            Debug.LogError("[ExpedController] Camera reference is missing.");
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

        if(worldTurnRunner != null)
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

        isMoving = false;
    }

    void OnClickedTile(Tile tile)
    {
        //SelectAndHighlight(tile);

        if (tile.Occupied) return;
        if (!tile.Walkable) return;

        int dx = Mathf.Abs(tile.Coord.x - player.Coord.x);
        int dy = Mathf.Abs(tile.Coord.y - player.Coord.y);
        bool adjacent = (dx + dy) <= player.maxMoveDistance;
        if(!adjacent)
        {
            Debug.Log("[ExpedController] too far");
            return;
        }

        if(moveThisTurn)
        {
            Debug.Log("[ExpedController] already moved this turn");
            return;
        }
        if(actionThisTurn)
        {
            Debug.Log("[ExpedController] already took action this turn");
            return;
        }

        StartCoroutine(MovePlayer(tile));
    }

    // selected tile highlight (지금은 x)
    void SelectAndHighlight(Tile tile)
    {
        if(selectedTile != null)
        {
            selectedTile.SetHighlight(false);
        }

        selectedTile = tile;

        if(selectedTile != null)
        {
            selectedTile.SetHighlight(true);
        }
    }

    IEnumerator MovePlayer(Tile dest)
    {
        isMoving = true;

        Tile src = gridManager.GetTile(player.Coord);
        if (src != null) src.Occupied = false;
        dest.Occupied = true;

        yield return player.MoveTo(dest.Coord, dest.transform.position);

        var state = GameManager.gameManager?.state;
        if (state != null)
        {
            state.expeditionMoveCount++;
            Debug.Log("[ExpedController] Player moved to " + dest.Coord + ". Expedition turn: "
                + state.expeditionTurn);
        }

        if(playerUnit != null)
        {
            playerUnit.SyncCoord(dest.Coord);
        }

        moveThisTurn = true;
        bool canRunWorldTurn = HandleEnterTile(dest);

        isMoving = false;
    }

    void DoTalk(string targetName)
    {
        var state = GameManager.gameManager?.state;
        if(state != null)
        {
            state.talkCount++;
            actionThisTurn = true;
            Debug.Log($"[Talk] talked with {targetName}. talkCount={state.talkCount}");
        }
    }    

    public void OnClick_EndTurn()
    {
        if(isMoving) return;
        if(worldTurnRunner != null && worldTurnRunner.busy) return;
        StartCoroutine(EndPlayerTurn());
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
