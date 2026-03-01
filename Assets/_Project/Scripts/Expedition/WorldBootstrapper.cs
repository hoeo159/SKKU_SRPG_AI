using UnityEngine;

public class WorldBootstrapper : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GridManager gridManager;

    [Header("Raycast")]
    [SerializeField] private LayerMask tileMask;
    [SerializeField] private float rayHeight = 5f;
    [SerializeField] private float rayDistance = 30f;

    private void Awake()
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
    }

    private void Start()
    {
        BootstrapAll();
    }

    void BootstrapAll()
    {
        var units = FindObjectsByType<CombatUnit>(FindObjectsSortMode.None);
        foreach (var u in units)    BootstrapUnit(u);
    }

    void BootstrapUnit(CombatUnit unit)
    {
        if (unit == null || gridManager == null) return;
        if (unit.UnitData == null)
        {
            Debug.LogWarning($"[WorldBootstrapper] {unit.name} has no UnitDataSO.", unit);
            return;
        }

        Vector3 origin = unit.transform.position + Vector3.up * rayHeight;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, tileMask))
        {
            Debug.LogWarning($"[WorldBootstrapper] No Tile under {unit.name}. Check tileMask/layer/position.", unit);
            Debug.LogWarning($"[WorldBootstrapper] Raycast origin: {origin}, rayDistance: {rayDistance}", unit);
            Debug.LogWarning($"[WorldBootstrapper] tileMask.value = {tileMask.value}");
            var t = gridManager.GetTile(new Vector2Int(7, 7));
            if (t != null) Debug.LogWarning($"[WorldBootstrapper] Tile(7,7) worldPos={t.transform.position} localPos={t.transform.localPosition}");
            Debug.DrawRay(origin, Vector3.down * rayDistance, Color.red, 1f);
            return;
        }

        Tile tile = hit.collider.GetComponentInParent<Tile>();
        if (tile == null)
        {
            Debug.LogWarning($"[WorldBootstrapper] Hit but Tile not found: {hit.collider.name}", unit);
            Debug.LogWarning($"[WorldBootstrapper] hit point: {hit.point}, hit normal: {hit.normal}", unit);
            return;
        }

        unit.Init(unit.UnitData, unit.UnitData.faction, tile.Coord, tile.transform.position);
        tile.Occupied = true;
    }
}