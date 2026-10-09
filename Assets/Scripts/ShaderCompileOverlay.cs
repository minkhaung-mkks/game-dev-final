using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Editor-only: on a fresh clone Unity has no shader cache and draws cyan
// placeholders while shaders compile in the background. On Play this queues
// compilation for every material in the build scenes (MainMenu + all 3 games)
// and covers the Game view with "Compiling shaders..." until it's done. Clicks are blocked meanwhile.
// Builds precompile shaders, so this never shows (or exists) in a build.
public class ShaderCompileOverlay : MonoBehaviour
{
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        var go = new GameObject("ShaderCompileOverlay");
        DontDestroyOnLoad(go);
        go.AddComponent<ShaderCompileOverlay>();
        CompileBuildSceneShaders();
        SceneManager.sceneLoaded += (scene, mode) => CompileLoadedSceneShaders();
    }

    static readonly string[] MaterialHolders = { ".mat", ".asset", ".fbx", ".obj", ".blend" };

    static void CompileBuildSceneShaders()
    {
        string[] scenes = UnityEditor.EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        // Materials can live inside other assets too (TMP font assets, models).
        var materials = UnityEditor.AssetDatabase.GetDependencies(scenes, true)
            .Where(p => MaterialHolders.Any(ext => p.EndsWith(ext)))
            .SelectMany(UnityEditor.AssetDatabase.LoadAllAssetsAtPath)
            .OfType<Material>()
            .Append(Canvas.GetDefaultCanvasMaterial())
            .Append(Graphic.defaultGraphicMaterial);

        Compile(materials);
    }

    // Catches what asset scanning can't: built-in skybox and UI materials,
    // and runtime material instances (e.g. TMP text) of the scene just loaded.
    static void CompileLoadedSceneShaders()
    {
        var materials = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .SelectMany(r => r.sharedMaterials)
            .Concat(FindObjectsByType<Graphic>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Select(g => g.materialForRendering))
            .Append(RenderSettings.skybox);

        Compile(materials);
    }

    static void Compile(IEnumerable<Material> materials)
    {
        // Queues async compile jobs; already-cached passes return immediately.
        foreach (Material mat in materials.Where(m => m != null && m.shader != null).Distinct())
            for (int pass = 0; pass < mat.passCount; pass++)
                UnityEditor.ShaderUtil.CompilePass(mat, pass);
    }

    GUIStyle style;
    float dots;
    EventSystem blockedEventSystem;

    // Compile jobs arrive in batches; wait a few idle frames before revealing
    // the scene so the overlay doesn't flicker off between batches.
    const int IdleFramesBeforeReveal = 10;
    int idleFrames;
    bool Showing => idleFrames < IdleFramesBeforeReveal;

    void Update()
    {
        idleFrames = UnityEditor.ShaderUtil.anythingCompiling ? 0 : idleFrames + 1;

        // Block menu clicks while the overlay is up.
        if (Showing && blockedEventSystem == null && EventSystem.current != null)
        {
            blockedEventSystem = EventSystem.current;
            blockedEventSystem.enabled = false;
        }
        else if (!Showing && blockedEventSystem != null)
        {
            blockedEventSystem.enabled = true;
            blockedEventSystem = null;
        }
    }

    void OnGUI()
    {
        if (!Showing) return;

        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                richText = true,
            };
            style.normal.textColor = Color.white;
        }

        GUI.depth = -1000;
        GUI.color = new Color32(14, 17, 30, 255);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        dots = (dots + Time.unscaledDeltaTime * 2f) % 4f;
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height),
            "Compiling shaders" + new string('.', (int)dots) +
            "\n<size=18>The game will start once shaders are ready</size>" +
            "\n<size=14>This only happens on first launch. After that, the game starts instantly.</size>", style);
    }
#endif
}
