using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class PromptContextBuilder
{
    public static string ConvertLevel(int value)
    {
        value = Mathf.Clamp(value, 0, 100);
        if (value <= 20) return "매우 낮음";
        if (value <= 40) return "낮음";
        if (value <= 60) return "보통";
        if (value <= 80) return "높음";
        return "매우 높음";
    }

    // affinity -100 ~ 100
    public static string AffinityLabel(int value)
    {
        value = Mathf.Clamp(value, -100, 100);
        if (value <= -60) return "극도로 적대적";
        if (value <= -20) return "적대적";
        if (value < 20) return "중립";
        if (value < 60) return "우호적";
        return "매우 우호적";
    }

    public static string BuildWorld(GameStateSO state)
    {
        if (state == null) return "[World Parameter]\nunknown\n";

        var sb = new StringBuilder();
        sb.AppendLine("[World Parameter]");
        sb.AppendLine($"- 외부 침입 내성(guardAlert): {ConvertLevel(state.guardAlert)}");
        sb.AppendLine($"- 상인 우호도(merchantTrust): {ConvertLevel(state.merchantTrust)}");
        sb.AppendLine($"- 적 위협/공격성(enemyAggressive): {ConvertLevel(state.enemyAgressive)}");
        sb.AppendLine($"- 허브 분위기(shelterComfort): {ConvertLevel(state.shelterComfort)}");
        sb.AppendLine($"- 외부 오염도(radiation): {ConvertLevel(state.radiation)}");
        return sb.ToString();
    }

    public static string BuildExpedition(GameStateSO state)
    {
        if (state == null) return "[Expedition State]\nunknown\n";

        var sb = new StringBuilder();
        sb.AppendLine("[Expedition State]");
        sb.AppendLine($"- expeditionTurn: {state.expeditionTurn}");
        sb.AppendLine($"- expeditionMoveCount: {state.expeditionMoveCount}");
        sb.AppendLine($"- farmingCount: {state.farmingCount}, talkCount: {state.talkCount}");
        sb.AppendLine($"- optionalKillCount: {state.optionalKillCount}, avoidCount: {state.avoidCount}");
        return sb.ToString();
    }

    public static string BuildPlayerProfile(PlayerProfile? profile)
    {
        if (profile == null) return "[Player Profile]\nunknown\n";

        var tmp = profile.Value;

        var traits = new List<(string name, int value)>
        {
            ("자비(mercy)", tmp.mercy),
            ("욕심(greedy)", tmp.greedy),
            ("호기심(curious)", tmp.curious),
            ("규율(discipline)", tmp.discipline),
            ("위험감수(risk)", tmp.risk),
            ("사회성(social)", tmp.social),
            ("잔인함(cruel)", tmp.cruel),
            ("조심성(caution)", tmp.caution),
        };

        traits.Sort((a, b) => b.value.CompareTo(a.value));
        var top1 = traits[0];
        var top2 = traits[1];
        var low1 = traits[traits.Count - 1];

        var sb = new StringBuilder();
        sb.AppendLine("[Player Profile]");
        sb.AppendLine($"- 강한 성향: {top1.name}({ConvertLevel(top1.value)}), {top2.name}({ConvertLevel(top2.value)})");
        sb.AppendLine($"- 약한 성향: {low1.name}({ConvertLevel(low1.value)})");
        return sb.ToString();
    }

    public static string BuildRelationship(CombatUnit target)
    {
        if (target == null) return "[Relationship]\nunknown\n";

        var sb = new StringBuilder();
        sb.AppendLine("[Relationship]");
        sb.AppendLine($"- NPC의 플레이어 호감도(affinity): {AffinityLabel(target.affinityToPlayer)}");
        if (!string.IsNullOrWhiteSpace(target.memorySummary))
            sb.AppendLine($"- 기억 요약(memorySummary): {target.memorySummary}");
        return sb.ToString();
    }

    public static string BuildScene(TileContentType tileType, int manhattanDistance)
    {
        var sb = new StringBuilder();
        sb.AppendLine("[Scene]");
        sb.AppendLine($"- playerTile: {tileType}");
        sb.AppendLine($"- distance: {manhattanDistance} (Manhattan)");
        return sb.ToString();
    }


}