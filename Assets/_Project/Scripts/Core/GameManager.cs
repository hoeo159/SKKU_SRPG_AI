using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager gameManager { get; private set; }
    [SerializeField] private GameStateSO gameStateSO;

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
}
