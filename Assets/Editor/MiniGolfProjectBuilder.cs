using System.IO;
using MiniGolfVR;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

// On the first import, builds editable .unity, .prefab and .mat assets INSIDE Assets.
// The source project remains small, while the scene is immediately visible in Unity.
[InitializeOnLoad]
internal static class MiniGolfProjectBuilder
{
    private const string ScenePath = "Assets/Scenes/MinigolfVR.unity";
    private const string Materials = "Assets/Materials/";
    private const string Models = "Assets/Models/";
    private const string XriVersion = "3.2.1";

    private static Material grass;
    private static Material rails;
    private static Material obstacles;
    private static Material white;
    private static Material dark;
    private static Material red;
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
    }

    [MenuItem("Minigolf VR/Crear escena inicial")]
    private static void CreateFromMenu()
    {
        if (File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath);
        else CreateScene();
    }

    [MenuItem("Minigolf VR/Abrir escena inicial")]
    private static void OpenFromMenu()
    {
        if (!File.Exists(ScenePath)) CreateScene();
        else EditorSceneManager.OpenScene(ScenePath);
    }

    private static void CreateScene()
    {
        if (File.Exists(ScenePath)) return;
        GameObject starterRig = FindStarterRig();
        if (starterRig == null) return;
        EnsureFolders();
        Scene previous = SceneManager.GetActiveScene();
        bool canSwitch = !previous.isDirty &&
            (string.IsNullOrEmpty(previous.path) || previous.path.EndsWith("/SampleScene.unity"));
        // Additive creation keeps any scene that the student already has open intact.
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        CreateMaterials();
        CreatePrefabs();

        GameObject gameObject = new GameObject("PARTIDA - turnos y puntajes");
        MiniGolfGame game = gameObject.AddComponent<MiniGolfGame>();
        MiniGolfGame.HoleLayout[] holes = new MiniGolfGame.HoleLayout[1];
        for (int i = 0; i < holes.Length; i++)
            holes[i] = MakeHole(i, game);

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
        rig.Configure(cameraObject.transform, game);
        new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();

        GameObject club = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(Models + "Palo.prefab"));
        club.name = "Palo - agarrar con grip VR";
        club.transform.position = new Vector3(0.55f, 0.95f, -2.7f);
        Transform clubHead = club.transform.Find("Cabeza del palo");
        club.GetComponent<GolfClub>().Configure(game, clubHead,
            clubHead.GetComponent<BoxCollider>());

        GameObject display = new GameObject("Puntaje VR");
        display.transform.SetParent(cameraObject.transform, false);
        display.transform.localPosition = new Vector3(-0.55f, -0.20f, 1.4f);
        TextMesh scoreboard = display.AddComponent<TextMesh>();
        scoreboard.text = "MINIGOLF VR";
        scoreboard.fontSize = 40;
        scoreboard.characterSize = 0.0018f;
        scoreboard.anchor = TextAnchor.UpperLeft;
        scoreboard.color = Color.white;

        GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arrow.name = "Direccion del golpe (PC)";
        Object.DestroyImmediate(arrow.GetComponent<Collider>());
        arrow.GetComponent<Renderer>().sharedMaterial = red;
        DesktopGolfInput input = gameObject.AddComponent<DesktopGolfInput>();
        input.Configure(game, arrow.transform);

        game.Configure(holes, ball.GetComponent<GolfBall>(), rig, scoreboard);
        rig.SetStation(holes[0].tee.position);

        GameObject lightObject = new GameObject("Sol");
        Light sunlight = lightObject.AddComponent<Light>();
        sunlight.type = LightType.Directional;
        sunlight.intensity = 1.3f;
        lightObject.transform.rotation = Quaternion.Euler(48f, -25f, 0f);
        RenderSettings.ambientLight = new Color(0.5f, 0.55f, 0.6f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings();
        SceneManager.SetActiveScene(previous);
        EditorSceneManager.CloseScene(scene, true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (canSwitch) EditorSceneManager.OpenScene(ScenePath);
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
        grass = MakeMaterial("Pasto verde", new Color(0.19f, 0.55f, 0.28f));
        rails = MakeMaterial("Bordes azul oscuro", new Color(0.09f, 0.19f, 0.38f));
        obstacles = MakeMaterial("Obstaculos celestes", new Color(0.18f, 0.68f, 0.8f));
        white = MakeMaterial("Pelota y lineas blancas", new Color(0.97f, 0.96f, 0.91f));
        dark = MakeMaterial("Interior del hoyo", new Color(0.025f, 0.04f, 0.055f));
        red = MakeMaterial("Bandera roja", new Color(0.91f, 0.18f, 0.14f));
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
        if (material != null) return material;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        material = new Material(shader) { name = name, color = color };
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void CreatePrefabs()
    {
        string ballPath = Models + "Pelota.prefab";
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

        string clubPath = Models + "Palo.prefab";
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

        string cupPath = Models + "Hoyo y bandera.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(cupPath) == null)
        {
            GameObject cup = new GameObject("Hoyo y bandera");
            SphereCollider trigger = cup.AddComponent<SphereCollider>();
            trigger.radius = 0.19f;
            trigger.isTrigger = true;
            cup.AddComponent<HoleCup>();
            GameObject disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disk.name = "Interior oscuro";
            disk.transform.SetParent(cup.transform, false);
            disk.transform.localPosition = new Vector3(0f, -0.068f, 0f);
            disk.transform.localScale = new Vector3(0.38f, 0.006f, 0.38f);
            disk.GetComponent<Renderer>().sharedMaterial = dark;
            Object.DestroyImmediate(disk.GetComponent<Collider>());
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Asta";
            pole.transform.SetParent(cup.transform, false);
            pole.transform.localPosition = new Vector3(0.22f, 0.48f, 0f);
            pole.transform.localScale = new Vector3(0.012f, 0.48f, 0.012f);
            pole.GetComponent<Renderer>().sharedMaterial = white;
            Object.DestroyImmediate(pole.GetComponent<Collider>());
            GameObject flag = Box("Bandera", new Vector3(0.36f, 0.83f, 0f),
                new Vector3(0.27f, 0.16f, 0.02f), red, cup.transform, false);
            Object.DestroyImmediate(flag.GetComponent<Collider>());
            PrefabUtility.SaveAsPrefabAsset(cup, cupPath);
            Object.DestroyImmediate(cup);
        }
    }

    private static MiniGolfGame.HoleLayout MakeHole(int index, MiniGolfGame game)
    {
        float x = index * 6f;
        GameObject root = new GameObject("HOYO " + (index + 1) + " - pista y obstaculos");
        GameObject floor = Box("Pasto", new Vector3(x, 0f, 0f), new Vector3(2.8f, 0.2f, 7f),
            grass, root.transform, true);
        floor.AddComponent<TeleportationArea>().interactionLayers =
            new InteractionLayerMask { value = -1 };
        Box("Borde izquierdo", new Vector3(x - 1.4f, 0.24f, 0f),
            new Vector3(0.18f, 0.36f, 7.2f), rails, root.transform, true);
        Box("Borde derecho", new Vector3(x + 1.4f, 0.24f, 0f),
            new Vector3(0.18f, 0.36f, 7.2f), rails, root.transform, true);
        Box("Borde inicial", new Vector3(x, 0.24f, -3.5f),
            new Vector3(2.8f, 0.36f, 0.18f), rails, root.transform, true);
        Box("Borde final", new Vector3(x, 0.24f, 3.5f),
            new Vector3(2.8f, 0.36f, 0.18f), rails, root.transform, true);

        GameObject tee = new GameObject("Salida de la pelota");
        tee.transform.SetParent(root.transform, false);
        tee.transform.position = new Vector3(x, 0.2f, -2.65f);
        GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mark.name = "Marca de salida";
        mark.transform.SetParent(root.transform, false);
        mark.transform.position = new Vector3(x, 0.105f, -2.65f);
        mark.transform.localScale = new Vector3(0.24f, 0.003f, 0.24f);
        mark.GetComponent<Renderer>().sharedMaterial = white;
        Object.DestroyImmediate(mark.GetComponent<Collider>());

        GameObject cupObject = (GameObject)PrefabUtility.InstantiatePrefab(
            AssetDatabase.LoadAssetAtPath<GameObject>(Models + "Hoyo y bandera.prefab"));
        cupObject.name = "Meta - hoyo " + (index + 1);
        cupObject.transform.SetParent(root.transform, false);
        cupObject.transform.position = new Vector3(x, 0.19f, 2.75f);
        HoleCup cup = cupObject.GetComponent<HoleCup>();
        cup.Configure(game);

        if (index == 0)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(
                Models + "ObstaculoTrapezoidal.obj");
            if (model != null)
            {
                GameObject block = (GameObject)PrefabUtility.InstantiatePrefab(model);
                block.name = "Obstaculo central - modelo OBJ";
                block.transform.SetParent(root.transform, false);
                block.transform.position = new Vector3(x, 0.10f, 0f);
                foreach (Renderer renderer in block.GetComponentsInChildren<Renderer>())
                    renderer.sharedMaterial = obstacles;
                BoxCollider collision = block.AddComponent<BoxCollider>();
                collision.center = new Vector3(0f, 0.21f, 0f);
                collision.size = new Vector3(0.60f, 0.42f, 0.60f);
                collision.material = rolling;
            }
            else
                Box("Obstaculo central", new Vector3(x, 0.27f, 0f),
                    new Vector3(0.48f, 0.35f, 0.48f), obstacles, root.transform, true);
        }
        return new MiniGolfGame.HoleLayout
        {
            name = "Hoyo " + (index + 1),
            root = root,
            tee = tee.transform,
            cup = cup
        };
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
