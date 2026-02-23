using UnityEngine;
using TMPro;

public class HubUI : MonoBehaviour
{
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text expedReportText;
    [SerializeField] private TMP_Text profileReprotText;
    [SerializeField] private TMP_Text worldParamText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        var state = GameManager.gameManager.state;
        dayText.text = $"Day {state.day}";
        goldText.text = $"Gold : {state.gold}";
        expedReportText.text = GameManager.gameManager.state?.lastExpedReport;
        profileReprotText.text = GameManager.gameManager.state?.lastProfileReport;
        worldParamText.text = $"[World]\n" +
            $"GuardAlert: {state.guardAlert}\n" +
            $"MerchantTrust: {state.merchantTrust}\n" +
            $"EnemyAggressive: {state.enemyAgressive}\n" +
            $"ShelterComfort: {state.shelterComfort}\n" +
            $"Radiation: {state.radiation}\n";

        Debug.Log($"holdingEvent={(state.holdingEvent != null ? state.holdingEvent.id : "null")}, " +
            $"lastEventGeneratedExpedId={state.lastEventGeneratedExpedId}, lastExpedId={state.lastExpedSnapShot.expedId}");
    }

    public void OnClick_GoToExpedition()
    {
        //GameManager.gameManager.state.ClearExpeditionState();
        GameManager.gameManager.LoadScene("Expedition");
    }

    public void OnClick_ResetRun()
    {
        GameManager.gameManager.ResetGameStateSO();
        Refresh();
    }
}
