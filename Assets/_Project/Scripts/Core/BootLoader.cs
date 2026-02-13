using UnityEngine;

public class BootLoader : MonoBehaviour
{
    [SerializeField] private string initSceneName = "Hub";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.gameManager.LoadScene(initSceneName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
