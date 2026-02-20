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

    public int manhattanDist; // change to average move later collecting more data
    public int extraMove;
    public float farmingPerMove;
    public float talkPerMove;

    public static BehaviorSummary ReadExpedSnapshot(GameStateSO.ExpedSnapShot snap)
    {
        int dist = Mathf.Abs(snap.startCoord.x - snap.endCoord.x) + Mathf.Abs(snap.startCoord.y - snap.endCoord.y);
        int extraMove = snap.moveCount - dist;
        extraMove = extraMove < 0 ? 0 : extraMove;

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
            manhattanDist       = dist,
            extraMove           = extraMove,
            farmingPerMove      = snap.moveCount > 0 ? (float)snap.farmingCount / snap.moveCount : 0,
            talkPerMove         = snap.moveCount > 0 ? (float)snap.talkCount / snap.moveCount : 0
        };
    }

    public string WriteJson()
    {
        return JsonUtility.ToJson(this);
    }
}
