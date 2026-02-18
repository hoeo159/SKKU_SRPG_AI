using UnityEngine;
using TMPro;

public class HubUI : MonoBehaviour
{
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text goldText;

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

        Debug.Log(GameManager.gameManager.state.lastExpedReport);
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
