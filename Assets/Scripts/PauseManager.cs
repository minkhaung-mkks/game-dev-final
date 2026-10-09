using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    private void Awake()
    {
        // Prevent duplicate GlobalUI objects
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Keep UI alive when switching scenes
        DontDestroyOnLoad(gameObject);

        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;

        pausePanel.SetActive(false);
    }

    private void Update()
    {
        // Disable pause functionality in Main Menu
        if (SceneManager.GetActiveScene().name == "MainMenu")
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
            return;

        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        Resume();

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void BackToMainMenu()
    {
        Resume();
        SceneManager.LoadScene("MainMenu");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Resume();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }
}