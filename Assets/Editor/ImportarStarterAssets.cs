using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;

// Son los recursos de los paquetes de Unity que usan las escenas guardadas.
[InitializeOnLoad]
public static class ImportarStarterAssets
{
    static ImportarStarterAssets() => EditorApplication.delayCall += Importar;

    private static void Importar()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += Importar;
            return;
        }

        // TextMesh Pro se importa antes para que los Starter Assets encuentren su fuente.
        if (!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
        {
            var ugui = PackageInfo.FindForAssetPath("Packages/com.unity.ugui/package.json");
            if (ugui != null)
            {
                string archivo = Path.Combine(ugui.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");
                if (File.Exists(archivo)) AssetDatabase.ImportPackage(archivo, false);
            }
        }

        foreach (Sample ejemplo in Sample.FindByPackage("com.unity.xr.interaction.toolkit", "3.5.1"))
        {
            bool seUsa = ejemplo.displayName == "Starter Assets" ||
                         ejemplo.displayName == "XR Interaction Simulator";
            if (seUsa && !ejemplo.isImported)
            {
                if (ejemplo.Import(Sample.ImportOptions.None))
                    EditorApplication.delayCall += Importar;
                return;
            }
        }
    }
}
