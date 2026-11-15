using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;

// Unity configura OpenXR al abrir el proyecto por primera vez.
[InitializeOnLoad]
internal static class MiniGolfVrPcSetup
{
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
            Prepare();
    }

    private static bool Prepare()
    {
        try
        {
            PrepareDirect3D11();
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
                Debug.LogError("Minigolf VR: no se pudo activar OpenXR para Windows. Revisá Project Settings > XR Plug-in Management > OpenXR.");
                return false;
            }

            bool changed = false;
            if (!general.InitManagerOnStart)
            {
                general.InitManagerOnStart = true;
                EditorUtility.SetDirty(general);
                changed = true;
            }

            FeatureHelpers.RefreshFeatures(Target);
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
            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogError("Minigolf VR: no se pudo preparar OpenXR. " + exception);
            return false;
        }
    }

    private static void PrepareDirect3D11()
    {
        // Quest Link usa Direct3D11 en este proyecto.
        const BuildTarget windows = BuildTarget.StandaloneWindows64;
        GraphicsDeviceType[] apis = PlayerSettings.GetGraphicsAPIs(windows);
        if (!PlayerSettings.GetUseDefaultGraphicsAPIs(windows) &&
            apis.Length == 1 && apis[0] == GraphicsDeviceType.Direct3D11)
            return;

        PlayerSettings.SetUseDefaultGraphicsAPIs(windows, false);
        PlayerSettings.SetGraphicsAPIs(windows,
            new[] { GraphicsDeviceType.Direct3D11 });
        Debug.LogWarning("Minigolf VR: Direct3D11 configurado para Windows. Reiniciá Unity antes de probar Quest Link.");
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
