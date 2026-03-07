using System.Buffers.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int        width       = 12;
    [SerializeField] private int        height      = 12;
    [SerializeField] public  float      tileSize    = 1.0f;
    [SerializeField] private Material   tileMat1;
    [SerializeField] private Material   tileMat2;
    [SerializeField] private Material   highlightMat;
    [SerializeField] private Material   raiderMat;
    [SerializeField] private Material   radiationMat;
    [SerializeField] private Material   farmingMat;
    [SerializeField] private Material   npcMat;
    [SerializeField] private Material   goalMat;

    [Header("Tiles")]
    [SerializeField] private Tile       tilePrefab;
    [SerializeField] private Transform  tileParent;

    [Header("Hazard Settings")]
    [SerializeField] private EnemyController    enemyPrefab;
    [SerializeField] private Transform          enemyParent;
    [SerializeField] private int                baseEnemyCount = 1;
    [SerializeField] private int                maxEnemyCount = 5;
    [SerializeField] private int                threatPerExtraEnemy = 15;
    [SerializeField] private int                minSpawnDistFromStart = 3;

    [Header("Debug Contents")]
    [SerializeField] private bool placeDebugContents = true;
    [SerializeField] private Vector2Int startCoord = new Vector2Int(0, 0);
    [SerializeField] private Vector2Int farmingCoord = new Vector2Int(1, 0);
    [SerializeField] private Vector2Int npcCoord = new Vector2Int(0, 1);
    [SerializeField] private Vector2Int goalCoord = new Vector2Int(10, 10);


    private Tile[,] tiles;

    public Tile GetTile(Vector2Int coord)
    {
        if (tiles == null) return null;
        if(coord.x < 0 || coord.x >= width || coord.y < 0 || coord.y >= height) return null;

        return tiles[coord.x, coord.y];
    }

    private void Awake()
    {
        GenerateTiles();

        if (placeDebugContents)
        {
            SetTileContent(farmingCoord, TileContentType.Farming, farmingMat);
            SetTileContent(npcCoord, TileContentType.NPC, npcMat);
            SetTileContent(goalCoord, TileContentType.Goal, goalMat);
        }

        PlaceWorldHazard();
    }
    private void SetTileContent(Vector2Int coord, TileContentType type, Material mat = null)
    {
        Tile curTile = GetTile(coord);
        if (curTile == null) return;

        curTile.SetTileContent(type, mat);
    }

    void PlaceWorldHazard()
    {
        var state = GameManager.gameManager?.state;
        if (state == null) return;

        int radCount = Mathf.Clamp(5 + state.radiation / 2, 0, 25);

        //int raiderCount = Mathf.Clamp(1 + (state.enemyAgressive - state.guardAlert) / 20, 0, 25);
        //state.radiationOpportunityCount = radCount;
        //state.raiderOpportunityCount = raiderCount;
        
        int extraFarming = Mathf.Clamp(Mathf.Max(state.shelterComfort, state.merchantTrust) - 40, 0, 4);

        var visited = new HashSet<Vector2Int>();
        visited.Add(startCoord);
        visited.Add(farmingCoord);
        visited.Add(npcCoord);
        visited.Add(goalCoord);

        PlaceRandomTiles(TileContentType.Radiation, radCount, visited, radiationMat);
        //PlaceRandomTiles(TileContentType.Raider, raiderCount, visited);
        PlaceRandomTiles(TileContentType.Farming, extraFarming, visited, farmingMat);

        SpawnEnemies(state, visited);
        RefreshOpportunityCount(state);
    }

    void SpawnEnemies(GameStateSO state, HashSet<Vector2Int> visited)
    {
        if (enemyPrefab == null || enemyParent == null )
        {
            Debug.LogError("[GridManager] Enemy prefab or parent is not assigned. Cannot spawn enemies.");
            return;
        }

        int threat = Mathf.Max(state.enemyAgressive - state.guardAlert, 0);
        int extra = Mathf.Clamp(threat / Mathf.Max(1, threatPerExtraEnemy), 0, maxEnemyCount - baseEnemyCount);
        int count = Mathf.Clamp(baseEnemyCount + extra, 0, maxEnemyCount);

        int placed = 0;
        int limited = 0;
        int maxAttempt = width * height * 10;
        while (placed < count && limited < maxAttempt)
        {
            limited++;

            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            Vector2Int coord = new Vector2Int(x, y);

            if (visited.Contains(coord)) continue;

            if (GridPath.Manhattan(coord, startCoord) < minSpawnDistFromStart) continue;

            Tile tile = GetTile(coord);
            if (tile == null) continue;
            if (!tile.Walkable) continue;
            if (tile.Occupied) continue;

            // spawn
            EnemyController enemy = Instantiate(enemyPrefab, enemyParent);
            CombatUnit      combatUnit = enemy.GetComponent<CombatUnit>();
            if (combatUnit != null && combatUnit.UnitData != null)
            {
                combatUnit.Init(combatUnit.UnitData, combatUnit.UnitData.faction, coord, tile.transform.position);
            }
            else if (combatUnit != null)
            {
                Debug.Log("[GridManager] Spawned enemy missing CombatUnit or UnitData. Initializing with default values.");
                // 없으면 일단 sync
                combatUnit.SyncCoord(coord);
                enemy.transform.position = tile.transform.position;
            }

            tile.Occupied = true;
            visited.Add(coord);
            placed++;
        }

        state.raiderOpportunityCount = placed;

    }

    void PlaceRandomTiles(TileContentType type, int count, HashSet<Vector2Int> visited, Material mat)
    {
        int placed = 0;
        int limited = 0;
        int maxAttempt = width * height * 10;

        while(placed < count && limited <= maxAttempt)
        {
            limited++;

            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            Vector2Int coord = new Vector2Int(x, y);

            if(visited.Contains(coord)) continue;

            Tile tile = GetTile(coord);
            if (tile == null) continue;
            if (tile.tileContent != TileContentType.None)
            {
                //visited.Add(coord);
                continue;
            }

            tile.SetTileContent(type, mat);
            visited.Add(coord);
            placed++;
        }
    }

    void GenerateTiles()
    {
        if(tilePrefab == null)
        {
            Debug.LogError("[GridManager] Tile prefab is not assigned.");
            return;
        }
        if(tileParent == null)
        {
            Debug.LogError("[GridManager] Tiles parent is not assigned. Using GridManager'state transform as parent.");
            tileParent = this.transform;
        }

        ClearChildren(tileParent);

        tiles = new Tile[width, height];

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                // GridRoot local position
                Vector3 localPos = new Vector3(x * tileSize, 0, y * tileSize);
                
                Tile curTile = Instantiate(tilePrefab, tileParent);

                curTile.transform.localPosition = localPos;
                var renderer = curTile.GetComponent<MeshRenderer>();
                if (renderer != null && tileMat1 != null && tileMat2 != null)
                {
                    renderer.sharedMaterial = (x + y) % 2 == 0 ? tileMat1 : tileMat2;
                }

                Transform highlightTransform = curTile.transform.Find("Highlight");
                if (highlightTransform != null)
                {
                    MeshRenderer highlightRenderer = highlightTransform.GetComponent<MeshRenderer>();
                    if (highlightRenderer != null && highlightMat != null)
                    {
                        highlightRenderer.sharedMaterial = highlightMat;
                    }
                }

                curTile.Init(new Vector2Int(x, y));
                tiles[x, y] = curTile;
            }
        }
    }
    void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject curGameObject = parent.GetChild(i).gameObject;
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(curGameObject);
            else Destroy(curGameObject);
#else
            Destroy(curGameObject);
#endif
        }
    }

    public Vector2Int WorldToGridCoord(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int y = Mathf.RoundToInt(worldPos.z / tileSize);
        return new Vector2Int(x, y);
    }

    public void RefreshOpportunityCount(GameStateSO state)
    {
        if (state == null || tiles == null) return;
        int farmingOpp = 0;
        int talkOpp = 0;
        int radiationOpp = 0;

        for (int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                Tile tile = tiles[x, y];
                if (tile == null) continue;
                switch(tile.tileContent)
                {
                    case TileContentType.Farming:
                        farmingOpp++;
                        break;
                    case TileContentType.NPC:
                        talkOpp++;
                        break;
                    case TileContentType.Radiation:
                        radiationOpp++;
                        break;
                }
            }
        }

        int enemyOpp = 0;
        var units = FindObjectsByType<CombatUnit>(FindObjectsSortMode.None);
        foreach(var unit in units)
        {
            if (unit == null || unit.isDead) continue;
            if (unit.Faction == Faction.Enemy)
                enemyOpp++;
        }

        state.farmingOpportunityCount = farmingOpp;
        state.talkOpportunityCount = talkOpp;
        state.raiderOpportunityCount = enemyOpp;
        state.radiationOpportunityCount = radiationOpp;
    }
}

