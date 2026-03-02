using UnityEngine;
using UnityEngine.InputSystem;

public class Tile : MonoBehaviour
{
    [field: SerializeField]     public  Vector2Int  Coord { get; private set; }
    [SerializeField]            private GameObject  highlight;

    // issue : tile type이 바뀔 때 material을 가져오지 못함, 기존의 material을 캐싱해야 할 듯
    [Header("Tile Content")]
    [SerializeField]            private MeshRenderer tileRenderer;

    public TileContentType tileContent { get; private set; } = TileContentType.None;
    private Material baseMaterial;

    public bool Walkable { get; set; } = true;
    public bool Occupied { get; set; } = false; 

    public void Init(Vector2Int coord)
    {
        Coord = coord;

        if(tileRenderer == null) tileRenderer = GetComponent<MeshRenderer>();
        if(tileRenderer != null && baseMaterial == null)    baseMaterial = tileRenderer.material;

        SetTileContent(TileContentType.None);
        SetHighlight(false);
    }

    public void SetTileContent(TileContentType contentType, Material mat = null)
    {
        tileContent = contentType;
        name = $"Tile_({Coord.x}_{Coord.y}_{tileContent})";

        if(tileRenderer == null) tileRenderer = GetComponent<MeshRenderer>();
        if (tileRenderer == null) return;

        if(baseMaterial == null) baseMaterial = tileRenderer.material;
        if (baseMaterial == null) return;

        if (contentType == TileContentType.None)
        {
            tileRenderer.material = baseMaterial;
        }
        else
        {
            if (mat != null)
            {
                tileRenderer.material = mat;
            }
            else
            {
                tileRenderer.material = baseMaterial;
            }
        }
    }

    public void SetHighlight(bool isHighlight)
    {
        if(highlight != null)
        {
            highlight.SetActive(isHighlight);
        }
    }
}
