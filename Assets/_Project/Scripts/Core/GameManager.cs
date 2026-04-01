using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager { get; private set; }
    [SerializeField] private GameStateSO gameStateSO;

    [Header("Pre Generation")]
    [SerializeField] private EventCardSO[] eventPool;
    [SerializeField] private HubEventGenerator preGenEventGenerator;

    public GameStateSO state { get; private set; }

    private void Awake()
    {
        if(gameManager != null)
        {
            Destroy(gameObject);
            return;
        }

        gameManager = this;
        DontDestroyOnLoad(gameObject);

        if (preGenEventGenerator == null)
            preGenEventGenerator = GetComponent<HubEventGenerator>();

        ResetGameStateSO();
    }

    public void ResetGameStateSO()
    {
        if(gameStateSO == null)
        {
            Debug.LogError("[GameManager] GameState not connected");
            return;
        }
        state = Instantiate(gameStateSO);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 탐험 종료 시 호출 event background 호출
    public void TriggerPreGenerate()
    {
        if (state == null || eventPool == null || eventPool.Length == 0) return;
        if (preGenEventGenerator == null)
        {
            Debug.LogWarning("[GameManager] preGenEventGenerator is not assigned.");
            return;
        }

        var picked = EventDirector.SelectEvent(state, eventPool);
        state.holdingEvent      = picked.eventCard;
        state.holdingEventDebug = picked.debug;
        state.lastEventGeneratedExpedId = state.lastExpedSnapShot.expedId;
        state.holdingEventJson  = "";
        state.holdingEventId    = (state.holdingEvent != null) ? state.holdingEvent.id : "";

        if (state.holdingEvent != null)
            StartCoroutine(Co_PreGenerate(state, state.holdingEvent));
    }

    private IEnumerator Co_PreGenerate(GameStateSO s, EventCardSO card)
    {
        yield return preGenEventGenerator.Generate(card, s,
            onOK: (data) =>
            {
                s.holdingEventJson = JsonUtility.ToJson(data);
                s.holdingEventId   = card.id;
                Debug.Log("[GameManager] Pre-generation complete: " + card.id);
            },
            onError: (e) =>
            {
                Debug.LogWarning("[GameManager] Pre-generation failed: " + e);
            }
        );
    }
}
