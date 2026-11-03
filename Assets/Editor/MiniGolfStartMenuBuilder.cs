using System.Collections.Generic;
using System.IO;
using MiniGolfVR;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

// Creates the start screen once. The generated scene stays editable in Unity.
internal static class MiniGolfStartMenuBuilder
{
    internal const string ScenePath = "Assets/Scenes/Inicio.unity";
    private const string GolfScenePath = "Assets/Scenes/MinigolfVR.unity";
    private const string ArtworkPath = "Assets/Sprites/Inicio.png";
    private const string StarterAssets = "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets";

    [MenuItem("Minigolf VR/Abrir pantalla de inicio")]
    private static void OpenStartScreen()
    {
        if (!EnsureCreated())
        {
            Debug.LogError("Minigolf VR: esperá a que termine de importar Starter Assets y elegí Minigolf VR > Crear escena inicial.");
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
            PutFirstInBuildSettings();
            return true;
        }

        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(GolfScenePath))
            return false;

        Sprite artwork = LoadArtwork();
        if (artwork == null)
        {
            Debug.LogError("Minigolf VR: falta la imagen Assets/Sprites/Inicio.png.");
            return false;
        }

        Scene previous = SceneManager.GetActiveScene();
        // Copy the working VR rig and its blue hands, then remove the golf course.
        // This retains the same Starter Assets controller bindings as the game.
        if (!AssetDatabase.CopyAsset(GolfScenePath, ScenePath))
        {
            Debug.LogError("Minigolf VR: no se pudo crear Assets/Scenes/Inicio.unity.");
            return false;
        }

        Scene intro = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        SceneManager.SetActiveScene(intro);

        GameObject rig = null;
        GameObject interactionManager = null;
        GameObject light = null;
        foreach (GameObject root in intro.GetRootGameObjects())
        {
            if (rig == null && root.GetComponentInChildren<Unity.XR.CoreUtils.XROrigin>(true) != null)
                rig = root;
            else if (interactionManager == null && root.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRInteractionManager>() != null)
                interactionManager = root;
            else if (light == null && root.GetComponent<Light>() != null)
                light = root;
            else
                Object.DestroyImmediate(root);
        }

        if (rig == null)
        {
            Debug.LogError("Minigolf VR: no se encontró XR Origin en la escena de juego.");
            EditorSceneManager.CloseScene(intro, true);
            AssetDatabase.DeleteAsset(ScenePath);
            return false;
        }

        MiniGolfRig gameRig = rig.GetComponent<MiniGolfRig>();
        if (gameRig != null) Object.DestroyImmediate(gameRig);
        rig.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        rig.name = "Jugador VR - manos y controles";

        Camera camera = rig.GetComponentInChildren<Camera>(true);
        if (camera == null)
        {
            Debug.LogError("Minigolf VR: falta la cámara VR en XR Origin.");
            EditorSceneManager.CloseScene(intro, true);
            AssetDatabase.DeleteAsset(ScenePath);
            return false;
        }

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.60f, 0.89f, 0.49f);
        foreach (XRRayInteractor ray in rig.GetComponentsInChildren<XRRayInteractor>(true))
            ray.enableUIInteraction = true;
        foreach (NearFarInteractor nearFar in rig.GetComponentsInChildren<NearFarInteractor>(true))
            nearFar.enableUIInteraction = true;

        if (interactionManager == null)
            interactionManager = new GameObject("XR Interaction Manager", typeof(UnityEngine.XR.Interaction.Toolkit.XRInteractionManager));
        if (light == null)
        {
            light = new GameObject("Luz para las manos", typeof(Light));
            Light sun = light.GetComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(48f, -25f, 0f);
        }

        CreateVrCanvas(camera, artwork);
        CreateEventSystem();

        EditorSceneManager.MarkSceneDirty(intro);
        EditorSceneManager.SaveScene(intro);
        PutFirstInBuildSettings();
        SceneManager.SetActiveScene(previous);
        EditorSceneManager.CloseScene(intro, true);

        // On first import, display the start screen instead of the golf scene.
        if (previous.path == GolfScenePath && !previous.isDirty && SceneManager.sceneCount == 1)
            EditorSceneManager.OpenScene(ScenePath);

        Debug.Log("Minigolf VR: pantalla Inicio con botón JUGAR creada en Assets/Scenes.");
        return true;
    }

    private static Sprite LoadArtwork()
    {
        TextureImporter importer = AssetImporter.GetAtPath(ArtworkPath) as TextureImporter;
        if (importer == null) return null;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(ArtworkPath);
    }

    private static void CreateVrCanvas(Camera camera, Sprite artwork)
    {
        GameObject canvasObject = new GameObject("Inicio - imagen y boton JUGAR",
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

        GameObject backgroundObject = new GameObject("Imagen Inicio (1920 x 1080)", typeof(RectTransform), typeof(Image));
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.SetParent(canvasRect, false);
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        Image background = backgroundObject.GetComponent<Image>();
        background.sprite = artwork;
        background.raycastTarget = false;

        GameObject buttonObject = new GameObject("JUGAR - boton VR", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(canvasRect, false);
        buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = new Vector2(470f, 110f);
        buttonRect.anchoredPosition = new Vector2(0f, -285f);
        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.08f, 0.08f, 0.08f, 1f);
        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.85f, 1f, 0.78f);
        colors.pressedColor = new Color(0.55f, 0.88f, 0.43f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        GameObject labelObject = new GameObject("Texto JUGAR", typeof(RectTransform), typeof(Text));
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.SetParent(buttonRect, false);
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
        Text label = labelObject.GetComponent<Text>();
        label.text = "JUGAR";
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 64;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;

        InicioMenuController controller = canvasObject.AddComponent<InicioMenuController>();
        UnityEventTools.AddPersistentListener(button.onClick, controller.Jugar);
    }

    private static void CreateEventSystem()
    {
        GameObject system = new GameObject("EventSystem - VR", typeof(EventSystem), typeof(XRUIInputModule));
        XRUIInputModule module = system.GetComponent<XRUIInputModule>();
        // Official Starter Assets preset wires up the Input System UI actions.
        foreach (string guid in AssetDatabase.FindAssets("XRI Default XR UI Input Module t:Preset", new[] { StarterAssets }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(path) != "XRI Default XR UI Input Module.preset") continue;
            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(path);
            if (preset != null) preset.ApplyTo(module);
            break;
        }

        module.enableXRInput = true;
        module.enableBuiltinActionsAsFallback = true;
    }

    private static void PutFirstInBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>();
        scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            if (scene.path != ScenePath) scenes.Add(scene);
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
