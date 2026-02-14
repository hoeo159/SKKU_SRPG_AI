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

    [Header("Prefabs and Parents")]
    [SerializeField] private Tile       tilePrefab;
    [SerializeField] private Transform  tileParent;

    [Header("Debug Contents")]
    [SerializeField] private bool placeDebugContents = true;
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
            SetTileContent(farmingCoord, TileContentType.Farming);
            SetTileContent(npcCoord, TileContentType.NPC);
            SetTileContent(goalCoord, TileContentType.Goal);
        }
    }
    private void SetTileContent(Vector2Int coord, TileContentType type)
    {
        Tile curTile = GetTile(coord);
        if (curTile != null)
            curTile.SetTileContent(type);
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
            Debug.LogError("[GridManager] Tiles parent is not assigned. Using GridManager's transform as parent.");
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

        //if (placeDebugContents)
        //{
        //    SetTileContent(farmingCoord, TileContentType.Farming);
        //    SetTileContent(npcCoord, TileContentType.NPC);
        //    SetTileContent(goalCoord, TileContentType.Goal);
        //}
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
}

