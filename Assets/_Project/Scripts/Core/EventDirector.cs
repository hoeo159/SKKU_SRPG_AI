using NUnit.Framework.Internal;
using System.Text;
using UnityEngine;

public class EventDirector
{
    private static float normalize(int value)
    {
        return (value - 50) / 50.0f;
    }

    public static (EventCardSO, string) SelectEvent(GameStateSO state, EventCardSO[] eventPool)
    {
        if(state == null || eventPool == null || eventPool.Length == 0)
        {
            Debug.LogError("[EventDirector] SelectEvent init get null");
            return;
        }

        var profile = state.playerProfile;

        float bestScore = float.NegativeInfinity;
        EventCardSO bestEvent = null;

        var EventDebug = new StringBuilder();
        EventDebug.AppendLine("[EventDirector Debug]");
        EventDebug.AppendLine($"Profile: mercy={profile.mercy}, greedy={profile.greedy}, " +
            $"curious={profile.curious}, disc={profile.discipline}, risk={profile.risk}, " +
            $"social={profile.social}, cruel={profile.cruel}, caution={profile.caution}");

        foreach (var eventCard in eventPool)
        {
            if (eventCard == null) continue;

            float score = eventCard.baseWeight + 
                eventCard.wMercy * normalize(profile.mercy) +
                eventCard.wGreedy * normalize(profile.greedy) +
                eventCard.wCurious * normalize(profile.curious) +
                eventCard.wDiscipline * normalize(profile.discipline) +
                eventCard.wRisk * normalize(profile.risk) +
                eventCard.wSocial * normalize(profile.social) +
                eventCard.wCruel * normalize(profile.cruel) +
                eventCard.wCaution * normalize(profile.caution);

            EventDebug.AppendLine($"{eventCard.id} | {eventCard.title} | score={score:F2}");

            if (score > bestScore)
            {
                bestScore = score;
                bestEvent = eventCard;
            }
        }

        EventDebug.AppendLine();
        EventDebug.AppendLine($"Picked: {(bestEvent != null ? bestEvent.id : "null")}  score={bestScore:F2}");

        return(bestEvent, EventDebug.ToString());
    }
}
