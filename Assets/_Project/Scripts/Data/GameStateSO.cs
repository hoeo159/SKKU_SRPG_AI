using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameStateTemplate", menuName = "Scriptable Objects/GameState")]
public class GameStateSO : ScriptableObject
{
    [System.Serializable]
    public struct ExpedSnapShot
    {
        public int expedId;
        public ExpedEndType endType;

        public Vector2Int startCoord;
        public Vector2Int endCoord;

        public float durationSecond;

        public int turn;
        public int moveCount;

        public int farmingCount;
        public int talkCount;

        public int optionalKillCount;
        public int avoidCount;
    }

    [Header("Run State")]
    public int day  = 1;
    public int gold = 0;

    [Header("Expedition State (temp log)")]
    public int expeditionTurn       = 0;
    public int expeditionMoveCount  = 0;
    public int optionalKillCount    = 0;
    public int avoidCount           = 0;
    public int farmingCount         = 0;
    public int talkCount            = 0;

    [Header("Expedition Start Session")]
    public int          expedId = 0;
    public Vector2Int   curExpedStartCoord;
    public float        curExpedStartTime;
    public bool         isExpedding = false;

    [Header("Expedition End Session")]
    public ExpedSnapShot    lastExpedSnapShot;

    [Header("Player Profile")]
    public bool             isInitPlayerProfile = false;
    public PlayerProfile    playerProfile;
    public PlayerProfile    deltaProfile;

    [TextArea(4, 12)]
    public string           lastExpedReport;
    //public List<ExpedSnapShot>   expedHistory = new List<ExpedSnapShot>();
    [TextArea(4, 12)]
    public string lastProfileReport;
    [TextArea(4, 12)]
    public string lastBehaviorSummaryJson;

    public void ClearExpeditionState()
    {
        expeditionTurn      = 0;
        expeditionMoveCount = 0;

        optionalKillCount   = 0;
        avoidCount          = 0;
        farmingCount        = 0;
        talkCount           = 0;
    }

    public void BeginExped(Vector2Int StartCoord)
    {
        ClearExpeditionState();
        expedId += 1;
        isExpedding = true;

        curExpedStartCoord  = StartCoord;
        curExpedStartTime   = Time.realtimeSinceStartup;
    }

    public void EndExped(Vector2Int EndCoord, ExpedEndType endType)
    {
        if(!isExpedding) return;

        float duration = Time.realtimeSinceStartup - curExpedStartTime;

        lastExpedSnapShot = new ExpedSnapShot
        {
            expedId         = expedId,
            endType         = endType,

            startCoord      = curExpedStartCoord,
            endCoord        = EndCoord,
            durationSecond = duration,
            turn            = expeditionTurn,

            moveCount       = expeditionMoveCount,
            farmingCount    = farmingCount,
            talkCount       = talkCount,
            optionalKillCount = optionalKillCount,
            avoidCount      = avoidCount
        };

        isExpedding = false;

        lastExpedReport =   $"[Expedition #{lastExpedSnapShot.expedId}] {lastExpedSnapShot.endType}\n" +
                            $"Start: {lastExpedSnapShot.startCoord}  End: {lastExpedSnapShot.endCoord}\n" +
                            $"Duration: {lastExpedSnapShot.durationSecond:F1}s\n" +
                            $"Turns: {lastExpedSnapShot.turn}, Moves: {lastExpedSnapShot.moveCount}\n" +
                            $"Farming: {lastExpedSnapShot.farmingCount}, Talk: {lastExpedSnapShot.talkCount}\n" +
                            $"OptionalKills: {lastExpedSnapShot.optionalKillCount}, Avoids: {lastExpedSnapShot.avoidCount}\n";
    
        ProfileCalculator.ApplyFromExpedition(this);
    }
}