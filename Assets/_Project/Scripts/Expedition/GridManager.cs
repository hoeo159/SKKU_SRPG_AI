using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 12;
    [SerializeField] private int height = 12;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private Material Mat1;
    [SerializeField] private Material Mat2;

    [Header("Prefabs and Parents")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform tileParent;

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
                if (renderer != null && Mat1 != null && Mat2 != null)
                {
                    renderer.sharedMaterial = (x + y) % 2 == 0 ? Mat1 : Mat2;
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
}
