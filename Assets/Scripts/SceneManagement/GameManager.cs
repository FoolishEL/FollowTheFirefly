using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool isPlaying = false;
    public bool hasCompas = false;
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private string gameSceneName = "Game";

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public void StartGame()
    {
        if(SceneManager.GetActiveScene().name == gameSceneName)
            return;
        isPlaying = true;
        hasCompas = false;
        SceneManager.LoadScene(gameSceneName);
    }

    public void ToMainMenu()
    {
        if(SceneManager.GetActiveScene().name == menuSceneName)
            return;
        isPlaying = false;
        SceneManager.LoadScene(menuSceneName);
    }
}
