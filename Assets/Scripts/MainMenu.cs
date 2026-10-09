using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayDrivingGame()
    {
        SceneManager.LoadScene("CarDriver");
    }

    public void PlayFlyingGame()
    {
        SceneManager.LoadScene("PlaneDriver");
    }

    public void PlaySumoGame()
    {
        SceneManager.LoadScene("SumoBalls");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
