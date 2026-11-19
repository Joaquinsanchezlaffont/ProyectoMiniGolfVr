using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

// Quest Link usa Windows. El APK usa la configuración de Android.
[InitializeOnLoad]
internal static class ConfigurarQuest
{
    private const string LoaderName = "UnityEngine.XR.OpenXR.OpenXRLoader";

    static ConfigurarQuest() => EditorApplication.delayCall += PrepareOnImport;

    private static void PrepareOnImport()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += PrepareOnImport;
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            PrepareDirect3D11();
            PrepararPlataforma(BuildTargetGroup.Standalone);
        }
        if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            PrepararPlataforma(BuildTargetGroup.Android);
    }

    private static void PrepararPlataforma(BuildTargetGroup plataforma)
    {
        XRGeneralSettingsPerBuildTarget ajustes = GetOrCreateSettings();
        if (ajustes.SettingsForBuildTarget(plataforma) == null)
            ajustes.CreateDefaultSettingsForBuildTarget(plataforma);
        if (ajustes.ManagerSettingsForBuildTarget(plataforma) == null)
            ajustes.CreateDefaultManagerSettingsForBuildTarget(plataforma);

        XRGeneralSettings general = ajustes.SettingsForBuildTarget(plataforma);
        if (general == null || general.Manager == null ||
            (!XRPackageMetadataStore.IsLoaderAssigned(LoaderName, plataforma) &&
             !XRPackageMetadataStore.AssignLoader(general.Manager, LoaderName, plataforma)))
        {
            Debug.LogError($"No se pudo activar OpenXR para {plataforma}.");
            return;
        }
        if (!general.InitManagerOnStart)
        {
            general.InitManagerOnStart = true;
            EditorUtility.SetDirty(general);
        }

        FeatureHelpers.RefreshFeatures(plataforma);
        var openxr = OpenXRSettings.GetSettingsForBuildTargetGroup(plataforma);
        if (openxr == null)
        {
            Debug.LogError($"No se encontraron los ajustes de OpenXR para {plataforma}.");
            return;
        }
        Activar(openxr.GetFeature<OculusTouchControllerProfile>());
        Activar(openxr.GetFeature<MetaQuestTouchPlusControllerProfile>());
        Activar(openxr.GetFeature<MetaQuestTouchProControllerProfile>());
        if (plataforma == BuildTargetGroup.Android)
            Activar(openxr.GetFeature<MetaQuestFeature>());
        AssetDatabase.SaveAssets();
    }

    private static void Activar(UnityEngine.XR.OpenXR.Features.OpenXRFeature funcion)
    {
        if (funcion == null || funcion.enabled) return;
        funcion.enabled = true;
        EditorUtility.SetDirty(funcion);
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
