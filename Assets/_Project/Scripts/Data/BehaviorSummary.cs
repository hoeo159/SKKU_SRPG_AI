using UnityEngine;
using System;

[Serializable]
public struct BehaviorSummary
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
    public int radiationCount;

    public int farmingOpportunityCount;
    public int talkOpportunityCount;
    public int raiderOpportunityCount;
    public int radiationOpportunityCount;

    public float farmingPerOpp;
    public float talkPerOpp;
    public float killPerOpp;
    public float avoidPerOpp;
    public float radiationPerOpp;

    public float detourRate;

    public int manhattanDist; // change to average move later collecting more data
    public int extraMove;
    public float farmingPerMove;
    public float talkPerMove;

    public static BehaviorSummary ReadExpedSnapshot(GameStateSO.ExpedSnapShot snap)
    {
        int dist = Mathf.Abs(snap.startCoord.x - snap.endCoord.x) + Mathf.Abs(snap.startCoord.y - snap.endCoord.y);
        int exMove = snap.moveCount - dist;
        exMove = exMove < 0 ? 0 : exMove;

        int farmingOpp = snap.farmingOpportunityCount;
        int talkOpp = snap.talkOpportunityCount;
        int raiderOpp = snap.raiderOpportunityCount;
        int radiationOpp = snap.radiationOpportunityCount;

        float fPerOpp = (farmingOpp > 0) ? (float)snap.farmingCount / farmingOpp : 0.5f;
        float tPerOpp    = (talkOpp > 0) ? (float)snap.talkCount / talkOpp : 0.5f;
        float kPerOpp    = (raiderOpp > 0) ? (float)snap.optionalKillCount / raiderOpp : 0.5f;
        float aPerOpp   = (raiderOpp > 0) ? (float)snap.avoidCount / raiderOpp : 0.5f;
        float radPerOpp     = (radiationOpp > 0) ? (float)snap.radiationCount / radiationOpp : 0.5f;
        float detourRate    = (snap.moveCount > 0) ? (float)exMove / snap.moveCount : 0;

        return new BehaviorSummary
        {
            expedId             = snap.expedId,
            endType             = snap.endType,
            startCoord          = snap.startCoord,
            endCoord            = snap.endCoord,
            turn                = snap.turn,
            moveCount           = snap.moveCount,
            farmingCount        = snap.farmingCount,
            talkCount           = snap.talkCount,
            optionalKillCount   = snap.optionalKillCount,
            avoidCount          = snap.avoidCount,
            radiationCount      = snap.radiationCount,

            farmingOpportunityCount = snap.farmingOpportunityCount,
            talkOpportunityCount    = snap.talkOpportunityCount,
            raiderOpportunityCount  = snap.raiderOpportunityCount,
            radiationOpportunityCount = snap.radiationOpportunityCount,

            farmingPerOpp       = fPerOpp,
            talkPerOpp          = tPerOpp,
            killPerOpp          = kPerOpp,
            avoidPerOpp         = aPerOpp,
            radiationPerOpp     = radPerOpp,

            manhattanDist       = dist,
            extraMove           = exMove,
            farmingPerMove      = snap.moveCount > 0 ? (float)snap.farmingCount / snap.moveCount : 0,
            talkPerMove         = snap.moveCount > 0 ? (float)snap.talkCount / snap.moveCount : 0
        };
    }

    public string WriteJson()
    {
        return JsonUtility.ToJson(this, true);
    }
}
