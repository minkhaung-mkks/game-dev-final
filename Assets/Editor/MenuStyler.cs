using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// One-shot tool: Tools > Style Menus restyles the MainMenu and PauseMenu canvases
// in the open MainMenu scene. Everything is undoable (Ctrl/Cmd+Z), and nothing is
// saved until you save the scene.
public static class MenuStyler
{
    static readonly Color32 BgMain = new Color32(14, 17, 30, 255);
    static readonly Color32 BgPause = new Color32(0, 0, 0, 190);
    static readonly Color32 Card = new Color32(28, 33, 54, 235);
    static readonly Color32 TitleColor = new Color32(255, 204, 51, 255);

    static readonly Color32 Orange = new Color32(240, 110, 40, 255);
    static readonly Color32 Blue = new Color32(50, 150, 240, 255);
    static readonly Color32 Pink = new Color32(215, 75, 150, 255);
    static readonly Color32 Green = new Color32(60, 185, 100, 255);
    static readonly Color32 Slate = new Color32(70, 76, 98, 255);

    [MenuItem("Tools/Style Menus")]
    public static void StyleMenus()
    {
        Transform mainMenu = FindInScene("MainMenu");
        Transform pauseMenu = FindInScene("PauseMenu");

        if (mainMenu == null && pauseMenu == null)
        {
            EditorUtility.DisplayDialog("Style Menus", "Open the MainMenu scene first.", "OK");
            return;
        }

        Undo.SetCurrentGroupName("Style Menus");
        int group = Undo.GetCurrentGroup();

        if (mainMenu != null)
        {
            StyleCanvas(mainMenu);
            StyleBackground(mainMenu.Find("Bg"), BgMain);
            StyleCard(mainMenu.Find("Panel (1)"), new Vector2(0, -65), new Vector2(540, 430));
            StyleTitle(FindTitle(mainMenu), new Vector2(0, 250), 84);

            StyleButton(mainMenu.Find("CarGameBtn"), Orange, new Vector2(0, 80));
            StyleButton(mainMenu.Find("PlaneGameBtn"), Blue, new Vector2(0, -10));
            StyleButton(mainMenu.Find("SumoGameBtn"), Pink, new Vector2(0, -100));
            StyleButton(mainMenu.Find("ExitBtn"), Slate, new Vector2(0, -210));
        }

        if (pauseMenu != null)
        {
            StyleCanvas(pauseMenu);
            StyleBackground(pauseMenu.Find("Bg"), BgPause);
            StyleCard(pauseMenu.Find("Panel (1)"), new Vector2(0, -40), new Vector2(520, 340));
            StyleTitle(FindTitle(pauseMenu), new Vector2(0, 210), 72);

            StyleButton(pauseMenu.Find("ResumeBtn"), Green, new Vector2(0, 50));
            StyleButton(pauseMenu.Find("RestartBtn"), Blue, new Vector2(0, -40));
            StyleButton(pauseMenu.Find("BackToMainMenuBtn"), Slate, new Vector2(0, -130));
        }

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Menus styled. Save the scene to keep the changes, or Undo to revert.");
    }

    static Transform FindInScene(string name)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name && t.GetComponent<Canvas>() != null)
                    return t;
            }
        }
        return null;
    }

    // The title is the TMP text sitting directly on the canvas (button labels are nested deeper)
    static TextMeshProUGUI FindTitle(Transform canvas)
    {
        foreach (Transform child in canvas)
        {
            var text = child.GetComponent<TextMeshProUGUI>();
            if (text != null)
                return text;
        }
        return null;
    }

    static void StyleCanvas(Transform canvas)
    {
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            return;

        // Scale UI with the window instead of fixed pixel size
        Undo.RecordObject(scaler, "Style Menus");
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    static void StyleBackground(Transform bg, Color color)
    {
        if (bg == null)
            return;

        var image = bg.GetComponent<Image>();
        Undo.RecordObject(image, "Style Menus");
        image.color = color;
    }

    static void StyleCard(Transform card, Vector2 position, Vector2 size)
    {
        if (card == null)
            return;

        var rect = (RectTransform)card;
        Undo.RecordObject(rect, "Style Menus");
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var image = card.GetComponent<Image>();
        Undo.RecordObject(image, "Style Menus");
        image.sprite = RoundedSprite();
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 0.4f; // bigger corner radius
        image.color = Card;
    }

    static void StyleTitle(TextMeshProUGUI title, Vector2 position, float fontSize)
    {
        if (title == null)
            return;

        var rect = title.rectTransform;
        Undo.RecordObject(rect, "Style Menus");
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(1400, 140);

        Undo.RecordObject(title, "Style Menus");
        title.enableAutoSizing = false;
        title.fontSize = fontSize;
        title.fontStyle = FontStyles.Bold;
        title.alignment = TextAlignmentOptions.Center;
        title.color = TitleColor;
        title.characterSpacing = 4;

        var shadowMat = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Drop Shadow.mat");
        if (shadowMat != null)
            title.fontSharedMaterial = shadowMat;
    }

    static void StyleButton(Transform button, Color color, Vector2 position)
    {
        if (button == null)
            return;

        var rect = (RectTransform)button;
        Undo.RecordObject(rect, "Style Menus");
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(420, 72);

        var image = button.GetComponent<Image>();
        Undo.RecordObject(image, "Style Menus");
        image.sprite = RoundedSprite();
        image.type = Image.Type.Sliced;
        image.pixelsPerUnitMultiplier = 0.5f;
        image.color = color;

        // Tint multiplies the base color: slightly dim at rest, full brightness on hover
        var btn = button.GetComponent<Button>();
        Undo.RecordObject(btn, "Style Menus");
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        colors.highlightedColor = Color.white;
        colors.selectedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.fadeDuration = 0.08f;
        btn.colors = colors;

        if (button.GetComponent<Shadow>() == null)
        {
            var shadow = Undo.AddComponent<Shadow>(button.gameObject);
            shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(0f, -5f);
        }

        if (button.GetComponent<MenuButtonFx>() == null)
            Undo.AddComponent<MenuButtonFx>(button.gameObject);

        var label = button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            Undo.RecordObject(label, "Style Menus");
            label.enableAutoSizing = false;
            label.fontSize = 30;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.characterSpacing = 2;
        }
    }

    static Sprite RoundedSprite()
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }
}
