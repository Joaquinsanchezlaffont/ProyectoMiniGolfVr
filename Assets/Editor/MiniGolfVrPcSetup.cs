using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;

// XR packages alone do not turn on the Windows loader or the Quest controllers.
// Prepare both on import and expose a single menu entry to launch the scene.
[InitializeOnLoad]
internal static class MiniGolfVrPcSetup
{
    private const string ScenePath = "Assets/Scenes/MinigolfVR.unity";
    private const string LoaderName = "UnityEngine.XR.OpenXR.OpenXRLoader";
    private const BuildTargetGroup Target = BuildTargetGroup.Standalone;

    static MiniGolfVrPcSetup() => EditorApplication.delayCall += PrepareOnImport;

    private static void PrepareOnImport()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += PrepareOnImport;
            return;
        }

        if (Application.platform == RuntimePlatform.WindowsEditor &&
            !EditorApplication.isPlayingOrWillChangePlaymode)
            Prepare(false);
    }

    [MenuItem("Minigolf VR/Jugar con Quest Link")]
    private static void PlayWithQuestLink()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (Application.platform != RuntimePlatform.WindowsEditor)
        {
            Debug.LogError("Minigolf VR: Quest Link desde Unity requiere Windows.");
            return;
        }

        if (!Prepare(true)) return;
        if (!File.Exists(ScenePath))
        {
            Debug.LogError("Minigolf VR: falta la escena. Elegí Minigolf VR > Crear escena inicial.");
            return;
        }

        if (SceneManager.GetActiveScene().path != ScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
        }

        EditorApplication.isPlaying = true;
    }

    [MenuItem("Minigolf VR/Preparar Quest Link (OpenXR)")]
    private static void PrepareFromMenu() => Prepare(true);

    private static bool Prepare(bool reportSuccess)
    {
        try
        {
            XRGeneralSettingsPerBuildTarget perTarget = GetOrCreateSettings();
            if (perTarget.SettingsForBuildTarget(Target) == null)
                perTarget.CreateDefaultSettingsForBuildTarget(Target);
            if (perTarget.ManagerSettingsForBuildTarget(Target) == null)
                perTarget.CreateDefaultManagerSettingsForBuildTarget(Target);

            XRGeneralSettings general = perTarget.SettingsForBuildTarget(Target);
            if (general == null || general.Manager == null)
            {
                Debug.LogError("Minigolf VR: Unity no pudo crear la configuración XR de Windows.");
                return false;
            }

            if (!XRPackageMetadataStore.IsLoaderAssigned(LoaderName, Target) &&
                !XRPackageMetadataStore.AssignLoader(general.Manager, LoaderName, Target))
            {
                Debug.LogError("Minigolf VR: no se pudo activar OpenXR para Windows. Cerrá Project Settings y usá Minigolf VR > Preparar Quest Link (OpenXR).");
                return false;
            }

            bool changed = false;
            if (!general.InitManagerOnStart)
            {
                general.InitManagerOnStart = true;
                EditorUtility.SetDirty(general);
                changed = true;
            }

            OpenXRSettings openxr = OpenXRSettings.GetSettingsForBuildTargetGroup(Target);
            if (openxr == null)
            {
                Debug.LogError("Minigolf VR: no se pudo abrir la configuración de OpenXR para Windows.");
                return false;
            }

            var touchProfiles = openxr.GetFeatures<OculusTouchControllerProfile>();
            if (touchProfiles.Length == 0)
            {
                Debug.LogError("Minigolf VR: falta Oculus Touch Controller Profile en el paquete OpenXR.");
                return false;
            }

            foreach (var profile in touchProfiles)
            {
                if (profile.enabled) continue;
                profile.enabled = true;
                EditorUtility.SetDirty(profile);
                changed = true;
            }

            if (!PlayerSettings.runInBackground)
            {
                PlayerSettings.runInBackground = true;
                changed = true;
            }

            if (changed) EditorUtility.SetDirty(openxr);
            AssetDatabase.SaveAssets();
            if (reportSuccess)
                Debug.Log("Minigolf VR: OpenXR de Windows y Oculus Touch listos. Conectá Quest Link y activá Meta Horizon Link como OpenXR Runtime en la PC.");
            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogError("Minigolf VR: no se pudo preparar OpenXR. " + exception);
            return false;
        }
    }

    private static XRGeneralSettingsPerBuildTarget GetOrCreateSettings()
    {
        EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey,
            out XRGeneralSettingsPerBuildTarget perTarget);
        if (perTarget != null) return perTarget;

        string[] assets = AssetDatabase.FindAssets("t:XRGeneralSettingsPerBuildTarget");
        if (assets.Length > 0)
            perTarget = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(
                AssetDatabase.GUIDToAssetPath(assets[0]));

        if (perTarget == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/XR"))
                AssetDatabase.CreateFolder("Assets", "XR");
            if (!AssetDatabase.IsValidFolder("Assets/XR/Settings"))
                AssetDatabase.CreateFolder("Assets/XR", "Settings");

            perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(perTarget,
                "Assets/XR/Settings/XRGeneralSettingsPerBuildTarget.asset");
        }

        EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perTarget, true);
        return perTarget;
    }
}
