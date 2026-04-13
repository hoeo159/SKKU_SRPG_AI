using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class EventDirector
{
    private static float normalize(int value)
    {
        return (value - 50) / 50.0f;
    }

    public static (EventCardSO eventCard, string debug) SelectEvent(GameStateSO state, EventCardSO[] eventPool)
    {
        if(state == null || eventPool == null || eventPool.Length == 0)
        {
            Debug.LogError("[EventDirector] SelectEvent init get null");
            return (null, null);
        }

        var profile = state.playerProfile;

        var EventDebug = new StringBuilder();
        EventDebug.AppendLine("[EventDirector Debug]");
        EventDebug.AppendLine($"Profile: mercy={profile.mercy}, greedy={profile.greedy}, " +
            $"curious={profile.curious}, disc={profile.discipline}, risk={profile.risk}, " +
            $"social={profile.social}, cruel={profile.cruel}, caution={profile.caution}");
        EventDebug.AppendLine($"Cooldown: [{string.Join(", ", state.recentEventIds)}]");

        var candidates = new List<EventCardSO>();
        foreach (var e in eventPool)
        {
            if (e != null && !state.recentEventIds.Contains(e.id))
                candidates.Add(e);
        }

        if (candidates.Count == 0)
        {
            EventDebug.AppendLine("All events on cooldown, resetting cooldown.");
            state.recentEventIds.Clear();
            foreach (var e in eventPool)
                if (e != null) candidates.Add(e);
        }

        float bestScore = float.NegativeInfinity;
        EventCardSO bestEvent = null;

        foreach (var eventCard in candidates)
        {
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

        if (bestEvent != null)
            state.AddEventCooldown(bestEvent.id);

        return (bestEvent, EventDebug.ToString());
    }
}
