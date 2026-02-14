using UnityEngine;

public class RuntimeBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject gameManagerPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if(GameManager.gameManager == null)
        {
            if(gameManagerPrefab == null)
            {
                Debug.LogError("[RuntimeBootstrap] GameManager prefab is not connected");
                return;
            }
            Instantiate(gameManagerPrefab);
        }
    }
}
