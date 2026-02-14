using UnityEngine;

public class Tile : MonoBehaviour
{
    [field : SerializeField] private Vector2Int Coord { get; set; }
    [SerializeField] private GameObject highlight;

    public bool Walkable { get; set; } = true;
    public bool Occupied { get; set; } = false;

    public void Init(Vector2Int coord)
    {
        Coord = coord;
        name = $"Tile_({Coord.x}_{Coord.y})";
        SetHighlight(false);
        
    }

    public void SetHighlight(bool isHighlight)
    {
        if(highlight != null)
        {
            highlight.SetActive(isHighlight);
        }
    }
}
