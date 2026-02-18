using UnityEngine;

public class ExpeditionReturnUI : MonoBehaviour
{
    public void OnClick_GoToHub()
    {
        var state = GameManager.gameManager?.state;
        if(state != null)
        {
            state.day += 1;
            var player = FindFirstObjectByType<ExpedPlayer>();
            var endCoord = (player != null) ? player.Coord : new Vector2Int(-1, -1);

            state.EndExped(endCoord, ExpedEndType.NormalReturn);
        }

        GameManager.gameManager.LoadScene("Hub");
    }
}
