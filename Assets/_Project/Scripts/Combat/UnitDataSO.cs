using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Scriptable Objects/UnitData")]
public class UnitDataSO : ScriptableObject
{
    public string unitName = "Unit Name";

    [Header("Visual")]
    public Material material;
    public float unitHeight = 1.0f; // height offset

    [Header("Stats")]
    public int maxHealth = 10;
    public int attack = 3;
    public int defense = 1;
    public int speed = 2; // acting earlier in turn order

    [Header("Action Ranges")]
    public int moveRange = 4; // BFS
    public int attackRange = 1; // Manhattan

    [Header("AI Values")] // TBD
    public int aiMoveWeight = 0;
}
