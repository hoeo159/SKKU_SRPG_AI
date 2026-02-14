using UnityEngine;

[CreateAssetMenu(fileName = "GameStateTemplate", menuName = "Scriptable Objects/GameState")]
public class GameStateSO : ScriptableObject
{
    [Header("Run State")]
    public int day  = 1;
    public int gold = 0;

    [Header("Expedition State (temp log)")]
    public int expeditionTurn       = 0;
    public int expeditionMoveCount  = 0;
    public int optionalKillCount    = 0;
    public int avoidCount           = 0;
    public int harvestCount         = 0;

    public void ClearExpeditionState()
    {
        expeditionTurn      = 0;
        expeditionMoveCount = 0;

        optionalKillCount   = 0;
        avoidCount          = 0;
        harvestCount        = 0;
    }
}
