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

        PlayerProfile delta = DeltaProcess(summary);
        state.deltaProfile = delta;
        state.playerProfile = PlayerProfile.Add(state.playerProfile, delta);

        state.lastProfileReport =
            $"[Profile Update from Expedition #{summary.expedId}]\n" +
            $"EndType : {summary.endType}\n" +
            $"Dist: {summary.manhattanDist}, ExtraMoves: {summary.extraMove}\n" +
            $"Turns: {summary.turn}, Moves: {summary.moveCount}\n" +
            $"Farming: {summary.farmingCount}, Talk: {summary.talkCount}\n" +
            $"OptionalKills: {summary.optionalKillCount}, Avoids: {summary.avoidCount}\n\n" +
            $"[Delta]\n" + 
            DeltaToString(delta) + "\n" +
            $"[Current Profile]\n" +
            state.playerProfile.ToReportForm();

    }

    private static PlayerProfile DeltaProcess(BehaviorSummary s)
    {

        int dMercy      = Mathf.Clamp(s.avoidCount * 1 + s.talkCount * 1 - s.optionalKillCount * 1, -10, 10);
        int dGreedy     = Mathf.Clamp(s.farmingCount * 1 - s.turn / 10, -10, 10);
        int dCurious    = Mathf.Clamp(s.extraMove * 1 + s.talkCount * 1 + s.farmingCount * 1, -10, 10);
        int dDiscipline = Mathf.Clamp(-s.extraMove * 1, -10, 10);
        if(s.endType != ExpedEndType.GoalReached || s.endType != ExpedEndType.NormalReturn) dDiscipline -= 5;

        int dRisk       = Mathf.Clamp(s.optionalKillCount * 1 - s.avoidCount * 1, -10, 10);
        int dSocial     = Mathf.Clamp(s.talkCount * 1, -10, 10);
        int dCruel      = Mathf.Clamp(s.optionalKillCount * 1, -10, 10);
        int dCaution    = Mathf.Clamp(s.avoidCount * 1 - s.optionalKillCount * 1, -10, 10);

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
