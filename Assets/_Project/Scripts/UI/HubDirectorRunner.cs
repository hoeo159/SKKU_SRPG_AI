using UnityEngine;

public class HubDirectorRunner : MonoBehaviour
{
    [SerializeField]
    private EventCardSO[] eventPool;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var state = GameManager.gameManager?.state;
        if (state != null) return;
        if (state.lastExpedSnapShot.expedId <= 0) return;

        var eventCard = EventDirector.SelectEvent(state, eventPool);

        state.holdingEvent              = eventCard.eventCard;
        state.holdingEventDebug         = eventCard.debug;
        state.lastEventGeneratedExpedId = state.lastExpedSnapShot.expedId;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
