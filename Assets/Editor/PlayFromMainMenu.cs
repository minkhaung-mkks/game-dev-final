// Assets/Editor/PlayFromMainMenu.cs
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayFromMainMenu
{
    const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
    const string OpenedKey = "PlayFromMainMenu.OpenedOnStartup";

    static PlayFromMainMenu()
    {
        EditorSceneManager.playModeStartScene =
            AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuPath);

        // Fallback if Unity starts on a blank Untitled scene (missing/stale Library).
        // Runs once per editor session so it never hijacks a scene opened later.
        EditorApplication.delayCall += () =>
        {
            if (SessionState.GetBool(OpenedKey, false)) return;
            SessionState.SetBool(OpenedKey, true);

            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path)) return;
            EditorSceneManager.OpenScene(MainMenuPath);
        };
    }
}
