using UnityEngine;
using UnityEngine.InputSystem;

public class Tile : MonoBehaviour
{
    [field: SerializeField]     public  Vector2Int  Coord { get; private set; }
    [SerializeField]            private GameObject  highlight;
    public TileContentType tileContent { get; private set; } = TileContentType.None;

    public bool Walkable { get; set; } = true;
    public bool Occupied { get; set; } = false; 

    public void Init(Vector2Int coord)
    {
        Coord = coord;
        SetTileContent(TileContentType.None);
        SetHighlight(false);
    }

    public void SetTileContent(TileContentType contentType)
    {
        tileContent = contentType;
        name = $"Tile_({Coord.x}_{Coord.y}_{tileContent})";
    }

    public void SetHighlight(bool isHighlight)
    {
        if(highlight != null)
        {
            highlight.SetActive(isHighlight);
        }
    }
}
