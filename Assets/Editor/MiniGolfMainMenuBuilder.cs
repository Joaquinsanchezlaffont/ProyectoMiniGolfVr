using System.Collections.Generic;
using System.IO;
using MiniGolfVR;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Builds the second editable Unity scene from the menu artwork supplied by the user.
internal static class MiniGolfMainMenuBuilder
{
    internal const string ScenePath = "Assets/Scenes/MenuPrincipal.unity";
    private const string IntroPath = "Assets/Scenes/Inicio.unity";
    private const string ArtworkPath = "Assets/Sprites/Menu.png";
    private const string ButtonSpritePath = "Assets/Sprites/BotonNegro.png";

    [MenuItem("Minigolf VR/Abrir menú principal")]
    private static void OpenFromMenu()
    {
        if (!MiniGolfStartMenuBuilder.EnsureCreated() || !EnsureCreated())
        {
            Debug.LogError("Minigolf VR: primero esperá a que se creen Inicio y el menú principal.");
            return;
        }
        if (SceneManager.GetActiveScene().path == ScenePath) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(ScenePath);
    }

    internal static bool EnsureCreated()
    {
        if (File.Exists(ScenePath))
        {
            AddToBuildSettings();
            return true;
        }
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(IntroPath))
            return false;

        Sprite artwork = LoadSprite(ArtworkPath, false);
        Sprite buttonSprite = LoadSprite(ButtonSpritePath, true);
        if (artwork == null || buttonSprite == null)
        {
            Debug.LogError("Minigolf VR: faltan Menu.png o BotonNegro.png en Assets/Sprites.");
            return false;
        }

        Scene previous = SceneManager.GetActiveScene();
        if (!AssetDatabase.CopyAsset(IntroPath, ScenePath))
        {
            Debug.LogError("Minigolf VR: no se pudo crear MenuPrincipal.unity.");
            return false;
        }

        Scene menu = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(menu);
        GameObject rig = null;
        foreach (GameObject root in menu.GetRootGameObjects())
            if (root.GetComponentInChildren<Unity.XR.CoreUtils.XROrigin>(true) != null)
            {
                rig = root;
                break;
            }

        Camera camera = rig != null ? rig.GetComponentInChildren<Camera>(true) : null;
        if (camera == null)
        {
            Debug.LogError("Minigolf VR: Inicio no tiene una cámara VR; no se creó el menú.");
            EditorSceneManager.CloseScene(menu, true);
            AssetDatabase.DeleteAsset(ScenePath);
            return false;
        }

        // This scene has its own copy of the rig. The game and Inicio keep their hands.
        foreach (AnimateHandOnInput hand in rig.GetComponentsInChildren<AnimateHandOnInput>(true))
            hand.gameObject.SetActive(false);

        foreach (Canvas oldCanvas in rig.GetComponentsInChildren<Canvas>(true))
            Object.DestroyImmediate(oldCanvas.gameObject);

        CreateCanvas(camera, artwork, buttonSprite);
        EditorSceneManager.MarkSceneDirty(menu);
        EditorSceneManager.SaveScene(menu);
        SceneManager.SetActiveScene(previous);
        EditorSceneManager.CloseScene(menu, true);
        AddToBuildSettings();
        Debug.Log("Minigolf VR: menú principal creado en Assets/Scenes/MenuPrincipal.unity.");
        return true;
    }

    private static Sprite LoadSprite(string path, bool sliced)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return null;
        Vector4 border = sliced ? new Vector4(32f, 32f, 32f, 32f) : Vector4.zero;
        if (importer.textureType != TextureImporterType.Sprite ||
            importer.spriteImportMode != SpriteImportMode.Single ||
            (sliced && importer.spriteBorder != border))
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            if (sliced) importer.spriteBorder = border;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void CreateCanvas(Camera camera, Sprite artwork, Sprite buttonSprite)
    {
        GameObject canvasObject = new GameObject("Menu principal - imagen y botones",
            typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(TrackedDeviceGraphicRaycaster));
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.SetParent(camera.transform, false);
        canvasRect.localPosition = new Vector3(0f, 0f, 2f);
        canvasRect.localRotation = Quaternion.identity;
        canvasRect.localScale = Vector3.one * 0.0015f;
        canvasRect.sizeDelta = new Vector2(1920f, 1080f);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        canvas.sortingOrder = 10;

        GameObject backgroundObject = new GameObject("Imagen Menu (1920 x 1080)",
            typeof(RectTransform), typeof(Image));
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.SetParent(canvasRect, false);
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        Image background = backgroundObject.GetComponent<Image>();
        background.sprite = artwork;
        background.raycastTarget = false;

        MenuPrincipalController controller = canvasObject.AddComponent<MenuPrincipalController>();
        AddButton(canvasRect, buttonSprite, "JUGAR SOLO", new Vector2(760f, 116f), 160f,
            controller.JugarSolo);
        AddButton(canvasRect, buttonSprite, "JUGAR CON AMIGOS", new Vector2(1040f, 116f), 0f,
            controller.JugarConAmigos);
        AddButton(canvasRect, buttonSprite, "VOLVER", new Vector2(430f, 116f), -160f,
            controller.Volver);
    }

    private static void AddButton(RectTransform parent, Sprite sprite, string caption,
        Vector2 size, float y, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(caption + " - botón VR",
            typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = new Vector2(0f, y);

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = Color.white;
        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.77f, 0.99f, 0.66f);
        colors.pressedColor = new Color(0.47f, 0.81f, 0.38f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        GameObject labelObject = new GameObject("Texto " + caption,
            typeof(RectTransform), typeof(Text));
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(rect, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        Text label = labelObject.GetComponent<Text>();
        label.text = caption;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 72;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;

        UnityEventTools.AddPersistentListener(button.onClick, action);
    }

    private static void AddToBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            if (scene.path != ScenePath) scenes.Add(scene);
        int introIndex = scenes.FindIndex(scene => scene.path == IntroPath);
        scenes.Insert(introIndex < 0 ? 0 : introIndex + 1,
            new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
