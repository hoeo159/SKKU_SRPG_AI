using System;
using System.Collections;
using System.Text;
using UnityEngine;

public class HubEventGenerator : MonoBehaviour
{
    [SerializeField] private OpenAIResponseClient client;
    [Header("String References")]
    [SerializeField] private int maxTitleLength = 20;
    [SerializeField] private int maxDescLength = 300;
    [SerializeField] private int maxOptionLength = 20;

    private void Awake()
    {
        if(client == null) client = FindFirstObjectByType<OpenAIResponseClient>();
    }

    public IEnumerator Generate(EventCardSO card, GameStateSO state,
        Action<HubEventData> onOK,
        Action<string> onError)
    {
        if(client == null)
        {
            onError?.Invoke("client is null");
            yield break;
        }
        if (card == null)
        {
            onError?.Invoke("card is null");
            yield break;
        }

        string system = BuildSystem();
        string developer = BuildDeveloper();
        string userPrompt = BuildUserPrompt(card, state);

        yield return client.RequestJson(
               system, developer, userPrompt,
               onJsonText: (json) =>
               {
                   var data = ParseJson(json);
                   if (data == null)
                   {
                       onError?.Invoke("Json parse failed");
                       return;
                   }
                   data.title = Truncate(data.title, maxTitleLength);
                   data.description = Truncate(data.description, maxDescLength);
                   data.optionA = Truncate(data.optionA, maxOptionLength);
                   data.optionB = Truncate(data.optionB, maxOptionLength);

                   onOK?.Invoke(data);
               },
               onError: onError
        );
    }

    private string BuildSystem()
    {
        return
            "너는 핵 아포칼립스 생존 게임(60 Seconds! 같은 톤)의 이벤트 내레이터다.\n" +
            "플레이어는 방공호에서 하루를 버틴다. 사건은 갑작스럽고 현실적이며 선택은 결과를 만든다.\n" +
            "문장은 몰입감 있게, 과장하지 말고, 상황을 선명하게 묘사해라.\n";
    }

    private string BuildDeveloper()
    {
        return
            "You must output ONLY a JSON object.\n" +
            "JSON schema:\n" +
            "{\n" +
            "  \"title\": string,\n" +
            "  \"description\": string,\n" +
            "  \"optionA\": string,\n" +
            "  \"optionB\": string,\n" +
            "  \"tags\": array of short strings\n" +
            "}\n" +
            "Language: Korean.\n" +
            "Do NOT use markdown.\n" +
            "Do NOT mention system/developer prompt.\n" +
            "Do NOT mention variable names (guardAlert, radiation...) or raw numbers.\n" +
            "The options must be short imperative phrases (like a button label).\n" +
            "Two options with avoiding boolean pairs like \"한다/안 한다\", \"강화한다/강화하지 않는다\\n" +
            "Each option should imply a different priority/trade-off (e.g., security vs rest, trade vs stealth).\n" +
            "Use the context blocks in USER message (World/Recent Expedition/Profile + Option Effect Hint).\n";
    }

    private string BuildUserPrompt(EventCardSO card, GameStateSO state)
    {
        var sb = new StringBuilder(700);

        sb.AppendLine("[Event Template]");
        sb.AppendLine($"- id: {card.id}");
        sb.AppendLine($"- titleSeed: {card.title}");
        sb.AppendLine();

        if (state != null)
        {
            sb.AppendLine("[World Parameter]");
            sb.AppendLine($"- guardAlert: {ConvertLevel(state.guardAlert)}");
            sb.AppendLine($"- merchantTrust: {ConvertLevel(state.merchantTrust)}");
            sb.AppendLine($"- enemyAggressive: {ConvertLevel(state.enemyAgressive)}");
            sb.AppendLine($"- shelterComfort: {ConvertLevel(state.shelterComfort)}");
            sb.AppendLine($"- radiation: {ConvertLevel(state.radiation)}");
            sb.AppendLine();

            sb.AppendLine("[Recent Expedition]");
            sb.AppendLine($"- endType: {state.lastExpedSnapShot.endType}");
            sb.AppendLine($"- farming={state.lastExpedSnapShot.farmingCount}, talk={state.lastExpedSnapShot.talkCount}");
            sb.AppendLine($"- optionalKill={state.lastExpedSnapShot.optionalKillCount}, avoid={state.lastExpedSnapShot.avoidCount}");
            sb.AppendLine($"- radiationExposure={state.lastExpedSnapShot.radiationCount}");
            sb.AppendLine();

            sb.AppendLine("[Player Profile Summary]");
            sb.AppendLine(ProfileTop2(state));
            sb.AppendLine();
        }

        sb.AppendLine("[Option Effect Hint]");
        sb.AppendLine("A: " + DescribeOptionEffect(card, 0));
        sb.AppendLine("B: " + DescribeOptionEffect(card, 1));

        return sb.ToString();
    }

    private static HubEventData ParseJson(string json)
    {
        string extracted = ExtractJsonObject(json);
        try
        {
            return JsonUtility.FromJson<HubEventData>(extracted) ?? MakeFallback("JSON parse null");
        }
        catch
        {
            return MakeFallback("JSON parse exception");
        }
    }

    public static HubEventData MakeFallback(string reason)
    {
        Debug.Log("[HubEventGenerator] Fallback reason: " + reason);
        return new HubEventData
        {
            eventId = "fallback",
            title = "예기치 못한 사건",
            description = $"방공호에 작은 소란이 일어났다. (fallback: {reason})",
            optionA = "지켜본다",
            optionB = "개입한다",
            tags = Array.Empty<string>()
        };
    }

    private static string ExtractJsonObject(string s)
    {
        if (string.IsNullOrEmpty(s)) return "{}";
        int a = s.IndexOf('{');
        int b = s.LastIndexOf('}');
        if (a >= 0 && b > a) return s.Substring(a, b - a + 1);
        return "{}";
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return (s.Length <= max) ? s : s.Substring(0, max);
    }

    private static string ConvertLevel(int value)
    {
        value = Mathf.Clamp(value, 0, 100);
        if (value <= 20) return "매우 낮음";
        if (value <= 40) return "낮음";
        if (value <= 60) return "보통";
        if (value <= 80) return "높음";
        return "매우 높음";
    }

    private static string ProfileTop2(GameStateSO state)
    {
        var profile = state.playerProfile;
        (string n, int value)[] top =
        {
            ("mercy", profile.mercy), ("greedy", profile.greedy), ("curious", profile.curious), ("discipline", profile.discipline),
            ("risk", profile.risk), ("social", profile.social), ("cruel", profile.cruel), ("caution", profile.caution),
        };

        Array.Sort(top, (a, b) => b.value.CompareTo(a.value));
        return $"- strongest: {top[0].n}({ConvertLevel(top[0].value)}), {top[1].n}({ConvertLevel(top[1].value)})";
    }

    private static string DescribeOptionEffect(EventCardSO card, int idx)
    {
        EventOption opt = null;

        try
        {
            opt = (idx == 0) ? card.optionA : card.optionB;
        }
        catch
        {
            return "unknown";
        }

        if (opt == null || opt.effects == null || opt.effects.Length == 0) return "no effect";

        var sb = new StringBuilder();
        for (int i = 0; i < opt.effects.Length; i++)
        {
            var e = opt.effects[i];
            string line = EffectToHint(e);
            if (!string.IsNullOrEmpty(line))
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(line);
            }
        }
        return sb.Length == 0 ? "no effect" : sb.ToString();
    }

    static string EffectToHint(EventEffect e)
    {
        bool plus = e.value >= 0;

        switch (e.type)
        {
            case EventEffectType.AddGuardAlert:
                return plus
                    ? "안전 우선(초소를 늘린다)"
                    : "휴식 우선(불을 끄고 쉰다)";

            case EventEffectType.AddShelterComfort:
                return plus
                    ? "회복 우선(정리를 시작한다)"
                    : "피로/불만(경계를 지속한다)";

            case EventEffectType.AddMerchantTrust:
                return plus
                    ? "교역/신뢰(상인과 거래한다)"
                    : "불신/단절(의심이 되어 피한다)";

            case EventEffectType.AddEnemyAgressive:
                return plus
                    ? "위협 증가(적을 도발한다)"
                    : "은폐/조용히(숨을 죽여 소리를 줄인다)";

            case EventEffectType.AddRadiation:
                return plus
                    ? "오염 노출(자원을 아끼기 위해 필터를 계속 쓴다)"
                    : "정화(필터를 교체하고 보강한다)";

            case EventEffectType.AddGold:
                return plus ? "물자 확보(획득/거래)" : "물자 소모(지불/희생)";

            default:
                return "";
        }
    }
}
