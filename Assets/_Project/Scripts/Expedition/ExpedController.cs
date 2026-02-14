using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ExpedController : MonoBehaviour
{
    [Header("Expedition Object")]
    [SerializeField] private GridManager    gridManager;
    [SerializeField] private ExpedPlayer    player;
    [SerializeField] private Camera         cam;

    [Header("Raycast")]
    [SerializeField] private LayerMask tileMask;

    private Tile selectedTile;
    private Tile hoveredTile;
    private bool isMoving = false;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (player == null) player = FindFirstObjectByType<ExpedPlayer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();

        Vector2Int startCoord = new Vector2Int(0, 0);
        Tile startTile = gridManager.GetTile(startCoord);

        if (startTile != null)
        {
            player.Place(startCoord, startTile.transform.position);
            startTile.Occupied = true;
        }
        else
        {
            Debug.LogError("[ExpedController] Start tile not found at coordinate: " + startCoord);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            ClearHover();
            return;
        }

        UpdateHover();

        if(Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if(hoveredTile != null)
            {
                OnClickedTile(hoveredTile);
            }
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

    void OnClickedTile(Tile tile)
    {
        SelectAndHighlight(tile);

        if (tile.Occupied) return;
        if (!tile.Walkable) return;

        int dx = Mathf.Abs(tile.Coord.x - player.Coord.x);
        int dy = Mathf.Abs(tile.Coord.y - player.Coord.y);
        bool adjacent = (dx + dy) <= player.maxMoveDistance;
        if(!adjacent)
        {
            Debug.Log("[ExpedController] Clicked tile is not adjacent to the player.");
            return;
        }

        StartCoroutine(MovePlayer(tile));
    }

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

        if(GameManager.gameManager != null && GameManager.gameManager.state != null)
        {
            GameManager.gameManager.state.expeditionTurn++;
            GameManager.gameManager.state.expeditionMoveCount++;
            Debug.Log("[ExpedController] Player moved to " + dest.Coord + ". Expedition turn: "
                + GameManager.gameManager.state.expeditionTurn);
        }

        HandleEnterTile(dest);

        isMoving = false;
    }

    void HandleEnterTile(Tile tile)
    {
        if (tile == null) return;
        if (GameManager.gameManager == null || GameManager.gameManager.state == null) return;

        var state = GameManager.gameManager.state;

        switch(tile.tileContent)
        {
            case TileContentType.Farming:
                state.farmingCount++;
                Debug.Log($"[Farming] ({tile.Coord.x},{tile.Coord.y})  farmingCount={state.farmingCount}");
                tile.SetTileContent(TileContentType.None);
                break;

            case TileContentType.NPC:
                state.talkCount++;
                Debug.Log($"[Talk] ({tile.Coord.x},{tile.Coord.y})  talkCount={state.talkCount}");
                break;

            case TileContentType.Goal:
                Debug.Log($"[Goal] ({tile.Coord.x},{tile.Coord.y})  Expedition Completed in {state.expeditionTurn} turns!");
                state.day++;
                GameManager.gameManager.LoadScene("Hub");
                break;
        }
    }
}
