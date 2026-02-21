using UnityEngine;
using TMPro;

public class HubEventBoard : MonoBehaviour
{
    [Header("Head")]
    [SerializeField] private GameObject head;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text optionAText;
    [SerializeField] private TMP_Text optionBText;
    [SerializeField] private TMP_Text debugText;

    private EventCardSO curEventCard;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RefreshFromState();
    }

    public void RefreshFromState()
    {
        var state = GameManager.gameManager?.state;
        if(state == null || state.holdingEvent == null)
        {
            if(head != null) head.SetActive(false);
            return;
        }

        curEventCard = state.holdingEvent;

        if(head != null) head.SetActive(true);

        titleText.text      = curEventCard.title;
        descText.text       = curEventCard.description;
        optionAText.text    = curEventCard.optionA.optionText;
        optionBText.text    = curEventCard.optionB.optionText;

        if (debugText != null) debugText.text = state.holdingEventDebug;
    }

    public void OnClick_OptionA()
    {
        Apply(curEventCard.optionA);
    }

    public void OnClick_OptionB()
    {
        Apply(curEventCard.optionB);
    }

    void Apply(EventOption option)
    {
        var state = GameManager.gameManager?.state;
        if (state == null || curEventCard == null) return;

        EventApplier.ApplyOption(state, option);

        state.holdingEvent = null;
        state.holdingEventDebug = "";

        curEventCard = null;

        var hub = FindFirstObjectByType<HubUI>();
        if (hub != null) hub.Refresh();
        if (head != null) head.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
