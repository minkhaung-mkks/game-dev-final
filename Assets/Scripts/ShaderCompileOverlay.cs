using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
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
        scannedScenes.Clear();
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scannedScenes.Add(scene.path)) CompileLoadedSceneShaders();
        };
    }

    static readonly HashSet<string> scannedScenes = new HashSet<string>();

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

    // Passes forward rendering never draws (deferred, lightmap baking, motion
    // vectors, 2D). Compiling them would only add wait time.
    static readonly string[] UnusedLightModes =
        { "UniversalGBuffer", "Meta", "MotionVectors", "XRMotionVectors", "Universal2D" };
    static readonly ShaderTagId LightModeTag = new ShaderTagId("LightMode");

    static void Compile(IEnumerable<Material> materials)
    {
        // Queues async compile jobs; already-cached passes return immediately.
        foreach (Material mat in materials.Where(m => m != null && m.shader != null).Distinct())
            for (int pass = 0; pass < mat.passCount; pass++)
                if (!UnusedLightModes.Contains(mat.shader.FindPassTagValue(pass, LightModeTag).name))
                    UnityEditor.ShaderUtil.CompilePass(mat, pass);
    }

    GUIStyle style;
    float dots;
    EventSystem blockedEventSystem;

    // Compile jobs arrive in batches; wait a few idle frames before revealing
    // the scene so the overlay doesn't flicker off between batches.
    const int IdleFramesBeforeReveal = 10;
    // After the first reveal, only bring the overlay back for real compiles,
    // not the brief cache checks Unity does on scene reloads (e.g. Restart).
    const float CompileSecondsBeforeReshow = 0.3f;

    bool showing = true;
    int idleFrames;
    float compilingSeconds;
    float timeScaleBeforeOverlay = 1f;

    void Awake()
    {
        // Freeze the game from the very first frame while the overlay is up.
        timeScaleBeforeOverlay = Time.timeScale;
        Time.timeScale = 0f;
    }

    void Update()
    {
        bool compiling = UnityEditor.ShaderUtil.anythingCompiling;
        idleFrames = compiling ? 0 : idleFrames + 1;
        compilingSeconds = compiling ? compilingSeconds + Time.unscaledDeltaTime : 0f;

        if (showing && idleFrames >= IdleFramesBeforeReveal)
        {
            showing = false;
            Time.timeScale = timeScaleBeforeOverlay;
        }
        else if (!showing && compilingSeconds >= CompileSecondsBeforeReshow)
        {
            showing = true;
            timeScaleBeforeOverlay = Time.timeScale;
        }

        // Keep the game paused while the overlay is up (scene loads call
        // PauseManager.Resume, which would otherwise unfreeze it).
        if (showing) Time.timeScale = 0f;

        // Block menu clicks while the overlay is up.
        if (showing && blockedEventSystem == null && EventSystem.current != null)
        {
            blockedEventSystem = EventSystem.current;
            blockedEventSystem.enabled = false;
        }
        else if (!showing && blockedEventSystem != null)
        {
            blockedEventSystem.enabled = true;
            blockedEventSystem = null;
        }
    }

    void OnGUI()
    {
        if (!showing) return;

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
