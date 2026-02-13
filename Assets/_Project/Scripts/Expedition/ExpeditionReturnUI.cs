using UnityEngine;

public class ExpeditionReturnUI : MonoBehaviour
{
    public void OnClick_GoToHub()
    {
        GameManager.gameManager.state.day += 1;
        GameManager.gameManager.LoadScene("Hub");
    }
}
