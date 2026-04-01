using TMPro;
using UnityEngine;

public class ExpeditionReturnUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text   titleText;
    [SerializeField] private TMP_Text   statsText;
    [SerializeField] private ActionMenuUI actionMenu;

    // EndExped() 뒤에 호출
    public void ShowSummary(GameStateSO state, ExpedEndType endType)
    {
        GameManager.gameManager?.TriggerPreGenerate();
        actionMenu?.Close();

        if (titleText != null)
            titleText.text = endType == ExpedEndType.GoalReached ? "탐험 완료!" : "귀환";

        if (statsText != null)
        {
            var snap = state.lastExpedSnapShot;
            statsText.text =
                $"소요 시간: {snap.durationSecond:F1} seconds\n" +
                $"소요 턴: {snap.turn}\n" +
                $"채집: {snap.farmingCount}\n" +
                $"대화: {snap.talkCount}\n" +
                $"처치: {snap.optionalKillCount}\n" +
                $"회피: {snap.avoidCount}\n" +
                $"방사능 노출: {snap.radiationCount}";
        }

        if (panel != null) panel.SetActive(true);
        else gameObject.SetActive(true);
    }

    // 수동 귀환
    public void OnClick_ReturnFromExped()
    {
        var state = GameManager.gameManager?.state;
        if (state != null)
        {
            state.day += 1;
            var player = FindFirstObjectByType<ExpedPlayer>();
            var endCoord = (player != null) ? player.Coord : new Vector2Int(-1, -1);
            state.EndExped(endCoord, ExpedEndType.NormalReturn);
            ShowSummary(state, ExpedEndType.NormalReturn);
        }
    }

    // Summary UI 귀환
    public void OnClick_GoToHub()
    {
        GameManager.gameManager?.LoadScene("Hub");
    }
}
