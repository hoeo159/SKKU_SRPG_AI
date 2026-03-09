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

        eventUI.Loading("捞亥飘 积己 吝ˇ");

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

        // LLM 积己
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

    void ChooseA() => ApplyAndClose(0);
    void ChooseB() => ApplyAndClose(1);

    void ApplyAndClose(int idx)
    {
        var state = GameManager.gameManager?.state;
        if (state == null || state.holdingEvent == null) return;

        Debug.Log($"Option {(idx == 0 ? "A" : "B")} chosen for event {state.holdingEvent.id}");
        EventOption opt = (idx == 0) ? state.holdingEvent.optionA : state.holdingEvent.optionB;
        EventApplier.ApplyOption(state, opt);

        state.holdingEvent = null;
        state.holdingEventJson = "";
        state.holdingEventId = "";

        if (eventUI != null) eventUI.Close();
    }
}
