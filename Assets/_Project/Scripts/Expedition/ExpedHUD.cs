using UnityEngine;
using TMPro;

public class ExpedHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private ExpedPlayer player;

    void Awake()
    {
        if(text == null)    text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        var gm = GameManager.gameManager;

        if (gm == null || gm.state == null) return;
        if(player == null) player = FindFirstObjectByType<ExpedPlayer>();

        var state = gm.state;

        string pos = (player != null) ? $"({player.Coord.x},{player.Coord.y})" : "(?)";

        text.text =
            $"[Expedition HUD]\n" +
            $"Pos: {pos}\n" +
            $"Turns: {state.expeditionTurn}  Moves: {state.expeditionMoveCount}\n" +
            $"Farming: {state.farmingCount}  Talk: {state.talkCount}\n";
    }
}
