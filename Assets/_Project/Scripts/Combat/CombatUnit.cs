using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CombatUnit : MonoBehaviour
{
    [SerializeField] private UnitDataSO unitData;
    [SerializeField] private Faction faction;

    public UnitDataSO UnitData => unitData;
    public Faction Faction => faction;

    public int HP { get; private set; }
    public Vector2Int coord { get; private set; }

    public bool isDead => HP <= 0;

    public void Init(UnitDataSO data, Vector2Int coord, Vector3 wPos)
    {
        Init(data, data != null ? data.faction : Faction.Neutral, coord, wPos);
    }

    public void Init(UnitDataSO udata, Faction fac, Vector2Int coor, Vector3 wPos)
    {
        // set active
        this.gameObject.SetActive(true);

        unitData = udata;
        faction = fac;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && unitData != null)
        {
            renderer.material = unitData.material;
        }

        HP = (unitData != null) ? unitData.maxHealth : 1;
        SetCoord(coor, wPos);
    }

    public void SetCoord(Vector2Int coor, Vector3 wPos)
    {
        float h = (unitData != null) ? unitData.unitHeight : 0f;
        wPos.y += h;
        coord = coor;
        transform.position = wPos;
    }

    public int DamageTo(CombatUnit target)
    {
        if (target == null) return 0;

        int atk = (unitData != null) ? unitData.attack : 0;
        int def = (target.UnitData != null) ? target.UnitData.defense : 0;

        return Mathf.Max(atk - def, 0);
    }

    public bool TakeDamage(int value)
    {
        HP -= value;
        if(HP <= 0)
        {
            HP = 0;
            return true;
        }
        return false;
    }

    public void SyncCoord(Vector2Int syncCoord)
    {
        coord = syncCoord;
    }
}
