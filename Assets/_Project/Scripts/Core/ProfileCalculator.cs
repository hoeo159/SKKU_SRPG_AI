using Unity.Burst.Intrinsics;
using UnityEngine;

public class ProfileCalculator
{
    public static void ApplyFromExpedition(GameStateSO state)
    {
        if (state == null) return;

        BehaviorSummary summary = BehaviorSummary.ReadExpedSnapshot(state.lastExpedSnapShot);
        state.lastBehaviorSummaryJson = summary.WriteJson();

        if(!state.isInitPlayerProfile)
        {
            state.playerProfile = PlayerProfile.Init();
            state.isInitPlayerProfile = true;
        }

        // update profile
        PlayerProfile delta = DeltaProcess(summary);
        state.deltaProfile = delta;
        state.playerProfile = PlayerProfile.Add(state.playerProfile, delta);

        // update world state
        state.radiation = Mathf.Clamp(state.radiation + summary.radiationCount, 0, 100);
        int deltaEnemy = summary.optionalKillCount * 2;
        state.enemyAgressive = Mathf.Clamp(state.enemyAgressive + deltaEnemy, 0, 100);

        state.lastProfileReport =
            $"[Profile Update from Expedition #{summary.expedId}]\n" +
            $"EndType : {summary.endType}\n" +
            $"Dist: {summary.manhattanDist}, ExtraMoves: {summary.extraMove}\n" +
            $"Turns: {summary.turn}, Moves: {summary.moveCount}\n" +
            $"Farming: {summary.farmingCount}, Talk: {summary.talkCount}\n" +
            $"OptionalKills: {summary.optionalKillCount}, Avoids: {summary.avoidCount}\n\n" +
            $"Radiation: {summary.radiationCount}\n" +
            $"[Delta]\n" + 
            DeltaToString(delta) + "\n" +
            $"[Current Profile]\n" +
            state.playerProfile.ToReportForm();

    }

    public static int CLAMP10(float value) => Mathf.Clamp(Mathf.RoundToInt(value), -10, 10);

    private static PlayerProfile DeltaProcess(BehaviorSummary s)
    {
        float farming   = s.farmingPerOpp - 0.5f;
        float talk      = s.talkPerOpp - 0.5f;
        float kill      = s.killPerOpp - 0.5f;
        float avoid     = s.avoidPerOpp - 0.5f;
        float radiation = (s.radiationOpportunityCount > 0) ? s.radiationPerOpp - 0.5f : 0f;
        float detour    = s.detourRate - 0.25f;

        int dGreedy = CLAMP10(farming * 12f);
        int dSocial = CLAMP10(talk * 12f);

        int dCurious = CLAMP10(detour * 12f + farming * 4f + talk * 4f);
        int dDiscipline = CLAMP10((-detour) * 12f);

        if (s.endType == ExpedEndType.Abort) dDiscipline = Mathf.Clamp(dDiscipline - 5, -10, 10);

        int dCruel = CLAMP10(kill * 14f);
        int dMercy = CLAMP10((avoid - kill) * 10f + talk * 6f);

        int dRisk = CLAMP10((kill - avoid) * 8f + radiation * 10f);
        int dCaution = CLAMP10((avoid - kill) * 8f - radiation * 8f);

        return new PlayerProfile
        {
            mercy = dMercy,
            greedy = dGreedy,
            curious = dCurious,
            discipline = dDiscipline,
            risk = dRisk,
            social = dSocial,
            cruel = dCruel,
            caution = dCaution
        };
    }

    private static string DeltaToString(PlayerProfile delta)
    {
        return
            $"Mercy: {delta.mercy:+#;-#;0}\n" +
            $"Greedy: {delta.greedy:+#;-#;0}\n" +
            $"Curious: {delta.curious:+#;-#;0}\n" +
            $"Discipline: {delta.discipline:+#;-#;0}\n" +
            $"Risk: {delta.risk:+#;-#;0}\n" +
            $"Social: {delta.social:+#;-#;0}\n" +
            $"Cruel: {delta.cruel:+#;-#;0}\n" +
            $"Caution: {delta.caution:+#;-#;0}\n";
    }
}
