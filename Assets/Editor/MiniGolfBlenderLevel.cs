using System;
using System.Collections.Generic;
using System.IO;
using MiniGolfVR;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

// Replaces only the generated first hole. The game, turns, timer and XR Origin stay wired up.
internal static class MiniGolfBlenderLevel
{
    private const string ScenePath = "Assets/Scenes/MinigolfVR.unity";
    private const string Models = "Assets/Models/Blender/";
    private const string Materials = "Assets/Materials/Blender/";
    private const string OldHole = "HOYO 1 - pista y obstaculos";
    private const string NewHole = "HOYO 1 - Blender";

    [Serializable] private sealed class LevelData
    {
        public float[] tee;
        public float[] cup;
        public float[] clubHead;
        public float ballRadius;
        public LevelMaterial[] materials;
        public LevelObject[] objects;
    }

    [Serializable] private sealed class LevelMaterial
    {
        public string id;
        public string name;
        public float[] color;
    }

    [Serializable] private sealed class LevelObject
    {
        public string name;
        public string material;
    }

    [MenuItem("Minigolf VR/Usar primer nivel de Blender")]
    private static void InstallFromMenu()
    {
        if (!File.Exists(ScenePath))
            EditorApplication.ExecuteMenuItem("Minigolf VR/Crear escena inicial");
        InstallIfNeeded(true);
    }

    internal static void InstallIfNeeded(bool manual = false)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        GameObject courseAsset = AssetDatabase.LoadAssetAtPath<GameObject>(Models + "PrimerNivel.obj");
        GameObject ballAsset = AssetDatabase.LoadAssetAtPath<GameObject>(Models + "PelotaBlender.obj");
        GameObject clubAsset = AssetDatabase.LoadAssetAtPath<GameObject>(Models + "PaloBlender.obj");
        TextAsset dataAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(Models + "NivelBlender.json");
        if (courseAsset == null || ballAsset == null || clubAsset == null || dataAsset == null)
        {
            if (manual) Debug.LogError("Minigolf VR: faltan los modelos exportados en Assets/Models/Blender.");
            return;
        }
        if (!File.Exists(ScenePath)) return;

        // Don't discard changes in a scene currently open in the student's Editor.
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene open = SceneManager.GetSceneAt(i);
            if (!open.isDirty) continue;
            if (manual) Debug.LogWarning("Guardá las escenas abiertas antes de usar el nivel de Blender.");
            return;
        }
        if (SceneManager.GetActiveScene().path != ScenePath)
        {
            if (!manual) return;
            EditorSceneManager.OpenScene(ScenePath, NewSceneMode.Single);
        }

        if (GameObject.Find(NewHole) != null) return;
        GameObject originalHole = GameObject.Find(OldHole);
        GameObject matchObject = GameObject.Find("PARTIDA - turnos y puntajes");
        GameObject playerObject = GameObject.Find("Jugador - XR Origin (VR) - Starter Assets");
        GameObject scoreObject = GameObject.Find("Puntaje VR");
        if (originalHole == null || matchObject == null || playerObject == null || scoreObject == null)
        {
            Debug.LogError("Minigolf VR: falta la escena generada. Abrí MinigolfVR.unity antes de importar el nivel.");
            return;
        }

        LevelData level = JsonUtility.FromJson<LevelData>(dataAsset.text);
        if (level == null || level.tee == null || level.tee.Length != 3 ||
            level.cup == null || level.cup.Length != 3 ||
            level.clubHead == null || level.clubHead.Length != 3 ||
            level.materials == null || level.objects == null)
        {
            Debug.LogError("Minigolf VR: NivelBlender.json está incompleto.");
            return;
        }

        Dictionary<string, Material> byId = CreateMaterials(level);
        Dictionary<string, string> materialByObject = new Dictionary<string, string>();
        foreach (LevelObject entry in level.objects)
            if (!string.IsNullOrEmpty(entry.name)) materialByObject[entry.name] = entry.material;

        GameObject ballPrefab = BuildBallPrefab(ballAsset, level, byId, materialByObject);
        GameObject clubPrefab = BuildClubPrefab(clubAsset, level, byId, materialByObject);
        if (ballPrefab == null || clubPrefab == null) return;

        MiniGolfGame match = matchObject.GetComponent<MiniGolfGame>();
        MiniGolfRig rig = playerObject.GetComponent<MiniGolfRig>();
        TextMesh score = scoreObject.GetComponent<TextMesh>();
        if (match == null || rig == null || score == null)
        {
            Debug.LogError("Minigolf VR: faltan componentes en la escena original.");
            return;
        }

        GameObject levelRoot = new GameObject(NewHole);
        GameObject course = (GameObject)PrefabUtility.InstantiatePrefab(courseAsset);
        course.name = "Pista de Sebastian - Blender";
        course.transform.SetParent(levelRoot.transform, false);
        ApplyMaterials(course, byId, materialByObject);
        if (!AddCourseColliders(course))
        {
            UnityEngine.Object.DestroyImmediate(levelRoot);
            Debug.LogError("Minigolf VR: no se encontraron superficies para las colisiones.");
            return;
        }

        GameObject teeObject = new GameObject("Salida de la pelota");
        teeObject.transform.SetParent(levelRoot.transform, false);
        teeObject.transform.position = Point(level.tee);

        GameObject cupObject = new GameObject("Meta - hoyo 1");
        cupObject.transform.SetParent(levelRoot.transform, false);
        cupObject.transform.position = Point(level.cup);
        SphereCollider cupTrigger = cupObject.AddComponent<SphereCollider>();
        cupTrigger.center = new Vector3(0f, 0.08f, 0f);
        cupTrigger.radius = 0.11f;
        cupTrigger.isTrigger = true;
        HoleCup cup = cupObject.AddComponent<HoleCup>();
        cup.Configure(match);
        cup.SetCaptureRadius(0.095f);

        GameObject oldBall = GameObject.Find("Pelota - fisica");
        GameObject oldClub = GameObject.Find("Palo - agarrar con grip VR");
        if (oldBall != null) UnityEngine.Object.DestroyImmediate(oldBall);
        if (oldClub != null) UnityEngine.Object.DestroyImmediate(oldClub);
        GameObject ball = (GameObject)PrefabUtility.InstantiatePrefab(ballPrefab);
        ball.name = "Pelota - fisica";
        GameObject club = (GameObject)PrefabUtility.InstantiatePrefab(clubPrefab);
        club.name = "Palo - agarrar con grip VR";
        Vector3 tee = teeObject.transform.position;
        club.transform.position = new Vector3(tee.x + 0.35f, 0.82f, tee.z + 0.02f);
        Transform strikingHead = club.transform.Find("Cabeza del palo - golpe");
        club.GetComponent<GolfClub>().Configure(match, strikingHead,
            strikingHead.GetComponent<BoxCollider>());

        UnityEngine.Object.DestroyImmediate(originalHole);
        match.Configure(new[] { new MiniGolfGame.HoleLayout {
            name = "Hoyo 1 - Blender", root = levelRoot,
            tee = teeObject.transform, cup = cup
        } }, ball.GetComponent<GolfBall>(), rig, score);
        rig.SetStationOffset(0.20f);
        rig.SetStation(tee);
        EditorUtility.SetDirty(match);
        EditorUtility.SetDirty(rig);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log("Minigolf VR: primer nivel, pelota y palo de protecto29sep.blend listos en la escena.");
    }

    private static Vector3 Point(float[] coordinates)
    {
        return new Vector3(coordinates[0], coordinates[1], coordinates[2]);
    }

    private static Dictionary<string, Material> CreateMaterials(LevelData level)
    {
        if (!AssetDatabase.IsValidFolder(Materials.TrimEnd('/')))
            AssetDatabase.CreateFolder("Assets/Materials", "Blender");
        Dictionary<string, Material> byId = new Dictionary<string, Material>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        foreach (LevelMaterial entry in level.materials)
        {
            if (entry == null || string.IsNullOrEmpty(entry.id) ||
                entry.color == null || entry.color.Length < 3) continue;
            string path = Materials + entry.id + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = entry.id };
                AssetDatabase.CreateAsset(material, path);
            }
            if (material.shader != shader) material.shader = shader;
            Color color = new Color(entry.color[0], entry.color[1], entry.color[2], 1f);
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            byId[entry.id] = material;
        }
        return byId;
    }

    private static void ApplyMaterials(GameObject instance,
        Dictionary<string, Material> byId, Dictionary<string, string> byObject)
    {
        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            Material[] slots = renderer.sharedMaterials;
            if (slots.Length == 0) continue;
            for (int i = 0; i < slots.Length; i++)
            {
                string id = slots[i] != null ? slots[i].name : null;
                if (id != null && byId.TryGetValue(id, out Material imported))
                    slots[i] = imported;
                else if (byObject.TryGetValue(renderer.gameObject.name, out id) &&
                         id != null && byId.TryGetValue(id, out Material mapped))
                    slots[i] = mapped;
            }
            renderer.sharedMaterials = slots;
        }
    }

    private static bool AddCourseColliders(GameObject course)
    {
        PhysicsMaterial rolling = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(
            "Assets/Materials/Rodamiento.physicMaterial");
        int surfaces = 0;
        foreach (MeshFilter filter in course.GetComponentsInChildren<MeshFilter>(true))
        {
            string n = filter.gameObject.name;
            bool ground = n.StartsWith("Terreno_de_la_cancha", StringComparison.OrdinalIgnoreCase) ||
                          n.StartsWith("Fairway", StringComparison.OrdinalIgnoreCase) ||
                          n.StartsWith("Green_de_putting", StringComparison.OrdinalIgnoreCase) ||
                          n.StartsWith("Plataforma_de_salida", StringComparison.OrdinalIgnoreCase);
            bool obstacle = n.StartsWith("Bunker_", StringComparison.OrdinalIgnoreCase) ||
                            n.StartsWith("Borde_de_rough", StringComparison.OrdinalIgnoreCase) ||
                            n.StartsWith("Borde_de_orilla", StringComparison.OrdinalIgnoreCase) ||
                            n.StartsWith("Tronco_", StringComparison.OrdinalIgnoreCase);
            if ((!ground && !obstacle) || filter.sharedMesh == null) continue;
            MeshCollider collider = filter.GetComponent<MeshCollider>();
            if (collider == null) collider = filter.gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = filter.sharedMesh;
            collider.convex = false; // Static course geometry, never a Rigidbody.
            collider.material = rolling;
            if (ground)
            {
                TeleportationArea area = filter.GetComponent<TeleportationArea>();
                if (area == null) area = filter.gameObject.AddComponent<TeleportationArea>();
                area.interactionLayers = new InteractionLayerMask { value = -1 };
            }
            surfaces++;
        }
        return surfaces > 0;
    }

    private static GameObject BuildBallPrefab(GameObject asset, LevelData level,
        Dictionary<string, Material> byId, Dictionary<string, string> byObject)
    {
        GameObject root = new GameObject("Pelota de golf - Blender");
        SphereCollider collider = root.AddComponent<SphereCollider>();
        collider.radius = level.ballRadius;
        collider.material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(
            "Assets/Materials/Rodamiento.physicMaterial");
        Rigidbody body = root.AddComponent<Rigidbody>();
        body.mass = 0.2f;
        body.linearDamping = 0.45f;
        body.angularDamping = 0.18f;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        root.AddComponent<GolfBall>();
        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        visual.name = "Modelo de pelota";
        visual.transform.SetParent(root.transform, false);
        ApplyMaterials(visual, byId, byObject);
        string path = "Assets/Models/Pelota.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject BuildClubPrefab(GameObject asset, LevelData level,
        Dictionary<string, Material> byId, Dictionary<string, string> byObject)
    {
        GameObject root = new GameObject("Palo de golf - Blender");
        Rigidbody body = root.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        CapsuleCollider handle = root.AddComponent<CapsuleCollider>();
        handle.center = new Vector3(0f, -0.05f, 0f);
        handle.radius = 0.033f;
        handle.height = 0.20f;
        GameObject visual = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        visual.name = "Modelo de palo";
        visual.transform.SetParent(root.transform, false);
        ApplyMaterials(visual, byId, byObject);
        GameObject head = new GameObject("Cabeza del palo - golpe");
        head.transform.SetParent(root.transform, false);
        head.transform.localPosition = Point(level.clubHead);
        BoxCollider hit = head.AddComponent<BoxCollider>();
        hit.size = new Vector3(0.23f, 0.09f, 0.08f);
        hit.isTrigger = true;
        GameObject grip = new GameObject("Punto de agarre");
        grip.transform.SetParent(root.transform, false);
        grip.transform.localPosition = new Vector3(0f, -0.05f, 0f);
        XRGrabInteractable grab = root.AddComponent<XRGrabInteractable>();
        grab.colliders.Add(handle);
        grab.attachTransform = grip.transform;
        grab.movementType = XRBaseInteractable.MovementType.Kinematic;
        root.AddComponent<GolfClub>();
        string path = "Assets/Models/Palo.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }
}
