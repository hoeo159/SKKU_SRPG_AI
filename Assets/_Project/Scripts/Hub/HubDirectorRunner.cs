using UnityEngine;
using System.Collections;

public class HubDirectorRunner : MonoBehaviour
{
    [SerializeField]
    private EventCardSO[] eventPool;

    [Header("Test Mode")]
    //[SerializeField] private bool useLLMEvent = true;

    [Header("UI / LLM")]
    [SerializeField] private HubEventUI eventUI;
    [SerializeField] private HubEventGenerator eventGenerator;
    [SerializeField] private bool autoOpenOnStart = true;
    [SerializeField] private bool forceRegnerateText = false;

    private void Awake()
    {
        if(eventUI == null) eventUI = FindFirstObjectByType<HubEventUI>(FindObjectsInactive.Include);
        if (eventGenerator == null) eventGenerator = FindFirstObjectByType<HubEventGenerator>();
    }

    void Start()
    {
        var state = GameManager.gameManager?.state;
        if (state == null) return;

        if (state.lastExpedSnapShot.expedId > 0 &&
            state.lastEventGeneratedExpedId < state.lastExpedSnapShot.expedId)
        {
            var picked = EventDirector.SelectEvent(state, eventPool);

            state.holdingEvent = picked.eventCard;
            state.holdingEventDebug = picked.debug;
            state.lastEventGeneratedExpedId = state.lastExpedSnapShot.expedId;

            state.holdingEventJson = "";
            state.holdingEventId = (state.holdingEvent != null) ? state.holdingEvent.id : "";
        }

        if (autoOpenOnStart)
            StartCoroutine(Co_OpenHoldingEvent());
    }

    IEnumerator Co_OpenHoldingEvent()
    {
        var state = GameManager.gameManager?.state;
        if (state == null || state.holdingEvent == null || eventUI == null) yield break;

        eventUI.Loading("사건 파일을 정리하는 중...");

        if (!forceRegnerateText &&
            !string.IsNullOrEmpty(state.holdingEventJson) &&
            state.holdingEventId == state.holdingEvent.id)
        { 
            HubEventData cached = JsonUtility.FromJson<HubEventData>(state.holdingEventJson);
            if (cached != null)
            {
                eventUI.Open(cached, ChooseA, ChooseB);
                yield break;
            }
        }

        // LLM ����
        if (eventGenerator == null)
        {
            eventUI.Open(HubEventGenerator.MakeFallback("No eventGenerator"), ChooseA, ChooseB);
            yield break;
        }

        bool done = false;
        HubEventData result = null;
        string err = null;

        yield return eventGenerator.Generate(state.holdingEvent, state,
            onOK: (r) => { result = r; done = true; },
            onError: (e) => { err = e; done = true; }
        );

        if (result == null)
            result = HubEventGenerator.MakeFallback(err ?? "Unknown error");

        state.holdingEventJson = JsonUtility.ToJson(result);
        state.holdingEventId = state.holdingEvent.id;

        eventUI.Open(result, ChooseA, ChooseB);
    }

    void ChooseA() => ApplyAndShowResult(0);
    void ChooseB() => ApplyAndShowResult(1);

    void ApplyAndShowResult(int idx)
    {
        var state = GameManager.gameManager?.state;
        if (state == null || state.holdingEvent == null) return;

        EventOption opt = (idx == 0) ? state.holdingEvent.optionA : state.holdingEvent.optionB;

        string resultText = BuildResultText(opt);
        EventApplier.ApplyOption(state, opt);

        state.holdingEvent = null;
        state.holdingEventJson = "";
        state.holdingEventId = "";

        if (eventUI != null) eventUI.ShowResult(resultText);
    }

    static string BuildResultText(EventOption opt)
    {
        if (opt == null || opt.effects == null || opt.effects.Length == 0)
            return "변화 없음";

        var sb = new System.Text.StringBuilder();
        foreach (var e in opt.effects)
        {
            string label = e.type switch
            {
                EventEffectType.AddGold            => "금화",
                EventEffectType.AddGuardAlert      => "경비 경계",
                EventEffectType.AddMerchantTrust   => "상인 신뢰도",
                EventEffectType.AddEnemyAgressive  => "적 공격성",
                EventEffectType.AddShelterComfort  => "대피소 편의",
                EventEffectType.AddRadiation       => "방사능",
                _                                  => e.type.ToString()
            };
            string sign = e.value >= 0 ? "+" : "";
            sb.AppendLine($"{label} {sign}{e.value}");
        }
        return sb.ToString().TrimEnd();
    }
}
