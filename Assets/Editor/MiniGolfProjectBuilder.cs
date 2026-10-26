using System.IO;
using MiniGolfVR;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using Unity.XR.CoreUtils;

// On the first import, builds editable .unity, .prefab and .mat assets INSIDE Assets.
// The source project remains small, while the scene is immediately visible in Unity.
[InitializeOnLoad]
internal static class MiniGolfProjectBuilder
{
    private const string ScenePath = "Assets/Scenes/MinigolfVR.unity";
    private const string Materials = "Assets/Materials/";
    private const string Models = "Assets/Models/";
    private const string HandPrefabs = Models + "Animated Hands/Prefabs/";
    private const string XriVersion = "3.5.1";

    private static Material grass;
    private static Material rails;
    private static Material white;
    private static Material dark;
    private static PhysicsMaterial rolling;

    static MiniGolfProjectBuilder() => EditorApplication.delayCall += BuildOnFirstImport;

    private static void BuildOnFirstImport()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += BuildOnFirstImport;
            return;
        }

        if (!File.Exists(ScenePath)) CreateScene();
        else OpenIfBlank();
        InstallSimpleSceneIfNeeded();
    }

    [MenuItem("Minigolf VR/Crear escena inicial")]
    private static void CreateFromMenu()
    {
        if (File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath);
        else CreateScene();
        InstallSimpleSceneIfNeeded();
    }

    [MenuItem("Minigolf VR/Abrir escena inicial")]
    private static void OpenFromMenu()
    {
        if (!File.Exists(ScenePath)) CreateScene();
        else EditorSceneManager.OpenScene(ScenePath);
        InstallSimpleSceneIfNeeded();
    }

    private static void CreateScene()
    {
        if (File.Exists(ScenePath)) return;
        GameObject starterRig = FindStarterRig();
        if (starterRig == null) return;
        EnsureFolders();
        Scene previous = SceneManager.GetActiveScene();
        bool untitled = string.IsNullOrEmpty(previous.path);
        bool canSwitch = !previous.isDirty &&
            (untitled || previous.path.EndsWith("/SampleScene.unity"));
        if (untitled && (!canSwitch || SceneManager.sceneCount != 1))
        {
            Debug.LogWarning("Guardá la escena Untitled y volvé a Minigolf VR > Crear escena inicial.");
            return;
        }
        // Unity 6.5 cannot open a scene additively while an untitled scene is open.
        // Replace only the untouched startup scene; preserve all saved scenes.
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
            untitled ? NewSceneMode.Single : NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        CreateMaterials();
        CreatePrefabs(true);

        GameObject gameObject = new GameObject("Prueba - un hoyo");
        MiniGolfGame game = gameObject.AddComponent<MiniGolfGame>();
        HoleCup cup;
        Transform tee;
        MakeHole(game, out tee, out cup);

        GameObject ball = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(Models + "Pelota.prefab"));
        ball.name = "Pelota - fisica";

        GameObject rigObject = (GameObject)PrefabUtility.InstantiatePrefab(starterRig);
        rigObject.name = "Jugador - XR Origin (VR) - Starter Assets";
        MiniGolfRig rig = rigObject.AddComponent<MiniGolfRig>();
        Camera camera = rigObject.GetComponentInChildren<Camera>(true);
        if (camera == null) throw new System.InvalidOperationException("Starter Assets: falta la cámara del XR Origin.");
        GameObject cameraObject = camera.gameObject;
        camera.nearClipPlane = 0.02f;
        rig.Configure(cameraObject.transform);
        AddHandVisuals(rigObject.transform);
        new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();

        GameObject club = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(Models + "Palo.prefab"));
        club.name = "Palo - agarrar con grip VR";
        club.transform.position = new Vector3(0.55f, 0.95f, -2.7f);
        Transform clubHead = club.transform.Find("Cabeza del palo");
        club.GetComponent<GolfClub>().Configure(game, clubHead,
            clubHead.GetComponent<BoxCollider>());

        game.Configure(tee, cup, ball.GetComponent<GolfBall>());
        rig.SetStation(tee.position);

        GameObject lightObject = new GameObject("Sol");
        Light sunlight = lightObject.AddComponent<Light>();
        sunlight.type = LightType.Directional;
        sunlight.intensity = 1.3f;
        lightObject.transform.rotation = Quaternion.Euler(48f, -25f, 0f);
        RenderSettings.ambientLight = new Color(0.5f, 0.55f, 0.6f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings();
        if (!untitled)
        {
            SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene, true);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!untitled && canSwitch) EditorSceneManager.OpenScene(ScenePath);
        SceneView view = SceneView.lastActiveSceneView;
        if (view != null)
        {
            view.pivot = Vector3.zero;
            view.rotation = Quaternion.Euler(35f, -15f, 0f);
            view.size = 9f;
            view.Repaint();
        }
        Debug.Log("Minigolf VR: escena, modelos y materiales creados en Assets.");
    }

    private static void OpenIfBlank()
    {
        Scene active = SceneManager.GetActiveScene();
        if (!active.isDirty &&
            (string.IsNullOrEmpty(active.path) || active.path.EndsWith("/SampleScene.unity")))
            EditorSceneManager.OpenScene(ScenePath);
    }

    private static void EnsureFolders()
    {
        foreach (string folder in new[] { "Scripts", "Models", "Materials", "Scenes", "Editor" })
            if (!AssetDatabase.IsValidFolder("Assets/" + folder))
                AssetDatabase.CreateFolder("Assets", folder);
    }

    private static GameObject FindStarterRig()
    {
        foreach (Sample sample in Sample.FindByPackage("com.unity.xr.interaction.toolkit", XriVersion))
        {
            if (sample.displayName != "Starter Assets") continue;
            if (!sample.isImported)
            {
                if (!sample.Import(Sample.ImportOptions.None))
                    Debug.LogError("No se pudieron importar Starter Assets. Importalos desde Package Manager > XR Interaction Toolkit > Samples.");
                else
                    EditorApplication.delayCall += BuildOnFirstImport;
                return null;
            }
            break;
        }

        string root = "Assets/Samples/XR Interaction Toolkit/" + XriVersion + "/Starter Assets";
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { root }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == "XR Origin (XR Rig)")
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        Debug.LogError("Falta el prefab XR Origin (XR Rig). Importá Starter Assets desde Package Manager > XR Interaction Toolkit > Samples y volvé a Minigolf VR > Crear escena inicial.");
        return null;
    }

    private static void CreateMaterials()
    {
        grass = MakeMaterial("Piso gris", new Color(0.48f, 0.50f, 0.52f));
        rails = MakeMaterial("Bordes grises", new Color(0.30f, 0.32f, 0.34f));
        white = MakeMaterial("Pelota y lineas blancas", new Color(0.97f, 0.96f, 0.91f));
        dark = MakeMaterial("Interior del hoyo", new Color(0.025f, 0.04f, 0.055f));
        // The video package ships a blue URP material. Use the current project's
        // shader so its real hand meshes also render in a Built-in project.
        Material handMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            Materials + "Animated Hands/Hands_solid.mat");
        Shader handShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (handMaterial != null && handShader != null && handMaterial.shader != handShader)
        {
            handMaterial.shader = handShader;
            EditorUtility.SetDirty(handMaterial);
        }
        rolling = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(Materials + "Rodamiento.physicMaterial");
        if (rolling == null)
        {
            rolling = new PhysicsMaterial("Rodamiento")
            {
                dynamicFriction = 0.25f,
                staticFriction = 0.25f,
                bounciness = 0.05f,
                frictionCombine = PhysicsMaterialCombine.Minimum
            };
            AssetDatabase.CreateAsset(rolling, Materials + "Rodamiento.physicMaterial");
        }
    }

    private static Material MakeMaterial(string name, Color color)
    {
        string path = Materials + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (material != null)
        {
            // Materials committed to GitHub use Standard; switch to URP when the
            // student's existing Unity project has the URP Lit shader installed.
            if (material.shader != shader) material.shader = shader;
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            return material;
        }
        material = new Material(shader) { name = name, color = color };
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void CreatePrefabs(bool replace = false)
    {
        string ballPath = Models + "Pelota.prefab";
        string clubPath = Models + "Palo.prefab";
        string cupPath = Models + "Hoyo.prefab";
        if (replace)
        {
            AssetDatabase.DeleteAsset(ballPath);
            AssetDatabase.DeleteAsset(clubPath);
            AssetDatabase.DeleteAsset(cupPath);
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(ballPath) == null)
        {
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Pelota";
            ball.transform.localScale = Vector3.one * 0.18f;
            ball.GetComponent<Renderer>().sharedMaterial = white;
            ball.GetComponent<Collider>().material = rolling;
            Rigidbody body = ball.AddComponent<Rigidbody>();
            body.mass = 0.2f;
            body.linearDamping = 0.45f;
            body.angularDamping = 0.18f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            ball.AddComponent<GolfBall>();
            PrefabUtility.SaveAsPrefabAsset(ball, ballPath);
            Object.DestroyImmediate(ball);
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(clubPath) == null)
        {
            GameObject club = new GameObject("Palo");
            Rigidbody body = club.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            CapsuleCollider handleCollider = club.AddComponent<CapsuleCollider>();
            handleCollider.center = new Vector3(0f, -0.37f, 0f);
            handleCollider.radius = 0.055f;
            handleCollider.height = 0.74f;
            GameObject shaft = Box("Mango del palo", new Vector3(0f, -0.37f, 0f),
                new Vector3(0.035f, 0.74f, 0.035f), white, club.transform, false);
            Object.DestroyImmediate(shaft.GetComponent<Collider>());
            GameObject head = Box("Cabeza del palo", new Vector3(0f, -0.76f, 0.07f),
                new Vector3(0.22f, 0.10f, 0.13f), rails, club.transform, false);
            head.GetComponent<BoxCollider>().isTrigger = true;
            GameObject grip = new GameObject("Punto de agarre");
            grip.transform.SetParent(club.transform, false);
            grip.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            XRGrabInteractable grab = club.AddComponent<XRGrabInteractable>();
            grab.colliders.Add(handleCollider);
            grab.attachTransform = grip.transform;
            grab.movementType = XRBaseInteractable.MovementType.Kinematic;
            club.AddComponent<GolfClub>();
            PrefabUtility.SaveAsPrefabAsset(club, clubPath);
            Object.DestroyImmediate(club);
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(cupPath) == null)
        {
            GameObject cup = new GameObject("Hoyo");
            SphereCollider trigger = cup.AddComponent<SphereCollider>();
            trigger.radius = 0.19f;
            trigger.isTrigger = true;
            cup.AddComponent<HoleCup>();
            GameObject disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disk.name = "Interior oscuro";
            disk.transform.SetParent(cup.transform, false);
            disk.transform.localPosition = new Vector3(0f, -0.085f, 0f);
            disk.transform.localScale = new Vector3(0.38f, 0.006f, 0.38f);
            disk.GetComponent<Renderer>().sharedMaterial = dark;
            Object.DestroyImmediate(disk.GetComponent<Collider>());
            PrefabUtility.SaveAsPrefabAsset(cup, cupPath);
            Object.DestroyImmediate(cup);
        }
    }

    private static void MakeHole(MiniGolfGame game, out Transform tee, out HoleCup cup)
    {
        GameObject root = new GameObject("HOYO 1 - pista gris");
        GameObject floor = Box("Piso gris", new Vector3(0f, 0f, 0f),
            new Vector3(2.8f, 0.2f, 7f), grass, root.transform, true);
        floor.AddComponent<TeleportationArea>().interactionLayers =
            new InteractionLayerMask { value = -1 };
        Box("Borde izquierdo", new Vector3(-1.4f, 0.24f, 0f),
            new Vector3(0.18f, 0.36f, 7.2f), rails, root.transform, true);
        Box("Borde derecho", new Vector3(1.4f, 0.24f, 0f),
            new Vector3(0.18f, 0.36f, 7.2f), rails, root.transform, true);
        Box("Borde inicial", new Vector3(0f, 0.24f, -3.5f),
            new Vector3(2.8f, 0.36f, 0.18f), rails, root.transform, true);
        Box("Borde final", new Vector3(0f, 0.24f, 3.5f),
            new Vector3(2.8f, 0.36f, 0.18f), rails, root.transform, true);

        GameObject teeObject = new GameObject("Salida de la pelota");
        teeObject.transform.SetParent(root.transform, false);
        teeObject.transform.position = new Vector3(0f, 0.2f, -2.65f);
        tee = teeObject.transform;

        GameObject cupObject = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(Models + "Hoyo.prefab"));
        cupObject.name = "Hoyo";
        cupObject.transform.SetParent(root.transform, false);
        cupObject.transform.position = new Vector3(0f, 0.19f, 2.75f);
        cup = cupObject.GetComponent<HoleCup>();
        cup.Configure(game);
    }

    private static void AddHandVisuals(Transform rig)
    {
        InputActionAsset actions = FindStarterActions();
        if (actions == null)
            throw new System.InvalidOperationException(
                "Falta XRI Default Input Actions. Importá XR Interaction Toolkit > Samples > Starter Assets.");

        foreach (bool left in new[] { true, false })
        {
            string name = left ? "Left Controller" : "Right Controller";
            Transform controller = null;
            foreach (Transform candidate in rig.GetComponentsInChildren<Transform>(true))
                if (candidate.name == name) { controller = candidate; break; }
            if (controller == null)
                throw new System.InvalidOperationException("Starter Assets: falta " + name +
                    ". Revisá que se importó XR Origin (XR Rig) del paquete XRI 3.5.1.");

            string visualName = left ? "Mano izquierda" : "Mano derecha";
            Transform existing = controller.Find(visualName);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // Keep teleport rays and interactors, but hide the generic controller model.
            foreach (MeshRenderer mesh in controller.GetComponentsInChildren<MeshRenderer>(true))
                mesh.enabled = false;
            foreach (SkinnedMeshRenderer mesh in controller.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                mesh.enabled = false;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                HandPrefabs + (left ? "Left Hand Model.prefab" : "Right Hand Model.prefab"));
            if (prefab == null)
                throw new System.InvalidOperationException(
                    "Faltan los prefabs Animated Hands del enlace original de Valem.");
            GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, controller);
            visual.name = visualName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            InputActionReference trigger = GetOrCreateReference(actions,
                FindInteractionAction(actions, left, "Activate Value"));
            InputActionReference grip = GetOrCreateReference(actions,
                FindInteractionAction(actions, left, "Select Value"));
            Animator animator = visual.GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null)
                throw new System.InvalidOperationException(
                    "El modelo de mano no trae el Animator Grip/Trigger del tutorial.");
            AnimateHandOnInput animation = visual.AddComponent<AnimateHandOnInput>();
            animation.Configure(trigger, grip, animator,
                left ? UnityEngine.XR.XRNode.LeftHand : UnityEngine.XR.XRNode.RightHand);
        }
    }

    private static InputActionAsset FindStarterActions()
    {
        string sample = "Assets/Samples/XR Interaction Toolkit/" + XriVersion + "/Starter Assets";
        foreach (string guid in AssetDatabase.FindAssets("t:InputActionAsset", new[] { sample }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == "XRI Default Input Actions")
                return AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
        }
        return null;
    }

    private static InputAction FindInteractionAction(InputActionAsset asset, bool left, string actionName)
    {
        string side = left ? "Left" : "Right";
        foreach (InputActionMap map in asset.actionMaps)
        {
            if (map.name.IndexOf(side, System.StringComparison.OrdinalIgnoreCase) < 0 ||
                map.name.IndexOf("Interaction", System.StringComparison.OrdinalIgnoreCase) < 0)
                continue;
            InputAction action = map.FindAction(actionName, false);
            if (action != null) return action;
        }
        throw new System.InvalidOperationException("Starter Assets: falta XRI " + side +
            " Interaction / " + actionName + ".");
    }

    private static InputActionReference GetOrCreateReference(InputActionAsset asset, InputAction action)
    {
        string source = AssetDatabase.GetAssetPath(asset);
        foreach (Object subasset in AssetDatabase.LoadAllAssetsAtPath(source))
            if (subasset is InputActionReference reference && reference.action != null &&
                reference.action.id == action.id)
                return reference;

        // Some importer versions do not expose the action references as subassets.
        // Save our own persistent reference so the .unity scene can serialize it.
        string folder = Models + "Animated Hands/Action References";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder(Models + "Animated Hands", "Action References");
        string path = folder + "/" + action.actionMap.name + " - " + action.name + ".asset";
        InputActionReference saved = AssetDatabase.LoadAssetAtPath<InputActionReference>(path);
        if (saved != null) return saved;
        saved = InputActionReference.Create(action);
        AssetDatabase.CreateAsset(saved, path);
        return saved;
    }

    [MenuItem("Minigolf VR/Dejar solo pista y manos")]
    private static void SimpleFromMenu()
    {
        if (!File.Exists(ScenePath)) CreateScene();
        if (!File.Exists(ScenePath)) return;
        if (SceneManager.GetActiveScene().path != ScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
        }
        InstallSimpleSceneIfNeeded();
    }

    internal static void InstallSimpleSceneIfNeeded()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling || EditorApplication.isUpdating ||
            SceneManager.GetActiveScene().path != ScenePath) return;

        GameObject oldHole = GameObject.Find("HOYO 1 - pista y obstaculos") ??
            GameObject.Find("HOYO 1 - Blender") ??
            GameObject.Find("HOYO 1 - pista gris");
        GameObject rigObject = GameObject.Find("Jugador - XR Origin (VR) - Starter Assets");
        if (rigObject == null) return;
        if (GameObject.Find("HOYO 1 - pista gris") != null &&
            rigObject.GetComponentsInChildren<AnimateHandOnInput>(true).Length == 2)
        {
            FixVrHeightIfNeeded();
            return;
        }
        MiniGolfRig rig = rigObject.GetComponent<MiniGolfRig>();
        GameObject gameObject = GameObject.Find("PARTIDA - turnos y puntajes") ??
            GameObject.Find("Prueba - un hoyo");
        MiniGolfGame game = gameObject?.GetComponent<MiniGolfGame>();
        if (game == null || rig == null || oldHole == null) return;
        CreateMaterials();
        // Remove the old ball and club before recreating their prefabs.
        foreach (GolfBall oldBall in Object.FindObjectsByType<GolfBall>(FindObjectsSortMode.None))
            Object.DestroyImmediate(oldBall.gameObject);
        foreach (GolfClub oldClub in Object.FindObjectsByType<GolfClub>(FindObjectsSortMode.None))
            Object.DestroyImmediate(oldClub.gameObject);
        Object.DestroyImmediate(oldHole);
        GameObject oldArrow = GameObject.Find("Direccion del golpe (PC)");
        if (oldArrow != null) Object.DestroyImmediate(oldArrow);
        GameObject score = GameObject.Find("Puntaje VR");
        if (score != null) Object.DestroyImmediate(score);
        DesktopGolfInput desktop = game.GetComponent<DesktopGolfInput>();
        if (desktop != null) Object.DestroyImmediate(desktop);
        game.name = "Prueba - un hoyo";
        CreatePrefabs(true);
        Transform tee;
        HoleCup cup;
        MakeHole(game, out tee, out cup);
        GameObject ball = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(Models + "Pelota.prefab"));
        ball.name = "Pelota - fisica";
        GameObject club = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(Models + "Palo.prefab"));
        club.name = "Palo - agarrar con grip VR";
        club.transform.position = new Vector3(0.55f, 0.95f, -2.7f);
        Transform head = club.transform.Find("Cabeza del palo");
        club.GetComponent<GolfClub>().Configure(game, head, head.GetComponent<BoxCollider>());
        AddHandVisuals(rigObject.transform);
        game.Configure(tee, cup, ball.GetComponent<GolfBall>());
        rig.Configure(rigObject.GetComponentInChildren<Camera>(true).transform);
        rig.SetStation(tee.position);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Minigolf VR: pista gris, hoyo, pelota, palo y manos VR listos.");
    }

    [MenuItem("Minigolf VR/Corregir altura VR")]
    private static void FixHeightFromMenu()
    {
        if (SceneManager.GetActiveScene().path != ScenePath)
            EditorSceneManager.OpenScene(ScenePath);
        FixVrHeightIfNeeded();
    }

    internal static void FixVrHeightIfNeeded()
    {
        GameObject rigObject = GameObject.Find("Jugador - XR Origin (VR) - Starter Assets");
        if (rigObject == null) return;
        XROrigin origin = rigObject.GetComponent<XROrigin>();
        Camera camera = rigObject.GetComponentInChildren<Camera>(true);
        MiniGolfRig rig = rigObject.GetComponent<MiniGolfRig>();
        if (origin == null || camera == null || rig == null) return;
        if (origin.RequestedTrackingOriginMode == XROrigin.TrackingOriginMode.Floor &&
            camera.transform.localPosition == Vector3.zero &&
            Mathf.Abs(rigObject.transform.position.y) < 0.001f) return;

        rig.Configure(camera.transform);
        Vector3 position = rigObject.transform.position;
        position.y = 0f;
        rigObject.transform.position = position;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("Minigolf VR: altura corregida. El XR Origin toma como referencia el piso.");
    }

    private static GameObject Box(string name, Vector3 position, Vector3 size,
        Material material, Transform parent, bool rollingSurface)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.position = position;
        box.transform.localScale = size;
        box.GetComponent<Renderer>().sharedMaterial = material;
        if (rollingSurface) box.GetComponent<Collider>().material = rolling;
        return box;
    }

    private static void AddToBuildSettings()
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(scene => scene.path == ScenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
